using CargoWise.EntityFramework;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyDependentWithCodeModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => DummyModuleIDs.DummyDependentWithCode;

		public override SecurityCheckpoint SecurityCheckpoint => DummyCheckPointWithSecuritySet;

		public new DummyDependentWithCodeController LastController;

		bool fAllowEdit = true;
		public override bool AllowEdit
		{
			get { return fAllowEdit; }
		}
		public void SetAllowEdit(bool value)
		{
			fAllowEdit = value;
		}

		bool fAllowView = true;
		public override bool AllowView
		{
			get { return fAllowView; }
		}
		public void SetAllowView(bool value)
		{
			fAllowView = value;
		}

		DummyCheckPointWithSecuritySet DummyCheckPointWithSecuritySet
		{
			get
			{
				if (fDummyCheckPointWithSecuritySet == null)
				{
					fDummyCheckPointWithSecuritySet = new DummyCheckPointWithSecuritySet(true);
				}
				return fDummyCheckPointWithSecuritySet;
			}
		}
		DummyCheckPointWithSecuritySet fDummyCheckPointWithSecuritySet;

		protected override LicenceCheckpoint LicenceCheckPointCore => (LicenceCheckpoint)EnvProxy.Instance.Licence.Core;

		protected internal override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			LastController = (DummyDependentWithCodeController)ZControllerFactory.Create(DummyControllerIDs.DummyDependentWithCode);
			return LastController;
		}

		protected override IBusinessObjectCollection GetNewGridCollection() => new DummyDependentWithCodeBusinessObjectCollection(Factory);

		protected override IFilterControl GetNewFilterControl() => new DummyDependentModuleControl(GridCollection, FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new DummyDependentFilterBusinessObject();
	}
}
