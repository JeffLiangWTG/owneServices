using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

class AdditionalTaxSelectionCriteriaTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("tariffDetail is null", () => new AdditionalTaxSelectionCriteria(null));
	}

	[TestDate]
	public void TestEffectiveDate()
	{
		AssertEquals(ZDateTime.Today, selectionCriteria.EffectiveDate);

		var tomorrow = ZDate.Today.AddDays(1);
		jobDeclaration.JE_ValuationDate = tomorrow;
		AssertEquals(tomorrow, selectionCriteria.EffectiveDate);
	}

	public void TestTradeGroupCountry()
	{
		AssertEquals(string.Empty, selectionCriteria.TradeGroupCountry);

		var switzerland = Core.Constants.CountryCodes.Switzerland;
		invoiceLine.JI_CountryOfOrigin = switzerland;
		var cusLineTariffDetail2 = invoiceLine.AdditionalTaxes.AddNew();
		var selectionCriteria2 = new AdditionalTaxSelectionCriteria(cusLineTariffDetail2);
		AssertEquals(switzerland, selectionCriteria2.TradeGroupCountry);
	}

	public void TestSecondTradeGroups()
	{
		AssertEquals(0, selectionCriteria.SecondTradeGroups.Count);
	}

	public void TestDataGrouping()
	{
		AssertEquals(Core.Constants.CountryCodes.Switzerland, selectionCriteria.DataGrouping);

		var germany = Core.Constants.CountryCodes.Germany;
		var country = Factory.New<RefCountry>();
		country.Code = germany;
		var company = Factory.New<GlbCompany>();
		company.GC_RN_NKCountryCode = country.Code;
		jobDeclaration.JE_GC = company.PK;
		AssertEquals(germany, selectionCriteria.DataGrouping);
	}

	public void TestPrimaryPreference()
	{
		AssertEquals(string.Empty, selectionCriteria.PrimaryPreference);

		var primaryPreference = "123";
		invoiceLine.JI_PrimaryPreference = primaryPreference;
		AssertEquals(primaryPreference, selectionCriteria.PrimaryPreference);
	}

	public void TestAdditionalCodes()
	{
		AssertEquals(string.Empty, selectionCriteria.AdditionalCodes.Single());

		invoiceLine.JI_Tariff = "12345678000123";
		var cusLineTariffDetail2 = invoiceLine.AdditionalTaxes.AddNew();
		var selectionCriteria2 = new AdditionalTaxSelectionCriteria(cusLineTariffDetail2);
		AssertEquals("12345678123", selectionCriteria2.AdditionalCodes.Single());
	}

	public void TestConcessionOrder()
	{
		AssertEquals(string.Empty, selectionCriteria.ConcessionOrder);

		var concessionOrder = "123";
		invoiceLine.JI_ConcessionOrder = concessionOrder;
		AssertEquals(concessionOrder, selectionCriteria.ConcessionOrder);
	}

	public void TestRateType()
	{
		AssertEquals(UniversalReferenceConstants.RateTypes.AdditionalTaxes, selectionCriteria.RateType);
	}

	public void TestRateCode()
	{
		AssertEquals(string.Empty, selectionCriteria.RateCode);

		var taxType = "123";
		cusLineTariffDetail.BZ_TaxType = taxType;
		AssertEquals(taxType, selectionCriteria.RateCode);
	}

	protected override void SetUp()
	{
		base.SetUp();

		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoiceHeader = jobDeclaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		cusLineTariffDetail = invoiceLine.AdditionalTaxes.AddNew();
		selectionCriteria = new AdditionalTaxSelectionCriteria(cusLineTariffDetail);
	}

	AdditionalTaxSelectionCriteria selectionCriteria;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoiceHeader;
	JobDeclaration jobDeclaration;
	CusLineTariffDetail cusLineTariffDetail;
}
