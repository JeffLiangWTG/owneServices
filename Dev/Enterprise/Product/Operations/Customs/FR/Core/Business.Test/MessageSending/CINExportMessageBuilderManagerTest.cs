using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class CINExportMessageBuilderManagerTest : TestCaseWithFactory
	{
		public void TestGetCIN755Message()
		{
			var sendingObject = CreateMessageObject();
			sendingObject.MessageType = EntryActionCodeList.Codes.CIN755;
			var errorCollector = new EU.Business.ErrorCollector();

			var messageSender = new CINExportMessageSenderForTest(sendingObject, errorCollector);
			var message = messageSender.GetEDIMessageText();

			AssertContains("<schemaID>755</schemaID>", message);
			AssertContains("<schemaVersion>EDIFACT</schemaVersion>", message);
			AssertContains("<MRN_ECS>FR11111111111</MRN_ECS>", message);
		}

		public void TestGetCIN745Message()
		{
			var sendingObject = CreateMessageObject();
			sendingObject.MessageType = EntryActionCodeList.Codes.CIN745;
			var errorCollector = new EU.Business.ErrorCollector();

			var messageSender = new CINExportMessageSenderForTest(sendingObject, errorCollector);
			var message = messageSender.GetEDIMessageText();

			AssertContains("<schemaID>745</schemaID>", message);
			AssertContains("<schemaVersion>XML</schemaVersion>", message);
		}

		public void TestBuilderType()
		{
			var manager = new CINExportMessageBuilderManager(new EU.Business.ErrorCollector());
			AssertEquals("Builder type should be equal CIN", "CIN", manager.BuilderType);

			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var manager745 = new CINExportMessageBuilderManager(new EU.Business.ErrorCollector());
			var messageObj745 = new DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObj745.MessageType = EntryActionCodeList.Codes.CIN745;
			manager745.NewMessageBuilder(messageObj745);
			AssertEquals("Builder type should be equal CIN after NewMessageBuilder is called.", "CIN", manager745.BuilderType);

			var manager755 = new CINExportMessageBuilderManager(new EU.Business.ErrorCollector());
			var messageObj755 = new DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObj755.MessageType = EntryActionCodeList.Codes.CIN755;
			manager755.NewMessageBuilder(messageObj755);
			AssertEquals("Builder type should be equal CIN after NewMessageBuilder is called.", "CIN", manager755.BuilderType);
		}

		DeltaGJobDeclarationMessageSendingObject CreateMessageObject()
		{
			var cTO = Factory.New<OrgHeader>();
			cTO.FillWithValidTestData();
			var addressCTO = cTO.Addresses.AddNew();

			var customCodeCTO = cTO.CustomsCodes.AddNew();
			customCodeCTO.OK_CodeType = CusEntryHeader.Schema.CINOACICode;
			customCodeCTO.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customCodeCTO.OK_CustomsRegNo = "FR123456800";
			customCodeCTO.OK_OA_PremisesAddress = addressCTO.PK;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.MovementReferenceNumberSetter("FR11111111111");
			declaration.JE_MasterBill = "12345";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = addressCTO.PK;

			var messageObject = new DeltaGJobDeclarationMessageSendingObject(declaration.CustomsEntryHeaders[0]);
			messageObject.SequenceNumber = 888;

			return messageObject;
		}
	}
}
