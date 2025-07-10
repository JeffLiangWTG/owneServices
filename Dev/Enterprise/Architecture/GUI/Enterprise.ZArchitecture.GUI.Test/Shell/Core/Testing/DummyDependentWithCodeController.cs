using System;
using CargoWise.EntityFramework;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	class DummyDependentWithCodeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;
		public override ControllerID ID => DummyControllerIDs.DummyDependentWithCode;
		public override ModuleIdentifier ModuleID => DummyModuleIDs.DummyDependentWithCode;
		public override Type TypeOfTopLevelBusinessObject => typeof(DummyDependentWithCodeBusinessObject);

		protected override IZForm GetForm(IBusiness businessEntity) => GetFormCore(businessEntity);

		public virtual IZForm GetFormCore(IBusiness businessEntity)
		{
			LastFormCreated = new ZDummyDependentWithCodeForm(businessEntity);
			return LastFormCreated;
		}

		protected override SecurityCheckpoint CheckPointForView => (SecurityCheckpoint)EnvProxy.Instance.Security.None;
		protected override SecurityCheckpoint CheckPointForNew => (SecurityCheckpoint)EnvProxy.Instance.Security.None;
		protected override SecurityCheckpoint CheckPointForEdit => (SecurityCheckpoint)EnvProxy.Instance.Security.None;
		protected override SecurityCheckpoint CheckPointForDelete => (SecurityCheckpoint)EnvProxy.Instance.Security.None;

		internal ZForm LastFormCreated;
	}
}
