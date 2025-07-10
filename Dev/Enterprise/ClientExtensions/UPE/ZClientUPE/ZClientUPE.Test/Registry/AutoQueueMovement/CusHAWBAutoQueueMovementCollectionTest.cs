using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(CusHAWBAutoQueueMovementCollection))]
	internal class CusHAWBAutoQueueMovementCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CusHAWBAutoQueueMovementCollection>
	{
		public void TestIndexerAndAddNew()
		{
			CusHAWBAutoQueueMovement movement1 = Collection.AddNew();
			CusHAWBAutoQueueMovement movement2 = Collection.AddNew();
			AssertEquals("Collection[0]", movement1, Collection[0]);
			AssertEquals("Collection[1]", movement2, Collection[1]);
		}

		protected override CusHAWBAutoQueueMovementCollection GetCollectionToTest()
		{
			return new CusHAWBAutoQueueMovementCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CusHAWBAutoQueueMovement();
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

		new CusHAWBAutoQueueMovementCollection Collection
		{
			get
			{
				return base.Collection;
			}
		}
	}
}
