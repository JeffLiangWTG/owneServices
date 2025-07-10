using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(EntryDetailsFor5ACModule))]
	sealed class EntryDetailsFor5ACModuleTest : ZModuleBasherWithFetchHintsTest
	{
		protected override ZFilterModule CreateModuleForFetchHintsTest() => new EntryDetailsFor5ACModule();

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.KR.ExportEntryDetails;

		protected override void SetupDataForFetchHintsTest()
		{
			for (int idx = 1; idx < 20; idx++)
			{
				CreateKREntryHeaderDetailsViewForFetchHintTest(idx);
			}
			Factory.Save();
		}
		void CreateKREntryHeaderDetailsViewForFetchHintTest(int idx)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Category = "BUS";
			orgHeader.OH_Code = "RK" + idx.ToString();
			orgHeader.OH_FullName = "RK Test" + idx.ToString();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_OA_SupplierAddress = orgHeader.MainAddress.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_NoOfPacks = 100 + idx;
			invoice.JZ_Weight = 100 + idx;
			invoice.JZ_WeightUQ = "KG";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "EXP";
			entryNum.CE_EntryNum = "123452212345" + idx.ToString("00");
			entryNum.CE_IssueDate = ZDateTime.Today;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 100 + idx;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var factory = collection.Factory;
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Category = "BUS";
			orgHeader.OH_Code = "RK";
			orgHeader.OH_FullName = "RK Test";

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_OA_SupplierAddress = orgHeader.MainAddress.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_NoOfPacks = 100;
			invoice.JZ_Weight = 100;
			invoice.JZ_WeightUQ = "KG";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "EXP";
			entryNum.CE_EntryNum = "1234522123450X";
			entryNum.CE_IssueDate = ZDateTime.Today;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 100;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			factory.Save();
		}
	}
}
