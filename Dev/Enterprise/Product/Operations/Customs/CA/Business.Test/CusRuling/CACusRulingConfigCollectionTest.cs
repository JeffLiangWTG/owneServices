using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACusRulingConfigCollection))]
	sealed class CACusRulingConfigCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
			return new CACusRulingConfigCollection(cusRuling);
		}
	}
}
