using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.H7.Module
{
	public class EUH7Module : ZFilterGridModule, IOperationalActionSupportable
	{
		public EUH7Module() : base()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.EUH7;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EuH7;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.EU.EUH7);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EUH7FilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new EUH7FilterStripControl(GridCollection, (EUH7FilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new AsycudaManifestModuleCollection(Factory);

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.EUH7AsycudaManifestHeaderWorkflowDescriptorCode;

		public OperationalActionSupporter OperationalActionSupporter => new EUH7OperationalActionSupporter();

		protected override bool CanBeCopied() => typeof(ITemplateCopyable).IsAssignableFrom(typeof(AsycudaManifestHeader));
	}
}
