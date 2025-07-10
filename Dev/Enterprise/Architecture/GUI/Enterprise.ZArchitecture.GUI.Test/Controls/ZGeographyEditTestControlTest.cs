using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGeographyEditTestControlTest : ZControlBaseTestCase<ZGeographyEdit>
	{
		protected override string[] BindablePropertyNames
		{
			get { return new[] { "GeographyValue", "ReadOnly" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Size"; }
		}

		protected override void BindControl()
		{
			base.BindControl();
			IDataBoundControl bindableControl = Control;
			Control.SetBindingMember(DummyBizoSchema.Z0_Geography.Name);
			bindableControl.SetDataBinding(Dummy, "");
		}
	}
}
