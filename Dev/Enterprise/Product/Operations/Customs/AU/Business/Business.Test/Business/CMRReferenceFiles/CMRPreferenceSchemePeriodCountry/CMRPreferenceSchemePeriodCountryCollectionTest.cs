using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRPreferenceSchemePeriodCountryCollection))]
	sealed class CMRPreferenceSchemePeriodCountryCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CMRPreferenceSchemePeriodCountryCollection(Factory, null);
	}
}
