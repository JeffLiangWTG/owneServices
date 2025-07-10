using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBondedWhsUnitQtyList()
		{
			var dec = Factory.New<JobDeclaration>();
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(invLine.Lookups.BondedWhsUnitQtyList, invLine.Lookups.InvoiceUQList);
		}

		public void TestProcedures()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IM4");
			helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP", group: "IM5");
			helper.CreateRefCusProcedure(currentCountry, "B", "33", "33", "333", "Three", "EXP", group: "IM6");
			var dec = Factory.New<JobDeclaration>();
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var cpcs = invLine.Lookups.Procedures;
			AssertEquals("CPC List should have all procedure codes", 3, cpcs.Count);
			AssertEquals("Procedures filter should not be defaulted", 0, ((IFilterBusinessObjectDefaultsProvider)cpcs).FilterBusinessObjectDefaults.Count);
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "IM4";
			invLine.JI_CEI = cei.PK;
			cpcs = invLine.Lookups.Procedures;
			var procedureFilterKey = "Group" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property";
			Assert("Procedures filter should be defaulted", ((IFilterBusinessObjectDefaultsProvider)cpcs).FilterBusinessObjectDefaults.ContainsDefaultFor(procedureFilterKey));
			AssertEquals("Default from CEI_Style", "IM4", ((IFilterBusinessObjectDefaultsProvider)cpcs).FilterBusinessObjectDefaults[procedureFilterKey].Value.ToString());
		}

		public void TestInvoiceLine()
		{
			var parent = Factory.New<JobComInvoiceLine>();
			AssertEquals(parent.Lookups.InvoiceLine, parent);
		}

		public void TestCustomsUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Namibia, "Namibia");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
			var cusCode = helper.CreateCusCodeList(Core.Constants.CountryCodes.Namibia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", "KG DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCode1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Namibia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DT", "DT DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var dec = Factory.New<JobDeclaration>();
				var invoiceHeader = dec.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var list = invoiceLine.Lookups.CustomsUQList;
				AssertEquals(2, list.Count);
				AssertEquals("KG DESC", list.GetDescriptionFromCode(cusCode.ZZD_Code));
				AssertEquals("DT DESC", list.GetDescriptionFromCode(cusCode1.ZZD_Code));
			}
		}

		public void TestTaxOrFeeCodeList()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var today = ZDateTime.Today;
				refDataHelper.CreateTaxOrFee("VZR", 0, Core.Constants.CountryCodes.Namibia, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "VAT Zero Rated");
				refDataHelper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.Namibia, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "VAT Normal");
				refDataHelper.CreateTaxOrFee("VEX", 0, Core.Constants.CountryCodes.Namibia, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "VAT Exempt");
				refDataHelper.CreateTaxOrFee("ZA1", 0, Core.Constants.CountryCodes.Namibia, 0, 0, "VAT", today.AddDays(-3), today.AddDays(-2), "DESC1");
				refDataHelper.CreateTaxOrFee("ZA2", 0, Core.Constants.CountryCodes.Namibia, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "DESC2");
				refDataHelper.CreateTaxOrFee("ZA3", 0, Core.Constants.CountryCodes.Namibia, 0, 0, "OTH", today.AddDays(-1), today.AddDays(1), "DESC3");
				refDataHelper.CreateTaxOrFee("ZA4", 0, Core.Constants.CountryCodes.Namibia, 0, 0, "VAT", today.AddDays(2), today.AddDays(3), "DESC4");
				refDataHelper.CreateTaxOrFee("AU1", 0, Core.Constants.CountryCodes.Italy, 0, 0, "VAT", today.AddDays(-1), today.AddDays(1), "AU1");
				Factory.Save();
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				var list = invoiceLine.Lookups.TaxOrFeeCodeList;
				AssertContainsExactElementsInAnyOrder(new ZString[] { "ZA2 - DESC2", "VAT - VAT Normal", "VEX - VAT Exempt", "VZR - VAT Zero Rated", }, list.ToArray().Select(x => $"{x.Code} - {x.Description}"));
			}
		}

		public void TestTaxOrFeeType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				AssertEquals("TaxOrFeeType should be VAT in Namibia", Core.Constants.Customs.CusEntryFeeTypes.VAT, invoiceLine.Lookups.TaxOrFeeType);
			}
		}
	}
}
