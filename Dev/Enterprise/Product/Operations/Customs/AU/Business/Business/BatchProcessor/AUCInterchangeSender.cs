using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCInterchangeSender : BaseInterchangeSender, ICustomsServiceTaskProcess
	{
		public AUCInterchangeSender() : base()
		{ }

		protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
		{
			if (messages.Count > 0)
			{
				InterchangeProviderBase interchangeProvider = null;

				switch (messages[0].EM_ApplicationCode.ToString())
				{
					case EDIMessage.ApplicationCodes.CMR:
						interchangeProvider = new CMRInterchangeProvider(messages);
						break;
					case EDIMessage.ApplicationCodes.OneStop:
						interchangeProvider = new OneStopInterchangeProvider(messages);
						break;
					case EDIMessage.ApplicationCodes.EXDOC:
						interchangeProvider = new EXDOCInterchangeProvider(messages);
						break;
					case EDIMessage.ApplicationCodes.NEXDOCS:
						interchangeProvider = new NEXDOCInterchangeProvider(messages);
						break;
					case EDIMessage.ApplicationCodes.COLS:
						interchangeProvider = new AUCOLSInterchangeProvider(messages);
						break;
					default:
						break;
				}

				if (interchangeProvider == null)
				{
					ErrorReporter.ReportOnce("KEY:C8BD7CD0-5028-49B3-8AA5-BC14CA5BBB7E", "Unsupported Application Code : " + messages[0].EM_ApplicationCode);
				}
				else
				{
					interchangeProvider.PackCollatedMessagesIntoInterchanges();
				}
			}
		}

		#region Implementation

		protected override void PrepareInterchanges(string[] applicationCodes)
		{
			var backgroundCargoReportMaxMsg = AUCustomsDataRegistry.Instance.BackgroundCargoReportMaxMsg.Value;
			var backgroundCargoReportSubThrottleWindow = AUCustomsDataRegistry.Instance.BackgroundCargoReportSubThrottleWindow.Value;

			if (!applicationCodes.Any(x => x == EDIMessage.ApplicationCodes.CMR) || backgroundCargoReportMaxMsg == 0 || backgroundCargoReportSubThrottleWindow == 0)
			{
				base.PrepareInterchanges(applicationCodes);
				return;
			}

			var factory = GetFactory();
			factory.RefreshEnabled = false;

			ClearHeldUntilDateOnUrgentAirCargoMessages(factory);
			ClearHeldUntilDateOnUrgentSeaCargoMessages(factory);
			var messageCount = PackageReadyMessages(factory, applicationCodes);

			var numberToBatch = NumberToBatch;
			if (messageCount < numberToBatch || numberToBatch == 1) // Just in case the registry is set to 1
			{
				messageCount += PackageCargoMessages(factory, messageCount, backgroundCargoReportMaxMsg, backgroundCargoReportSubThrottleWindow);
			}

			try
			{
				factory.Save();

				if (messageCount > 0)
				{
					Logger.Log(FormattableString.Invariant($"{messageCount} message(s) prepared for sending."));
				}
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex, this);
			}
			catch (InterchangePreparationException ex)
			{
				Logger.Log(ex.Message);
			}
		}

		int PackageCargoMessages(BusinessObjectFactory factory, int messageCount, int backgroundCargoReportMaxMsg, int backgroundCargoReportSubThrottleWindow)
		{
			int result = 0;

			var maximumAllow = Math.Max(1, NumberToBatch - messageCount); // just in case NumberToBatch is set at 1
			var numberOfMessageTillLimit = Math.Max(0, backgroundCargoReportMaxMsg - GetNumberOfHeldCargoMessagesSentInLastWindowTimeFrame(backgroundCargoReportSubThrottleWindow));
			var maxMessages = Math.Min(maximumAllow, numberOfMessageTillLimit);

			if (maxMessages > 0)
			{
				var filter = GetHeldCargoFilter(new[] { CMRMessage.CMRMessageTypes.AIRCR, CMRMessage.CMRMessageTypes.AIROUT, CMRMessage.CMRMessageTypes.SEACR });
				filter.MaximumRows = maxMessages;
				filter.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc + "," + EDIMessage.Schema.EM_MessageNum;

				var heldMessages = new NonDependentEDIMessageCollection(factory);
				heldMessages.Load(filter);
				PackageMessagesIntoInterchanges(heldMessages);

				result = heldMessages.Count;
				if (result > 0)
				{
					var lastAirCargoMessage = heldMessages.Cast<EDIMessage>().LastOrDefault(x => x.EM_MessageType == CMRMessage.CMRMessageTypes.AIRCR);
					if (lastAirCargoMessage != null)
					{
						AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldAirCargoMessage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, lastAirCargoMessage.EM_SystemCreateTimeUtc.ToDateTime());
					}

					var lastSeaCargoMessage = heldMessages.Cast<EDIMessage>().LastOrDefault(x => x.EM_MessageType == CMRMessage.CMRMessageTypes.SEACR);
					if (lastSeaCargoMessage != null)
					{
						AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldSeaCargoMessage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, lastSeaCargoMessage.EM_SystemCreateTimeUtc.ToDateTime());
					}
				}
			}

			return result;
		}

		int PackageReadyMessages(BusinessObjectFactory factory, string[] applicationCodes)
		{
			var filter = GetMessageQuery(applicationCodes, true);
			var notHeldNotOriginalNotCargoFilter = new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.NotEqual, new[] { CMRMessage.CMRMessageTypes.AIRCR, CMRMessage.CMRMessageTypes.AIROUT, CMRMessage.CMRMessageTypes.SEACR });
			notHeldNotOriginalNotCargoFilter.DefaultJoinCondition = JoinCondition.Or;
			notHeldNotOriginalNotCargoFilter.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.NotEqual, CMRMessage.MessageSubTypes.Original);
			notHeldNotOriginalNotCargoFilter.AddToFilter(EDIMessageSchema.EM_HeldUntilDate, null);
			filter.AddToFilter(notHeldNotOriginalNotCargoFilter);

			var readyMessages = new NonDependentEDIMessageCollection(factory);
			readyMessages.Load(filter);
			PackageMessagesIntoInterchanges(readyMessages);

			return readyMessages.Count;
		}

		ZQuery GetHeldCargoFilter(string[] messageTypes)
		{
			var filter = GetMessageQuery(new[] { EDIMessage.ApplicationCodes.CMR }, false);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, messageTypes);
			filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, CMRMessage.MessageSubTypes.Original);
			filter.AddToFilter(EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.NotEqual, null);
			return filter;
		}

		void ClearHeldUntilDateOnUrgentAirCargoMessages(BusinessObjectFactory factory)
		{
			var filterSql = $@"
{CusHAWBSchema.Constants.CS_CM} IN (
SELECT {CusMAWBSchema.Constants.PK}
FROM {CusMAWBSchema.Constants.SqlSchemaName}.{CusMAWBSchema.Constants.TableName}
CROSS APPLY dbo.GetTimeZoneOffsetInMinutes({CusMAWBSchema.Constants.CM_RL_NKDischargePort}, {CusMAWBSchema.Constants.CM_ArrivalDate}) AS UtcOffset
WHERE {CusMAWBSchema.Constants.CM_RL_NKDischargePort} <> '' AND {CusMAWBSchema.Constants.CM_ArrivalDate} IS NOT NULL
AND DATEADD(minute, UtcOffset.Offset + 240, DATEADD(MINUTE, 15, GETUTCDATE())) > {CusMAWBSchema.Constants.CM_ArrivalDate})";

			var urgentAirCargoFilter = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.PK);
			urgentAirCargoFilter.AddFilterAndZSQLParameterCollection(filterSql, new ZSqlParameterCollection());

			var filter = new ZDBOnlyQuery(typeof(EDIMessage));
			var lastCreateTime = AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldAirCargoMessage.Value;
			if (lastCreateTime == DateTime.MinValue || lastCreateTime > ZDateTime.UtcNow.ToDateTime())
			{
				lastCreateTime = ZDateTime.UtcNow.AddMonths(-1).ToDateTime(); // help performance
			}
			filter.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, lastCreateTime);
			filter.AddToFilter(GetHeldCargoFilter(new[] { CMRMessage.CMRMessageTypes.AIRCR }));
			filter.AddSubQuery(EDIMessageSchema.EM_LinkUniqueID, urgentAirCargoFilter, JoinCondition.And);

			var readyMessages = new NonDependentEDIMessageCollection(factory);
			readyMessages.Load(filter);
			foreach (EDIMessage message in readyMessages)
			{
				message.EM_HeldUntilDate = ZDateTime.Empty;
			}
		}

		void ClearHeldUntilDateOnUrgentSeaCargoMessages(BusinessObjectFactory factory)
		{
			var filterSql = $@"
{CusSCAHouseSchema.Constants.CA_CB} IN (
SELECT {CusSCAOceanBillSchema.Constants.PK}
FROM {CusSCAOceanBillSchema.Constants.SqlSchemaName}.{CusSCAOceanBillSchema.Constants.TableName}
CROSS APPLY dbo.GetTimeZoneOffsetInMinutes({CusSCAOceanBillSchema.Constants.CB_RL_NKPortOfDischarge}, {CusSCAOceanBillSchema.Constants.CB_DateOfArrival}) AS UtcOffset
WHERE {CusSCAOceanBillSchema.Constants.CB_RL_NKPortOfDischarge} <> '' AND {CusSCAOceanBillSchema.Constants.CB_DateOfArrival} IS NOT NULL
AND DATEADD(minute, UtcOffset.Offset + 240, DATEADD(MINUTE, 15, GETUTCDATE())) > {CusSCAOceanBillSchema.Constants.CB_DateOfArrival})";

			var urgentSeaCargoFilter = new ZDBOnlySubQuery(typeof(CusSCAHouse), CusSCAHouseSchema.PK);
			urgentSeaCargoFilter.AddFilterAndZSQLParameterCollection(filterSql, new ZSqlParameterCollection());

			var filter = new ZDBOnlyQuery(typeof(EDIMessage));
			var lastCreateTime = AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldSeaCargoMessage.Value;
			if (lastCreateTime == DateTime.MinValue || lastCreateTime > ZDateTime.UtcNow.ToDateTime())
			{
				lastCreateTime = ZDateTime.UtcNow.AddMonths(-1).ToDateTime(); // help performance
			}
			filter.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, lastCreateTime);
			filter.AddToFilter(GetHeldCargoFilter(new[] { CMRMessage.CMRMessageTypes.SEACR }));
			filter.AddSubQuery(EDIMessageSchema.EM_LinkUniqueID, urgentSeaCargoFilter, JoinCondition.And);

			var readyMessages = new NonDependentEDIMessageCollection(factory);
			readyMessages.Load(filter);
			foreach (EDIMessage message in readyMessages)
			{
				message.EM_HeldUntilDate = ZDateTime.Empty;
			}
		}

		int GetNumberOfHeldCargoMessagesSentInLastWindowTimeFrame(int backgroundCargoReportSubThrottleWindow)
		{
			var messageTypes = new[] { CMRMessage.CMRMessageTypes.AIRCR, CMRMessage.CMRMessageTypes.AIROUT, CMRMessage.CMRMessageTypes.SEACR };
			var throttleWindow = ZDateTime.UtcNow.AddMinutes(-backgroundCargoReportSubThrottleWindow);
			var interchangeSubQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
			interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, throttleWindow);
			interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.CMR);
			interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, messageTypes);
			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			messageQuery.AddSubQuery(EDIMessageSchema.EM_EI, interchangeSubQuery, JoinCondition.And);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CMR);
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, messageTypes);
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, CMRMessage.MessageSubTypes.Original);
			messageQuery.AddToFilter(EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.NotEqual, null);
			messageQuery.AddToFilter(ValidBranchesForMessageFilter(new[] { EDIMessage.ApplicationCodes.CMR }));
			messageQuery.AddToFilter(AdditionalFilter);
			var factory = new BusinessObjectFactory();
			var result = factory.GetDatabaseCount(typeof(EDIMessage), messageQuery);
			return result;
		}

		protected override bool SendInt(EDIInterchange interchange)
		{
			switch (interchange.EI_ApplicationCode)
			{
				case EDIInterchange.ApplicationCodes.OneStop:
					return (SendOneStopInterchange(interchange));
				case EDIInterchange.ApplicationCodes.EXDOC:
					return (SendExdocInterchange(interchange));
				case EDIInterchange.ApplicationCodes.NEXDOCS:
					return true;
				case EDIInterchange.ApplicationCodes.COLS:
					return true;
				default:
					throw new ApplicationException("Invalid application code");
			}
		}

		protected bool SendOneStopInterchange(EDIInterchange interchange)
		{
			ZString destinationEmailAddress = "";
			switch (interchange.EI_To)
			{
				case "CSXTEST":
				case "CSXLIVE":
					destinationEmailAddress = AUCConstants.CSXADLLiveEmailAddress;
					break; // CSX Test system is reached by chaing the account code, not the email address. See SharedComponents.
				case "1STOPLIVE":
					destinationEmailAddress = AUCConstants.OneStopLiveEmailAddress;
					break;
				case "1STOPTEST":
					destinationEmailAddress = AUCConstants.OneStopTestEmailAddress;
					break;
			}

			bool success = false;
			if (!destinationEmailAddress.IsEmpty)
			{
				if (new OutgoingInterchange(interchange).Send(Logger, destinationEmailAddress, Core.MessagingConstants.eRouterPRAEmailAddress))
				{
					success = true;
					interchange.LogInterchangeInProgressForAllMessages();
					Logger.DebugLog("Interchange #" + interchange.EI_InterchangeNum + " being sent for 1st and only time.");
					interchange.EI_Status = EDIInterchange.Status.Sent;
				}
			}
			return success;
		}

		protected bool SendExdocInterchange(EDIInterchange interchange)
		{
			bool success = false;
			ZString destinationEmailAddress = Env.Registry.AQISMessagingTestMode ? AUCustomsDataRegistry.GetEXDOCTestEmailAddress() : AUCustomsDataRegistry.GetEXDOCProdEmailAddress();
			OutgoingInterchange interchangeToSend = new OutgoingInterchange(interchange);
			if (interchangeToSend.Send(Logger, destinationEmailAddress))
			{
				success = true;
				interchange.LogInterchangeInProgressForAllMessages();
				Logger.DebugLog("Interchange #" + interchange.EI_InterchangeNum + " being sent for 1st and only time.");
				interchange.EI_Status = EDIInterchange.Status.Sent;
			}
			return success;
		}

		protected override void SendOutboundInterchanges(CancellationToken token)
		{
			NumberOfOperationsAttempted = 0;
			SendOutboundCMRInterchanges();
			SendableInterchanges = null;
			SendOutboundInterchanges(EDIInterchange.ApplicationCodes.OneStop, token);
			SendableInterchanges = null;
			SendOutboundInterchanges(EDIInterchange.ApplicationCodes.EXDOC, token);
			SendableInterchanges = null;
			SendOutboundInterchanges(EDIInterchange.ApplicationCodes.NEXDOCS, token);
			SendableInterchanges = null;
			SendOutboundInterchanges(EDIInterchange.ApplicationCodes.COLS, token);
		}

		int numberOfInterchangesSent;
		int interchangeNum;

		protected void SendOutboundCMRInterchanges()
		{
			if (CertManager.CompanyCertificate != null)
			{
				InterchangesResent = 0;
				numberOfInterchangesSent = 0;
				interchangeNum = 0;

				try
				{
					do
					{
						SendOutboundCMRInterchange();
					}
					while (SendMoreInterchanges());
				}
				finally
				{
					NumberOfOperationsSucceeded = numberOfInterchangesSent;
					if (SendableInterchangesCache != null) // an exception may have occured in GetSendableInterchanges
					{
						if (numberOfInterchangesSent - InterchangesResent > 0)
						{
							Logger.Log(numberOfInterchangesSent - InterchangesResent + " new interchange(s) sent.");
						}
						if (InterchangesResent > 0)
						{
							Logger.LogWarning(InterchangesResent + " unacknowledged interchange(s) resent.");
						}
						if (SendableInterchanges.Length - numberOfInterchangesSent > 0)
						{
							Logger.LogWarning((SendableInterchanges.Length - numberOfInterchangesSent) + " interchange(s) not sent.");
						}
					}
				}
			}
		}

		public override TimeSpan TimeToSpendProcessing
		{
			get { return new TimeSpan(0, 5, 0); }
		}

		protected override int NumberToBatch
		{
			get
			{
				var result = base.NumberToBatch;
				return result < 100 ? result : 100;
			}
		}

		protected
