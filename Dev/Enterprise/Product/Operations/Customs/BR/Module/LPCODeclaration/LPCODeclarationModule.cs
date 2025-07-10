using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Module
{
	public class LPCODeclarationModule : ZFilterGridModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.BR.LPCODeclaration);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new LPCODeclarationFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new LPCODeclarationFilterStripControl(GridCollection, (LPCODeclarationFilterStripBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

		public override ModuleIdentifier ID => ModuleIDs.Customs.BR.LPCODeclaration;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.BRLPCODeclaration;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode;
	}
}
