using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Module
{
	public class ForeignOperatorModule : ZFilterGridModule
	{
		public sealed override ModuleIdentifier ID => ModuleIDs.Customs.BR.ForeignOperator;

		public sealed override SecurityCheckpoint SecurityCheckpoint => Env.Security.BRForeignOperator;

		protected sealed override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected sealed override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.BR.ForeignOperator);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ForeignOperatorFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new ForeignOperatorFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusBRForeignOperatorCollection(Factory);

		public override string WorkflowType => WorkflowDescriptors.ForeignOperatorWorkflowDescriptorCode;

		public override bool SupportsWorkflow => true;
	}
}
