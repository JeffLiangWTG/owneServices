using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACTaxRefNumberCollection))]
	sealed class CACTaxRefNumberCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CACTaxRefNumberCollection(Factory.New<CACTaxRefNumHeader>());
		}
	}
}
