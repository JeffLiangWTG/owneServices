using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDropEditControlTest : ZControlBaseTestCase<ZDropEdit>
	{
		protected override string[] BindablePropertyNames
		{
			get { return new string[] { "Text", "ReadOnly", "IsVisibleForBinding", "ShowDescriptionBox", "ShowDescriptionInDropDown" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Size"; }
		}

		protected override void BindControl()
		{
			base.BindControl();
			Control.SetDataBinding(Dummy, DummyBizoSchema.Z0_Description.Name);
		}
	}
}
