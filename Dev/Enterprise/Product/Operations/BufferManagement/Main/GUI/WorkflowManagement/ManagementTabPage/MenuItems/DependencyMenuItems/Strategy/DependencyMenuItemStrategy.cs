using System.Collections.Generic;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI
{
	abstract class DependencyMenuItemStrategy
	{
		internal abstract string RootMenuItemText { get; }
		internal abstract string OpenItemMenuItemText { get; }
		internal abstract string EditItemMenuItemText { get; }
		internal abstract string IndirectRelationshipMenuItemText { get; }
		internal abstract string NoDependenciesMenuItemText { get; }
		internal abstract RelationshipDirection OtherWorkflowLinkPosition { get; }
		internal abstract string LinkType { get; }
		internal abstract IEnumerable<LinkedProcessHeader> GetFullNetworkDependencies(ProcessHeader startingPoint);

		internal bool SaveAfterActions { get; set; }
		internal bool RefreshOpenPrerequisitesAfterEdits { get; set; }

		internal static string EditDependencyLinkText => Res.GetString("488ffa20-3ca8-42c9-a121-f7dcf1a8e45b", "Edit Dependency Link");
		internal static string EditParentChildLinkText => Res.GetString("5c53d9e8-0cb2-4fe2-98d1-8bf236373ceb", "Edit Parent-Child Link");
	}
}
