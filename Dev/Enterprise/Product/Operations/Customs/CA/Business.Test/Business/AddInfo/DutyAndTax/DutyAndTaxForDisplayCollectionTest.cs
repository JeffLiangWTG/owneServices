using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DutyAndTaxForDisplayCollection))]
	sealed class DutyAndTaxForDisplayCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DutyAndTaxForDisplayCollection(new BusinessObjectFactory());
		}
	}
}
