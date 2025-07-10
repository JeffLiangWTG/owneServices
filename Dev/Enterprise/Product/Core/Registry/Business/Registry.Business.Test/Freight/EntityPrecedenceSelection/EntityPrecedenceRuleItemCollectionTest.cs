using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EntityPrecedenceRuleItemCollection))]
	sealed class EntityPrecedenceRuleItemCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<EntityPrecedenceRuleItemCollection>
	{
		public void TestReadOnly()
		{
			var entityPrecedenceRuleItemCollection = new EntityPrecedenceRuleItemCollection();
			Assert(entityPrecedenceRuleItemCollection.ReadOnly);
		}

		public void TestMoveItem()
		{
			var collection = new EntityPrecedenceRuleItemCollection
			{
				new EntityPrecedenceRuleItem { Code = "AAA", Description = (NoResString)"ADesc" },
				new EntityPrecedenceRuleItem { Code = "BBB", Description = (NoResString)"BDesc" },
				new EntityPrecedenceRuleItem { Code = "CCC", Description = (NoResString)"CDesc" },
			};

			collection.MoveItem(2, 0);

			AssertEquals("CCC", collection[0].Code);
			AssertEquals("AAA", collection[1].Code);
			AssertEquals("BBB", collection[2].Code);
		}

		protected override EntityPrecedenceRuleItemCollection GetCollectionToTest()
		{
			return new EntityPrecedenceRuleItemCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EntityPrecedenceRuleItem();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}
	}
}
