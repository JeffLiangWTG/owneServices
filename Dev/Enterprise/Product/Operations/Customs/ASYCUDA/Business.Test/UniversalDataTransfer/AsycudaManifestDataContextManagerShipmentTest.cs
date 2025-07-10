using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderDataContextManager))]
	sealed class AsycudaManifestDataContextManagerShipmentTest : ShipmentDataContextManagerTestCase<AsycudaManifestHeaderDataContextManager, AsycudaManifestHeader>
	{
		public void TestCanImportEventViaUniversalDataBuss()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "SGI");
			var sg9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "9", "9 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.FalseString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "SGI");
			Factory.SaveForTesting();

			var accessEnable = (RegistryItemWrapper)ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable;
			using (accessEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
				header.AMA_JobReference = "MANABC1234";
				header.AMA_ManifestType = "MGE";
				header.AMA_MasterBill = "MB1708101330";
				var bill = header.Bills.AddNew();
				bill.ABL_BillNumber = "HB1708101330";
				var pack = bill.Packs.AddNew();
				pack.ConsignmentReference = 123;
				var packedItem = pack.PackedItemForTesting();

				var senderStaff = Factory.New<GlbStaff>();
				senderStaff.GS_Code = "NS2";
				senderStaff.GS_LoginName = "NS2";
				senderStaff.GS_EmailAddress = "ns2@cargowise.com";

				Factory.SaveForTesting();

				var message = (Messaging.Business.EDIMessage)GetQueuedUniversalEventMessage(string.Format(EventXmlText, AsycudaEventMessageConstants.ActionPurpose.ERR, AsycudaEventMessageConstants.EventTypes.MRR, header.AMA_JobReference));
				message.EM_SystemCreateUser = senderStaff.GS_Code;
				Env.OutgoingMailManager.EmailsCreated.Clear();
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);
				Factory.SaveForTesting();
				var factory = new BusinessObjectFactory();
				CombineAssertions(delegate
				{
					message = factory.Load<Messaging.Business.EDIMessage>(message.PK);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					AssertMultilineASCIIEquals("Service Task Log", @"
	Linked Event to SG Manifest MANABC1234.
	".Trim(), serviceTaskLog.ToString());

					AssertMultilineASCIIEquals("Message Log Note", @"
	Linked Event to SG Manifest MANABC1234.
	".Trim(), message.GetLogNoteText());

					header = factory.Load<AsycudaManifestHeader>(header.PK);
					AssertEquals("header.AMA_MessageStatus", MessageStatusCodeList.Codes.Error, header.AMA_MessageStatus);
					bill = factory.Load<AsycudaBill>(bill.PK);
					AssertEquals("bill.ABL_BillStatus", "CLR", bill.ABL_BillStatus);
					AssertEquals("bill.ABL_MessageStatus", MessageStatusCodeList.Codes.Error, bill.ABL_MessageStatus);
					packedItem = factory.Load<AsycudaPackedItem>(packedItem.PK);
					AssertEquals("packedItem.API_PackStatus", "REJ", packedItem.API_PackStatus);
					AssertEquals("packedItem.API_MessageStatus", MessageStatusCodeList.Codes.Error, packedItem.API_MessageStatus);

					header = factory.Load<AsycudaManifestHeader>(header.PK);
					var logs = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageReceivedCode));
					AssertEquals("[MRR] - Message Received event count", 1, logs.Length);
					var log = logs[0];

					var contextItems = log.SourceInfoItems;
					var actualContextItems = string.Join("\r\n", contextItems.Cast<KeyDataPair>().Select((item) => item.Key + " - " + item.Data).ToArray());
					AssertEquals("Context Items on Event", @"
Master Bill - MB1708101330
Data Source Action Purpose - ERR
Data Source Data Provider - SGA
".Trim(), actualContextItems);
					var notes = message.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.MessageInterpretation.Code);
					AssertEquals(1, notes.Length);

					var htmlData = message.EM_MessageInterpretation;
					AssertXMLEquals("ST_NoteText", htmlData, notes[0].ST_NoteText);
					var email = Env.OutgoingMailManager.EmailsCreated.Find(obj => obj.Subject == "Global Manifest Universal Event Response Received: Failed");
					AssertEquals("Should have 1 recipient", 1, email.Recipients.Count);
					AssertEquals("Should have a recipient ns2@cargowise.com", "ns2@cargowise.com", email.Recipients[0].Email);
					AssertXMLContains(htmlData, email.Body);
				});
			}
		}

		public void TestMessageInterpretationIsCreated()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "ManifestMessageStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "8", "8 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.TrueString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg8.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "SGI");
			var sg9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestMessageStatus, "9", "9 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MessageCleared, bool.FalseString);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg9.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ManifestType, "SGI");

			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_JobReference = "MANABC1234";
			header.AMA_ManifestType = "MGE";
			header.AMA_MasterBill = "MB1708101330";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "HB1708101330";
			var pack = bill.Packs.AddNew();
			pack.ConsignmentReference = 123;
			var packedItem = pack.PackedItemForTesting();

			Factory.SaveForTesting();

			var message = (Messaging.Business.EDIMessage)GetQueuedUniversalEventMessage(string.Format(EventXmlText, "REJ", "MRJ", header.AMA_JobReference));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();
			var factory = new BusinessObjectFactory();
			CombineAssertions(delegate
			{
				header = factory.Load<AsycudaManifestHeader>(header.PK);
				var messageType = header.MessagingProvider.GetAsycudaEDIMessageType();
				message = factory.Load(messageType, message.PK) as AsycudaEDIMessage;
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				var notes = message.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.MessageInterpretation.Code);
				AssertEquals(1, notes.Length);

				var htmlData = message.EM_MessageInterpretation;
				AssertContains(@"<span class=""text"">MANABC1234</span>", htmlData);
			});
		}

		const string EventXmlText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
  <Event>
    <DataContext>
      <Workflow>
        <ActionPurpose>{0}</ActionPurpose>
      </Workflow>
      <DataSource>
        <DataProvider>SGA</DataProvider>
      </DataSource>
      <DataTargetCollection>
        <DataTarget>
          <Key>{2}</Key>
          <Type>AsycudaManifest</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2017-12-04T04:37:00.0000000Z</EventTime>
    <EventType>{1}</EventType>
    <EventReference>MST=MGE</EventReference>

    <ContextCollection>
      <Context>
        <Type>MasterBill</Type>
        <Value>MB1708101330</Value>
        <SubContextCollection>
          <SubContext>
            <Type>HouseBill</Type>
            <Value>HB1708101330</Value>
            <SubContextCollection>
              <SubContext>
                <Type>ConsignmentReference</Type>
                <Value>123</Value>
                <SubContextCollection>
                  <SubContext>
                    <Type>ErrorCode</Type>
                    <Value>E3</Value>
                  </SubContext>
                  <SubContext>
                    <Type>ConsignmentStatus</Type>
                    <Value>REJ</Value>
                  </SubContext>
                </SubContextCollection>
              </SubContext>
              <SubContext>
                <Type>MessageStatusCode</Type>
                <Value>8</Value>
              </SubContext>
              <SubContext>
                <Type>ConsignmentStatus</Type>
                <Value>CLR</Value>
              </SubContext>
            </SubContextCollection>
          </SubContext>
          <SubContext>
            <Type>MessageStatusCode</Type>
            <Value>9</Value>
          </SubContext>
          <SubContext>
            <Type>ConsignmentStatus</Type>
            <Value>FAL</Value>
          </SubContext>
        </SubContextCollection>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => AsycudaManifestUniversalMessagingHelperTest.SampleUxml;

		protected override bool ManagerChecksDataTargetToImport => true;

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			setupCreator?.Dispose();
		}
	}
}
