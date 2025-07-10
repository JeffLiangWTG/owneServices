using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyDependentModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => DummyModuleIDs.DummyDependent;
		public override SecurityCheckpoint SecurityCheckpoint => DummyCheckPointWithSecuritySet;

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

		protected internal override ZController GetNewController(BusinessObject selectedBusinessObject) => (DummyDependentController)ZControllerFactory.Create(DummyControllerIDs.DummyDependent);

		protected override IBusinessObjectCollection GetNewGridCollection() => new DummyDependentBusinessObjectCollection(Factory);

		protected override IFilterControl GetNewFilterControl() => new DummyDependentModuleControl(GridCollection, FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new DummyDependentFilterBusinessObject();

		protected override void ExportVisibleIntoAndOpenExcel()
		{
			throw new NotImplementedException();
		}
	}
}
