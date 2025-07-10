using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ControlReadOnlyPorpertyHelperTest : TestCase
	{
		class DummyControl : Control, IEditableInViewMode
		{
			public bool EditableInViewMode { get; set; }
			public void SetReadOnly(bool value) => Enabled = !value;
		}

		public void TestViewControlsEditableInViewMode()
		{
			using (var control = new DummyControl())
			{
				control.EditableInViewMode = true;

				var helper = new ControlReadOnlyPropertyHelper(control, control.GetReadOnly, control.SetReadOnly);

				MimicViewMode(helper);

				Assert("When a control only edits the view, it should be enabled regardless of the BusinessObject's readonly", control.Enabled);
			}
		}

		public void TestModelControlsNotEditableInViewMode()
		{
			using (var control = new DummyControl())
			{
				control.EditableInViewMode = false;

				var helper = new ControlReadOnlyPropertyHelper(control, control.GetReadOnly, control.SetReadOnly);

				MimicViewMode(helper);

				Assert("When a control edits the model, it must be respect the BusinessObject's readonly property", !control.Enabled);
			}
		}

		public void TestModelControlsEditableInEditMode()
		{
			using (var control = new DummyControl())
			{
				control.EditableInViewMode = false;

				var helper = new ControlReadOnlyPropertyHelper(control, control.GetReadOnly, control.SetReadOnly);

				MimicEditMode(helper);

				Assert("Model control in edit mode, should be editable (BusinessObject's readonly=false)", control.Enabled);
			}
		}

		public void TestViewControlsEditableInEditMode()
		{
			using (var control = new DummyControl())
			{
				control.EditableInViewMode = true;

				var helper = new ControlReadOnlyPropertyHelper(control, control.GetReadOnly, control.SetReadOnly);

				MimicEditMode(helper);

				Assert("Model control in edit mode, should be editable (regadless of Bizo readonly)", control.Enabled);
			}
		}

		void MimicViewMode(ControlReadOnlyPropertyHelper propertyHelper)
			=> propertyHelper.ReadOnlyForBinding = true;

		void MimicEditMode(ControlReadOnlyPropertyHelper propertyHelper)
			=> propertyHelper.ReadOnlyForBinding = false;
	}
}
