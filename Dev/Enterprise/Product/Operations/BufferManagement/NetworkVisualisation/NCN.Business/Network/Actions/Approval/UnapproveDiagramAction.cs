using System.Globalization;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class UnapproveDiagramAction : ApproveDiagramActionBase
	{
		public UnapproveDiagramAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region JobNetworkAction

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckDiagramIsScaled(Network.DiagramEntity, shape)
					.Union(BMNetworkActionAccessibilityHelper.CheckShapeIsApproved(shape))
					.Union(new NetworkActionAccessibility(entity.IsRoot || !shape.HasApprovedParent,
						shape,
						() => Res.GetString("DE75F650-2DA9-4CBF-AE09-460DC87CFF0E", "The shape should either be the root diagram or have no approved parent."))));
		}

		protected override INetworkActionAccessibility PerformPreExecutionChecksForShape(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckShapeIsApproved(shape)
				.UnionIfAllowed(() => new NetworkActionAccessibility(!shape.HasApprovedParent,
					shape,
					() => Res.GetString("58061885-b216-479e-b9df-a95f988809ba", "Shapes with approved parents cannot be un-approved. This shape is approved on [{0}].", shape.FindApprovedRoot().Name)));
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			var resourceCode = string.Empty;
			var logReference = string.Format(CultureInfo.InvariantCulture, (NoResString)"Diagram un-approved by [{0}]", GlbStaff.CurrentUser.GS_FullName); // Log reference should not be translated

			shape.ToggleApproval(resourceCode);

			foreach (var item in GetItemsRequiringApproval(Network.Entities.GetInstance(shape), expectedApprovalState: !shape.IsApproved))
			{
				item.ToggleApproval(resourceCode);
			}

			Network.Refresher.Refresh(RefreshType.EntityApprovedOrUnapproved, shape);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			shape.Logs.AddNew(AutoEvents.EditedARecord, logReference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("97c841de-b89d-4cd4-9fd0-9425505980b5", "Un-approve Diagram");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("eac2f1d4-a95f-424b-b593-4a7c689e0dd0", "Marks this diagram as un-approved");
		}

		protected override string IconName => (NoResString)"Unapprove"; // resource name

		#endregion
	}
}
