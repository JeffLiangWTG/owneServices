using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	// Test for lookups when under a PIVOT 

	public class EUAddInfoTaxLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxRateDutyList()
		{
			AssertNotNull(tax.Data.Lookups.RateDutyList);
		}

		public virtual void TestTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Env.CurrentCompany.Country.Code;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: eunId);
			helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Duty, "Duty");
			var dut = helper.CreateNewOrGetExistingRateType(countryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Duty, "Duty");
			var exp = helper.CreateNewOrGetExistingRateType(countryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.ExportTaxes, "Export Taxes");
			var moe = helper.CreateNewOrGetExistingRateType(countryCode, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.MiscellaneousOnlyForExport, "Miscellaneous Only for Export");
			helper.LoadOrCreateNewCusRateCode(Factory, "A00", dut.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "350", exp.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "149", moe.PK);
			Factory.Save();

			var classification = Factory.New<CusClassification>();
			pivot.CI_CC = classification.PK;

			CombineAssertions("When CC_ClassificationType = IMP", () =>
			{
				classification.CC_ClassificationType = "IMP";
				var typeList = tax.Data.Lookups.TypeList;
				AssertEquals("Contains A00", true, typeList.ContainsCode("A00"));
				AssertEquals("Contains B00", true, typeList.ContainsCode("B00"));
				AssertEquals("Contains 149", false, typeList.ContainsCode("149"));
				AssertEquals("Contains 350", false, typeList.ContainsCode("350"));
			});

			CombineAssertions("When CC_ClassificationType = EXP", () =>
			{
				classification.CC_ClassificationType = "EXP";
				var typeList = tax.Data.Lookups.TypeList;
				AssertEquals("Contains A00", false, typeList.ContainsCode("A00"));
				AssertEquals("Contains B00", true, typeList.ContainsCode("B00"));
				AssertEquals("Contains 149", true, typeList.ContainsCode("149"));
				AssertEquals("Contains 350", true, typeList.ContainsCode("350"));
			});

			CombineAssertions("When CC_ClassificationType = BTH", () =>
			{
				classification.CC_ClassificationType = "BTH";
				var typeList = tax.Data.Lookups.TypeList;
				AssertEquals("Contains A00", true, typeList.ContainsCode("A00"));
				AssertEquals("Contains B00", true, typeList.ContainsCode("B00"));
				AssertEquals("Contains 149", true, typeList.ContainsCode("149"));
				AssertEquals("Contains 350", true, typeList.ContainsCode("350"));
			});
		}

		public void TestTaxMOPList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");

			var gBImportMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "Immediate payment by cash or equivalent (Paper declarations)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBImportMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBImportMOP.PK, RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty);
			var gBExportMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "L", "CAP Export Licence", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBExportMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
			helper.CreateNewOrGetExistingCusCodeListAttribute(gBExportMOP.PK, RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty);

			var dEMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "D", "Andere (z.B. Abbuchung vom Konto eines Zollagenten)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(dEMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);

			var iTMOP = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "T", "Garanzia sul conto dello spedizioniere doganale", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(iTMOP.PK, RefCusCodeListAttributeTypes.Codes.Category, Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);

			Factory.Save();

			// Can't just toggle a pivot's country after instantiation, so need to make new products
			var deProduct = (MasterFiles.OrgSupplierPart)Factory.New<Integration.Customs.DE.IOrgSupplierPart>();
			var dePivot = deProduct.PivotsForBinding.AddNew();
			dePivot.CI_RN_NKCountry = Core.Constants.CountryCodes.Germany;
			var tax = dePivot.Taxes.AddNew();
			AssertEquals("D", tax.Data.Lookups.MOPList.CodesAsString);

			var itProduct = (MasterFiles.OrgSupplierPart)Factory.New<Integration.Customs.IT.IOrgSupplierPart>();
			var itPivot = itProduct.PivotsForBinding.AddNew();
			itPivot.CI_RN_NKCountry = Core.Constants.CountryCodes.Italy;
			tax = itPivot.Taxes.AddNew();
			AssertEquals("T", tax.Data.Lookups.MOPList.CodesAsString);

			var gbProduct = (MasterFiles.OrgSupplierPart)Factory.New<Integration.Customs.GB.IOrgSupplierPart>();
			var gbPivot = gbProduct.PivotsForBinding.AddNew();
			gbPivot.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			tax = gbPivot.Taxes.AddNew();
			gbPivot.CI_ChildType = JobMessageTypeList.Codes.Import;
			AssertEquals("A", tax.Data.Lookups.MOPList.CodesAsString);
			gbPivot.CI_ChildType = JobMessageTypeList.Codes.Export;
			AssertEquals("L", tax.Data.Lookups.MOPList.CodesAsString);
		}

		public virtual void TestSortTypeList()
		{
			pivot.CI_ChildType = JobMessageTypeList.Codes.Import;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Env.CurrentCompany.Country.Code;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, parent: eunId);
			Factory.Save();

			helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DUT", "Duty");
			var dut = helper.CreateNewOrGetExistingRateType(countryCode, "DUT", "Duty");
			var msc = helper.CreateNewOrGetExistingRateType(countryCode, "MSC", "Miscellaneous");
			helper.LoadOrCreateNewCusRateCode(Factory, "A00", dut.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "411", msc.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "E00", msc.PK);

			Factory.Save();

			var typeList = tax.Data.Lookups.TypeList;
			var indexA00 = typeList.IndexOf(typeList.ToArray().FirstOrDefault(t => t.Code.StartsWith("A00")));
			var indexE00 = typeList.IndexOf(typeList.ToArray().FirstOrDefault(t => t.Code.StartsWith("E00")));
			var index411 = typeList.IndexOf(typeList.ToArray().FirstOrDefault(t => t.Code.StartsWith("411")));
			Assert("A00 and B00 will be used more often than numeric codes, it should look like: A00, A10...B00, B10...411, 412...", indexA00 < indexE00 && indexE00 < index411);
		}
		#region Implementation
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
		#endregion
	}
}
