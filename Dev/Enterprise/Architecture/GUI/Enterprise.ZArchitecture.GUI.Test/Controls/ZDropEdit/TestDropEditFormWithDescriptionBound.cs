using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestDropEditFormWithDescriptionBound : TestDropEditForm
	{
		public TestDropEditFormWithDescriptionBound(BusinessObject bizObj)
			: base(bizObj)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.DropEdit.BindToForDescription = BindToPrefix + DummyBusinessObject.Schema.Z0_Description;
		}
	}
}
