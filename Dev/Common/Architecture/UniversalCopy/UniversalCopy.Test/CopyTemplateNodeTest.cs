using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.UniversalCopy.Test
{
	public abstract class CopyTemplateNodeTest<T> : TestCase
		where T : CopyTemplateNode
	{
		public void TestNodeProperties()
		{
			var target = CreateConfigurationNode();

			Assert("Id should not be empty", !string.IsNullOrWhiteSpace(target.Id));
			AssertEquals(ExpectedName, target.Name);

			target.Id = "New Id";
			target.Name = "New Name";

			AssertEquals("New Id", target.Id);
			AssertEquals("New Name", target.Name);
		}

		public void TestFindNode()
		{
			var target = CreateConfigurationNode();

			AssertSame(target, target.FindNode(target.Id));
			AssertNull(target.FindNode(Guid.NewGuid().ToString()));

			foreach (KeyValuePair<string, CopyTemplateNode> pair in GetExpectedFindNodePairs(target))
			{
				AssertSame(pair.Value, target.FindNode(pair.Key));
			}
		}

		#region TestResetAndUpdateId

		public void TestResetAndUpdateId()
		{
			var target = CreateConfigurationNode();
			PrepareDataForResetAndUpdateId(target);
			AssertEquals("Some Id", target.Id);
			AssertEquals("Some Name", target.Name);

			InvokeResetAndUpdateId(target);

			wasBaseCalled = false;
			AssertDataAfterResetAndUpdateId(target);
			Assert("Overridden method AssertDataAfterResetAndUpdateId should call base", wasBaseCalled);
		}

		protected virtual void PrepareDataForResetAndUpdateId(T target)
		{
			PrepareDataForResetAndUpdateIdBase(target);
		}

		protected void PrepareDataForResetAndUpdateIdBase(CopyTemplateNode target)
		{
			target.Id = "Some Id";
			target.Name = "Some Name";
		}

		protected virtual void AssertDataAfterResetAndUpdateId(T target)
		{
			AssertDataAfterResetAndUpdateIdBase(target);
			wasBaseCalled = true;
		}

		protected void AssertDataAfterResetAndUpdateIdBase(CopyTemplateNode target)
		{
			AssertNotEquals("Id should be changed", "Some Id", target.Id);
			Assert("Id should not be empty", !string.IsNullOrWhiteSpace(target.Id));
			AssertEquals("Name should remain", "Some Name", target.Name);
		}

		bool wasBaseCalled;

		#endregion

		#region Implementation

		protected abstract T CreateConfigurationNode();

		protected virtual IEnumerable<KeyValuePair<string, CopyTemplateNode>> GetExpectedFindNodePairs(CopyTemplateNode root)
		{
			return Enumerable.Empty<KeyValuePair<string, CopyTemplateNode>>();
		}

		protected abstract string ExpectedName { get; }

		protected void InvokeResetAndUpdateId(CopyTemplateNode target)
		{
			target.GetType().GetMethod("ResetAndUpdateId", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, Array.Empty<object>());
		}

		protected bool CheckHasData(CopyTemplateNode target)
		{
			return (bool)target.GetType().GetMethod("HasData", BindingFlags.Instance | BindingFlags.Public).Invoke(target, Array.Empty<object>());
		}

		protected TNode FindCopyTemplateNodeByName<TNode>(CopyTemplateNode parentNode, string name)
			where TNode : CopyTemplateNode
		{
			if (parentNode != null && parentNode.Name == name)
			{
				return parentNode as TNode;
			}

			EntityCopyTemplateNode entityNode = parentNode as EntityCopyTemplateNode;
			if (entityNode != null)
			{
				return entityNode.Nodes.FirstOrDefault(node => node.Name == name) as TNode;
			}

			WrappedCopyTemplateNode wrappedNode = parentNode as WrappedCopyTemplateNode;
			if (wrappedNode != null)
			{
				return FindCopyTemplateNodeByName<TNode>(wrappedNode, name);
			}

			return null;
		}

		#endregion
	}
}
