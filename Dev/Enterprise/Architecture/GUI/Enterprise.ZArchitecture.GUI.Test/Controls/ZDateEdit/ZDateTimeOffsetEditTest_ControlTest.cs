using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDateTimeOffsetEditTest_ControlTest : ZControlBaseTestCase<ZDateTimeOffsetEdit>
	{
		protected override string[] BindablePropertyNames
		{
			get { return new[] { "DateTimeOffsetValue", "ReadOnly" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Size"; }
		}

		protected override void BindControl()
		{
			base.BindControl();
			IDataBoundControl bindableControl = Control;
			Control.SetBindingMember(DummyBizoSchema.Z0_DateTimeOffset.Name);
			bindableControl.SetDataBinding(Dummy, "");
		}
	}
}
