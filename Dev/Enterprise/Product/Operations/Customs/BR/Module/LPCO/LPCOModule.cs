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
	public class LPCOModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.BR.LPCO;

		public override bool AllowNew => true;

		public override bool AllowEdit => true;

		public override bool AllowDelete => true;

		public override bool AllowView => true;

		public override bool AllowUniversalCopy => false;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.BRLPCO;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CusBRLPCOHeaderWorkflowDescriptorCode;

		protected override bool ShouldLoadFilterBusinessObjectDefaults => true;

		protected override IFilterControl GetNewFilterControl() => new LPCOFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusLPCOHeaderCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new LPCOFilterStripBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.BR.LPCO);
	}
}
