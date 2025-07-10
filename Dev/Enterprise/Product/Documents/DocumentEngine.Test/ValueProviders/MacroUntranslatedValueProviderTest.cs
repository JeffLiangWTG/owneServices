using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(MacroUntranslatedValueProvider))]
	sealed class MacroUntranslatedValueProviderTest : ValueProviderTest
	{
		public void TestReplacementWithMacrosInTheReportName()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Germany);

			PrepareRenderer();

			((IReportForUnitTesting)Report).fName = "<CompanyCountry> My Title";
			AssertEquals("Germany My Title", ValueProviderToTest.GetReplacement("<ReportNameUntranslated>", Report));

			((IReportForUnitTesting)Report).fName = "<Upper(\"<CompanyCountry>\")> My Title";
			AssertEquals("GERMANY My Title", ValueProviderToTest.GetReplacement("<ReportNameUntranslated>", Report));
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <Reprot Name Short>", !ValueProviderToTest.IsResponsibleForReplacing("<Reprot Name Short>", Passes.FirstPass));
			Assert("should match < report      name         short Untranslated>", ValueProviderToTest.IsResponsibleForReplacing("< report      name         short Untranslated>", Passes.FirstPass));
			Assert("should match <ReportNameShortUntranslated>", ValueProviderToTest.IsResponsibleForReplacing("<ReportNameShortUntranslated>", Passes.FirstPass));
			Assert("should match < report      name    untranslated   >", ValueProviderToTest.IsResponsibleForReplacing("< report      name    untranslated   >", Passes.FirstPass));
			Assert("should match <ReportNameUntranslated>", ValueProviderToTest.IsResponsibleForReplacing("<ReportNameUntranslated>", Passes.FirstPass));
		}

		[TestDate(2024, 1, 1)]
		public void TestNestedMarcoWithNonDefaultCulture()
		{
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Core.SharedConstants.Languages.Khmer)))
			{
				AssertEquals("01-Jan-24", ValueProviderToTest.GetReplacement("<DateTimeAsString('<Now>', 'dd-MMM-yy')Untranslated>", Report));
			}
		}

		public void TestReplacement()
		{
			using (var resourceStrings = Res.GetLanguageInstance(SharedConstants.Languages.German).UseMockData())
			{
				resourceStrings.Put("ReportTitle|Shipment Cartage Advice for {0}", new ResourceStringData("", "Karthago Beratung für {0}", "", "Versand Karthago Beratung für {0}", ""));

				using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.German))
				{
					Report.Parent.Language = SharedConstants.Languages.German;
					PrepareRenderer();
					((IReportForUnitTesting)Report).fName = "Shipment Cartage Advice for <CompanyCountry>";

					var reportNameProvider = new ReportName();
					var reportNameShotProvider = new ReportNameShort();
					AssertEquals("Versand Karthago Beratung für Australia", reportNameProvider.GetReplacement("<ReportName>", Report));
					AssertEquals("Karthago Beratung für Australia", reportNameShotProvider.GetReplacement("<ReportNameShort>", Report));

					AssertNotEquals("Versand Karthago Beratung für Australia", ValueProviderToTest.GetReplacement("<ReportNameUnTranslated>", Report));
					AssertEquals("Shipment Cartage Advice for Australia", ValueProviderToTest.GetReplacement("<ReportNameUnTranslated>", Report));

					AssertNotEquals("Karthago Beratung für Australia", ValueProviderToTest.GetReplacement("<ReportNameShortUnTranslated>", Report));
					AssertEquals("Cartage Advice for Australia", ValueProviderToTest.GetReplacement("<ReportNameShortUnTranslated>", Report));
				}
			}
		}

		public override void TestExistsInValueProviderCollection()
		{
			Assert(true); // MacroUntranslatedProvider is not added to ValueProviderCollector
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new MacroUntranslatedValueProvider();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Germany);
		}
	}
}
