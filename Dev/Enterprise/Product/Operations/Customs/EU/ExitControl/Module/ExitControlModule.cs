using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Module
{
	public class ExitControlModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.ExitControl;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EuExitControl;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.EU.ExitControl);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusExitHeaderCollection<CusExitHeader>(Factory, new ZQuery(CusExitHeaderSchema.CXH_GC_Company, GlbCompany.CurrentCompany.PK));

		protected override IFilterControl GetNewFilterControl() => new ExitControlFilterStripControl(GridCollection, (ExitControlFilterBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ExitControlFilterBusinessObject();

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CusExitHeaderWorkflowDescriptorCode;
	}
}
