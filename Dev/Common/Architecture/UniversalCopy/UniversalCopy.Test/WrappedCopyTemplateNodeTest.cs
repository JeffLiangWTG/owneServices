using System.Collections.Generic;

namespace CargoWise.UniversalCopy.Test
{
	public abstract class WrappedCopyTemplateNodeTest<T> : CopyTemplateNodeTest<T>
		where T : WrappedCopyTemplateNode
	{
		#region TestResetAndUpdateId

		protected override void PrepareDataForResetAndUpdateId(T target)
		{
			AssertNotNull(target.InnerNode);
			base.PrepareDataForResetAndUpdateId(target);
			PrepareDataForResetAndUpdateIdBase(target.InnerNode);
		}

		protected override void AssertDataAfterResetAndUpdateId(T target)
		{
			base.AssertDataAfterResetAndUpdateId(target);
			if (target.InnerNode != null)
			{
				AssertDataAfterResetAndUpdateIdBase(target.InnerNode);
			}
		}

		#endregion

		#region Implementation

		protected override IEnumerable<KeyValuePair<string, CopyTemplateNode>> GetExpectedFindNodePairs(CopyTemplateNode root)
		{
			CopyTemplateNode innerNode = ((WrappedCopyTemplateNode)root).InnerNode;
			AssertNotNull(innerNode);
			return new[] { new KeyValuePair<string, CopyTemplateNode>(innerNode.Id, innerNode) };
		}

		#endregion
	}
}
