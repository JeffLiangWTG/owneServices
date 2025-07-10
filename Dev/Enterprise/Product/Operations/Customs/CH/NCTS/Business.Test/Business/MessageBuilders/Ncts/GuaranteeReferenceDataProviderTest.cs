using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class GuaranteeReferenceDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		var guaranteeReferenceDataProvider = GuaranteeReferenceDataProvider.New(null, 1);

		AssertNull("guarantee is null", guaranteeReferenceDataProvider);
	}

	public void TestAmountToBeCovered() => CombineAssertions(() =>
	{
		var exchangeRate = Factory.New<RefExchangeRate>();
		exchangeRate.RE_StartDate = ZDateTime.MinSmallDateTimeValue;
		exchangeRate.RE_ExpiryDate = ZDateTime.MaxSmallDateTimeValue;
		exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
		exchangeRate.RE_RX_NKExCurrency = "XXX";
		exchangeRate.RE_SellRate = 2.5m;

		const decimal bondAmount = 100.11m;

		Guarantee.PW_BondAmount = bondAmount;

		AssertEquals(bondAmount, DataProvider.AmountToBeCovered);

		Guarantee.PW_RX_NKCurrency = Core.Constants.CurrencyCodes.Switzerland;
		Guarantee.PW_BondAmount = 100.11m;
		AssertEquals("Local currency (CHF)", 100.11m, DataProvider.AmountToBeCovered);

		Guarantee.PW_RX_NKCurrency = exchangeRate.RE_RX_NKExCurrency;
		ResetDataProvider();
		AssertEquals("Foreign currency (rounded)", 250.28m, DataProvider.AmountToBeCovered);

		exchangeRate.RE_ExpiryDate = ZDateTime.MinSmallDateTimeValue;
		ResetDataProvider();
		AssertEquals("No valid exchange rate", 0m, DataProvider.AmountToBeCovered);
	});

	public void TestGRN()
	{
		const string bondNumber = "123";

		Guarantee.PW_BondNumber = bondNumber;

		AssertEquals(bondNumber, DataProvider.GRN);
	}

	public void TestGuaranteeAccessCode()
	{
		const string password = "123";

		Guarantee.PW_Password = password;

		AssertEquals(password, DataProvider.AccessCode);
	}

	public void TestSequenceNumber()
	{
		AssertEquals(1, DataProvider.SequenceNumber);
	}

	NctsGuarantee CreateNctsGuarantee()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
		return nctsHeader.MovementHeader.Guarantees.AddNew();
	}

	NctsGuarantee Guarantee => guarantee ?? (guarantee = CreateNctsGuarantee());
	NctsGuarantee guarantee;

	GuaranteeReferenceDataProvider DataProvider => dataProvider ?? (dataProvider = GuaranteeReferenceDataProvider.New(Guarantee, 1));
	GuaranteeReferenceDataProvider dataProvider;

	void ResetDataProvider()
	{
		dataProvider = null;
	}
}
