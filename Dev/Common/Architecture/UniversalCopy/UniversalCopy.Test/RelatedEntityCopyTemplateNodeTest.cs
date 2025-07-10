namespace CargoWise.UniversalCopy.Test
{
	public class RelatedEntityCopyTemplateNodeTest : WrappedCopyTemplateNodeTest<RelatedEntityCopyTemplateNode>
	{
		public void TestRelatedNodeProperties()
		{
			var target = CreateConfigurationNode();

			AssertEquals("PropertyName", target.Name);
			AssertEquals("RelatedPropertyName", target.RelatedPropertyName);
			AssertEquals("TableName", target.RelatedEntityTableName);
		}

		public void TestHasData()
		{
			var target = CreateConfigurationNode();
			Assert(!CheckHasData(target));

			target.CopyMethod = RelatedEntityCopyMethod.Link;
			Assert(CheckHasData(target));
		}

		#region TestResetAndUpdateId

		protected override void PrepareDataForResetAndUpdateId(RelatedEntityCopyTemplateNode target)
		{
			base.PrepareDataForResetAndUpdateId(target);
			target.CopyMethod = RelatedEntityCopyMethod.Copy;
		}

		protected override void AssertDataAfterResetAndUpdateId(RelatedEntityCopyTemplateNode target)
		{
			base.AssertDataAfterResetAndUpdateId(target);
			AssertEquals(default(RelatedEntityCopyMethod), target.CopyMethod);
		}

		#endregion

		#region TestCanCopyAndLink

		public void TestCanCopy()
		{
			var node = new RelatedEntityCopyTemplateNode();
			Assert("No inner node - allow copy", node.CanCopy);

			node.InnerNode = new EntityCopyTemplateNode();
			Assert("Zero configuration nodes - don't copy", !node.CanCopy);

			node.CanCopyWithZeroNodes = true;
			Assert("Can copy with zero nodes", node.CanCopy);

			node.DisableCopyMethodCopy = true;
			Assert("Copy is disabled", !node.CanCopy);
		}

		public void TestCanLink()
		{
			var node = new RelatedEntityCopyTemplateNode();
			Assert("Can link by default", node.CanLink);

			node.DisableCopyMethodLink = true;
			Assert("Link is disabled", !node.CanLink);
		}

		#endregion

		#region Implementation

		protected override RelatedEntityCopyTemplateNode CreateConfigurationNode()
		{
			return new RelatedEntityCopyTemplateNode(ExpectedName, "RelatedPropertyName", "TableName", new CopyTemplateTree(typeof(SimpleElement)).InnerNode);
		}

		protected override string ExpectedName
		{
			get { return "PropertyName"; }
		}

		#endregion
	}
}
