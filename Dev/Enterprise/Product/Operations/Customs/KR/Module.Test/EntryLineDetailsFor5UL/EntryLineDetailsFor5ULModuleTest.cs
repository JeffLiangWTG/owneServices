using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(EntryLineDetailsFor5ULModule))]
	sealed class EntryLineDetailsFor5ULModuleTest : ZModuleBasherWithFetchHintsTest
	{
		protected override ZFilterModule CreateModuleForFetchHintsTest() => new EntryLineDetailsFor5ULModule();

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.KR.EntryLineDetailsFor5UL;

		protected override void SetupDataForFetchHintsTest()
		{
			for (int idx = 1; idx < 20; idx++)
			{
				CreateKREntryHeaderDetailsViewForFetchHintTest(Factory, idx);
			}
			Factory.Save();
		}
		void CreateKREntryHeaderDetailsViewForFetchHintTest(BusinessObjectFactory factory, int idx)
		{
			var orgHeaderPayer = factory.New<OrgHeader>();
			orgHeaderPayer.OH_Category = "BUS";
			orgHeaderPayer.OH_Code = "RK Payer" + idx.ToString();
			orgHeaderPayer.OH_FullName = "RK Payer Test" + idx.ToString();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_OH_DutyPayer = orgHeaderPayer.PK;

			var invoice = declaration.Invoices.AddNew();

			var entry = declaration.ActiveEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "123452212345" + idx.ToString("00");
			entryNum.CE_IssueDate = ZDateTime.Today;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = (ZShort)idx;
			entryLine.CL_AdValoremTariff = "01023990" + idx.ToString("00");
			entryLine.CL_CustomsValue = 1000 + idx;
			entryLine.CL_Description = "Description" + idx.ToString("00");
			entryLine.CL_ValueForVAT = 100 + idx;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var refundDeclaration = factory.New<CusReconDeclaration>();
			var refundEntry = refundDeclaration.CusReconEntries.AddNew();
			refundEntry.CRE_EntryType = "AA";
			refundEntry.CRE_CH_OriginalEntry = entry.PK;
			refundEntry.CRE_OA_DeclarantAddress = factory.NewWithValidTestData<OrgAddress>().PK;
			refundEntry.CRE_OriginalEntryNumber = entryNum.CE_EntryNum;
			var refundEntryLine = refundEntry.CusReconEntryLines.AddNew();
			refundEntryLine.CRL_OriginalEntryLineNumber = entryLine.CL_LineNumber;

			var entryNum2 = factory.New<CusEntryNumber>();
			entryNum2.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum2.CE_EntryNum = "123452212345" + idx.ToString("00");
			entryNum2.CE_IssueDate = ZDateTime.Today;
			entryNum2.CE_ParentID = refundDeclaration.PK;
			entryNum2.CE_ParentTable = CusReconDeclaration.Schema.TableName;
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var factory = collection.Factory;
			CreateKREntryHeaderDetailsViewForFetchHintTest(factory, 1);
			factory.Save();
		}
	}
}
