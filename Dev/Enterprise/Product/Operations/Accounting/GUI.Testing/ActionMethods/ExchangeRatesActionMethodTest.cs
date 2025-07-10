using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(ExchangeRatesActionMethod))]
	internal sealed class ExchangeRatesActionMethodTest : OperationalActionMethodTest<ExchangeRatesActionMethod>
	{
		public void TestApplicatorIsOfTheCorrectType()
		{
			OperationalActionMethodApplicator applicator = Method.NewApplicator(Factory, new ExchangeRatesSettings(new ExRateSourceType[] { ExRateSourceType.Voyage }));
			AssertNotNull(applicator);
			AssertEquals(typeof(ExchangeRatesActionMethodApplicator), applicator.GetType());
		}

		public void TestHasSettings()
		{
			AssertEquals("Should have settings enabled", true, Method.HasSettings);
			AssertEquals("Expected Settings Type", typeof(ExchangeRatesSettings), Method.NewSetting(Factory).GetType());
		}

		public void TestNameIsAsExpected()
		{
			AssertEquals("Update Ex Rates as per Defined Rules", Method.Name);
		}
		#region Implementation

		protected override ExchangeRatesActionMethod NewMethod()
		{
			return new ExchangeRatesActionMethod(new ExRateSourceType[] { ExRateSourceType.Voyage });
		}

		#endregion
	}
}
