using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSPackageCollection))]
	class EMCSPackageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			return new EMCSPackageCollection(declaration);
		}
	}
}
