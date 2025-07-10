using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ReportOrderCollection))]
	public class ReportOrderCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ReportOrderCollection>
	{
		public void TestFindByLanguageAndCountryCode()
		{
			ReportOrder reportOrder1 = Collection.AddNew();
			ReportOrder reportOrder2 = Collection.AddNew();

			reportOrder1.Language = reportOrder1.LanguageList[0].Code;
			reportOrder2.Language = reportOrder2.LanguageList[1].Code;
			reportOrder1.CountryCode = reportOrder1.Countries[0].Code;
			reportOrder2.CountryCode = reportOrder2.Countries[1].Code;
			AssertEquals("FindByLanguage(\"" + reportOrder1.Language + "\") should return null.", null, Collection.FindByLanguageAndCountryCode(reportOrder1.Language, ""));
			AssertEquals("FindByLanguage(\"" + reportOrder1.Language + "\") should return ReportOrder1.", reportOrder1, Collection.FindByLanguageAndCountryCode(reportOrder1.Language, reportOrder1.CountryCode));
			AssertEquals("FindByLanguage(\"" + reportOrder2.Language + "\") should return ReportOrder2.", reportOrder2, Collection.FindByLanguageAndCountryCode(reportOrder2.Language, reportOrder2.CountryCode));
			AssertEquals("FindByLanguage(\"!@#\") should return null.", null, Collection.FindByLanguageAndCountryCode("!@#", "a2"));
			AssertEquals("FindByLanguage(\"!@#\") should return null.", null, Collection.FindByLanguageAndCountryCode("!@#", reportOrder1.CountryCode));
		}

		#region Implementation

		protected override ReportOrderCollection GetCollectionToTest()
		{
			return new ReportOrderCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReportOrder(Factory);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new ReportOrderCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}
