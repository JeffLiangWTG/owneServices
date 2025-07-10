using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInvoiceLine()
		{
			var parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Lookups.InvoiceLine, parent);
		}

		public void TestCustomsUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "IL1", "IL First Code", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "IL2", ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(3));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "IL3", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "US1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var customsUQList = invoiceLine.Lookups.CustomsUQList;
			Assert("valid code", customsUQList.ContainsCode("IL1"));
			Assert("valid code", customsUQList.ContainsCode("IL2"));
			Assert("should not load expired codes", !customsUQList.ContainsCode("IL3"));
			Assert("should not load codes from other country.", !customsUQList.ContainsCode("US1"));
			AssertEquals("IL First Code", customsUQList.GetDescriptionFromCode("IL1"));
		}

		public void TestInvoiceUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "IL Package Type List");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "IL1", "IL First Code", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "IL2", ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(3));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "IL3", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILManifestPackageTypes, "IL4", ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "US1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var invoiceUQList = invoiceLine.Lookups.InvoiceUQList;
			Assert("valid code", invoiceUQList.ContainsCode("IL1"));
			Assert("valid code", invoiceUQList.ContainsCode("IL2"));
			Assert("should not load expired codes", !invoiceUQList.ContainsCode("IL3"));
			Assert("should not load ILManifestPackageTypes codes", !invoiceUQList.ContainsCode("IL4"));
			Assert("should not load codes from other country.", !invoiceUQList.ContainsCode("US1"));
			AssertEquals("IL First Code", invoiceUQList.GetDescriptionFromCode("IL1"));
		}

		public void TestBondedWhsUnitQtyList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "IL Package Type List");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "IL1", "IL First Code", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "IL2", ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(3));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "IL3", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(-1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "US1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var bondedWhsUnitQtyList = invoiceLine.Lookups.BondedWhsUnitQtyList;
			Assert("valid code", bondedWhsUnitQtyList.ContainsCode("IL1"));
			Assert("valid code", bondedWhsUnitQtyList.ContainsCode("IL2"));
			Assert("should not load expired codes", !bondedWhsUnitQtyList.ContainsCode("IL3"));
			Assert("should not load codes from other country.", !bondedWhsUnitQtyList.ContainsCode("US1"));
			AssertEquals("IL First Code", bondedWhsUnitQtyList.GetDescriptionFromCode("IL1"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
		}
		JobComInvoiceLine invoiceLine;
	}
}