#if DEBUG
		virtual
#endif
		void SendOutboundCMRInterchange()
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;

			PrepareInterchanges(new[] { Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages });
			GetSendableCMRInterchanges(factory);

			foreach (EDIInterchange baseInterchange in SendableInterchanges)
			{
				var interchange = baseInterchange as CMRInterchange;
				if (interchange == null)
				{
					throw new ApplicationException(string.Format("Expected a CMRInterchange but was a {0}", interchange.GetType().ToString()));
				}

				bool iseHubInterchangeBeingReprocessed = !interchange.eHubID.IsEmpty;
				if (SendCMRInterchange(interchange, iseHubInterchangeBeingReprocessed))
				{
					if (interchange.EI_InterchangeType != "CTL" || !iseHubInterchangeBeingReprocessed)
					{
						interchangeNum++;
						numberOfInterchangesSent++;
						Logger.Log("Sending interchange " + interchangeNum);
					}
				}
				else
				{
					interchangeNum++;
					Logger.LogWarning("Interchange " + interchangeNum + " not sent");
				}
				try
				{
					interchange.Factory.Save();
				}
				catch (ZSaveConcurrencyException ex)
				{
					Logger.LogWarning("Save failed for Interchange " + interchangeNum + ": " + ex.Message);
				}
			}
		}

		protected
