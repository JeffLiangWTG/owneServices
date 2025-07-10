using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public abstract class CreateWorkflowActionBase : CreateShapeActionBase
	{
		protected CreateWorkflowActionBase(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("05cccbda-756d-48fe-8d2a-2bdf5995bd28", "Workflow");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("a9c5f245-aab0-45e5-8c33-2076b8191c32", "Creates a new workflow for this job");
		}

		protected override ProcessHeader GetNewProcessHeaderForShape(IJobNetwork network, BMNCNShape newShape, BMNCNShape parentShape)
		{
			var processHeader = parentShape.ProcessHeader;
			var jobHeader = processHeader.IsWorkflow ? processHeader.JobHeader : (ProcessJobHeader)processHeader;

			return jobHeader.ProcessHeaders.AddNew();
		}

		protected override string GetDefaultName(IJobNetwork network, BMNCNShape shape)
		{
			return Res.GetString("ebe7d2c9-f598-42b2-827e-19df8f2ce613", "New Workflow");
		}

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return base.IsApplicableCore(shape).Union(BMNetworkActionAccessibilityHelper.CheckShapeIsLinkedToRealEntity(shape));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckShapeIsLinkedToRealEntityEnabledForBMS(shape);
		}
	}
}
