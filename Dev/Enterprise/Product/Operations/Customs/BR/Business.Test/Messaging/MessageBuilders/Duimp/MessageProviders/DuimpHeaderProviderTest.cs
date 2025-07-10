using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class DuimpHeaderProviderTest : TestCaseWithFactory
	{
		public void TestIdentification()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "IMPORTER COMPANY";
			consignee.PrimaryRegistrationNumber.Number = "58.500.398/0001-05";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.DeclarantType = DeclarantTypeList.Codes.DiplomaticMission;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new DuimpMessageSendingObject(entryHeader);
			var dataProvider = new DuimpHeaderProvider(sendingObject);

			AssertNull("Identification should be null", dataProvider.Identification);

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.AdditionalInformation = "TEST_AUTO";
			instruction.AdditionalInformationManual = "TEST_MANUALLY";
			entryHeader.CH_CEI_Instruction = instruction.PK;

			dataProvider = new DuimpHeaderProvider(sendingObject);
			declaration.JE_OH_Importer = consignee.PK;
			declaration.DeclarantType = DeclarantTypeList.Codes.LegalPerson;

			AssertNotNull("Identification should NOT be null", dataProvider.Identification);
		}

		public void TestCargo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.DeclarantType = DeclarantTypeList.Codes.DiplomaticMission;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new DuimpMessageSendingObject(entryHeader);
			var dataProvider = new DuimpHeaderProvider(sendingObject);

			AssertNull(dataProvider.Cargo);

			declaration.JE_UCR = "123456";
			declaration.DeclarantType = DeclarantTypeList.Codes.LegalPerson;
			entryHeader.CH_CEI_Instruction = declaration.CustomsEntryInstructions.AddNew().PK;
			dataProvider = new DuimpHeaderProvider(sendingObject);

			AssertNotNull(dataProvider.Cargo);
		}

		public void TestDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = declaration.CustomsEntryInstructions.AddNew().PK;
			var sendingObject = new DuimpMessageSendingObject(entryHeader);
			var dataProvider = new DuimpHeaderProvider(sendingObject);
			AssertEquals(0, dataProvider.Document.InstructionDocuments.Count());

			var dispatchNumber1 = declaration.DispatchInstructionNumbers.AddNew();
			dispatchNumber1.CE_EntryType = DispatchInstructionDocumentTypes.Codes._01;
			dispatchNumber1.CE_EntryNum = "FAT00001";

			var dispatchNumber2 = declaration.DispatchInstructionNumbers.AddNew();
			dispatchNumber2.CE_EntryType = DispatchInstructionDocumentTypes.Codes._02;
			dispatchNumber2.CE_EntryNum = "FAT00002";

			var dispatchNumber3 = declaration.DispatchInstructionNumbers.AddNew();
			dispatchNumber3.CE_EntryType = DispatchInstructionDocumentTypes.Codes._28;
			dispatchNumber3.CE_EntryNum = "FAT00003";

			dataProvider = new DuimpHeaderProvider(sendingObject);
			AssertEquals(2, dataProvider.Document.InstructionDocuments.Count());
		}
	}
}
