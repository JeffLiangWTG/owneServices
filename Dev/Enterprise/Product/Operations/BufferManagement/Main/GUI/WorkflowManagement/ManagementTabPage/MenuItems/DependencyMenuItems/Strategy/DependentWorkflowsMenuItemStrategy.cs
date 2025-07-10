using System.Collections.Generic;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.GUI
{
	class DependentWorkflowsMenuItemStrategy : DependencyMenuItemStrategy
	{
		internal override RelationshipDirection OtherWorkflowLinkPosition => RelationshipDirection.To;

		internal override string LinkType => ProcessHeaderLinkTypeList.Codes.Dependency;

		internal override string RootMenuItemText => ResString.GetMultilingualString("5fbd688f-5ba8-4317-a0af-4f479f798b6f", "Dependent Workflows");

		internal override string OpenItemMenuItemText => ResString.GetMultilingualString("3e7e0ec5-f8b8-4f12-b334-dcee0b580d8d", "Open Dependent Item");

		internal override string EditItemMenuItemText => EditDependencyLinkText;

		internal override string IndirectRelationshipMenuItemText => ResString.GetMultilingualString("953acccd-093d-4e07-82df-0caef84596b5", "indirect dependency");

		internal override string NoDependenciesMenuItemText => ResString.GetMultilingualString("fc1bc5ec-4ec1-4e6e-9419-4fb4cf06fd75", "There are no dependent items");

		internal override IEnumerable<LinkedProcessHeader> GetFullNetworkDependencies(ProcessHeader startingPoint)
		{
			return startingPoint.GetPostRequisitesAndTheirLinksDownTheTree();
		}
	}
}
