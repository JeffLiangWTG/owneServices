using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.CA.Services;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class AVSQueryMessageProcessor
	{
		public AVSQueryMessageProcessor(ILogger serviceLogger)
		{
			logger = Argument.NotNull(serviceLogger, "serviceLogger");
		}
		readonly ILogger logger;

		#region Process

		public void Process()
		{
			logger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "AIRS Validation Query executing for Company {0}.", GlbCompany.CurrentCompany.GC_Code));

			var factory = new BusinessObjectFactory();
			foreach (var message in GetProcessableAVSQueryMessages(factory, GlbCompany.CurrentCompany))
			{
				SqlApplicationLock sqlAppLock = null;
				try
				{
					if (TryGetLock(Db.Connection, message, out sqlAppLock))
					{
						ProcessCore(factory, message);
					}
				}
				finally
				{
					if (sqlAppLock != null)
					{
						sqlAppLock.Dispose();
					}
				}
			}
		}

		void ProcessCore(BusinessObjectFactory factory, EDIMessage message)
		{
			var avsQuerySucceed = false;
			var declaration = GetJobDeclaration(factory, message.EM_ApplicationReference, message.EM_GB);
			if (declaration == null)
			{
				UpdateEDIMessageWhenFailed(message, "Cannot find Declaration.", false);
			}
			else
			{
				var cts = CreateNewCancellationTokenSource(declaration);
				var avsRunner = GetAIRSValidationRunner(declaration);
				AIRSValidationQueriedLineCollection queriedLines = null;
				var checkResult = avsRunner.ValidateSetting();
				try
				{
					if (!checkResult.IsEmpty)
					{
						logger.Log(LogType.Error, checkResult);
						return;
					}
					else
					{
						queriedLines = avsRunner.AIRSValidationAll(false, cts);
					}
				}
				finally
				{
					cts.Dispose();
				}

				if (queriedLines == null || queriedLines.Count == 0)
				{
					UpdateEDIMessageWhenFailed(message, "There's no invoice line for AIRS Validation.", false, true);
				}
				else
				{
					var errorQueriedLines = queriedLines.Where(l => l.ValidationFaultMessageType == ValidationFaultMessageType.QueryAborted || l.ValidationFaultMessageType == ValidationFaultMessageType.HttpError);
					if (errorQueriedLines.Any())
					{
						UpdateEDIMessageWhenFailed(message, GetErrorMessages(errorQueriedLines), true);
					}
					else
					{
						avsRunner.PopulateValidateRequirementResults(declaration, queriedLines, ZDateTime.Now);
						UpdateEDIMessageWhenSucceed(message);
						avsQuerySucceed = true;
					}
				}
			}

			try
			{
				factory.Save();
			}
			catch (ZSaveException e)
			{
				avsQuerySucceed = false;
				if (e is ZSaveConcurrencyException && declaration != null)
				{
					logger.Log(LogType.Error, ZString.Format("AIRS Validation Query failed for Declaration {0}. There was a conflict with another users changes and the changes could not be saved.", declaration.JE_DeclarationReference));
				}
				else
				{
					ZExceptionReporting.HandleSaveException(e);
				}
			}
			finally
			{
				if (avsQuerySucceed)
				{
					logger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "AIRS Validation Query Succeed for Declaration {0}.", message.EM_ApplicationReference));
				}
			}
		}

		protected virtual AIRSValidationRunner GetAIRSValidationRunner(JobDeclaration declaration)
		{
			return new AIRSValidationRunner(declaration);
		}

		AVSQueryMessage[] GetProcessableAVSQueryMessages(BusinessObjectFactory factory, GlbCompany company)
		{
			var filter = new ZDBOnlyQuery(typeof(EDIMessage));
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CFIAQuery);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.AVSQuery);
			filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, MessageTypeList.Codes.AVSQuery);
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			filter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			filter.AddToFilter(EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
			filter.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));
			var branchFilter = new ZDBOnlySubQuery(typeof(GlbBranch), EDIMessageSchema.EM_GB);
			branchFilter.AddToFilter(GlbBranchSchema.GB_GC, company.PK);
			filter.AddSubQuery(branchFilter, JoinCondition.And);

			return factory.Load<AVSQueryMessage>(filter);
		}

		JobDeclaration GetJobDeclaration(BusinessObjectFactory factory, ZString reference, ZGuid branchPK)
		{
			var filter = new ZQuery();
			filter.AddToFilter(JobDeclarationSchema.JE_DeclarationReference, reference);
			filter.AddToFilter(JobDeclarationSchema.JE_GB, branchPK);

			return factory.Load<JobDeclaration>(filter).FirstOrDefault();
		}

