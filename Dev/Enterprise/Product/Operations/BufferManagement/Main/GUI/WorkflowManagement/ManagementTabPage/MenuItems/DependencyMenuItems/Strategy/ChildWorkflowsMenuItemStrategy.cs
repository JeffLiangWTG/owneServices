using System.Collections.Generic;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.GUI
{
	class ChildWorkflowsMenuItemStrategy : DependencyMenuItemStrategy
	{
		internal override RelationshipDirection OtherWorkflowLinkPosition => RelationshipDirection.From;

		internal override string LinkType => ProcessHeaderLinkTypeList.Codes.ParentChild;

		internal override string RootMenuItemText => ResString.GetMultilingualString("b3b96319-cd29-4136-9dfe-494a653d3def", "Child Workflows");

		internal override string OpenItemMenuItemText => ResString.GetMultilingualString("4e9dbfc1-39f2-4f74-8759-c65354c84902", "Open Child Item");

		internal override string EditItemMenuItemText => EditParentChildLinkText;

		internal override string IndirectRelationshipMenuItemText => ResString.GetMultilingualString("8af583fa-02a3-4127-92fb-a830b5a85597", "indirect child");

		internal override string NoDependenciesMenuItemText => ResString.GetMultilingualString("102cb5aa-061b-46f4-9b58-794fd21b0eec", "There are no children");

		internal override IEnumerable<LinkedProcessHeader> GetFullNetworkDependencies(ProcessHeader startingPoint)
		{
			return startingPoint.GetChildrenDownTheHierarchy();
		}
	}
}
