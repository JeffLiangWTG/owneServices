using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusLineTariffDetailCollection))]
class CusLineTariffDetailCollectionTest : Customs.Business.Testing.CusLineTariffDetailCollectionTest
{
	public void TestConstructor()
	{
		var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
		AssertExceptionThrown<ArgumentException>("Type is null", () => new CusLineTariffDetailCollection(invoiceLine, null));
	}

	public void TestCusLineTariffDetailCollectionShouldBeReadOnly()
	{
		var collection = GetCollectionToTest(RateTypes.AdditionalTaxes);
		AssertEquals(false, collection.AllowNew);
		AssertEquals(false, collection.AllowRemove);
	}

	public void TestCusLineTariffDetailCollectionShouldBeReadWrite()
	{
		var collection = GetCollectionToTest(RateTypes.AdditionalFees);
		AssertEquals(true, collection.AllowNew);
		AssertEquals(true, collection.AllowRemove);
	}

	public void TestSetCollectionRelationships()
	{
		var collection = GetCollectionToTest() as CusLineTariffDetailCollection;
		var cusLineTariffDetail = collection.AddNew();

		AssertEquals(RateTypes.AdditionalTaxes, cusLineTariffDetail.BZ_Type);
	}

	public void TestCreateRelationshipFilter()
	{
		var collection = GetCollectionToTest() as CusLineTariffDetailCollection;
		var invoiceLine = collection.Master as JobComInvoiceLine;
		var cusLineTariffDetail1 = Factory.New<CusLineTariffDetail>();
		cusLineTariffDetail1.BZ_ParentTableCode = invoiceLine.TablePrefix;
		cusLineTariffDetail1.BZ_ParentID = invoiceLine.PK;
		cusLineTariffDetail1.BZ_Type = RateTypes.AdditionalTaxes;
		var cusLineTariffDetail2 = Factory.New<CusLineTariffDetail>();
		cusLineTariffDetail2.BZ_ParentTableCode = invoiceLine.TablePrefix;
		cusLineTariffDetail2.BZ_ParentID = invoiceLine.PK;
		cusLineTariffDetail2.BZ_Type = RateTypes.AdditionalFees;
		collection.Load();

		AssertEquals("Only one CusLineTariffDetail should be loaded", 1, collection.Count);
		AssertEquals("ADT CusLineTariffDetail should be loaded", RateTypes.AdditionalTaxes, collection[0].BZ_Type);
	}

	public void TestRebuildAdditionalTaxes()
	{
		var invoiceLine = new BusinessObjectFactory().New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		invoiceLine.JI_Tariff = "12345678000912";
		invoiceLine.Factory.Save();

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode();
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode("290-002");
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff);

		invoiceLine = Factory.Load<JobComInvoiceLine>(invoiceLine.PK);
		var collection = new CusLineTariffDetailCollection(invoiceLine, RateTypes.AdditionalTaxes);
		collection.RebuildAdditionalTaxes();

		AssertEquals("2 Addiitional Taxes created", 2, collection.Count);
		AssertEquals("280", invoiceLine.AdditionalTaxes[0].BZ_TaxType);
		AssertEquals("Single tariff is preselected", "280-000", invoiceLine.AdditionalTaxes[0].BZ_Tariff);
		AssertEquals("290", invoiceLine.AdditionalTaxes[1].BZ_TaxType);
		AssertEquals("Mutiple tariff found", "", invoiceLine.AdditionalTaxes[1].BZ_Tariff);

		invoiceLine.JI_Tariff = "1111111111";
		collection.RebuildAdditionalTaxes();
		AssertEquals("Existing Addiitional Taxes removed", 0, collection.Count);

		collection = new CusLineTariffDetailCollection(invoiceLine, RateTypes.AdditionalFees);
		collection.RebuildAdditionalTaxes();
		AssertEquals("No Addiitional Taxes created for AdditionalFees", 0, collection.Count);
	}

	public void TestRebuildAdditionalTaxes_ShouldCalculateBaseValue_WhenTaxTypeIs660()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 50m);

		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_LinePrice = 100m;

		var additionalTax = invoiceLine.AdditionalTaxes.AddNew();

		AssertEquals("Precondition", 0m, additionalTax.BZ_BaseValue);

		additionalTax.BZ_TaxType = AdditionalTaxesTypes.MotorVehicle;
		AssertEquals(150m, additionalTax.BZ_BaseValue);
	}

	BusinessObjectCollection GetCollectionToTest(string type)
	{
		var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
		return new CusLineTariffDetailCollection(invoiceLine, type);
	}

	protected override BusinessObjectCollection GetCollectionToTest() => GetCollectionToTest(RateTypes.AdditionalTaxes);
}
