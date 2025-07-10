using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUCClassCollection))]
	sealed class AUCClassCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<AUCClass>();
			return new AUCClassCollection(parent, Factory);
		}
	}
}
