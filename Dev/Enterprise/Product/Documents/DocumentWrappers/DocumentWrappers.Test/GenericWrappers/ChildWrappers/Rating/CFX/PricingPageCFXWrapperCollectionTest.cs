using System.Text;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageCFXWrapperCollection))]
	sealed class PricingPageCFXWrapperCollectionTest : GenericWrapperCollectionTest<PricingPageCFXWrapperCollection>
	{
		public void TestPopulate()
		{
			ILocation local = GlbCompany.CurrentCompany.Country;
			ILocation notLocal = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, local.Code));

			Env.Registry.Rating.IncludeCFXOnQuote = true;

			CompanyTariff tariff = Factory.New<CompanyTariff>();
			tariff.TH_AirCFX = 1.1m;
			tariff.TH_SeaCFX = 1.2m;
			tariff.TH_ExportAirCFX = 2.1m;
			tariff.TH_ExportSeaCFX = 2.2m;

			RateEntry entryIA = tariff.AddRateEntry("AIR", Core.Constants.RateMode.AIR, notLocal.Code, local.Code);
			RateEntry entryEA = tariff.AddRateEntry("AIR", Core.Constants.RateMode.AIR, local.Code, notLocal.Code);
			RateEntry entryIS = tariff.AddRateEntry("FCL", Core.Constants.RateMode.FCL, notLocal.Code, local.Code);
			RateEntry entryES = tariff.AddRateEntry("FCL", Core.Constants.RateMode.FCL, local.Code, notLocal.Code);

			PricingPageCFXWrapperCollection collection;

			collection = new PricingPageCFXWrapperCollection(new PricingPage(entryIA, Factory, PricingPageStyle.Standard), Factory);
			AssertMultilineASCIIEquals("", "Import - Air 1.10%", Render(collection));

			collection = new PricingPageCFXWrapperCollection(new PricingPage(entryIS, Factory, PricingPageStyle.Standard), Factory);
			AssertMultilineASCIIEquals("", "Import - Sea 1.20%", Render(collection));

			collection = new PricingPageCFXWrapperCollection(new PricingPage(entryEA, Factory, PricingPageStyle.Standard), Factory);
			AssertMultilineASCIIEquals("", "Export - Air 2.10%", Render(collection));

			collection = new PricingPageCFXWrapperCollection(new PricingPage(entryES, Factory, PricingPageStyle.Standard), Factory);
			AssertMultilineASCIIEquals("", "Export - Sea 2.20%", Render(collection));

			PricingPage page = new PricingPage(entryIA, Factory, PricingPageStyle.Landscape);
			page.AddRateEntry(entryIS);
			page.AddRateEntry(entryEA);
			page.AddRateEntry(entryES);

			const string expected =
@"Import - Air 1.10%
Import - Sea 1.20%
Export - Air 2.10%
Export - Sea 2.20%
";

			collection = new PricingPageCFXWrapperCollection(page, Factory);
			AssertMultilineASCIIEquals("", expected, Render(collection));

			Env.Registry.Rating.IncludeCFXOnQuote = false;

			collection = new PricingPageCFXWrapperCollection(page, Factory);
			AssertMultilineASCIIEquals("", "", Render(collection));
		}

		#region Implementation

		string Render(PricingPageCFXWrapperCollection collection)
		{
			StringBuilder builder = new StringBuilder();

			foreach (PricingPageCFXWrapper wrapper in collection)
			{
				builder.AppendLine(wrapper.NameAndValue);
			}

			return builder.ToString();
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new PricingPageCFXWrapper(Factory, "name", 9.8m);
		}

		protected override PricingPageCFXWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new PricingPageCFXWrapperCollection(Factory);
		}

		#endregion
	}
}
