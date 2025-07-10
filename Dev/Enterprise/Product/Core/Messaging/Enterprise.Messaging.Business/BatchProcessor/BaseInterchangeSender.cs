using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Messaging.Business
{
	public abstract class BaseInterchangeSender : BatchProcess, IProcessor, INotificationHandler
	{
		protected BaseInterchangeSender()
		{
		}

		protected BaseInterchangeSender(LoggingInformation logger)
			: base(logger)
		{
		}

		protected abstract bool SendInt(EDIInterchange interchange);
		protected abstract void SendOutboundInterchanges(CancellationToken cancellationToken);

		#region SendIntToFile
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		protected internal bool SendIntToFile(EDIInterchange interchange, string outputDirectory, string fileName)
		{
			try
			{
				using (FileStream outStream = File.Create(Path.Combine(outputDirectory, fileName)))
				{
					byte[] outBytes = Encoding.ASCII.GetBytes(interchange.EI_InterchangeText);
					outStream.Write(outBytes, 0, outBytes.Length);
				}
			}
			catch (ArgumentException e)
			{
				return ReportException(e, "The output registry entry contains invalid characters.", outputDirectory);
			}
			catch (SecurityException e)
			{
				return ReportException(e, "The required permissions to write to the output path have not been configured." + System.Environment.NewLine +
					"The network administrator will be required to resolve either network/user permissions.", outputDirectory);
			}
			catch (DirectoryNotFoundException e)
			{
				return ReportException(e, "The specified path for the output registry entry is invalid, such as being on an unmapped drive.", outputDirectory);
			}
			catch (UnauthorizedAccessException e)
			{
				return ReportException(e, "Write access  is not permitted by the operating system for the output path.", outputDirectory);
			}
			catch (PathTooLongException e)
			{
				return ReportException(e, "The output registry entry path exceed the system-defined maximum length." + System.Environment.NewLine +
					"For example, on Windows-based platforms, paths must be less than 248 characters.", outputDirectory);
			}
			catch (IOException e)
			{
				return ReportException(e, e.Message.Replace("\r\n", ""), outputDirectory);
			}

			OnSuccessfulSend(interchange);

			return true;
		}

		protected virtual void OnSuccessfulSend(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchange.Status.Sent;
		}

		#endregion

		#region ReportException
		protected bool ReportException(Exception e, string reasonsText, string outputDirectory)
		{
			var shouldReport = false;
			lock (LastSenderFailureTimeLock)
			{
				if (lastSenderFailureTime.IsEmpty || lastSenderFailureTime.AddMinutes(5) < ZDateTime.Now)
				{
					lastSenderFailureTime = ZDateTime.Now;
					shouldReport = true;
				}
			}
			if (shouldReport)
			{
				Logger.Log("The batch processor has failed to create outbound files with the following error : " + e.Message);
				ZString subject = Res.GetString("c6b62fc2-a0ab-4c19-8c81-c0dd08240528", "batch processor is unable to create files in outbound directory.");
				ZString body = Res.GetString("b3313c0f-523d-400b-a962-a8e515e859dc", "The batch processor has failed to create outbound files with the following error : {0}\r\nThe directory that the batch processor is trying to write to is {1}.\r\nThe machine name that the batch processor is running on is : {2}\r\n\r\nThe problem may be caused by one of the following : \r\n{3}\r\n\r\nPlease fix the problem.  Once the problem is resolved the batch processor will resume processing correctly", e.Message, outputDirectory, System.Environment.MachineName, reasonsText);

				Env.OutgoingMailManager.CreateAndSaveToCompanyNotificationGroup(subject, body);
			}
			return false;
		}
		#endregion

		protected virtual BusinessObjectFactory GetFactory()
		{
			return new BusinessObjectFactory();
		}

		protected ZQuery GetMessageQuery(string[] applicationCode, bool includeValidTransmitDateFilter)
		{
			var filter = new ZQuery();
			filter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
			filter.AddToFilter(EDIMessageSchema.EM_EI, null);
			filter.AddToFilter(EDIMessageSchema.EM_ApplicationCode, applicationCode);
			filter.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			filter.AddToFilter(ValidBranchesForMessageFilter(applicationCode));
			if (includeValidTransmitDateFilter)
			{
				filter.AddToFilter(ValidTransmitDateMessageFilter);
			}
			filter.AddToFilter(AdditionalFilter);
			filter.IncludeBlob(EDIMessageSchema.EM_MessageText);
			filter.IncludeBlob(EDIMessageSchema.EM_MessageNText);
			filter.MaximumRows = NumberToBatch;
			filter.OrderBy = EDIMessage.Schema.EM_MessageNum;
			filter.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));
			return filter;
		}

		protected virtual void PrepareInterchanges(string[] applicationCode)
		{
			var factory = GetFactory();
			factory.RefreshEnabled = false;

			var readyMessages = new NonDependentEDIMessageCollection(factory);
			readyMessages.Load(GetMessageQuery(applicationCode, true));

			try
			{
				PackageMessagesIntoInterchanges(readyMessages);

				factory.Save();

				if (readyMessages.Count > 0)
				{
					Logger.Log(FormattableString.Invariant($"{readyMessages.Count} message(s) prepared for sending."));
				}
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex, this);
				Logger.Log(ex.Message, Enterprise.Integration.LogType.Error);
			}
			catch (InterchangePreparationException ex)
			{
				Logger.Log(ex.Message);
			}
		}

		protected abstract void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages);

		#region ValidBranchesForMessageFilter

		protected virtual ZQuery ValidBranchesForMessageFilter(string[] applicationCode) => new ZQuery(EDIMessageSchema.EM_GB, GetBranchPks());

		#endregion

		#region ValidTransmitDateMessageFilter
		protected ZQuery ValidTransmitDateMessageFilter
		{
			get
			{
				ZQuery result = new ZQuery();
				result.AddToFilter(EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.Equal, null);
				result.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.LessThanOrEqualTo, (IsDateFilterUTC ? ZDateTime.UtcNow : ZDateTime.Now));
				return result;
			}
		}

		protected virtual bool IsDateFilterUTC
		{
			get { return false; }
		}

		#endregion

		#region ValidBranchesForInterchangesFilter

		protected virtual ZQuery ValidBranchesForInterchangesFilter => new ZQuery(EDIInterchangeSchema.EI_GB, GetBranchPks());

		#endregion

		#region SendOutboundInterchanges
		protected void SendOutboundInterchanges(string applicationCode, CancellationToken token)
		{
			InterchangesResent = 0;
			int numberOfInterchangesSent = 0;

			int interchangeNum = 0;

			try
			{
				do
				{
					token.ThrowIfCancellationRequested();
					numberOfInterchangesSent = SendOutboundInterchange(applicationCode, interchangeNum, numberOfInterchangesSent, token);
				} while (SendMoreInterchanges());
			}
			finally
			{
				NumberOfOperationsSucceeded += numberOfInterchangesSent;
				LogMessagesOnSent(numberOfInterchangesSent);
			}
		}

		protected virtual void LogMessagesOnSent(int numberOfInterchangesSent)
		{
			if (numberOfInterchangesSent - InterchangesResent > 0)
			{
				Logger.Log((numberOfInterchangesSent - InterchangesResent).ToString(CultureInfo.InvariantCulture) + " new interchange(s) sent.");
			}
			if (InterchangesResent > 0)
			{
				Logger.Log(InterchangesResent.ToString(CultureInfo.InvariantCulture) + " unacknowledged interchange(s) resent.");
			}
			if (SendableInterchangesCache != null)
			{
				if (SendableInterchanges.Length - numberOfInterchangesSent > 0)
				{
					Logger.Log((SendableInterchanges.Length - numberOfInterchangesSent).ToString(CultureInfo.InvariantCulture) + " interchange(s) failed to be sent.");
				}
			}
		}

		public bool CommitPerInterchange = true;

		protected virtual bool SendMoreInterchanges()
		{
			return SendableInterchanges.Length > 0 && WithinExecutionTime;
		}

		protected BusinessObjectFactory sharedFactory;
		protected BusinessObjectFactory SharedFactory
		{
			get
			{
				if (sharedFactory == null)
				{
					sharedFactory = GetFactory();
				}
				return sharedFactory;
			}
		}

		protected virtual void ReleaseSharedFactory()
		{
			sharedFactory = null;
		}

		protected ZGuid[] GetBranchPks()
		{
			var query = new ZDBOnlyQuery(typeof(GlbBranch));
			query.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);

			return SharedFactory.Load<GlbBranch>(query).Select(c => c.PK).ToArray();
		}

		protected virtual int SendOutboundInterchange(string applicationCode, int interchangeNum, int numberOfInterchangesSent, CancellationToken token)
		{
			SharedFactory.RefreshEnabled = false;
			PrepareInterchanges(new[] { applicationCode });
			GetSendableInterchanges(SharedFactory, applicationCode);
			bool saveChanges = false;
			foreach (EDIInterchange interchange in SendableInterchanges)
			{
				token.ThrowIfCancellationRequested();
				saveChanges = true;
				interchangeNum++;
				if (SendInt(interchange))
				{
					numberOfInterchangesSent++;
					LogSentInterchangeMessage(interchangeNum.ToString(CultureInfo.InvariantCulture));
				}
				else
				{
					OnInterchangeSendFailed(interchange);
				}
				if (CommitPerInterchange)
				{
					Commit(SharedFactory);
				}
			}
			if (saveChanges && !CommitPerInterchange)
			{
				Commit(SharedFactory);
			}

			ReleaseSharedFactory();

			return numberOfInterchangesSent;
		}

		protected virtual void LogSentInterchangeMessage(string interchangeNum)
		{
			Logger.Log("Sent interchange " + interchangeNum);
		}

		protected virtual void OnInterchangeSendFailed(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchange.Status.Failed;
		}

		#endregion

		protected virtual void Commit(BusinessObjectFactory factory)
		{
			factory.Save();
		}

		#region GetSendableInterchanges
		protected virtual void GetSendableInterchanges(BusinessObjectFactory factory, string applicationCode)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(EDIInterchangeSchema.EI_Status, StatusMeaningQueued);
			filter.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, applicationCode);
			filter.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			filter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			filter.AddToFilter(EDIInterchangeSchema.EI_TransportType, SQLComparisonOperator.NotEqual, EDIInterchangeTransportTypeList.Codes.xT);
			filter.AddToFilter(ValidBranchesForInterchangesFilter);
			filter.IncludeBlob(EDIMessageSchema.EM_MessageText);
			filter.IncludeBlob(EDIMessageSchema.EM_MessageNText);
			filter.MaximumRows = NumberOfInterchangesToSendInThisRun;
			filter.OrderBy = EDIInterchange.Schema.EI_SystemLastEditTimeUtc + ", " + EDIInterchange.Schema.EI_InterchangeNum;
			filter.AddToFilter(ExtraFilter);

			SendableInterchanges = factory.Load(typeof(EDIInterchange), filter);
			NumberOfOperationsAttempted += SendableInterchanges.Length;
		}
		#endregion

		protected virtual ZString StatusMeaningQueued
		{
			get { return EDIInterchange.Status.Queued; }
		}

		protected virtual ZQuery ExtraFilter
		{
			get { return new ZQuery(); }
		}

		protected virtual ZQuery AdditionalFilter
		{
			get { return new ZQuery(); }
		}

		#region Execute
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		protected override void Execute(CancellationToken cancellationToken)
		{
			ExecuteProcessStarted = ZDateTime.Now;
			try
			{
				SendOutboundInterchanges(cancellationToken);
			}
			catch (MessageProcessingException e)
			{
				ZStringBuilder message = new ZStringBuilder();
				message.Append(e.Message);
				message.Append("\r\nThe interchange/message text:\r\n");
				message.Append(e.MessageOrInterchangeText);

				Logger.LogWarning(message.ToString());

				if (e.ShouldSendDeveloperInformation)
				{
					ErrorReporter.ReportOnce(e.Message, message.ToString(), e);
				}

				if (e.ShouldSendEmailToUsers)
				{
					Env.OutgoingMailManager.CreateAndSaveToCompanyNotificationGroup(Res.GetString("ffe7e7b2-6886-40e7-bfb9-13bbc94147be", "Batch Processor problems while processing interchanges and messages"), message.ToString());
				}
			}
		}

		#endregion

		#region NumberOfInterchangesToSendInThisRun

		protected virtual int NumberOfInterchangesToSendInThisRun
		{
			get { return Env.Registry.InterchangesPerRun; }
		}

		#endregion

		#region NumberToBatch

		protected virtual int NumberToBatch
		{
			get { return Env.Registry.MessagesPerInterchange; }
		}

		#endregion

		#region WithinExecutionTime

		protected ZBool WithinExecutionTime
		{
			get
			{
#if DEBUG
				// For tests, it's either IMMEDIATE or NEVER so the tests are predictable. Prefer IMMEDIATE since it's fast unless you want to try the maximum number of retries then use NEVER.
				if (TimedOutBehaviour.Value == TimedOutBehaviourForTest.IMMEDIATE)
				{
					return false;
				}
				if (TimedOutBehaviour.Value == TimedOutBehaviourForTest.NEVER)
				{
					return true;
				}
#endif
				ZBool result = ZBool.True;

				if (ExecuteProcessStarted.IsEmpty)
				{
					ExecuteProcessStarted = ZDateTime.Now;
				}
				ZDateTime timeWeNeedToStopExecuting = ExecuteProcessStarted.AddTicks(TimeToSpendProcessing.Ticks);

				if (ZDateTime.Now >= timeWeNeedToStopExecuting)
				{
					result = ZBool.False;
				}

				return result;
			}
		}

		public virtual TimeSpan TimeToSpendProcessing
		{
			get { return new TimeSpan(0, 1, 0); }
		}

		#endregion

		ZDateTime ExecuteProcessStarted;
		protected int InterchangesResent;
		protected internal BusinessObject[] SendableInterchanges
		{
			get
			{
				if (SendableInterchangesCache == null)
				{
					SendableInterchangesCache = Array.Empty<BusinessObject>();
					ErrorReporter.ReportOnce("F67D2F8D-360F-4E2A-B1AA-B3847A367528", "Access of SendableInterchanges before GetSendableInterchanges is called");
				}
				return SendableInterchangesCache;
			}
			set { SendableInterchangesCache = value; }
		}
		protected BusinessObject[] SendableInterchangesCache;

		protected string IntFilename = "ESEND-99.EDI";

		[ThreadSafe] static ZDateTime lastSenderFailureTime = ZDateTime.Empty;
		static readonly object LastSenderFailureTimeLock = new object();

		#region IProcessor Members

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			LoggingInformation previousLogger = Logger;
			try
			{
				Logger = new NotificationSubscriberLogger(notifications);
				ExecuteBatch(token);
			}
			finally
			{
				Logger = previousLogger;
			}
		}

		#endregion

		#region INotificationHandler Members

		void INotificationHandler.ReportError(string message, string caption, string errorContext, Exception exception)
		{
			Logger.LogError(message);
		}

		void INotificationHandler.ReportInformation(string message, string caption)
		{
			Logger.Log(message);
		}

		#endregion

		#region Test
#if DEBUG
		public enum TimedOutBehaviourForTest
		{
			DEFAULT,
			NEVER,
			IMMEDIATE
		}

		public static readonly Overridable<TimedOutBehaviourForTest> TimedOutBehaviour = new Overridable<TimedOutBehaviourForTest>(TimedOutBehaviourForTest.IMMEDIATE);

#endif
		#endregion
	}

	/// <summary>
	/// This exception is caught while preparing interchanges
	/// </summary>
	[Serializable]
	public class InterchangePreparationException : ApplicationException
	{
		public InterchangePreparationException(ZString errorMessage)
			: base(errorMessage)
		{
		}

#if NETFRAMEWORK
		protected InterchangePreparationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
