using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(EntryDetailsFor5GWModule))]
	sealed class EntryDetailsFor5GWModuleTest : ZModuleBasherWithFetchHintsTest
	{
		protected override ZFilterModule CreateModuleForFetchHintsTest() => new EntryDetailsFor5GWModule();

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.KR.ImportEntryDetails;

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

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0301929090", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, description: "Test Tariff Description");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_OH_DutyPayer = orgHeader.PK;
			declaration.JE_LocationOtherInformation = "111111" + idx.ToString("00");
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_PackQty = 100 + idx;
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillNum = "1112345678" + idx;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_Weight = 100 + idx;
			invoice.JZ_WeightUQ = "KG";
			invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice.JZ_ImportCargoManagementNumber = "123451234512345" + idx.ToString("0000");

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "123452212345" + idx.ToString("00");
			entryNum.CE_IssueDate = ZDateTime.Today;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 100 + idx;
			entryLine.CL_AdValoremTariff = tariff.ZZ1_TariffCode;

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

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0301929090", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, description: "Test Tariff Description");

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_OH_DutyPayer = orgHeader.PK;
			declaration.JE_LocationOtherInformation = "11111111";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_PackQty = 100;
			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillNum = "11123456789";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_Weight = 100;
			invoice.JZ_WeightUQ = "KG";
			invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
			invoice.JZ_ImportCargoManagementNumber = "1234512345123450000";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "IMP";
			entryNum.CE_EntryNum = "1234522123450X";
			entryNum.CE_IssueDate = ZDateTime.Today;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 100;
			entryLine.CL_AdValoremTariff = tariff.ZZ1_TariffCode;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			factory.Save();
		}
	}
}
