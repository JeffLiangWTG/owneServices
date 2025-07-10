using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class InvoiceLineChargeDefinitionProviderTest : TestCaseWithFactory
	{
		public void TestConstructor_NullParameter()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new InvoiceLineChargeDefinitionProvider(null));
		}

		public void TestAmount()
		{
			AssertEquals(42.12m, provider.Amount);
		}

		public void TestChargeType()
		{
			AssertEquals("001", provider.ChargeType);
		}

		public void TestCurrency()
		{
			AssertEquals("GBP", provider.Currency);
		}

		public void TestIsDutiable()
		{
			AssertEquals(true, provider.IsDutiable);
		}

		public void TestIsGSTApplicable()
		{
			AssertEquals(true, provider.IsGSTApplicable);
		}

		public void TestIsIncludedInITOT()
		{
			AssertEquals(false, provider.IsIncludedInITOT);
		}

		public void TestIsStatisticalValueApplicable()
		{
			AssertEquals(true, provider.IsStatisticalValueApplicable);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var cusAddInfo = Factory.New<WarehouseCustomsAddInfo>();
			cusAddInfo.B7_AddInfoData =
				"*Amount=42.12*ChargeType=001*Currency=GBP*IsDutiable=Y*IsGSTApplicable=Y*IsIncludedInITOT=*IsStatisticalValueApplicable=Y";

			provider = new InvoiceLineChargeDefinitionProvider(cusAddInfo);
		}

		InvoiceLineChargeDefinitionProvider provider;
	}
}
