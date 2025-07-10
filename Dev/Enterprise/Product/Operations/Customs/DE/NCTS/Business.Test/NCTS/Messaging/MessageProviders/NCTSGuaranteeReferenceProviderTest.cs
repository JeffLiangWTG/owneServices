using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSGuaranteeReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSGuaranteeReferenceProvider>
	{
		public void TestArgumentNull()
		{
			AssertExceptionThrown<ArgumentException>(() => new NCTSGuaranteeReferenceProvider(null));
		}

		public void TestGRN()
		{
			AssertEquals("DE1234567", Provider.GRN);
		}

		public void TestAccessCode()
		{
			AssertEquals("1234", Provider.AccessCode);
		}

		public void TestAmount()
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.RE_RX_NKExCurrency = "YSN";
			exchangeRate.RE_SellRate = 2.0m;

			CombineAssertions(() =>
			{
				guarantee.PW_RX_NKCurrency = CurrencyCodes.EuropeanUnion;
				AssertEquals(1.12M, Provider.Amount);

				guarantee.PW_RX_NKCurrency = ZString.Empty;
				AssertEquals(1.12M, Provider.Amount);

				guarantee.PW_RX_NKCurrency = "YSN";
				AssertEquals("specific ExchangeRate exists", 0.56M, Provider.Amount);

				guarantee.PW_RX_NKCurrency = "SBB";
				AssertEquals("specific ExchangeRate not exist", decimal.Zero, Provider.Amount);
			});
		}

		public void TestCurrency()
		{
			AssertEquals(CurrencyCodes.EuropeanUnion, Provider.Currency);
		}

		protected override NCTSGuaranteeReferenceProvider GetProvider() => new NCTSGuaranteeReferenceProvider(guarantee);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			guarantee = Factory.New<NctsGuarantee>();
			guarantee.PW_BondNumber = "DE1234567";
			guarantee.PW_Password = "1234";
			guarantee.PW_BondAmount = 1.1234M;
			guarantee.Parent = header.MovementHeader;
		}
		NctsGuarantee guarantee;
		NctsHeader header;
	}
}
