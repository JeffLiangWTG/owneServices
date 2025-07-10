using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ConvertToWorkflowsAction : JobNetworkAction
	{
		public ConvertToWorkflowsAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Overrides

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckShapeCanHaveChildren(shape));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			var result = false;

			var entity = Network.Entities.GetInstance(shape);
			var jobHeader = !IsJobLevelWorkflowShape(shape)
				? entity.Owner != null ? entity.Owner.ProcessHeader?.JobHeader : null
				: shape.ProcessHeader as ProcessJobHeader;

			if (jobHeader != null && !jobHeader.FH_ParentId.IsEmpty)
			{
				if (!IsJobLevelWorkflowShape(shape))
				{
					var processHeader = shape.ProcessHeader;
					result = processHeader == null || processHeader.FH_ParentId.IsEmpty;
				}
				else
				{
					var workflowShapes = shape.ChildShapes.Where(s => !IsJobLevelWorkflowShape(s));
					result = workflowShapes.Any(s => s.ProcessHeader == null || s.ProcessHeader.FH_ParentId.IsEmpty);
				}
			}

			return new NetworkActionAccessibility(result,
				() => new NetworkActionDenialReason(shape,
					Res.GetString("2A6E03DB-3257-499B-A62F-1E8E441E101F", "The shape should be either linked to a job and have non-linked child shapes or be not linked itself and have a parent linked to a job or workflow."),
					needsNotification: false));
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			if (!IsJobLevelWorkflowShape(shape))
			{
				EnsureShapeHasWorkflowOnJob(Network, shape);
			}
			else
			{
				foreach (var childShape in shape.ChildShapes.Where(s => s.ProcessHeader == null && s.BNS_ShapeType == ShapeTypeList.Codes.Shape))
				{
					EnsureShapeHasWorkflowOnJob(Network, childShape);
				}
			}
		}

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("E2344FCE-D5A8-418C-93BF-C1B4166C29B5", "Convert Shapes to Workflows");

		protected override ResourceString GetNameCore(BMNCNShape shape)
		{
			return !IsJobLevelWorkflowShape(shape)
				? ResString.GetMultilingualString("8374ad77-382f-4b36-9cee-fed05b8f5663", "Convert Shape to Workflow")
				: ResString.GetMultilingualString("0e75e7f2-73cf-46f9-a8d1-8bcf89d08a96", "Convert Shapes to Workflows");
		}

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("FD1E2D81-52CD-4BC0-8ED1-C91D12354523", "Converts shapes into workflows on the linked job");

		protected override ResourceString GetDescriptionCore(BMNCNShape shape)
		{
			return !IsJobLevelWorkflowShape(shape)
				? ResString.GetMultilingualString("e68be2b4-4018-43c6-a695-30fb56a6f7d0", "Converts this shape into a workflow on the linked job")
				: ResString.GetMultilingualString("237fab00-8f1d-4477-890e-b848936dfcd5", "Converts all leaf shapes within the selected diagram into workflows on the linked job");
		}

		protected override string IconName => (NoResString)"Workflow"; // resource name

		#endregion

		#region Implementation

		bool IsJobLevelWorkflowShape(BMNCNShape shape) => shape.ProcessJobHeader != null;

		static void EnsureShapeHasWorkflowOnJob(IJobNetwork network, BMNCNShape shape)
		{
			var entity = network.Entities.GetInstance(shape);

			var completionCriteria = shape.BNS_CompletionStatements;
			var processHeader = shape.ProcessHeader;
			var jobHeader = entity.Owner.ProcessHeader.JobHeader;

			if (processHeader == null || processHeader.FH_ParentId != jobHeader.FH_ParentId)
			{
				processHeader = jobHeader.ProcessHeaders.AddNew();
				processHeader.FH_CompletionStatement = shape.BNS_Name.SubstringSafe(0, ProcessHeaderSchema.FH_CompletionStatement.MaxLength);

				network.LinkEntity(shape, processHeader);
			}

			((IProposedNetworkEntity)processHeader).CompletionCriteria = completionCriteria;
		}

		#endregion
	}
}
