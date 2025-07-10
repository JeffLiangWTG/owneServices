using System.Globalization;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ApproveNonApprovedShapesAction : ApproveDiagramActionBase
	{
		public ApproveNonApprovedShapesAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckDiagramIsScaled(Network.DiagramEntity, shape)
					.Union(BMNetworkActionAccessibilityHelper.CheckEntityIsRoot(entity))
					.Union(BMNetworkActionAccessibilityHelper.CheckShapeIsApproved(shape)));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return new NetworkActionAccessibility(GetItemsRequiringApproval(Network.Entities.GetInstance(shape)).Any(),
				shape,
				() => Res.GetString("31CB7DAE-17BA-4A31-9ACC-BDD7CA488965", "The diagram should contain unapproved shapes."));
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			var itemsRequiringApproval = GetItemsRequiringApproval(Network.Entities.GetInstance(shape)).Distinct().ToArray();
			if (itemsRequiringApproval.Length > 0)
			{
				var resourceCode = GlbStaff.CurrentUser.GS_Code;

				foreach (var nonApprovedShape in itemsRequiringApproval)
				{
					nonApprovedShape.Approve(resourceCode);
				}

				foreach (var buffer in Network.Shapes.OfType<BMNCNBufferShape>())
				{
					buffer.EnsureRelatedBuffersSet();
				}

				Network.Refresher.Refresh(RefreshType.EntityApprovedOrUnapproved, shape);

				var logReference = string.Format(CultureInfo.InvariantCulture, (NoResString)"Additional shapes approved by [{0}]", GlbStaff.CurrentUser.GS_FullName); // Log reference should not be translated
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				shape.Logs.AddNew(AutoEvents.EditedARecord, logReference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("94d90ea0-b530-4f27-b528-3f05477cb058", "Approve Non-approved Shapes and Arrows");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("cb52a39a-74af-4c1a-994d-6df88d2ef1c5", "Approves shapes and arrows that have been added since the diagram was last approved");
		}

		protected override string IconName => (NoResString)"Approve"; // resource name
	}
}
