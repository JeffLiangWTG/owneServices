using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ValidateWorkflowLoopsAction : JobNetworkAction
	{
		public ValidateWorkflowLoopsAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("E295E5B0-784B-4B30-B903-4E1174649550", "Validate Workflow Loops");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("9D682ED1-ABEC-4F38-BAE8-C819E7E693E9", "Performs validation on linked workflows to determine if there are any loops");

		protected override string IconName => (NoResString)"Approve"; // resource name

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape) =>
			BMNetworkActionAccessibilityHelper.CheckEntityIsRoot(Network.Entities.GetInstance(shape));

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			new ProcessHeaderLoopsChecker().CheckLoops(Network.Controller.UserInteractionImplementor.ProgressReporterProvider,
				Network.Entities.ShapeEntities.Where(e => e.ProcessHeader != null).Select(p => p.ProcessHeader).ToArray());
		}
	}
}
