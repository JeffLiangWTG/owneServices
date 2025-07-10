using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZPopupCalendarTest : TestCase
	{
		public void TestOnDeactivateDoesNotCallDoEvents()
		{
			using (var form = new ZChildForm())
			using (var testDateEdit = new ZDateEdit { BindTo = AutoDummyBizo.Schema.Z0_Date })
			using (var e = new ZPopupCalendar())
			{
				form.Controls.Add(testDateEdit);
				form.Show();
				e.Popup(new Point(), DateTime.Now, form, ZDateTimePickerFormat.Short);

				var i = 0;
				testDateEdit.BeginInvoke(() => { i++; });

				var onDeactivate = typeof(ZForm).GetMethod("OnDeactivate", BindingFlags.Instance | BindingFlags.NonPublic);
				onDeactivate.Invoke(e, new[] { new EventArgs() });
				AssertEquals("i should not have changed", 0, i);
			}
		}

		public void TestOnDeactivateDontRecur()
		{
			using (var form = new ZChildForm())
			using (var otherForm = new ZChildForm())
			using (var testDateEdit = new ZDateEdit { BindTo = AutoDummyBizo.Schema.Z0_Date })
			using (var e = new ZPopupCalendar())
			{
				form.Controls.Add(testDateEdit);
				form.Show();

				var onDeactivate = typeof(ZForm).GetMethod("OnDeactivate", BindingFlags.Instance | BindingFlags.NonPublic);
				var counter = 0;
				e.DateTimeSelected += (sender, evnt) =>
				{
					counter++;
					if (counter < 3)
					{
						onDeactivate.Invoke(e, new[] { new EventArgs() });
					}
				};
				e.Popup(new Point(), DateTime.Now, form, ZDateTimePickerFormat.Short);

				onDeactivate.Invoke(e, new[] { new EventArgs() });
				AssertEquals("deactivate shouldn't recur", 1, counter);
			}
		}
#if !WINZOR
		public void TestTimePickerControlCorrectSize_WhenTimeZoneFindBoxToggledOn()
		{
			using (var form = new ZChildForm())
			using (var dateOffsetEdit = new ZDateTimeOffsetEdit {  HasTimeZoneFindBox = true, })
			{
				form.Controls.Add(dateOffsetEdit);
				form.Show();

				// Calendar.HasTimeZoneFindBox is set to true automatically in the constructor of ZDateTimeOffsetEdit

				dateOffsetEdit.ReadOnly = false;
				KeySender.SendKeyDownToProcessCmdKey(dateOffsetEdit, (int)Keys.F4);
				Application.DoEvents();

				var calendar = Application.OpenForms.OfType<ZPopupCalendar>().FirstOrDefault();

				CombineAssertions("Preconditions : The TimeZoneFindBox is on the control.", () =>
				{
					AssertNotNull("Calendar should be visible since we pressed the hotkey", calendar);
					Assert("HasOffsetDropEdit should be set to true on the calendar.", calendar.HasTimeZoneFindBox);
					Assert("TimeZoneFindBox DropEdit should be visible on the calendar.", calendar.timeZoneFindBox.Visible);
				});

				Assert("TimePicker should be the correct size which is around below half the width of the calendar.", calendar.TimePicker.Width <= calendar.Width / 2 && calendar.TimePicker.Width > 0);
				Assert("TimeZoneFindBox should be correct size which is around below half the width of the calendar.", calendar.timeZoneFindBox.Width <= calendar.Width / 2 && calendar.TimePicker.Width < calendar.Width);
			}
		}

		public void TestTimePickerControlCorrectSize_WhenTimeZoneFindBoxToggledOff()
		{
			using (var form = new ZChildForm())
			using (var dateEdit = new ZDateEdit())
			{
				form.Controls.Add(dateEdit);
				form.Show();

				// Calendar.HasTimeZoneFindBox is set to false by default and is not set to true in ZDateEdit

				dateEdit.ReadOnly = false;
				KeySender.SendKeyDownToProcessCmdKey(dateEdit, (int)Keys.F4);
				Application.DoEvents();

				var calendar = Application.OpenForms.OfType<ZPopupCalendar>().FirstOrDefault();

				CombineAssertions("Preconditions : The TimeZoneFindBox is not on the control.", () =>
				{
					AssertNotNull("Calendar should be visible since we pressed the hotkey", calendar);
					Assert("HasOffsetDropEdit should be set to false on the calendar.", !calendar.HasTimeZoneFindBox);
					Assert("TimeZoneFindBox DropEdit should not be visible on the calendar.", !calendar.timeZoneFindBox.Visible);
				});

				AssertEquals("TimePicker should be the correct size of which its width is around the width of the calendar.", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 20, true).Width, calendar.TimePicker.Width);
			}
		}
#endif
	}
}
