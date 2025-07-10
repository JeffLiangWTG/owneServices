using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	class EUAddInfoTaxLookupsBaseOnlyTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeB00_GB()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				pivot.CI_ChildType = JobMessageTypeList.Codes.Import;
				var tax = pivot.Taxes.AddNew();
				AssertContains("B00", tax.Data.Lookups.TypeList.CodesAsString);
				AssertContains("VAT", tax.Data.Lookups.TypeList.GetDescriptionFromCode(UniversalReferenceConstants.RefCusRateCodes.Vat));
			}
		}
		public void TestTypeB00_FR()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				pivot.CI_ChildType = JobMessageTypeList.Codes.Import;
				var tax = pivot.Taxes.AddNew();
				AssertContains("B00", tax.Data.Lookups.TypeList.CodesAsString);
				AssertContains("TVA", tax.Data.Lookups.TypeList.GetDescriptionFromCode(UniversalReferenceConstants.RefCusRateCodes.Vat));
			}
		}

		public void TestTypeB00_IT()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				pivot.CI_ChildType = JobMessageTypeList.Codes.Import;
				var tax = pivot.Taxes.AddNew();
				AssertContains("B00", tax.Data.Lookups.TypeList.CodesAsString);
				AssertContains("IVA", tax.Data.Lookups.TypeList.GetDescriptionFromCode(UniversalReferenceConstants.RefCusRateCodes.Vat));

				pivot.CI_ChildType = JobMessageTypeList.Codes.Export;
				AssertContains("B00", tax.Data.Lookups.TypeList.CodesAsString);
				AssertContains("IVA", tax.Data.Lookups.TypeList.GetDescriptionFromCode(UniversalReferenceConstants.RefCusRateCodes.Vat));
			}
		}

		public void TestTypeListContainsOnlyOneVATPair()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Env.CurrentCompany.Country.Code;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: eunId);
			Factory.Save();

			helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "TAX", "Tax");
			var taxFees = helper.CreateNewOrGetExistingRateType(countryCode, "TAX", "Tax");
			helper.LoadOrCreateNewCusRateCode(Factory, "B00", taxFees.PK);

			Factory.Save();
			AssertEquals("B00", tax.Data.Lookups.TypeList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var product = Factory.New<MasterFiles.OrgSupplierPart>();
			product.OP_PartNum = "DANIEL";
			pivot = product.PivotsForBinding.AddNew();
			tax = pivot.Taxes.AddNew();
		}
		CusClassPartPivot pivot;
		CusAddInfo<Tax_CusAddInfoOnlyForPIVOT> tax;
	}
}
