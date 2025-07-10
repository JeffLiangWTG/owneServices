using System.Collections.Generic;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.GUI
{
	class ParentWorkflowsMenuItemStrategy : DependencyMenuItemStrategy
	{
		internal override RelationshipDirection OtherWorkflowLinkPosition => RelationshipDirection.To;

		internal override string LinkType => ProcessHeaderLinkTypeList.Codes.ParentChild;

		internal override string RootMenuItemText => ResString.GetMultilingualString("94a7ac78-c46a-4aa9-9407-2f997a194602", "Parent Workflows");

		internal override string OpenItemMenuItemText => ResString.GetMultilingualString("5cd5b5c3-8372-460b-a060-8d2fc5dc0f96", "Open Parent Item");

		internal override string EditItemMenuItemText => EditParentChildLinkText;

		internal override string IndirectRelationshipMenuItemText => ResString.GetMultilingualString("d547b6d8-334d-43ca-974c-e64c5d76cbd8", "indirect parent");

		internal override string NoDependenciesMenuItemText => ResString.GetMultilingualString("f3e1893f-40de-42a7-acf6-d1b5bd922edc", "There are no parents");

		internal override IEnumerable<LinkedProcessHeader> GetFullNetworkDependencies(ProcessHeader startingPoint)
		{
			return startingPoint.GetParentsUpTheHierarchy();
		}
	}
}
