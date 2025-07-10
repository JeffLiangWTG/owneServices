using System.Collections.Generic;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.GUI
{
	class PreRequisitesMenuItemStrategy : DependencyMenuItemStrategy
	{
		internal override RelationshipDirection OtherWorkflowLinkPosition => RelationshipDirection.From;

		internal override string LinkType => ProcessHeaderLinkTypeList.Codes.Dependency;

		internal override string RootMenuItemText => ResString.GetMultilingualString("0bd1cc57-1588-470b-b37f-ef51f64856c6", "Pre-requisite Workflows");

		internal override string OpenItemMenuItemText => ResString.GetMultilingualString("26a0f35b-0edd-476f-9a52-1d6c468e12fd", "Open Pre-requisite Item");

		internal override string EditItemMenuItemText => EditDependencyLinkText;

		internal override string IndirectRelationshipMenuItemText => ResString.GetMultilingualString("b096e832-6a40-496c-9fb2-8afb75e5eb61", "indirect pre-requisite");

		internal override string NoDependenciesMenuItemText => ResString.GetMultilingualString("4d193392-826e-459f-8479-f06298d9900f", "There are no pre-requisites");

		internal override IEnumerable<LinkedProcessHeader> GetFullNetworkDependencies(ProcessHeader startingPoint)
		{
			return startingPoint.GetPrerequisitesAndTheirLinksUpTheTree(getApplicableDependenciesOnly: true);
		}
	}
}
