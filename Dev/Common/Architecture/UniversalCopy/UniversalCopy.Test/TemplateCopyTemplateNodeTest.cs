using WTG.Glow.Data.Annotations;

namespace CargoWise.UniversalCopy.Test
{
	public class TemplateCopyTemplateNodeTest : WrappedCopyTemplateNodeTest<TemplateCopyTemplateNode>
	{
		public void TestFindTemplateAndInitializeInnerNode()
		{
			EntityCopyTemplateNode entityNode = new EntityCopyTemplateNode(typeof(SimpleElement));
			TemplateCopyTemplateNode templateNode = new TemplateCopyTemplateNode(entityNode);

			AssertNull(templateNode.InnerNode);
			AssertSame(entityNode, templateNode.TemplateNode);
			AssertEquals(entityNode.Id, templateNode.TemplateNodeId);
			AssertNotEquals(entityNode.Id, templateNode.Id);

			templateNode.FindTemplateAndInitializeInnerNode(entityNode);

			AssertNotNull(templateNode.InnerNode);
			AssertNotEquals(entityNode.Id, templateNode.InnerNode.Id);
			AssertNotEquals(templateNode.Id, templateNode.InnerNode.Id);
			AssertEquals(ExpectedName, templateNode.InnerNode.Name);
		}

		public void TestCloneTemplateNodeWithMandatoryFilterSplitCollection()
		{
			var entityNode = new EntityCopyTemplateNode(typeof(SimpleElement));
			var collectionNode = new CollectionCopyTemplateNode(new CollectionRelationPropertyAttribute("Collection", "Item", "Entities"), "Table", entityNode)
			{
				Filter = new EntityFilter
				{
					FilterTypeId = EntityFilterTypeIds.MandatoryExpressionFilter,
					FilterData = "abc",
				},
				CopyMethod = CollectionCopyMethod.None,
				Order = 1,
			};

			var templateNode = new TemplateCopyTemplateNode(collectionNode);

			AssertNull(templateNode.InnerNode);

			templateNode.FindTemplateAndInitializeInnerNode(collectionNode);

			AssertNotNull(templateNode.InnerNode);

			var clonedCollectionNode = templateNode.InnerNode as CollectionCopyTemplateNode;
			AssertNotNull(clonedCollectionNode);
			AssertNotEquals(collectionNode.Id, clonedCollectionNode.Id);
			AssertNotNull(clonedCollectionNode.Filter);
			AssertEquals(EntityFilterTypeIds.MandatoryExpressionFilter, clonedCollectionNode.Filter.FilterTypeId);
			AssertEquals("abc", clonedCollectionNode.Filter.FilterData);
			AssertEquals(1, clonedCollectionNode.Order);
		}

		#region TestResetAndUpdateId

		protected override void PrepareDataForResetAndUpdateId(TemplateCopyTemplateNode target)
		{
			base.PrepareDataForResetAndUpdateId(target);
			Assert(!string.IsNullOrEmpty(target.TemplateNodeId));
			AssertNotNull(target.TemplateNodeId);
			AssertNotNull(target.InnerNode);
		}

		protected override void AssertDataAfterResetAndUpdateId(TemplateCopyTemplateNode target)
		{
			base.AssertDataAfterResetAndUpdateId(target);
			Assert(!string.IsNullOrEmpty(target.TemplateNodeId));
			AssertNotNull(target.TemplateNodeId);
			AssertNull(target.InnerNode);
		}

		#endregion

		#region Implementation

		protected override TemplateCopyTemplateNode CreateConfigurationNode()
		{
			EntityCopyTemplateNode entityNode = new EntityCopyTemplateNode(typeof(SimpleElement));
			TemplateCopyTemplateNode templateNode = new TemplateCopyTemplateNode(entityNode);
			templateNode.FindTemplateAndInitializeInnerNode(entityNode);
			return templateNode;
		}

		protected override string ExpectedName
		{
			get { return "SimpleElement"; }
		}

		#endregion
	}
}
