using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CreateJobActionCollection : JobNetworkAction
	{
		public CreateJobActionCollection(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckShapeHasJobTypes(shape);
		}

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckShapeCanHaveChildren(shape));
		}

		protected override IEnumerable<INetworkAction> GetChildActionsCore(BMNCNShape shape)
		{
			return createJobActions ?? (createJobActions = GetCreateJobActions());
		}

		IEnumerable<INetworkAction> createJobActions;

		IEnumerable<INetworkAction> GetCreateJobActions()
		{
			yield return new CreateWorkItemAction(NetworkViewModel);
			yield return new CreateProjectAction(NetworkViewModel);
			yield return new CreateJobAction(NetworkViewModel);
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("d71accfd-f30b-4902-98ee-32209fbda583", "Creates and opens a new job");
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("e8c3466e-fc2f-4cf9-998d-9e4f0496f081", "Create Job");
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
		}

		protected override string IconName => (NoResString)"Job";
	}
}
