using WTG.Glow.Data.Annotations;

namespace CargoWise.UniversalCopy.Test
{
	public class CollectionCopyTemplateNodeTest : WrappedCopyTemplateNodeTest<CollectionCopyTemplateNode>
	{
		public void TestCollectionNodeProperties()
		{
			var target = CreateConfigurationNode();

			AssertEquals("CollectionName", target.Name);
			AssertEquals("PropertyName", target.ItemPropertyName);
			AssertEquals("SimpleElement", target.ItemsTableName);
		}

		public void TestHasData()
		{
			var target = CreateConfigurationNode();
			Assert(!CheckHasData(target));

			target.CopyMethod = CollectionCopyMethod.All;
			Assert(CheckHasData(target));
		}

		#region TestResetAndUpdateId

		protected override void PrepareDataForResetAndUpdateId(CollectionCopyTemplateNode target)
		{
			base.PrepareDataForResetAndUpdateId(target);
			target.CopyMethod = CollectionCopyMethod.Filter;
			target.Filter = new EntityFilter();
		}

		protected override void AssertDataAfterResetAndUpdateId(CollectionCopyTemplateNode target)
		{
			base.AssertDataAfterResetAndUpdateId(target);
			AssertEquals(default(CollectionCopyMethod), target.CopyMethod);
			AssertNull(target.Filter);
		}

		#endregion

		#region Implementation

		protected override CollectionCopyTemplateNode CreateConfigurationNode()
		{
			return new CollectionCopyTemplateNode(
				new CollectionRelationPropertyAttribute(ExpectedName, "PropertyName", "EntitySet"),
				"SimpleElement",
				new CopyTemplateTree(typeof(SimpleElement)).InnerNode);
		}

		protected override string ExpectedName
		{
			get { return "CollectionName"; }
		}

		#endregion
	}
}
