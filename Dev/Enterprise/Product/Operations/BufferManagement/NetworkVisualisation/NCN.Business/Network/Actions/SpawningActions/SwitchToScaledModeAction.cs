using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class SwitchToScaledModeAction : SpawningIndependentNetworkAction
	{
		public SwitchToScaledModeAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckEntityIsRoot(entity));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape) => BMNetworkActionAccessibilityHelper.CheckDiagramIsNotScaled(Network.DiagramEntity, shape);

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			shape.RequireDiagram();

			var factoryForSpawnedNetwork = CreateNewFactoryForSpawnedNetwork();
			var clone = Network.DiagramEntity.CloneDiagramAndAllDescendants(factoryForClone: factoryForSpawnedNetwork);
			clone.BNS_Name += " " + Res.GetString("7099c317-1c47-45fe-a038-81da91f8f4a9", "(scaled copy)");
			var cloneNetwork = CreateSpawnedTemporaryNetwork(clone);
			cloneNetwork.SwitchToScaled();

			var attachment = clone.Factory.New<BMNCNAttachment>();
			attachment.BNA_Type = AttachmentTypeList.Codes.SwitchToScaled;
			attachment.BNA_BNS_Owner = shape.PK;
			attachment.BNA_BNS_FromShape = shape.PK;
			attachment.BNA_BNS_ToShape = clone.PK;

			Controller.ViewDiagram(clone);
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("4c28e39a-32e0-4955-ba66-55cad88d76f2", "Switch to Scaled Mode");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("9d5b917d-e147-4bfa-88c5-87ba07c5cc1d", "Clones a scaled-mode version of the current diagram. In scaled mode, entities have a Planned Duration relative to their size, and a starting time relative to their position from the left side.");
		}

		protected override string UserConfirmationMessage
		{
			get
			{
				return Res.GetString("ffa06a68-82fb-4640-85bd-4451c4df6a20", "Are you sure you want to create a new copy of this diagram in scaled mode?");
			}
		}

		protected override string IconName => (NoResString)"Scale"; // resource name

		protected override bool RequiresUserConfirmation => true;

		protected override bool CanPerformOnApprovedShape => true;

		protected override bool RequiresSaveBeforeExecute => true;
	}
}
