using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ServiceTypeModuleMappingCollection))]
	internal sealed class ServiceTypeModuleMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ServiceTypeModuleMappingCollection>
	{
		public void TestAddIfNotExists()
		{
			var collection = new ServiceTypeModuleMappingCollection();
			collection.AddNew("CD1");
			collection.AddNew("CD2");
			collection.AddIfNotExists("CD1");

			AssertEquals(2, collection.Count);
			AssertEquals("CD1", collection[0].Code);
			AssertEquals("CD2", collection[1].Code);
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

		protected override ServiceTypeModuleMappingCollection GetCollectionToTest()
		{
			return new ServiceTypeModuleMappingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ServiceTypeModuleMapping();
		}

		#endregion
	}
}
