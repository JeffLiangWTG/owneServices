using CargoWise.EntityFramework.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Tests
{
	class DummyTreeModelView : ZTreeModelView<DummyBusinessObject>
	{
		public DummyTreeModelView(ZTreeModel<DummyBusinessObject> inner)
			: base(inner)
		{
		}

		public override SecurityCheckpoint SecurityCheckpointForEdit
		{
			get { return SecurityCheckpointForEditOverride ?? base.SecurityCheckpointForEdit; }
		}

		public SecurityCheckpoint SecurityCheckpointForEditOverride;
	}
}
