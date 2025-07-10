using System;
using System.Collections.Generic;
using System.Linq;
using WTG.Glow.Data.Annotations;

namespace CargoWise.UniversalCopy.Test
{
	public class EntityCopyTemplateNodeTest : CopyTemplateNodeTest<EntityCopyTemplateNode>
	{
		public void TestHasData()
		{
			var target = CreateConfigurationNode();
			Assert(!CheckHasData(target));

			FindCopyTemplateNodeByName<PropertyCopyTemplateNode>(target, "X").CopyMethod = CopyMethod.Copy;
			Assert(CheckHasData(target));
		}

		#region TestResetAndUpdateId

		protected override void PrepareDataForResetAndUpdateId(EntityCopyTemplateNode target)
		{
			base.PrepareDataForResetAndUpdateId(target);
			FindCopyTemplateNodeByName<PropertyCopyTemplateNode>(target, "X").CopyMethod = CopyMethod.Copy;
			FindCopyTemplateNodeByName<RelatedEntityCopyTemplateNode>(target, "B").CopyMethod = RelatedEntityCopyMethod.Link;
		}

		protected override void AssertDataAfterResetAndUpdateId(EntityCopyTemplateNode target)
		{
			base.AssertDataAfterResetAndUpdateId(target);
			AssertEquals(default(CopyMethod), FindCopyTemplateNodeByName<PropertyCopyTemplateNode>(target, "X").CopyMethod);
			AssertEquals(default(RelatedEntityCopyMethod), FindCopyTemplateNodeByName<RelatedEntityCopyTemplateNode>(target, "B").CopyMethod);
		}

		#endregion

		#region Overridden methods

		protected override EntityCopyTemplateNode CreateConfigurationNode()
		{
			EntityCopyTemplateNode entity = (EntityCopyTemplateNode)new CopyTemplateTree(typeof(ClassA)).InnerNode;
			Assert(entity.Nodes.Count > 0);
			return entity;
		}

		protected override string ExpectedName
		{
			get { return "ClassA"; }
		}

		protected override IEnumerable<KeyValuePair<string, CopyTemplateNode>> GetExpectedFindNodePairs(CopyTemplateNode root)
		{
			yield return new KeyValuePair<string, CopyTemplateNode>(root.Id, root);

			EntityCopyTemplateNode entity = root as EntityCopyTemplateNode;
			if (entity != null)
			{
				foreach (var pair in entity.Nodes.SelectMany(GetExpectedFindNodePairs))
				{
					yield return pair;
				}
			}
		}

		#endregion

		#region Model definition

		[TableNameProvider("TableA")]
		public interface ClassA
		{
			string X { get; set; }

			Guid C2 { get; set; }

			[RelationProperty(nameof(C2))]
			ClassB B { get; }
		}

		[TableNameProvider("TableB")]
		public interface ClassB
		{
			string Y { get; set; }
		}

		#endregion
	}
}
