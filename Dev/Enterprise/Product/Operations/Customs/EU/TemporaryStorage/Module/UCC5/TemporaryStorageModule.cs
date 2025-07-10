using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public abstract class TemporaryStorageModule : ZFilterGridModule
	{
		protected TemporaryStorageModule()
		{
		}

		public override ModuleIdentifier ID => ModuleIDs.Customs.TemporaryStorage;

		public override bool AllowNew => true;

		public override bool AllowEdit => true;

		public override bool AllowDelete => false;

		public override bool AllowView => false;

		public override bool AllowUniversalCopy => false;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsTemporaryStorage;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => new CusTempStorageJobHeaderWorkflowDescriptor().Code;

		protected override IFilterControl GetNewFilterControl() => new TemporaryStorageFilterControl(GridCollection, FilterBusinessObject);

		protected abstract override IBusinessObjectCollection GetNewGridCollection();

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TemporaryStorageFilterStripBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.TemporaryStorage);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;
	}
}
