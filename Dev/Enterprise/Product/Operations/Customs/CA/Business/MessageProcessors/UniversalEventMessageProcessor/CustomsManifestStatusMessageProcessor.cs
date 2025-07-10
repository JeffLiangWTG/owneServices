using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using AutoEvents = Enterprise.ZArchitecture.Business.AutoEvents;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class CustomsManifestStatusMessageProcessor : CAUniversalEventMessageProcessor
	{
		public CustomsManifestStatusMessageProcessor(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, BusinessObject businessObject)
			: base(logger, universalEvent, message, businessObject)
		{
		}

		protected override bool ProcessCore()
		{
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			AddSTUEvents();
			AutoSendRNSQueryIfNeeded();
			UpdateD4MessageStatusAndRNSProcessingDate();
			UpdateCustomsStatus();
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		void AddSTUEvents()
		{
			var eventTime = universalEvent?.GetEventTime() ?? ZDateTimeOffset.Now;
			var statusColl = UniversalEventMessageProcessorHelper.GetContextValuesByType(universalEvent?.ContextCollection, UniversalEventMessageProcessorConstants.ContextType.Status.Name);
			if (businessObject != null && statusColl != null)
			{
				foreach (var status in statusColl)
				{
					businessObject.GetLogs().AddNew(AutoEvents.StatusUpdated, ZString.Format(Res.GetString("1498F66D-9D78-49CB-8F53-31D4640FAE2D", "|RES={0}|TYP=D4", status)), eventTime, false);
				}
			}
		}

		protected override ZString GetMessageTypeDescription() => AutoEvents.CustomsManifestStatus.Description;

		protected override ZString GetResponseTypeDescription() => Res.GetString("a6fbabf9-f5d3-4eb1-93f0-27239bbfbf4d", "A status update message");

		protected override ZString GetBusinessObjectType()
		{
			var result = "Declaration";
			if (businessObject is ForwardingShipment)
			{
				result = "Shipment";
			}
			else if (businessObject is CusCAeMHHouse)
			{
				result = "eManifest House Bill";
			}
			else if (businessObject is CusCAeMHMaster)
			{
				result = "eManifest Master Bill";
			}
			else if (businessObject is ForwardingConsol)
			{
				result = "Forwarding Consolidation";
			}
			return result;
		}

		protected override ZString GetMessageInterpretation() => new D4MessageInterpretationGenerator(factory, universalEvent).GetInterpretatedHTML();

		protected override ZBool ShouldUpdateMessageStatus(CusEntryHeader entryHeader) => ZBool.False;

		protected override ZGuid NotifyEmailGroup
			=> (businessObject is CusCAeMHMaster || businessObject is CusCAeMHHouse) ?
				GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendeManifestForwarderMessageAcknowledgementsToGroupAppliesAllCountries) :
				GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup);

		protected override ZString NotifyEmailMode
			=> (businessObject is CusCAeMHMaster || businessObject is CusCAeMHHouse) ?
				GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendeManifestForwarderMessageAcknowledgementsAppliesAllCountries) :
				GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgements);

		#region AutoSendRNSQuery

		void AutoSendRNSQueryIfNeeded()
		{
			var declaration = businessObject as JobDeclaration;
			relEntryHeader = declaration?.ReleaseEntryHeader;
			if (relEntryHeader != null && relEntryHeader.CH_Status == MessageStatusList.Codes.AwaitingOriginal)
			{
				var lastSendMessage = GetLastSendMessage(relEntryHeader) as RNSRequestMessage;
				if (lastSendMessage == null || lastSendMessage.EM_MessageSubType != RNSMessageTypes.Codes.StatusQuery)
				{
					factory.Saved -= AutoSendRNSQueryOnFactorySaved;
					factory.Saved += AutoSendRNSQueryOnFactorySaved;
				}
			}
		}

		CusEntryHeader relEntryHeader;

		void AutoSendRNSQueryOnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= AutoSendRNSQueryOnFactorySaved;

				ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
				{
					var newFactory = new BusinessObjectFactory
					{
						NameForDebugging = "CustomsManifestStatusMessageProcessorFactory"
					};

					var query = new ZQuery(CusEntryHeaderSchema.PK, relEntryHeader.PK);
					query.AddToFilter(CusEntryHeaderSchema.CH_Status, MessageStatusList.Codes.AwaitingOriginal);
					var relEntryHeaderInDB = newFactory.LoadTop1<CusEntryHeader>(query);
					if (relEntryHeaderInDB != null && !new RNSAutoSender(relEntryHeaderInDB, new UserNotificationWrapper(logger)).Process())
					{
						logger.Log(Integration.LogType.Information, "Error during sending RNS Query.");
					}
				}, () => { logger.Log(Integration.LogType.Warning, "Error during sending RNS Query. Retrying."); });
			}
		}

		#endregion

		#region UpdateD4MessageStatusAndRNSProcessingDate

		void UpdateD4MessageStatusAndRNSProcessingDate()
		{
			if (businessObject is CusCAeMHMaster master)
			{
				var rnsProcessingDate = universalEvent.GetEventTime().ToZDateTime();

				if (master.BP_RNSProcessingDate.IsEmpty || master.BP_RNSProcessingDate <= rnsProcessingDate)
				{
					master.BP_RNSProcessingDate = rnsProcessingDate;
					master.BP_D4MessageStatus = UniversalEventMessageProcessorHelper.GetContextValueByType(universalEvent.ContextCollection, UniversalEventMessageProcessorConstants.ContextType.Status.Name);
				}
			}
			else if (businessObject is CusCAeMHHouse house)
			{
				var rnsProcessingDate = universalEvent.GetEventTime().ToZDateTime();
				if (house.BW_RNSProcessingDate.IsEmpty || house.BW_RNSProcessingDate <= rnsProcessingDate)
				{
					house.BW_RNSProcessingDate = rnsProcessingDate;
					house.BW_D4MessageStatus = UniversalEventMessageProcessorHelper.GetContextValueByType(universalEvent.ContextCollection, UniversalEventMessageProcessorConstants.ContextType.Status.Name);
				}
			}
		}

		void UpdateCustomsStatus()
		{
			if (businessObject is IEDIFACTMessageAttachee messageAttachee)
			{
				if (messageAttachee is CusCAeMHMaster || messageAttachee is CusCAeMHHouse)
				{
					var house = messageAttachee as CusCAeMHHouse;
					var master = house != null ? house.MasterBill : messageAttachee as CusCAeMHMaster;
					var isAllHouseBillsAcceptedBefore = master?.ReadyToClose ?? ZBool.False;
					var oldStatus = messageAttachee.JobStatus;

					var messageTypeDescription = messageAttachee is CusCAeMHMaster ? MessageTypeList.Descriptions.ACIForwarderClose : MessageTypeList.Descriptions.ACIHouseBill;
					var statusCalculator = new ACIEManifestForwaderStatusCalculator(messageTypeDescription);
					var newStatus = statusCalculator.CalculateJobStatus(new[] { message }, messageAttachee);
					if (!newStatus.IsEmpty)
					{
						messageAttachee.JobStatus = newStatus;
					}

					var log = new LoggingInformation();
					var processor = new EManifestResponse(log);
					if (processor.ShouldGenerateAutoCloseReport(master, messageAttachee, isAllHouseBillsAcceptedBefore, oldStatus))
					{
						var (isError, logText) = processor.GenerateAutoCloseReport(master);

						if (isError)
						{
							logger.Log(Integration.LogType.Error, logText);
						}
						else
						{
							logger.Log(Integration.LogType.Information, logText);
						}
					}
				}
			}
		}

		#endregion
	}
}
