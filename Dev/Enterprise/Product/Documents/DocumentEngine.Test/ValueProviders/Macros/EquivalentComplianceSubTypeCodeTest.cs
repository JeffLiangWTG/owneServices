using CargoWise.Application;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(EquivalentComplianceSubTypeCode))]
	sealed class EquivalentComplianceSubTypeCodeTest : ValueProviderTest
	{
		public override void TestDocumentation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Argentina))
			{
				base.TestDocumentation();
			}
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertMacroMatch($"<EquivalentComplianceSubTypeCode(TXA)>", true);
			AssertMacroMatch($"<EquivalentComplianceSubTypeCode(TXA)>", true);
			AssertMacroMatch($"<EQUIVALENTCOMPLIANCESUBTYPECODE(TXA)>", true);
			AssertMacroMatch($"<Equivalent ComplianceSubType Code(TXA)>", false);
			AssertMacroMatch($"<EquivalentComplianceSubTypeCode()>", false);
			AssertMacroMatch($"<EquivalentComplianceSubTypeCode('TXA')>", true);
			AssertMacroMatch($"<EquivalentComplianceSubTypeCode( 'TXA' )>", true);
			AssertMacroMatch($"<EquivalentComplianceSubTypeCode ('TXA')>", true);
			AssertMacroMatch($"< EquivalentComplianceSubTypeCode('TXA') >", true);
			AssertMacroMatch($"< EquivalentComplianceSubTypeCode ('TXA') >", true);
			AssertMacroMatch($"< EquivalentComplianceSubTypeCode 'TXA' >", false);

			void AssertMacroMatch(string macro, bool shouldMatch)
			{
				AssertEquals($"Should{(shouldMatch ? "" : " not")} match {macro}", shouldMatch, ValueProviderToTest.IsResponsibleForReplacing(macro, Passes.FirstPass));
			}
		}

		public void TestReplacement()
		{
			var mockIEquivalentComplianceSubTypeProvider = new Mock<IEquivalentComplianceSubTypeProvider>();
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();

			var complianceSubtype = "ABC";
			var expectedEquivalentSubType = "123";
			mockIEquivalentComplianceSubTypeProvider.Setup(x => x.GetEquivalentComplianceSubType(complianceSubtype)).Returns(expectedEquivalentSubType);

			mockICountryComplianceFactory.Setup(x => x.GetIEquivalentComplianceSubTypeProvider(GlbCompany.CurrentCompany.Country.Code)).Returns(mockIEquivalentComplianceSubTypeProvider.Object);
			ObjectFactory.Substitute(mockICountryComplianceFactory.Object);

			AssertEquals($"Equivalent Code {complianceSubtype}", expectedEquivalentSubType, ValueProviderToTest.GetReplacement($"<EquivalentComplianceSubTypeCode({complianceSubtype})>", Report));
			mockICountryComplianceFactory.Verify(x => x.GetIEquivalentComplianceSubTypeProvider(GlbCompany.CurrentCompany.Country.Code), Times.Once);
			mockIEquivalentComplianceSubTypeProvider.Verify(x => x.GetEquivalentComplianceSubType(complianceSubtype), Times.Once);

			mockICountryComplianceFactory.Reset();
			mockIEquivalentComplianceSubTypeProvider.Reset();

			mockICountryComplianceFactory.Setup(x => x.GetIEquivalentComplianceSubTypeProvider(GlbCompany.CurrentCompany.Country.Code)).Returns((IEquivalentComplianceSubTypeProvider)null);

			complianceSubtype = "DEF";
			AssertEquals($"Equivalent Code {complianceSubtype}", string.Empty, ValueProviderToTest.GetReplacement($"<EquivalentComplianceSubTypeCode({complianceSubtype})>", Report));
			mockICountryComplianceFactory.Verify(x => x.GetIEquivalentComplianceSubTypeProvider(GlbCompany.CurrentCompany.Country.Code), Times.Once);
			mockIEquivalentComplianceSubTypeProvider.Verify(x => x.GetEquivalentComplianceSubType(complianceSubtype), Times.Never);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new EquivalentComplianceSubTypeCode();
		}
	}
}
