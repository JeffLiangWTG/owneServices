using CargoWise.EntityFramework;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.EMCS.Module
{
	public class Module : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.EMCS;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.EMCS;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EuCustomsEmcs;
		public override bool AllowView => true;
		public override bool AllowNew => true;
		public override bool AllowEdit => true;
		public override bool AllowDelete => true;
		public override bool AllowUniversalCopy => true;
		public override bool SupportsWorkflow => true;
		public override string WorkflowType => WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode;
		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new Controller();
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new FilterStripBusinessObject();
		protected override IBusinessObjectCollection GetNewGridCollection() => new EMCSJobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
		protected override IFilterControl GetNewFilterControl() => new DeclarationFilterStripControl(GridCollection, FilterBusinessObject);
	}
}
