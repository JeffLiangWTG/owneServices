using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DeltaGCusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNationalFeeTypeCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", euGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain", euGrouping);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Spain, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, "ZZZZ", true, false, "ZZZZ_Description");
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, UniversalReferenceConstants.RefCusRateCodes.U165, true, false, "U165_Description");
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, UniversalReferenceConstants.RefCusRateCodes.U167, true, false, "U167_Description");
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.France, "DEV", UniversalReferenceConstants.RefCusRateCodes.Q416, true, false, "Q416_Description");

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.France, "HSN");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.France, tariffType.PK, "11111111", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.France, "ZA1", startDate: ZDateTime.BrettsBirthday, endDate: ZDateTime.Today.AddDays(1), category: "N434");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First().MergedLines.Cast<CusEntryLine>().First();
			var entryLineFee = entryLine.Fees.AddNew();

			AssertNotNull(entryLineFee.Lookups.NationalFeeTypeCodeList);

			AssertContainsExactElementsInExactOrder("NationalFeeTypeCodeList should be populated from Ref DB.", new string[] { UniversalReferenceConstants.RefCusRateCodes.Q416, UniversalReferenceConstants.RefCusRateCodes.U165, UniversalReferenceConstants.RefCusRateCodes.U167 }, entryLineFee.Lookups.NationalFeeTypeCodeList.GetAllCodes());
		}
	}
}
