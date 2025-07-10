using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TransitReferenceMappingCollection))]
	sealed class TransitReferenceMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TransitReferenceMappingCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override TransitReferenceMappingCollection GetCollectionToTest()
		{
			return new TransitReferenceMappingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TransitReferenceMapping();
		}

		#endregion
	}
}
