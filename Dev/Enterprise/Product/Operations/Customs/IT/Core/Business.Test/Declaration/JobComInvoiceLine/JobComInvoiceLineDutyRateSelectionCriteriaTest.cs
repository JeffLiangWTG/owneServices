using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobComInvoiceLineDutyRateSelectionCriteriaTest : TestCaseWithFactory
{
	public void TestDutyRateSelectionCriteria_IsTurkeyDutyRateSelectionCriteria()
	{
		declaration.JE_MessageType = "IMP";
		declaration.JE_GoodsOrigin = "TR";
		invoiceLine.JI_PrimaryPreference = "400";
		invoiceLine.JI_CountryOfOrigin = "ZA";

		CombineAssertions(() =>
		{
			AssertType<TurkeyDutyRateSelectionCriteria>("Duty Rate Selection Criteria Type", invoiceLine.DutyRateSelectionCriteria);
			AssertDutyRateSelectionCriteria(invoiceLine, "TR", "400");
		});
	}

	public void TestDutyRateSelectionCriteria_WhenMessageTypeIsExport()
	{
		declaration.JE_MessageType = "EXP";
		declaration.JE_GoodsOrigin = "TR";
		invoiceLine.JI_PrimaryPreference = "400";
		invoiceLine.JI_CountryOfOrigin = "ZA";
		invoiceLine.ZG_CountryOfDestination = "ZA";

		CombineAssertions(() => AssertDutyRateSelectionCriteria(invoiceLine, "ZA", "400"));
	}

	public void TestDutyRateSelectionCriteria_WhenGoodsOriginIsUS()
	{
		declaration.JE_MessageType = "IMP";
		declaration.JE_GoodsOrigin = "US";
		invoiceLine.JI_PrimaryPreference = "400";
		invoiceLine.JI_CountryOfOrigin = "ZA";

		CombineAssertions(() => AssertDutyRateSelectionCriteria(invoiceLine, "ZA", "400"));
	}

	public void TestDutyRateSelectionCriteria_WhenPreferenceIsNot400()
	{
		declaration.JE_MessageType = "IMP";
		declaration.JE_GoodsOrigin = "TR";
		invoiceLine.JI_PrimaryPreference = "100";
		invoiceLine.JI_CountryOfOrigin = "ZA";

		CombineAssertions(() => AssertDutyRateSelectionCriteria(invoiceLine, "ZA", "100"));
	}

	public void TestDutyRateSelectionCriteria_WhenCountryOfOriginIsTR()
	{
		declaration.JE_MessageType = "IMP";
		declaration.JE_GoodsOrigin = "TR";
		invoiceLine.JI_PrimaryPreference = "400";
		invoiceLine.JI_CountryOfOrigin = "TR";

		CombineAssertions(() =>
		{
			AssertNotEquals("Duty Rate Selection Criteria Type", typeof(TurkeyDutyRateSelectionCriteria), invoiceLine.DutyRateSelectionCriteria.GetType());
			AssertDutyRateSelectionCriteria(invoiceLine, "TR", "400");
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;

	static void AssertDutyRateSelectionCriteria(
		JobComInvoiceLine invoiceLine,
		ZString expectedCountryOfOrigin,
		ZString expectedPreference)
	{
		var dutyRateSelectionCriteria = invoiceLine.DutyRateSelectionCriteria;
		AssertEquals("EffectiveDate", invoiceLine.EffectiveAssessmentDate, dutyRateSelectionCriteria.EffectiveDate);
		AssertEquals("TradeGroupCountry", expectedCountryOfOrigin, dutyRateSelectionCriteria.TradeGroupCountry);
		AssertEquals("PrimaryPreference", expectedPreference, dutyRateSelectionCriteria.PrimaryPreference);
		AssertEquals("AdditionalCodes Count", 0, dutyRateSelectionCriteria.AdditionalCodes.Count);
		AssertEquals("ConcessionOrder", "", dutyRateSelectionCriteria.ConcessionOrder);
		AssertEquals("RateType", "DTY", dutyRateSelectionCriteria.RateType);
		AssertEquals("RateCode", "", dutyRateSelectionCriteria.RateCode);
	}
}
