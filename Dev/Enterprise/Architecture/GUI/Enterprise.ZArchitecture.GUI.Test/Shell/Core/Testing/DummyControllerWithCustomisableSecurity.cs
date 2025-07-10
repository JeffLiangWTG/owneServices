using CargoWise.EntityFramework;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class DummyControllerWithCustomisableSecurity : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.Dummy2;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new DummyPlugIn2(businessEntity);

		protected override SecurityCheckpoint CheckPointForEdit => fCheckPointForEdit;

		public void SetCheckPointForEdit(SecurityCheckpoint checkPointForEdit)
		{
			fCheckPointForEdit = checkPointForEdit;
		}

		SecurityCheckpoint fCheckPointForEdit = (SecurityCheckpoint)EnvProxy.Instance.Security.None;

		protected override SecurityCheckpoint CheckPointForView => fCheckPointForView;

		public void SetCheckPointForView(SecurityCheckpoint checkPointForView)
		{
			fCheckPointForView = checkPointForView;
		}

		SecurityCheckpoint fCheckPointForView = (SecurityCheckpoint)EnvProxy.Instance.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => fCheckPointForDelete;

		public void SetCheckPointForDelete(SecurityCheckpoint checkPointForDelete)
		{
			fCheckPointForDelete = checkPointForDelete;
		}

		SecurityCheckpoint fCheckPointForDelete = (SecurityCheckpoint)EnvProxy.Instance.Security.None;
	}
}
