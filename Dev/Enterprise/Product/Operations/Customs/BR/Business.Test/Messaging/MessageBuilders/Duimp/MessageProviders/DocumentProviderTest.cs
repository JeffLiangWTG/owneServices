using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class DocumentProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(DocumentProvider.New(null));
			AssertNull(DocumentProvider.New(Factory.New<CusEntryInstruction>()));
			AssertType<DocumentProvider>(DocumentProvider.New(Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew()));
		}

		public void TestInstructionDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var dataProvider = DocumentProvider.New(entryInstruction);
			AssertEquals(0, dataProvider.InstructionDocuments.Count());

			var dispatchNumber1 = declaration.DispatchInstructionNumbers.AddNew();
			dispatchNumber1.CE_EntryType = DispatchInstructionDocumentTypes.Codes._01;
			dispatchNumber1.CE_EntryNum = "FAT00001";

			var dispatchNumber2 = declaration.DispatchInstructionNumbers.AddNew();
			dispatchNumber2.CE_EntryType = DispatchInstructionDocumentTypes.Codes._02;
			dispatchNumber2.CE_EntryNum = "FAT00002";

			var dispatchNumber3 = declaration.DispatchInstructionNumbers.AddNew();
			dispatchNumber3.CE_EntryType = DispatchInstructionDocumentTypes.Codes._28;
			dispatchNumber3.CE_EntryNum = "FAT00003";

			dataProvider = DocumentProvider.New(entryInstruction);
			AssertEquals(2, dataProvider.InstructionDocuments.Count());
		}

		public void TestProcesses()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var dataProvider = DocumentProvider.New(entryInstruction);
			AssertEquals(0, dataProvider.InstructionDocuments.Count());

			var relatedNumber1 = declaration.ProcessRelatedNumbers.AddNew();
			relatedNumber1.CE_EntryType = ProcessRelatedTypeList.Codes.ADM;
			relatedNumber1.CE_EntryNum = "569874522";

			var relatedNumber2 = declaration.ProcessRelatedNumbers.AddNew();
			relatedNumber2.CE_EntryType = ProcessRelatedTypeList.Codes.JUD;
			relatedNumber2.CE_EntryNum = "569874523";

			dataProvider = DocumentProvider.New(entryInstruction);
			AssertEquals(2, dataProvider.Processes.Count());
		}

		public void TestDeclarationsExportForeign()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var dataProvider = DocumentProvider.New(entryInstruction);
			AssertEquals(0, dataProvider.ForeignExportDeclarations.Count());

			var mercosulForeign = entryInstruction.MercosulForeignDeclarations.AddNew();
			mercosulForeign.CSI_Description = "2106199";
			mercosulForeign.CSI_ReferenceNumber = "100";
			mercosulForeign.CSI_ReferenceNumber2 = "999";

			var mercosulForeign2 = entryInstruction.MercosulForeignDeclarations.AddNew();
			mercosulForeign.CSI_Description = "2106199";
			mercosulForeign.CSI_ReferenceNumber = "100";
			mercosulForeign.CSI_ReferenceNumber2 = "999";

			dataProvider = DocumentProvider.New(entryInstruction);
			AssertEquals(2, dataProvider.ForeignExportDeclarations.Count());
		}
	}
}
