using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class DepartureNctsGuaranteeValidationTest : BaseNctsGuaranteeValidationTest
{
	public void TestValidation()
	{
		Assert("Validation should inherit from EU.NctsGuaranteePhase5Validation", typeof(NctsGuaranteePhase5Validation).IsInstanceOfType(Guarantee.Validation));
	}

	public void TestCheckPW_BondType()
	{
		new RefDataTestHelper(Factory).CreateNctsBondTypeList();
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Guarantee.PW_BondTypeInfo, "X", "0");
	}

	public void TestCheckPW_BondAmount_TR0089() => CombineAssertions(() =>
	{
		const string message = "[TR0089]";

		EnsureNoValidExchangeRate(Core.Constants.CurrencyCodes.EuropeanUnion);
		EnsureNoValidExchangeRate(Core.Constants.CurrencyCodes.Switzerland);

		Guarantee.NctsHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;

		Guarantee.PW_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
		AssertHasMessageErrorContaining("Currency requiring exchange rate", Guarantee.PW_RX_NKCurrencyInfo, message);
		Guarantee.PW_RX_NKCurrency = Core.Constants.CurrencyCodes.Switzerland;
		AssertNoMessageErrorContaining("No exchange rate required for CHF", Guarantee.PW_RX_NKCurrencyInfo, message);

		void EnsureNoValidExchangeRate(string currency)
		{
			var exchangeRate = new RefExchangeRate.Loader(Factory).GetEffectiveRateOn(ZDate.Today, currency, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK);
			exchangeRate?.Delete();
		}
	});

	protected override string MovementType => Common.EU.NctsMoveHeaderType.Codes.Departure;
}