#if DEBUG
 virtual
#endif
		bool SendCMRInterchange(CMRInterchange interchange, bool iseHubInterchangeBeingReprocessed)
		{
			bool success = false;
			if (iseHubInterchangeBeingReprocessed && interchange.EI_InterchangeType != "CTL")
			{
				interchange.EI_RetryCount = interchange.InterchangeInProgressLogs.Count;// eHub may have incremented retry count, so we set to what we want now that we are going to re-send
			}
			int retries = interchange.EI_RetryCount;
			if (retries >= Env.Registry.InterchangeMaxSends)
			{
				Logger.LogError(string.Format("Interchange #{0}, sent via eHub, has exceeded its maximum resend count.", interchange.EI_InterchangeNum));
				interchange.MarkAsFailedToBeSent("Max Resends exceeded");
			}
			else
			{
				success = SendViaEHub(interchange, iseHubInterchangeBeingReprocessed);
				if (success)
				{
					interchange.InterchangeInProgressLogs.AddNew();
					interchange.LogInterchangeInProgressForAllMessages();
					if (interchange.EI_InterchangeType == "CTL")
					{
						if (!iseHubInterchangeBeingReprocessed)
						{
							Logger.DebugLog(string.Format("Interchange #{0} being sent for 1st and only time via eHub (because the interchange contains only CONTRL messages).", interchange.EI_InterchangeNum));
						}
					}
					else
					{
						if (retries > 0)
						{
							InterchangesResent++;
							Logger.LogWarning(string.Format("Interchange #{0} being re-sent for {1} time via eHub.", interchange.EI_InterchangeNum, GetNth(retries + 1)));
						}
						else
						{
							Logger.DebugLog(string.Format("Interchange #{0} being sent for first time, via eHub.", interchange.EI_InterchangeNum));
						}
						interchange.EI_RetryCount++;
					}
				}
				else
				{
					interchange.EI_RetryCount++;
				}
			}
			return success;
		}

		bool SendViaEHub(CMRInterchange interchange, bool iseHubInterchangeBeingReprocessed)
		{
			bool result = false;
			if (iseHubInterchangeBeingReprocessed)
			{
				interchange.EI_Status = interchange.EI_InterchangeType == "CTL" ? EDIInterchange.Status.Sent : EDIInterchange.Status.Queued; // CTLs are never acknowledged and so never re-sent
				result = true;
			}
			else
			{
				try
				{
					var intToSend = new OutgoingInterchange(interchange);
					intToSend.InterchangeFilename = interchange.EI_From + "_" + interchange.EI_InterchangeNum;
					intToSend.SigningCertificateStore = CertManager.CompanyCertificate;
					intToSend.PrepareSignedPayloadForEHub(Logger);
					result = true;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					var msg = "Failed to prepare payload for eHub";
					if (Logger != null)
					{
						Logger.LogWarning(msg + " : " + e.Message);
					}

					interchange.Logs.AddNew(Events.InterchangeFailedToBeSent, msg);
				}
			}
			if (result && interchange.EI_InterchangeType != "CTL")
			{
				interchange.EI_RetryCount = -1;// eHub requires count to be zero (incremented later) to avoid an eHub delay in sending
			}

			return result;
		}

		string GetNth(int number)
		{
			return NumberFormatter.ToOrdinalString(number);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		protected void GetSendableCMRInterchanges(BusinessObjectFactory factory)
		{
			var subTimeFilterForNewMessages = new ZQuery(EDIInterchangeSchema.EI_RetryCount, 0);

			var subTimeFilterForRetryMessages = new ZQuery(EDIInterchangeSchema.EI_SystemLastEditTimeUtc, SQLComparisonOperator.LessThan, ZDateTime.UtcNow.AddMinutes(-Env.Registry.InterchangeResendDelay));
			subTimeFilterForRetryMessages.AddToFilter(EDIInterchangeSchema.EI_RetryCount, SQLComparisonOperator.GreaterThan, 0);

			var timeFilter = new ZQuery();
			timeFilter.AddToFilter(subTimeFilterForNewMessages);
			timeFilter.AddToFilter(subTimeFilterForRetryMessages, JoinCondition.Or);

			var filter = new ZQuery();
			filter.AddToFilter(EDIInterchangeSchema.EI_Status, new ZString[] { EDIInterchange.Status.Queued, EDIInterchange.Status.SendPending });
			filter.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.CMR);
			filter.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			filter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			filter.AddToFilter(timeFilter);
			filter.AddToFilter(ValidBranchesForInterchangesFilter);

			filter.MaximumRows = Env.Registry.InterchangesPerRun;
			filter.OrderBy = EDIInterchange.Schema.EI_SystemLastEditTimeUtc + ", " + EDIInterchange.Schema.EI_InterchangeNum;

			SendableInterchanges = factory.Load<EDIInterchange>(filter);
			NumberOfOperationsAttempted += SendableInterchanges.Length;
		}

		protected virtual CertificateManager CertManager => certManager ?? (certManager = new CertificateManager(new BusinessObjectFactory()));
		CertificateManager certManager;

		public override void Dispose()
		{
			base.Dispose();
			certManager?.Dispose();
		}

		#endregion
	}
}
