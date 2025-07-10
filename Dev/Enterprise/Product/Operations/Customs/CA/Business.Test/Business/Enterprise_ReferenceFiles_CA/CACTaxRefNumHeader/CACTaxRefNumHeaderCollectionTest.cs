using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACTaxRefNumHeaderCollection))]
	sealed class CACTaxRefNumHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CACTaxRefNumHeaderCollection(Factory.New<CACClassHeader>());
		}
	}
}
