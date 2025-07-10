using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using AsycudaManifestHeader = Enterprise.Customs.GB.GVMS.AsycudaManifestHeader;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	[TestedType(typeof(GBGVMSAsycudaManifestHeaderDataContextManager))]
	class GvmsAsycudaManifestDataContextManagerShipmentTest : ShipmentDataContextManagerTestCase<GBGVMSAsycudaManifestHeaderDataContextManager, AsycudaManifestHeader>
	{
		public void TestCanImportEventViaUniversalDataBuss()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var gvm1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "1", "1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gvm1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gvm1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "GVM");
			var gvm2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "2", "2 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gvm2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.FalseString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gvm2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "GVM");
			Factory.SaveForTesting();

			var accessEnable = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.GVMS, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.GVMS, CountryCodes.UnitedKingdom, ZDate.Today, true))
			{
				var header = Factory.BOFactory.New<AsycudaManifestHeader>();
				header.AMA_JobReference = "GVMSMAN123";
				header.AMA_ManifestType = "GVM";
				header.AMA_MasterBill = "MAN123";
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;

				Factory.SaveForTesting();

				var outgoingInterchange = Factory.New<EDIInterchange>();
				var eHubTrackingIdGuid = ZGuid.NewZGuid();
				outgoingInterchange.EI_SessionGUID = eHubTrackingIdGuid;
				outgoingInterchange.EI_HeaderText = "";
				var outgoingSentMessage = outgoingInterchange.ContainedMessages.AddNew(typeof(EDIMessage));
				header.Messages.Add(outgoingSentMessage);
				outgoingSentMessage.EM_MessageText = "";
				outgoingSentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsGVMSManifest;
				outgoingSentMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory.BOFactory, EDIMessage.ApplicationCodes.GbCustomsGVMSManifest);
				outgoingSentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingSentMessage.EM_MessageNum = "999";
				outgoingSentMessage.EM_LinkedObject = header;
				outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
				outgoingSentMessage.EM_Status = EDIMessage.Status.Sent;
				outgoingInterchange.EI_From = "Sender";
				outgoingInterchange.EI_To = "WISETECHGLOBAL";
				outgoingInterchange.EI_BodyText = "";

				var message = (EDIMessage)GetQueuedUniversalEventMessage(string.Format(eventXmlText, header.AMA_JobReference, AsycudaEventMessageConstants.EventTypes.MRR, eHubTrackingIdGuid));
				Env.OutgoingMailManager.EmailsCreated.Clear();
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				Factory.SaveForTesting();
				var factory = new BusinessObjectFactory();
				CombineAssertions(delegate
				{
					message = factory.Load<EDIMessage>(message.PK);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
	Linked Event to GVMS Manifest GVMSMAN123.
	".Trim(), serviceTaskLog.ToString());

					AssertMultilineASCIIEquals("Message Log Note", @"
	Linked Event to GVMS Manifest GVMSMAN123.
	".Trim(), message.GetLogNoteText());

					header = factory.Load<AsycudaManifestHeader>(header.PK);
					AssertEquals("header.AMA_MessageStatus", MessageStatusCodeList.Codes.Sent, header.AMA_MessageStatus);

					header = factory.Load<AsycudaManifestHeader>(header.PK);
					AssertEquals("originalMessage.EM_Status", EDIMessage.Status.Acknowledged, outgoingSentMessage.EM_Status);

					header = factory.Load<AsycudaManifestHeader>(header.PK);
					var logs = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageReceivedCode));
					AssertEquals("[MRR] - Message Received event count", 1, logs.Length);
					var log = logs[0];

					var contextItems = log.SourceInfoItems;
					var actualContextItems = contextItems.Cast<KeyDataPair>().Select((item) => item.Data).ToArray();
					AssertEquals("Context: Notification Box Id", "123456", actualContextItems[0]);
					AssertEquals("Context: Notification Message Id", "654321", actualContextItems[1]);
					AssertEquals("Context: eHub Tracking ID", eHubTrackingIdGuid.ToString(), actualContextItems[2]);
				});
			}
		}

		const string eventXmlText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataSource>
            <DataProvider>GVMS</DataProvider>
          </DataSource>
        </DataContext>
        <EventTime>2020-05-19T15:53:59</EventTime>
        <EventType>{1}</EventType>
		<EventReference>1</EventReference>
        <ContextCollection>
          <Context>
            <Type>NotificationBoxId</Type>
            <Value>123456</Value>
          </Context>
          <Context>
            <Type>NotificationMessageId</Type>
            <Value>654321</Value>
          </Context>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{2}</Value>
          </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";

		public void TestCanImportEventWithDataContextViaUniversalDataBuss()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var gvm1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "1", "1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gvm1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gvm1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "GVM");
			var gvm2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "2", "2 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gvm2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.FalseString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gvm2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "GVM");
			Factory.SaveForTesting();

			var accessEnable = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.GVMS, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.GVMS, CountryCodes.UnitedKingdom, ZDate.Today, true))
			{
				var header = Factory.BOFactory.New<AsycudaManifestHeader>();
				header.AMA_JobReference = "GVMSMAN123";
				header.AMA_ManifestType = "GVM";
				header.AMA_MasterBill = "MAN123";
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;

				Factory.SaveForTesting();

				var message = (EDIMessage)GetQueuedUniversalEventMessage(string.Format(eventWithDataContextXmlText, header.AMA_JobReference, AsycudaEventMessageConstants.EventTypes.MRR, "Dummy"));
				Env.OutgoingMailManager.EmailsCreated.Clear();
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				Factory.SaveForTesting();
				var factory = new BusinessObjectFactory();
				CombineAssertions(delegate
				{
					message = factory.Load<EDIMessage>(message.PK);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
	Linked Event to GVMS Manifest GVMSMAN123.
	".Trim(), serviceTaskLog.ToString());

					AssertMultilineASCIIEquals("Message Log Note", @"
	Linked Event to GVMS Manifest GVMSMAN123.
	".Trim(), message.GetLogNoteText());
				});
			}
		}

		const string eventWithDataContextXmlText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>{0}</Key>
              <Type>AsycudaManifest</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2020-05-19T15:53:59</EventTime>
        <EventType>{1}</EventType>
		<EventReference>1</EventReference>
        <ContextCollection>
          <Context>
            <Type>NotificationBoxId</Type>
            <Value>123456</Value>
          </Context>
          <Context>
            <Type>NotificationMessageId</Type>
            <Value>654321</Value>
          </Context>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{2}</Value>
          </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => ASYCUDA.Business.UniversalDataTransfer.Testing.AsycudaManifestUniversalMessagingHelperTest.SampleUxml;

		protected override bool ManagerChecksDataTargetToImport => true;

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			enableGVMS = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.GVMS, CountryCodes.UnitedKingdom, ZDate.Today, true);
			base.SetUp();
		}
		IDisposable setupCreator;
		IDisposable enableGVMS;

		protected override void TearDown()
		{
			base.TearDown();
			setupCreator?.Dispose();
			enableGVMS?.Dispose();
		}
	}
}
