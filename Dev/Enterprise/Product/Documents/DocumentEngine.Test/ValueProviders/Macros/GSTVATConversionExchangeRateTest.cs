using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.Integration.Accounting;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GSTVATConversionExchangeRate))]
	sealed class GSTVATConversionExchangeRateTest : ValueProviderWithLoadControlFactoryTest<GSTVATConversionExchangeRate>
	{
		public override void TestReplacement()
		{
			var mockSupporter = new Mock<IAccounting>();
			mockSupporter.Setup(m => m.GetGSTVATConversionExchangeRate(It.IsAny<ZGuid>(), It.IsAny<ZGuid>())).Returns(0.5675m);
			using (ObjectFactory.Substitute<IAccounting>(mockSupporter.Object))
			{
				AssertEquals(0.5675m, ValueProviderToTest.GetReplacement(GSTVATConversionExchangeRateMacro("FD24AA1B-60B4-4A4D-982C-36622D50F6B2", "878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"), Report));
			}
		}

		public override void TestDocumentation()
		{
			var mockSupporter = new Mock<IAccounting>();
			mockSupporter.Setup(m => m.GetGSTVATConversionExchangeRate(It.IsAny<ZGuid>(), It.IsAny<ZGuid>())).Returns(1m);
			using (ObjectFactory.Substitute<IAccounting>(mockSupporter.Object))
			{
				base.TestDocumentation();
			}
		}

		public override void TestIsResponsibleForReplacing()
		{
			CombineAssertions(() =>
			{
				Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("GSTVATConversionExchangeRate", Passes.FirstPass));
				Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<GSTVATConversionExchangeRate meh>", Passes.FirstPass));
				Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GSTVATConversionExchangeRate(69184D15-289F-4CFB-97F0-CE1678DACF96,E8EB9302-CFC1-4DBD-A45C-15750B066C1F)>", Passes.FirstPass));
			});
		}

		protected override ValueProvider GetNewValueProvider() => new GSTVATConversionExchangeRate();

		string GSTVATConversionExchangeRateMacro(string transactionHeaderPK, string companyPK) => $"<GSTVATConversionExchangeRate({transactionHeaderPK}, {companyPK})>";
	}
}
