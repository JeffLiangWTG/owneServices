using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(ChaseQueueValidationCollection))]
	internal class ChaseQueueValidationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ChaseQueueValidationCollection>
	{
		public void TestIndexerAndAddNew()
		{
			ChaseQueueValidation movement1 = Collection.AddNew();
			ChaseQueueValidation movement2 = Collection.AddNew();
			AssertEquals("Collection[0]", movement1, Collection[0]);
			AssertEquals("Collection[1]", movement2, Collection[1]);
		}

		protected override ChaseQueueValidationCollection GetCollectionToTest()
		{
			return new ChaseQueueValidationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ChaseQueueValidation();
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		new ChaseQueueValidationCollection Collection
		{
			get
			{
				return base.Collection;
			}
		}
	}
}
