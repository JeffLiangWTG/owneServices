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
	public class UCC6TemporaryStorageModule : ZFilterGridModule
	{
		public UCC6TemporaryStorageModule()
		{
		}

		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.UCC6TemporaryStorage;

		public override bool AllowNew => true;

		public override bool AllowEdit => true;

		public override bool AllowDelete => false;

		public override bool AllowView => false;

		public override bool AllowUniversalCopy => false;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsTemporaryStorage;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => new TemporaryStorageHeaderWorkflowDescriptor().Code;

		protected override IFilterControl GetNewFilterControl() => new UCC6TemporaryStorageFilterControl(GridCollection, FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new UCC6TemporaryStorageFilterStripBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.EU.UCC6TemporaryStorage);

		protected override IBusinessObjectCollection GetNewGridCollection() => new TemporaryStorageHeaderCollection(Factory);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;
	}
}
