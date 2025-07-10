using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ToggleResourceDependencyVisibilityAction : JobNetworkAction
	{
		public ToggleResourceDependencyVisibilityAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Overrides

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckEntityIsRoot(entity));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return new NetworkActionAccessibility(
				GetResourceDependencyAttachments().Any(),
				shape,
				() => Res.GetString("D929ADBF-72DE-495A-8CEB-13B4626FB368", "The network should have resource dependencies."));
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			var dependencyAttachments = GetResourceDependencyAttachments();
			var requiredIsHiddenValue = HasVisibleDependenciesOrNoDependencies(dependencyAttachments);

			foreach (var dependency in dependencyAttachments)
			{
				dependency.BNA_IsHidden = requiredIsHiddenValue;
			}

			Network.Refresh(RefreshType.RedrawDiagram);
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("040D0E64-0ACA-41F5-809C-0386B726FF8E", "Show Resource Dependencies");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("f66ae32b-0682-427d-b5cd-f67bffeb6168", "Toggle the visibility of all resource dependencies in this diagram.");
		}

		protected override string IconName => (NoResString)"Link"; // resource name

		protected override bool IsActivatedCore(BMNCNShape shape) => HasVisibleDependenciesOrNoDependencies(GetResourceDependencyAttachments());

		#endregion

		#region Implementation

		bool HasVisibleDependenciesOrNoDependencies(IEnumerable<BMNCNAttachment> dependencyAttachments)
		{
			return !dependencyAttachments.Any() || dependencyAttachments.Any(d => !d.BNA_IsHidden);
		}

		IEnumerable<BMNCNAttachment> GetResourceDependencyAttachments()
		{
			BMNCNShape.AddDependencyAttachmentsFetchHints(Network.Shapes);
			return Network.Shapes.SelectMany(s => s.DependencyAttachments.Where(a => a.IsResourceDependency)).ToArray();
		}

		#endregion
	}
}
