using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(SsasCube))]
	class SsasCubeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SsasCube("Logistics Model");
		}

		public void TestSsasCube()
		{
			var ssasCube = new SsasCube("Logistics Model");
			AssertNotNull(ssasCube);
		}
	}
}
