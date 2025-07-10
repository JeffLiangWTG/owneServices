using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ComponentChargeAmountWrapper))]
	sealed class ComponentChargeAmountWrapperTest : GenericWrapperTest
	{
		public void TestWrapperMappingsFull()
		{
			CombineAssertions(delegate
			{
				ComponentChargeAmountWrapper wrapper = new ComponentChargeAmountWrapper(1000, 100, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR"), Factory);
				AssertEquals("1,000.00 EUR", wrapper.WithoutTax.ToString());
				AssertEquals("100.00 EUR", wrapper.Tax.ToString());
				AssertEquals("1,100.00 EUR", wrapper.WithTax.ToString());
			});
		}

		public override void TestWrapperMappingsEmpty()
		{
			CombineAssertions(delegate
			{
				ComponentChargeAmountWrapper wrapper = new ComponentChargeAmountWrapper(0, 0, 0, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD"), Factory);
				AssertEquals("", wrapper.WithoutTax.ToString());
				AssertEquals("", wrapper.Tax.ToString());
				AssertEquals("", wrapper.WithTax.ToString());
				AssertEquals("USD - United States Dollar", wrapper.Currency.ToString());
			});
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Currency : USD - United States Dollar
Registry : (No Default Field Value Available on Registry)
Tax : 20.00 USD
WithoutTax : 200.00 USD
WithTax : 220.00 USD
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new ComponentChargeAmountWrapper(200, 20, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD"), Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Charge Breakdown                              (Default Field: WithTax)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Currency                                Currency
Tax                                     Money
WithoutTax                              Money
WithTax                                 Money
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ComponentChargeAmountWrapper(100, 10, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR"), Factory);
		}

		#endregion
	}
}
