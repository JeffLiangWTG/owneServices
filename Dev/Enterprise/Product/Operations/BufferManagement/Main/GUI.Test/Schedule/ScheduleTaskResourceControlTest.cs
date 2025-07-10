using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class ScheduleTaskResourceControlTest : TestCase
	{
		public void TestIsCollectionActiveCheckboxVisible()
		{
			using (var control = new ScheduleTaskRecurrenceControl())
			{
				AssertCheckboxVisible(control, true);
			}

			using (var control = new ScheduleTaskRecurrenceControl())
			{
				control.IsCollectionActiveCheckboxVisible = false;
				AssertCheckboxVisible(control, false);
			}
		}

		static void AssertCheckboxVisible(ScheduleTaskRecurrenceControl control, bool expectedVisible)
		{
			using (var form = new Form { Size = ControlDpiScalingHelper.NewScaledSize(1000, 1000) })
			{
				form.Controls.Add(control);
				form.Show();

				var checkBox = form.FindSingle<CheckBox>(x => x.Name == "isActiveCheckBox");
				AssertEquals(expectedVisible, checkBox.Visible);
			}
		}
	}
}
