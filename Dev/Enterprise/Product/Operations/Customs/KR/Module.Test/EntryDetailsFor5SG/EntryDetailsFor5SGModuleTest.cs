using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(EntryDetailsFor5SGModule))]
	sealed class EntryDetailsFor5SGModuleTest : ZModuleBasherWithFetchHintsTest
	{
		protected override ZFilterModule CreateModuleForFetchHintsTest() => new EntryDetailsFor5SGModule();

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.KR.EntryDetailsFor5SG;

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

			var orgHeaderImporter = factory.New<OrgHeader>();
			orgHeaderImporter.OH_Category = "BUS";
			orgHeaderImporter.OH_Code = "RK Import" + idx.ToString();
			orgHeaderImporter.OH_FullName = "RK Importer Test" + idx.ToString();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_OH_DutyPayer = orgHeaderPayer.PK;
			declaration.JE_OH_Importer = orgHeaderImporter.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ProvAdditionalRate = 98;
			invoice.JZ_ProvAdditionalAmount = 180;
			invoice.JZ_ImpContractExpiryDate = ZDateTime.Today;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_TotalPaid = 200;
			entry.CH_EntryReleaseDate = ZDateTime.Today;
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "123452212345" + idx.ToString("00");
			entryNum.CE_IssueDate = ZDateTime.Today;

			entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "934";
			entryNum.CE_ExpiryDate = ZDateTime.Today;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 100 + idx;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var factory = collection.Factory;
			CreateKREntryHeaderDetailsViewForFetchHintTest(factory, 0);
			factory.Save();
		}
	}
}
