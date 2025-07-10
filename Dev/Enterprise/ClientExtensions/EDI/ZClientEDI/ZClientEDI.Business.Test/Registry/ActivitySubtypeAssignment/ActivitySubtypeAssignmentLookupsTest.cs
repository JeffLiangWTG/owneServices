using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	internal sealed class ActivitySubtypeAssignmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestActivitySubtypeList()
		{
			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 4);
			tree.AddSystemChildren();
			const string ALL = CodeDescriptionBoolTreeNode.AllCode;
			var parent = tree.Find(ALL, ALL, ALL);
			tree.Add("AAA", (NoResString)"Description for AAA", true, false, parent);
			tree.Add("BBB", (NoResString)"Description for BBB", true, false, parent);
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var assignment = new ActivitySubtypeAssignment();
			AssertEquals("Item Count correct", 2, assignment.Lookups.ActivitySubtypeList.Count);
			AssertEquals("Items should contain BBB", true, assignment.Lookups.ActivitySubtypeList.ContainsCode("BBB"));
		}
	}
}
