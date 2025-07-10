using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryLineFee()
		{
			var parent = Factory.New<CusEntryLineFee>();
			AssertEquals(parent.Lookups.EntryLineFee, parent);
		}

		public void TestChargeTypeList_IncludeRateCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			var bwDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Botswana, parent: parentDataGrouping);
			Factory.Save();
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, bwDataGrouping.ZZZ_DataGrouping, "VAT", "ABC");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, Core.Constants.CountryCodes.Vanuatu, "VU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.LoadOrCreateNewCusRateCode(Factory, "123", ZGuid.Empty, countryCode: Core.Constants.CountryCodes.Vanuatu, cusRateType: "DTY", isSystem: false, description: "Cus Ref Rate Code desc");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Botswana))
			{
				var lineFee = CreateCusEntryLineFee();
				AssertCollectionContains("ChargeTypeList should include code from WTGData for BLNC country", "ABC", lineFee.Lookups.ChargeTypeList.GetAllCodes());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Vanuatu))
			{
				var lineFee = CreateCusEntryLineFee();
				AssertCollectionContains("ChargeTypeList should include code from ownData for non-BLNC country", "123", lineFee.Lookups.ChargeTypeList.GetAllCodes());
			}
		}

		public void TestChargeTypeList_IncludeVATOnly()
		{
			SetupTaxOrFeeData();

			var lineFee = CreateCusEntryLineFee();
			var chargeTypeList = lineFee.Lookups.ChargeTypeList;
			AssertEquals("ChargeTypeList should include only VAT and not every RefCusTaxOrFee", "VAT", chargeTypeList.CodesAsString);
			AssertEquals("VAT description is from GetConsumptionTaxDescription: Botswana", "VAT", chargeTypeList.GetDescriptionFromCode("VAT"));
		}

		public void TestChargeTypeList_IncludeVATOnly_DeclarationCountryDifferentWithCurrentCountry()
		{
			SetupTaxOrFeeData();

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DCG";
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Congo;

			var dec = Factory.New<JobDeclaration>();
			dec.JE_GC = company.PK;
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var lineFee = entryLine.Fees.AddOrUpdate("AAA", 10m);

			var chargeTypeList = lineFee.Lookups.ChargeTypeList;
			AssertEquals("ChargeTypeList should include only VAT and not every RefCusTaxOrFee", "VAT", chargeTypeList.CodesAsString);
			AssertEquals("VAT description is from GetConsumptionTaxDescription: Congo", "TVA", chargeTypeList.GetDescriptionFromCode("VAT"));
		}

		void SetupTaxOrFeeData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			helper.CreateRefCusTaxOrFeeType("VAT", "VAT TEST");
			Factory.Save();
			helper.CreateTaxOrFee("VA1", 0.15m, currentCountry, 0m, 1m, "VAT", description: "VA1");
			helper.CreateTaxOrFee("VA2", 0.15m, currentCountry, 0m, 1m, "VAT", description: "VA2");
			Factory.Save();
		}

		CusEntryLineFee CreateCusEntryLineFee()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var lineFee = entryLine.Fees.AddOrUpdate("AAA", 10m);
			return lineFee;
		}

		public void TestChargeTypeListIsCached()
		{
			var lineFee = CreateCusEntryLineFee();
			AssertSame("Should be cached", lineFee.Lookups.ChargeTypeList, lineFee.Lookups.ChargeTypeList);
		}
	}
}
