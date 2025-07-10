using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	sealed class FieldFindBoxGridControlTest : ZControlBaseTestCase<FieldFindBoxGridControl>
	{
		#region Implementation

		protected override string[] BindablePropertyNames
		{
			get { return new[] { "FieldName", "ReadOnly" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Size"; }
		}

		protected override void BindControl()
		{
			base.BindControl();
			IDataBoundControl bindableControl = Control;
			Control.SetBindingMember(DummyBizoSchema.Z0_Date.Name);
			bindableControl.SetDataBinding(Dummy, "");
		}

		#endregion
	}
}
