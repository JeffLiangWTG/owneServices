using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDateEditTest : TestCaseWithDummy
	{
		public void TestControlLayout()
		{
			AssertNoErrorMessage(testControl.LayoutErrors);
		}

		public void TestPressF2()
		{
			Form.Controls.Add(DateEdit);
			Form.Show();

			DateEdit.DateTextBox.Text = "ABC";
			DateEdit.DateTextBox.SelectionLength = 3;

			DateEdit.IsOnGrid = false;
			KeySender.SendKeyDown(DateEdit.DateTextBox, DateEdit.DateTextBox.Handle, Keys.F2);

			AssertEquals("DateTextBox.SelectionStart", 0, DateEdit.DateTextBox.SelectionStart);
			AssertEquals("DateTextBox.SelectionLength", 3, DateEdit.DateTextBox.SelectionLength);

			DateEdit.IsOnGrid = true;
			KeySender.SendKeyDown(DateEdit.DateTextBox, DateEdit.DateTextBox.Handle, Keys.F2);

			AssertEquals("DateTextBox.SelectionStart", 3, DateEdit.DateTextBox.SelectionStart);
			AssertEquals("DateTextBox.SelectionLength", 0, DateEdit.DateTextBox.SelectionLength);
		}

		public void TestPressingHotkey_ReadOnly()
		{
			var dateToUse = ZDateTime.BrettsBirthday; // Assume that brett wasnt born today
			var hotkeysToTest = new Keys[] {
				Keys.F5,
				Keys.Control | Keys.Up,
				Keys.Control | Keys.Down,
				Keys.Alt | Keys.Up,
				Keys.Alt | Keys.Down,
				Keys.Control | Keys.Alt | Keys.Up,
				Keys.Control | Keys.Alt | Keys.Down,
				Keys.Control | Keys.Right,
				Keys.Control | Keys.Left,
			};

			CombineAssertions(() =>
			{
				using (var form = new ZChildForm())
				using (var dateEdit = new ZDateEdit())
				{
					dateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;

					form.Controls.Add(dateEdit);
					form.Show();

					foreach (var hotkey in hotkeysToTest)
					{
						dateEdit.DateTimeValue = dateToUse;
						dateEdit.ReadOnly = true;

						KeySender.SendKeyDownToProcessCmdKey(dateEdit, (int)hotkey);
						Application.DoEvents();

						AssertEquals("Time should not be changed - we're readonly. Hotkey: " + hotkey, dateToUse, dateEdit.DateTimeValue);

						dateEdit.ReadOnly = false;

						KeySender.SendKeyDownToProcessCmdKey(dateEdit, (int)hotkey);
						Application.DoEvents();

						AssertNotEquals("Looks like the hotkey was changed - please update this test. Hotkey: " + hotkey, dateToUse, dateEdit.DateTimeValue);
					}
				}
			});
		}

#if !WINZOR
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		public void TestPressingShowCalendarHotkey()
		{
			using (var form = new ZChildForm())
			using (var dateEdit = new ZDateEdit())
			{
				form.Controls.Add(dateEdit);
				form.Show();

				dateEdit.ReadOnly = true;
				KeySender.SendKeyDownToProcessCmdKey(dateEdit, (int)Keys.F4);
				Application.DoEvents();

				Assert("Calendar shouldnt be visible since we're read only", !Application.OpenForms.OfType<ZPopupCalendar>().Any());

				dateEdit.ReadOnly = false;
				KeySender.SendKeyDownToProcessCmdKey(dateEdit, (int)Keys.F4);
				Application.DoEvents();

				Assert("Calendar should be visible since we pressed the hotkey", Application.OpenForms.OfType<ZPopupCalendar>().Any());
			}
		}
#endif

		#region Binding

		public void TestBinding()
		{
			var table = new DataTable();
			var column1 = new DataColumn("TestDate", typeof(ZDateTime));
			table.Columns.Add(column1);

			var row1 = table.NewRow();
			row1[column1] = new ZDateTime(2002, 11, 30, 0, 0, 0);
			table.Rows.Add(row1);

			using (var testForm = new ZForm())
			{
				testControl.DateTimeFormat = ZDateTimePickerFormat.Long;

				testForm.Controls.Add(testControl);
				testForm.Show();

				testControl.SetDataBinding(table, "TestDate");
				AssertEquals("Selected Date value after binding", row1[column1], testControl.DateTimeValue);
				var expectedText = ((ZDateTime)row1[column1]).ToString(testControl.FormatString).ToUpper();
				AssertEquals("Text property value after binding", expectedText, testControl.Text);

				testControl.DateTimeValue = new DateTime(2003, 02, 17, 0, 0, 0);
				expectedText = testControl.DateTimeValue.ToString(testControl.FormatString).ToUpper();
				AssertEquals("Text property value after changing SelectedDate", expectedText, testControl.Text);
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestBinding_DateTimeOffsetVersion()
		{
			var table = new DataTable();
			var column1 = new DataColumn("TestDateTimeOffset", typeof(ZDateTimeOffset));
			table.Columns.Add(column1);

			var row1 = table.NewRow();
			row1[column1] = new ZDateTimeOffset(2002, 11, 30, 0, 0, 0, TimeSpan.FromHours(11));
			table.Rows.Add(row1);

			using (var testForm = new ZForm())
			{
				testOffsetControl.DateTimeFormat = ZDateTimePickerFormat.Long;

				testForm.Controls.Add(testOffsetControl);
				testForm.Show();

				testOffsetControl.SetDataBinding(table, "TestDateTimeOffset");
				AssertEquals("Selected Date value after binding", row1[column1], testOffsetControl.DateTimeOffsetValue);
				var offsetString = " GMT+11:00";
				var expectedText = ((ZDateTimeOffset)row1[column1]).ToString(testOffsetControl.FormatString).ToUpper() + offsetString;
				AssertEquals("Text property value after binding", expectedText, testOffsetControl.Text);

				testOffsetControl.DateTimeOffsetValue = new DateTimeOffset(2003, 02, 17, 0, 0, 0, TimeSpan.FromHours(11));
				expectedText = testOffsetControl.DateTimeOffsetValue.ToString(testOffsetControl.FormatString).ToUpper() + offsetString;
				AssertEquals("Text property value after changing SelectedDate", expectedText, testOffsetControl.Text);
			}
		}

		#endregion

		#region Properties

		public void TestSizeAndFormat()
		{
			using (var testDateEdit = new ZDateEditForTest())
			{
				Assert("Default: ShortFormat => DateFormatWidth", testDateEdit.AreSizeAndFormatOnSync());

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Time;
				Assert("TimeFormat => TimeFormatWidth", testDateEdit.AreSizeAndFormatOnSync());

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
				Assert("LongFormat => DateTimeFormatWidth", testDateEdit.AreSizeAndFormatOnSync());

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
				Assert("ShortFormat => DateFormatWidth", testDateEdit.AreSizeAndFormatOnSync());

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.LongIncludingSeconds;
				Assert("LongIncludingSecondsFormat => DateTimeFormatIncludingSecondsWidth", testDateEdit.AreSizeAndFormatOnSync());

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.TimeIncludingSeconds;
				Assert("TimeIncludingSecondsFormat => TimeFormatIncludingSecondsWidth", testDateEdit.AreSizeAndFormatOnSync());

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Custom;
				Assert("AnyOtherFormat => DateFormatWidth", testDateEdit.AreSizeAndFormatOnSync());
			}
		}

		public void TestSizeFormatChanges()
		{
			using (var testDateEdit = new ZDateEditForTest())
			{
				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
				var shortFormatWidth = testDateEdit.Width;

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
				var longFormatWidth = testDateEdit.Width;

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Time;
				var timeFormatWidth = testDateEdit.Width;

				Assert("Long Width > Short Width", longFormatWidth > shortFormatWidth);

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
				Assert("Short Width < Long Width", testDateEdit.Width < longFormatWidth);

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.TimeIncludingSeconds;
				Assert("Short including seconds > Time Width", testDateEdit.Width > timeFormatWidth);

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.LongIncludingSeconds;
				Assert("Long including seconds > Long Width", testDateEdit.Width > longFormatWidth);
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestAutoCompleteYear()
		{
			var currentYr = EnvProxy.Instance.Time.CurrentLocalDate.Year;
			testControl.AutoCompleteYear = true;

			testControl.AutoCompleteMonthThreshold = 1;
			TestDateAttribute.Date = new DateTime(2006, 11, 1);
			AssertExpectedYear(currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr);

			testControl.AutoCompleteMonthThreshold = 1;
			TestDateAttribute.Date = new DateTime(2006, 12, 1);
			AssertExpectedYear(currentYr + 1, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr);

			testControl.AutoCompleteMonthThreshold = 2;
			AssertExpectedYear(currentYr + 1, currentYr + 1, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr);

			testControl.AutoCompleteMonthThreshold = 1;
			TestDateAttribute.Date = new DateTime(2006, 1, 1);
			AssertExpectedYear(currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr - 1);

			testControl.AutoCompleteMonthThreshold = 1;
			TestDateAttribute.Date = new DateTime(2006, 2, 1);
			AssertExpectedYear(currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr);

			testControl.AutoCompleteMonthThreshold = 2;
			AssertExpectedYear(currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr - 1);

			testControl.AutoCompleteMonthThreshold = 5;
			AssertExpectedYear(currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr - 1, currentYr - 1, currentYr - 1, currentYr - 1);

			testControl.AutoCompleteMonthThreshold = 5;
			TestDateAttribute.Date = new DateTime(2006, 6, 1);
			AssertExpectedYear(currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr, currentYr);
		}

		void AssertExpectedYear(
			int yearForJAN, int yearForFEB, int yearForMAR, int yearForAPR, int yearForMAY, int yearForJUN,
			int yearForJUL, int yearForAUG, int yearForSEP, int yearForOCT, int yearForNOV, int yearForDEC)
		{
			AssertEquals(yearForJAN, testControl.GetYearFromGetDateTimeAsObject("1701"));
			AssertEquals(yearForFEB, testControl.GetYearFromGetDateTimeAsObject("1702"));
			AssertEquals(yearForMAR, testControl.GetYearFromGetDateTimeAsObject("1703"));
			AssertEquals(yearForAPR, testControl.GetYearFromGetDateTimeAsObject("1704"));
			AssertEquals(yearForMAY, testControl.GetYearFromGetDateTimeAsObject("1705"));
			AssertEquals(yearForJUN, testControl.GetYearFromGetDateTimeAsObject("1706"));
			AssertEquals(yearForJUL, testControl.GetYearFromGetDateTimeAsObject("1707"));
			AssertEquals(yearForAUG, testControl.GetYearFromGetDateTimeAsObject("1708"));
			AssertEquals(yearForSEP, testControl.GetYearFromGetDateTimeAsObject("1709"));
			AssertEquals(yearForOCT, testControl.GetYearFromGetDateTimeAsObject("1710"));
			AssertEquals(yearForNOV, testControl.GetYearFromGetDateTimeAsObject("1711"));
			AssertEquals(yearForDEC, testControl.GetYearFromGetDateTimeAsObject("1712"));
		}

		public void TestFormatChangeUpdatesText()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			dummyBizO.Z0_Date = new ZDateTime(1981, 5, 28, 9, 32, 0);

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
			DateEdit.BindTo = AutoDummyBizo.Schema.Z0_Date;
			Form.Controls.Add(DateEdit);

			Form.SetDataBinding(dummyBizO, "");

			Form.Show();

			AssertEquals("Date should be long but is " + DateEdit.Text, "28-MAY-81 09:32", DateEdit.Text);

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
			AssertEquals("Date should be short but is " + DateEdit.Text, "28-MAY-81", DateEdit.Text);

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
			AssertEquals("Date should be long again but is " + DateEdit.Text, "28-MAY-81 00:00", DateEdit.Text);

			DateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
			AssertEquals("Date should be short again but is " + DateEdit.Text, "28-MAY-81", DateEdit.Text);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestFormatChangeUpdatesText_DateTimeOffsetVersion()
		{
			using (RawDataRegistry.Instance.DisplayUtcOffset.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dummyBizO = Factory.New<DummyBusinessObject>();
				dummyBizO.Z0_DateTimeOffset = new ZDateTimeOffset(1981, 5, 28, 9, 32, 0, TimeSpan.FromHours(10));

				DateTimeOffsetEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
				DateTimeOffsetEdit.BindTo = AutoDummyBizo.Schema.Z0_DateTimeOffset;
				Form.Controls.Add(DateTimeOffsetEdit);

				Form.SetDataBinding(dummyBizO, "");

				Form.Show();

				AssertEquals("Date should be long but is " + DateTimeOffsetEdit.Text, "28-MAY-81 09:32 GMT+10:00", DateTimeOffsetEdit.Text);

				DateTimeOffsetEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
				AssertEquals("Date should be short but is " + DateTimeOffsetEdit.Text, "28-MAY-81", DateTimeOffsetEdit.Text);

				DateTimeOffsetEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
				AssertEquals("Date should be long again but is " + DateTimeOffsetEdit.Text, "28-MAY-81 09:32 GMT+10:00", DateTimeOffsetEdit.Text);

				DateTimeOffsetEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
				AssertEquals("Date should be short again but is " + DateTimeOffsetEdit.Text, "28-MAY-81", DateTimeOffsetEdit.Text);
			}

			using (RawDataRegistry.Instance.DisplayUtcOffset.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				DateTimeOffsetEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
				AssertEquals("Date should be long but is " + DateTimeOffsetEdit.Text, "28-MAY-81 09:32", DateTimeOffsetEdit.Text);

				DateTimeOffsetEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
				AssertEquals("Date should be short but is " + DateTimeOffsetEdit.Text, "28-MAY-81", DateTimeOffsetEdit.Text);
			}
		}

		#endregion

		#region EditableControl

		public void TestIsEditing()
		{
			Form.Controls.Add(DateEdit);
			Form.Show();
			DateEdit.BindTo = AutoDummyBizo.Schema.Z0_Date;
			Form.SetDataBinding(Dummy, "");
			Application.DoEvents();
			var editableControl = EditableControl.Get(DateEdit);

			DateEdit.Focus();
			AssertEquals(false, editableControl.IsEditing);
			DateEdit.DateTextBox.Text = "2006-1-2";
			AssertEquals(true, editableControl.IsEditing);
		}

		public void TestIsEditing_DateTimeOffsetVersion()
		{
			Form.Controls.Add(DateTimeOffsetEdit);
			Form.Show();
			DateTimeOffsetEdit.BindTo = AutoDummyBizo.Schema.Z0_DateTimeOffset;
			Form.SetDataBinding(Dummy, "");
			Application.DoEvents();
			var editableControl = EditableControl.Get(DateTimeOffsetEdit);

			DateTimeOffsetEdit.Focus();
			AssertEquals(false, editableControl.IsEditing);
			DateTimeOffsetEdit.DateTextBox.Text = "2006-1-2";
			AssertEquals(true, editableControl.IsEditing);
		}

		public void TestGetFrontMostActiveControl()
		{
			Form.Controls.Add(DateEdit);
			Form.Show();
			DateEdit.BindTo = AutoDummyBizo.Schema.Z0_Date;
			Form.SetDataBinding(Dummy, "");
			Application.DoEvents();

			DateEdit.Focus();
			var control = Form.GetFrontMostActiveControl();
			AssertEquals(DateEdit, control);
		}

		#endregion

		#region DateTimeValue

		public void TestDateTimeValueChanged()
		{
			Form.Controls.Add(DateEdit);
			Form.Show();
			Application.DoEvents();

			var dateTimeValueChangedFired = false;
			DateEdit.DateTimeValueChanged += delegate
			{ dateTimeValueChangedFired = true; };

			DateEdit.DateTimeValue = ZDateTime.Empty;
			AssertEquals(false, dateTimeValueChangedFired);
			DateEdit.DateTimeValue = ZDateTime.Now;
			AssertEquals(true, dateTimeValueChangedFired);
			dateTimeValueChangedFired = false;
			DateEdit.DateTimeValue = new ZDateTime(2000, 1, 1);
			AssertEquals(true, dateTimeValueChangedFired);
			dateTimeValueChangedFired = false;
			DateEdit.DateTimeValue = ZDateTime.Empty;
			AssertEquals(true, dateTimeValueChangedFired);
			dateTimeValueChangedFired = false;
			DateEdit.Text = "2006-1-1";
			AssertEquals(true, dateTimeValueChangedFired);
		}

		public void TestZPopupCalendarOnDeactivateDoesChangeValue()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			using (var form = new ZChildForm())
			using (var testDateEdit = new DateEditTestClass { BindTo = AutoDummyBizo.Schema.Z0_Date })
			{
				form.SetDataBinding(dummy, "");
				form.Controls.Add(testDateEdit);
				form.Show();

				AssertEquals(ZDateTime.Empty, testDateEdit.DateTimeValue);

				using (testDateEdit.CalendarForTest)
				{
					testDateEdit.CalendarButton.PerformClick();
					dummy.Z0_Date = new ZDateTime(2022, 8, 1);
				}
				AssertEquals(new ZDateTime(2022, 8, 1), testDateEdit.DateTimeValue);
			}
		}

		public void TestCalendarShiftsToLeftWhenOnRightOfScreen()
		{
			using (var form = new ZChildForm())
			using (var testDateEdit = new DateEditTestClass())
			{
				var currentScreenRect = CachedScreenInfo.Instance.FromControl(form);
				form.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
				form.Size = currentScreenRect.Size;
				form.Controls.Add(testDateEdit);
				form.Show();

				using (var calendar = testDateEdit.CalendarForTest)
				{
					testDateEdit.Location = ControlDpiScalingHelper.NewScaledPoint(currentScreenRect.Right - calendar.Width / 2, currentScreenRect.Y + currentScreenRect.Height / 2);
					testDateEdit.CalendarButton.PerformClick();
					AssertEquals("Left of the popup calendar should've been shifted so we don't go off the screen", currentScreenRect.Right - calendar.Width, calendar.Left);
				}
			}

			using (var form = new ZChildForm())
			using (var testDateEdit = new DateEditTestClass())
			{
				var currentScreenRect = CachedScreenInfo.Instance.FromControl(form);
				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
				form.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
				form.Size = currentScreenRect.Size;
				form.Controls.Add(testDateEdit);
				form.Show();

				using (var calendar = testDateEdit.CalendarForTest)
				{
					testDateEdit.Location = ControlDpiScalingHelper.NewScaledPoint(currentScreenRect.Right - calendar.Width / 2, currentScreenRect.Y + currentScreenRect.Height / 2);
					testDateEdit.CalendarButton.PerformClick();
					AssertEquals("Left of the popup calendar should've been shifted so we don't go off the screen (long format)", currentScreenRect.Right - calendar.Width, calendar.Left);
				}
			}

			using (var form = new ZChildForm())
			using (var testDateEdit = new DateEditTestClass())
			{
				var currentScreenRect = CachedScreenInfo.Instance.FromControl(form);
				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.LongIncludingSeconds;
				form.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
				form.Size = currentScreenRect.Size;
				form.Controls.Add(testDateEdit);
				form.Show();

				using (var calendar = testDateEdit.CalendarForTest)
				{
					testDateEdit.Location = ControlDpiScalingHelper.NewScaledPoint(currentScreenRect.Right - calendar.Width / 2, currentScreenRect.Y + currentScreenRect.Height / 2);
					testDateEdit.CalendarButton.PerformClick();
					AssertEquals("Left of the popup calendar should've been shifted so we don't go off the screen (long format with seconds)", currentScreenRect.Right - calendar.Width, calendar.Left);
				}
			}
		}

		public void TestCalendarShiftsUpWhenOnBottomOfScreen()
		{
			using (var form = new ZChildForm())
			using (var testDateEdit = new DateEditTestClass())
			{
				var currentScreenRect = CachedScreenInfo.Instance.FromControl(form);
				form.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
				form.Size = currentScreenRect.Size;
				form.Controls.Add(testDateEdit);
				form.Show();

				using (var calendar = testDateEdit.CalendarForTest)
				{
					testDateEdit.Location = ControlDpiScalingHelper.NewScaledPoint(currentScreenRect.X + currentScreenRect.Width / 2, currentScreenRect.Bottom - calendar.Height / 2);
					testDateEdit.CalendarButton.PerformClick();
					var adjustedLocation = form.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(testDateEdit.Left, testDateEdit.Bottom));
					AssertEquals("Popup calendar should've been shifted up so we don't go off the screen", adjustedLocation.Y - calendar.Height - testDateEdit.Height, calendar.Location.Y);
				}
			}

			using (var form = new ZChildForm())
			using (var testDateEdit = new DateEditTestClass())
			{
				var currentScreenRect = CachedScreenInfo.Instance.FromControl(form);
				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
				form.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
				form.Size = currentScreenRect.Size;
				form.Controls.Add(testDateEdit);
				form.Show();

				using (var calendar = testDateEdit.CalendarForTest)
				{
					testDateEdit.Location = ControlDpiScalingHelper.NewScaledPoint(currentScreenRect.X + currentScreenRect.Width / 2, currentScreenRect.Bottom - calendar.Height / 2);
					testDateEdit.CalendarButton.PerformClick();
					var adjustedLocation = form.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(testDateEdit.Left, testDateEdit.Bottom));
					AssertEquals("Popup calendar should've been shifted up so we don't go off the screen (Long Format)", adjustedLocation.Y - calendar.Height - testDateEdit.Height, calendar.Location.Y);
				}
			}

			using (var form = new ZChildForm())
			using (var testDateEdit = new DateEditTestClass())
			{
				var currentScreenRect = CachedScreenInfo.Instance.FromControl(form);
				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.LongIncludingSeconds;
				form.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
				form.Size = currentScreenRect.Size;
				form.Controls.Add(testDateEdit);
				form.Show();

				using (var calendar = testDateEdit.CalendarForTest)
				{
					testDateEdit.Location = ControlDpiScalingHelper.NewScaledPoint(currentScreenRect.X + currentScreenRect.Width / 2, currentScreenRect.Bottom - calendar.Height / 2);
					testDateEdit.CalendarButton.PerformClick();
					var adjustedLocation = form.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(testDateEdit.Left, testDateEdit.Bottom));
					AssertEquals("Popup calendar should've been shifted up so we don't go off the screen (Long Format with Seconds)", adjustedLocation.Y - calendar.Height - testDateEdit.Height, calendar.Location.Y);
				}
			}
		}

		public void TestDateTimeValueChanged_DateTimeOffsetVersion()
		{
			Form.Controls.Add(DateTimeOffsetEdit);
			Form.Show();
			Application.DoEvents();

			var dateTimeOffsetValueChangedFired = false;
			DateTimeOffsetEdit.DateTimeOffsetValueChanged += delegate
			{ dateTimeOffsetValueChangedFired = true; };

			DateTimeOffsetEdit.DateTimeOffsetValue = ZDateTimeOffset.Empty;
			AssertEquals(false, dateTimeOffsetValueChangedFired);
			dateTimeOffsetValueChangedFired = false;
			DateTimeOffsetEdit.DateTimeOffsetValue = ZDateTimeOffset.Now;
			AssertEquals(true, dateTimeOffsetValueChangedFired);
			dateTimeOffsetValueChangedFired = false;
			DateTimeOffsetEdit.DateTimeOffsetValue = new ZDateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromHours(11));
			AssertEquals(true, dateTimeOffsetValueChangedFired);
			dateTimeOffsetValueChangedFired = false;
			DateTimeOffsetEdit.DateTimeOffsetValue = ZDateTimeOffset.Empty;
			AssertEquals(true, dateTimeOffsetValueChangedFired);
			dateTimeOffsetValueChangedFired = false;
			DateTimeOffsetEdit.Text = "2006-1-1";
			AssertEquals(true, dateTimeOffsetValueChangedFired);
		}

		#endregion

		#region ReadOnly

		public void TestReadOnlyChanged()
		{
			Form.Controls.Add(DateEdit);
			Form.Show();
			Application.DoEvents();

			var readOnlyChangedFiredCount = 0;
			DateEdit.ReadOnlyChanged += delegate
			{ readOnlyChangedFiredCount++; };

			DateEdit.ReadOnly = false;
			AssertEquals(0, readOnlyChangedFiredCount);

			DateEdit.ReadOnly = true;
			AssertEquals(1, readOnlyChangedFiredCount);

			DateEdit.DateTextBox.ReadOnly = false;
			AssertEquals(2, readOnlyChangedFiredCount);
		}

		#endregion

		#region Implementation

		DateEditTestClass testControl;
		DateTimeOffsetEditTestClass testOffsetControl;

		ZChildForm Form
		{
			get { return form ?? (form = new ZChildForm()); }
		}
		ZChildForm form;

		ZDateEdit DateEdit
		{
			get { return dateEdit ?? (dateEdit = new ZDateEdit()); }
		}
		ZDateEdit dateEdit;

		ZDateTimeOffsetEdit DateTimeOffsetEdit
		{
			get { return dateTimeOffsetEdit ?? (dateTimeOffsetEdit = new ZDateTimeOffsetEdit()); }
		}
		ZDateTimeOffsetEdit dateTimeOffsetEdit;

		protected override void SetUp()
		{
			base.SetUp();
			testControl = new DateEditTestClass();
			testOffsetControl = new DateTimeOffsetEditTestClass();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (dateEdit != null)
			{
				dateEdit.Dispose();
			}
			if (testControl != null)
			{
				testControl.Dispose();
			}
			if (testOffsetControl != null)
			{
				testOffsetControl.Dispose();
			}
		}

		static void AssertNoErrorMessage(string message)
		{
			Assert(message, string.IsNullOrEmpty(message));
		}

		#endregion
	}
}
