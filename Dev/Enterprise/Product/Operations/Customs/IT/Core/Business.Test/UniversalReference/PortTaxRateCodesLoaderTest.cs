using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class PortTaxRateCodesLoaderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When factory is null",
			() => new PortTaxRateCodesLoader(factory: null, ZDate.Empty));
	}

	public void TestGetAllRateCodes()
	{
		SetUpHarbourRates();

		var rateCodesLoader = (IPortTaxRateCodesLoader)new PortTaxRateCodesLoader(Factory, ZDateTime.Now);
		var rateCodes = rateCodesLoader.GetAllRateCodes();
		AssertContainsExactElementsInAnyOrder(rateCodes, new ZString[] { "9AA", "9AB" });
	}

	void SetUpHarbourRates()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.CreateHarbourRate("XYZ", "", "ALL", "", ZDate.Today.AddYears(-1), ZDate.Today.AddYears(1), "", "IT", "000");
		helper.CreateHarbourRate("TAX", "", "XYZ", "", ZDate.Today.AddYears(-1), ZDate.Today.AddYears(1), "", "IT", "001");
		helper.CreateHarbourRate("TAX", "", "ALL", "", ZDate.Today.AddYears(-2), ZDate.Today.AddYears(-1), "", "IT", "002");
		helper.CreateHarbourRate("TAX", "", "ALL", "", ZDate.Today.AddYears(-1), ZDate.Today.AddYears(1), "", "FR", "003");
		helper.SetupHarbourRates();
	}
}
