using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyDependentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;
		public override ControllerID ID => DummyControllerIDs.DummyDependent;
		public override ModuleIdentifier ModuleID => DummyModuleIDs.DummyDependent;
		public override Type TypeOfTopLevelBusinessObject => typeof(DummyDependantBusinessObject);
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotImplementedException("This form isn't yet needed... feel free to add one if you need it.");
		}

		protected override SecurityCheckpoint CheckPointForView => (SecurityCheckpoint)EnvProxy.Instance.Security.None;
		protected override SecurityCheckpoint CheckPointForNew => (SecurityCheckpoint)EnvProxy.Instance.Security.None;
		protected override SecurityCheckpoint CheckPointForEdit => (SecurityCheckpoint)EnvProxy.Instance.Security.None;
		protected override SecurityCheckpoint CheckPointForDelete => (SecurityCheckpoint)EnvProxy.Instance.Security.None;
	}
}
