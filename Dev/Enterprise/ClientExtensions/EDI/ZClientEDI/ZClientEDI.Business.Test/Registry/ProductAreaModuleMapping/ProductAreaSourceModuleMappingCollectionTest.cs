using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ProductAreaSourceModuleMappingCollection))]
	internal sealed class ProductAreaSourceModuleMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ProductAreaSourceModuleMappingCollection>
	{
		public void TestAddIfNotExists()
		{
			var collection = new ProductAreaSourceModuleMappingCollection();
			collection.AddNew("XXX", "P1");
			collection.AddIfNotExists("XXX", "P2");

			AssertEquals(1, collection.Count);
			AssertEquals("P1", collection[0].ProductArea);
		}

		public void TestGetSourceModuleMapping()
		{
			var collection = new ProductAreaSourceModuleMappingCollection();
			collection.AddNew("XXX", "P1");
			collection.AddNew("YYY", "P2");

			AssertEquals("P1", collection.GetSourceModuleMapping("XXX").ProductArea);
			AssertEquals("P2", collection.GetSourceModuleMapping("yyy").ProductArea);
			AssertNull(collection.GetSourceModuleMapping("ZZZ"));
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ProductAreaSourceModuleMappingCollection GetCollectionToTest()
		{
			return new ProductAreaSourceModuleMappingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProductAreaSourceModuleMapping();
		}

		#endregion
	}
}