#if DEBUG
		protected virtual
#endif
		CancellationTokenSource CreateNewCancellationTokenSource(JobDeclaration declaration)
		{
			var timeoutInMinutes = CACustomsDataRegistry.Instance.AVSTimeoutInMinutes.GetFallBackValueAtAllLevels(declaration.CompanyPK.ToGuid(), declaration.JE_GB.ToGuid(), Guid.Empty);
			return new CancellationTokenSource(new TimeSpan(0, timeoutInMinutes, 0));
		}

		#endregion

		#region RetryInformation

		string GetErrorMessages(IEnumerable<IAIRSValidationQueriedLine> errorQueriedLines)
		{
			var errorMessages = new List<string>();
			foreach (var queriedLine in errorQueriedLines)
			{
				if (!queriedLine.ValidationFaultMessage.IsEmpty && !errorMessages.Contains(queriedLine.ValidationFaultMessage))
				{
					errorMessages.Add(queriedLine.ValidationFaultMessage);
				}
			}
			return new ZStringBuilder(errorMessages).ToStringWithNewLineBetweenAppends();
		}

		AVSQueryRetryInfoBO SetRetryInformation(EDIMessage message, string failureMessage)
		{
			var retryInfo = new AVSQueryRetryInfoBO();
			if (!message.EM_MessageText.IsEmpty)
			{
				retryInfo.Deserialize(message.EM_MessageText);
				retryInfo.RetryTimes++;
			}
			retryInfo.LastFailure = failureMessage;
			message.EM_MessageText = retryInfo.Serialize();
			return retryInfo;
		}

		void UpdateEDIMessageWhenFailed(EDIMessage message, ZString failureMessage, bool canRetry, bool isWarning = false)
		{
			var retryInfo = SetRetryInformation(message, failureMessage);

			if (canRetry)
			{
				if (retryInfo.RetryTimes == RetryTimes)
				{
					message.EM_HeldUntilDate = ZDateTime.UtcNow.AddHours(12);
				}
				else if (retryInfo.RetryTimes < RetryTimes)
				{
					message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(RetryIntervalInMinutes);
				}
				else
				{
					canRetry = false;
				}
			}
			if (!canRetry)
			{
				message.EM_Status = isWarning ? EDIMessage.Status.Warning : EDIMessage.Status.Failed;
				message.EM_HeldUntilDate = ZDateTime.Empty;
			}

			logger.Log(LogType.Error, string.Format(CultureInfo.CurrentCulture, "AIRS Validation Query for Declaration {0}: {1}", message.EM_ApplicationReference, failureMessage));
		}

		void UpdateEDIMessageWhenSucceed(EDIMessage message)
		{
			message.EM_Status = EDIMessage.Status.Acknowledged;
			message.EM_HeldUntilDate = ZDateTime.Empty;
			message.EM_MessageText = ZString.Empty;
		}

		#endregion

		#region Lock EDIMessasge

		internal static bool TryGetLock(DbConnection connection, EDIMessage message, out SqlApplicationLock appLock)
		{
			try
			{
				var key = "AVSQueryMessageProcessor:" + message.PK.ToString().ToUpperInvariant();
				return connection.TryGetLock(key, out appLock);
			}
			catch (SqlException sqlException)
			{
				ErrorReporter.ReportOnce("DB Connection error while grabbing EDIMessage mutexes", sqlException);

				appLock = null;
				return false;
			}
		}

		#endregion

		#region Registry

		int RetryTimes
		{
			get { return CACustomsDataRegistry.Instance.AVSRetryTimes.Value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		int RetryIntervalInMinutes
		{
			get { return CACustomsDataRegistry.Instance.AVSRetryIntervalInMinutes.Value; }
		}

		#endregion
	}
}
