using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDateEdit_Test : TestCaseWithFactory
	{
		[TestDate(1981, 5, 28, 9, 32, 0)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestGetDateTimeForPopUpCalendar()
		{
			using (var testDateEdit = new ZDateEdit())
			{
				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
				AssertEquals(EnvProxy.Instance.Time.CurrentLocalDateTime, testDateEdit.GetDateTimeForPopupCalendar());

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
				AssertEquals(EnvProxy.Instance.Time.CurrentLocalDate, testDateEdit.GetDateTimeForPopupCalendar());

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Custom;
				AssertEquals(EnvProxy.Instance.Time.CurrentLocalDateTime, testDateEdit.GetDateTimeForPopupCalendar());
			}

			using (var testDateEdit = new ZDateTimeOffsetEdit())
			{
				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
				AssertEquals(EnvProxy.Instance.Time.CurrentLocalDateTime, testDateEdit.GetDateTimeForPopupCalendar());

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
				AssertEquals(EnvProxy.Instance.Time.CurrentLocalDate, testDateEdit.GetDateTimeForPopupCalendar());

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Custom;
				AssertEquals(EnvProxy.Instance.Time.CurrentLocalDateTime, testDateEdit.GetDateTimeForPopupCalendar());

				AssertEquals(Env.Time.GetUtcOffsetBasedOnUtc(EnvProxy.Instance.CurrentBranch.NKUNLOCO, EnvProxy.Instance.Time.CurrentUtcDateTime), testDateEdit.GetOffsetForPopupCalendar());
			}
		}

		public void TestGetNewCore()
		{
			using (var testDateEdit = new ZDateEdit())
			{
				AssertNotNull("Must not be null", testDateEdit.GetNewCore());
				AssertEquals("Must have correct core type", testDateEdit.GetNewCore().GetType(), typeof(ZDateEditCore));
			}
		}

		[TestDate(2001, 01, 02)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestTTodayEntryYTodayEntry()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			using (var form = new ZChildForm())
			{
				var testDateEdit = new ZDateEdit { TabIndex = 0, BindTo = AutoDummyBizo.Schema.Z0_Date };
				var testDateTimeOffsetEdit = new ZDateTimeOffsetEdit { TabIndex = 0, BindTo = AutoDummyBizo.Schema.Z0_DateTimeOffset };
				form.Controls.Add(testDateEdit);
				form.Controls.Add(testDateTimeOffsetEdit);
				form.Show();

				testDateEdit.Text = "T";
				AssertEquals("Should be formatted", ZDateTime.Today, testDateEdit.DateTimeValue);

				testDateEdit.Text = "Y";
				AssertEquals("Should be formatted", ZDateTime.Today.AddDays(-1), testDateEdit.DateTimeValue);

				testDateTimeOffsetEdit.Text = "T";
				AssertEquals("Should be formatted", ZDateTimeOffset.Today, testDateTimeOffsetEdit.DateTimeOffsetValue);

				testDateTimeOffsetEdit.Text = "Y";
				AssertEquals("Should be formatted", ZDateTimeOffset.Today.AddDays(-1), testDateTimeOffsetEdit.DateTimeOffsetValue);
			}
		}

		public void TestEnteringTheSameShortDateMaskTwiceReparsesTextInDateTextBox()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			using (var form = new ZChildForm())
			{
				var testDateEdit = new ZDateEdit { TabIndex = 0, BindTo = AutoDummyBizo.Schema.Z0_Date };
				form.Controls.Add(testDateEdit);
				form.SetDataBinding(dummy, "");
				form.Show();
				UserIdleWorker.Flush();

				var box = new TextBox { TabIndex = 1 };
				form.Controls.Add(box);

				testDateEdit.Focus();

				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.NumPad3);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.OemMinus);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.NumPad3);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.OemMinus);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.NumPad7);
				Application.DoEvents();

				AssertEquals("3-3-7", testDateEdit.Text);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.Tab);
				Application.DoEvents();
				AssertEquals("Should be short date string", "03-MAR-07", testDateEdit.Text);

				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.Tab);
				Application.DoEvents();

				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.NumPad3);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.OemMinus);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.NumPad3);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.OemMinus);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.NumPad7);
				Application.DoEvents();

				AssertEquals("3-3-7", testDateEdit.Text);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.Tab);
				Application.DoEvents();
				AssertEquals("Should be short date string", "03-MAR-07", testDateEdit.Text);
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestValidatingSetsCorrectDateTimeValue()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			using (var form = new ZChildForm())
			{
				var testDateEdit = new ZDateEdit { TabIndex = 0, BindTo = AutoDummyBizo.Schema.Z0_Date };
				var testDateTimeOffsetEdit = new ZDateTimeOffsetEdit { TabIndex = 0, BindTo = AutoDummyBizo.Schema.Z0_DateTimeOffset };
				form.Controls.Add(testDateEdit);
				form.Controls.Add(testDateTimeOffsetEdit);
				form.SetDataBinding(dummy, "");
				form.Show();

				testDateEdit.Text = "010101";
				AssertEquals("Should be formatted", new ZDateTime(2001, 01, 01), testDateEdit.DateTimeValue);
				testDateTimeOffsetEdit.Text = "010101";
				AssertEquals("Should be formatted", new ZDateTimeOffset(2001, 01, 01, 0, 0, 0, TimeSpan.FromHours(11)), testDateTimeOffsetEdit.DateTimeOffsetValue);

				testDateEdit.DateTimeFormat = ZDateTimePickerFormat.Long;
				testDateTimeOffsetEdit.DateTimeFormat = ZDateTimePickerFormat.Long;

				testDateEdit.Text = "01-JAN-2052 12:30";
				AssertEquals("Should be formatted", new ZDateTime(2052, 01, 01, 12, 30, 0), testDateEdit.DateTimeValue);

				testDateEdit.Text = "01-01-2057 12:30";
				AssertEquals("Should be formatted", new ZDateTime(2057, 01, 01, 12, 30, 0), testDateEdit.DateTimeValue);

				testDateEdit.Text = "01-JAN-53 12:30";
				AssertEquals("Should be formatted", new ZDateTime(1953, 01, 01, 12, 30, 0), testDateEdit.DateTimeValue);

				testDateEdit.Text = "010182 12:30";
				AssertEquals("Should be formatted", new ZDateTime(1982, 01, 01, 12, 30, 0), testDateEdit.DateTimeValue);

				testDateTimeOffsetEdit.Text = "01-JAN-2052 12:30";
				AssertEquals("Should be formatted", new ZDateTimeOffset(2052, 01, 01, 12, 30, 0, TimeSpan.FromHours(11)), testDateTimeOffsetEdit.DateTimeOffsetValue);

				testDateTimeOffsetEdit.Text = "01-01-2057 12:30";
				AssertEquals("Should be formatted", new ZDateTimeOffset(2057, 01, 01, 12, 30, 0, TimeSpan.FromHours(11)), testDateTimeOffsetEdit.DateTimeOffsetValue);

				testDateTimeOffsetEdit.Text = "01-JAN-53 12:30";
				AssertEquals("Should be formatted", new ZDateTimeOffset(1953, 01, 01, 12, 30, 0, TimeSpan.FromHours(10)), testDateTimeOffsetEdit.DateTimeOffsetValue);

				testDateTimeOffsetEdit.Text = "010182 12:30";
				AssertEquals("Should be formatted", new ZDateTimeOffset(1982, 01, 01, 12, 30, 0, TimeSpan.FromHours(10)), testDateTimeOffsetEdit.DateTimeOffsetValue);
			}
		}

		public void TestEnteringShortDateManually()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			using (var form = new ZChildForm())
			{
				var testDateEdit = new ZDateEdit { TabIndex = 0, BindTo = AutoDummyBizo.Schema.Z0_Date };
				form.Controls.Add(testDateEdit);
				form.SetDataBinding(dummy, "");
				form.Show();
				UserIdleWorker.Flush();

				var box = new TextBox { TabIndex = 1 };
				form.Controls.Add(box);

				testDateEdit.Focus();

				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.NumPad0);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.NumPad3);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.OemMinus);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.M);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.A);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.R);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.OemMinus);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.NumPad8);
				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.NumPad7);
				Application.DoEvents();

				AssertEquals("Should be short date string", "03-MAR-87", testDateEdit.Text);

				KeySender.PostKeyDown(testDateEdit.DateTextBox, testDateEdit.DateTextBox.Handle, Keys.Tab);
				Application.DoEvents();
				AssertEquals("Should have full year", 1987, dummy.Z0_Date.Year);
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestDateEditPreservesCenturyDataFromCalendarPicker()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Date = new ZDateTime(1950, 3, 3);
			dummy.Z0_DateTimeOffset = new ZDateTimeOffset(1951, 4, 4, 0, 0, 0, TimeSpan.FromHours(10));

			using (var form = new ZChildForm())
			{
				var testDateEdit = new ZDateEdit { BindTo = AutoDummyBizo.Schema.Z0_Date };
				var testDateTimeOffsetEdit = new ZDateTimeOffsetEdit { TabIndex = 0, BindTo = AutoDummyBizo.Schema.Z0_DateTimeOffset };
				form.Controls.Add(testDateEdit);
				form.Controls.Add(testDateTimeOffsetEdit);
				form.SetDataBinding(dummy, "");
				form.Show();
				{
					var date = testDateEdit.GetDateTimeForPopupCalendarCore();
					AssertNotNull("Nullable date should never be null", date);
					AssertEquals("Should be date displayed", dummy.Z0_Date, new ZDateTime(date));

					AssertEquals("Should have date", "03-MAR-50", testDateEdit.Text);

					var e = new ZPopupCalendar.DateTimeSelectedEventArgs(dummy.Z0_Date.ToDateTime());
					testDateEdit.DateTimeSelectedFromPopup(this, e);

					AssertEquals("Should have date", "03-MAR-50", testDateEdit.Text);

					AssertEquals("Should have correct date in BizLayer", new ZDateTime(1950, 3, 3), dummy.Z0_Date);
				}

				{
					var date = testDateTimeOffsetEdit.GetDateTimeForPopupCalendarCore();
					AssertNotNull("Nullable date should never be null", date);
					AssertEquals("Should be date displayed", dummy.Z0_DateTimeOffset, new ZDateTimeOffset((DateTime)date, TimeSpan.FromHours(10)));

					AssertEquals("Should have date", "04-APR-51 00:00 GMT+10:00", testDateTimeOffsetEdit.Text);

					var e = new ZPopupCalendar.DateTimeOffsetSelectedEventArgs(dummy.Z0_DateTimeOffset.ToDateTime(), dummy.Z0_DateTimeOffset.Offset);
					testDateTimeOffsetEdit.DateTimeSelectedFromPopup(this, e);

					AssertEquals("Should have date", "04-APR-51 00:00 GMT+10:00", testDateTimeOffsetEdit.Text);

					AssertEquals("Should have correct date in BizLayer", new ZDateTimeOffset(1951, 4, 4, 0, 0, 0, TimeSpan.FromHours(10)), dummy.Z0_DateTimeOffset);
				}
			}
		}

		public void TestDateOfRangeForPopUpCalendar()
		{
			using (var form = new ZChildForm())
			{
				var testDateEdit = new ZDateEdit { BindTo = AutoDummyBizo.Schema.Z0_Date };
				form.Controls.Add(testDateEdit);
				form.Show();

				var e = new ZPopupCalendar.DateTimeSelectedEventArgs(new DateTime(2222, 3, 3));
				testDateEdit.DateTimeSelectedFromPopup(this, e);
				AssertEquals("Shouldn't have date", "<INVALID>", testDateEdit.Text);
			}
		}

		public void TestInvalidToEmpty()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Date = new ZDateTime(1980, 9, 10, 9, 32, 0);
			dummy.Z0_DateTimeOffset = new ZDateTimeOffset(1980, 9, 10, 9, 32, 0, TimeSpan.FromHours(11));

			using (var form = new ZChildForm())
			{
				var testDateEdit =
					new ZDateEdit
					{
						DateTimeFormat = ZDateTimePickerFormat.Long,
						BindTo = AutoDummyBizo.Schema.Z0_Date
					};
				var testDateTimeOffsetEdit =
					new ZDateTimeOffsetEdit
					{
						DateTimeFormat = ZDateTimePickerFormat.Long,
						BindTo = AutoDummyBizo.Schema.Z0_DateTimeOffset
					};
				form.Controls.Add(testDateEdit);
				form.Controls.Add(testDateTimeOffsetEdit);

				form.SetDataBinding(dummy, "");
				var textBox = new ZTextBox();
				form.Controls.Add(textBox);
				form.Show();
				{
					UserIdleWorker.Flush();
					testDateEdit.DateTextBox.Text = "INVLD";
					textBox.Focus();

					AssertEquals("Not Valid after invalid input", false, dummy.Z0_Date.IsValid);
					dummy.Z0_Date = ZDateTime.Empty;

					AssertEquals("Text cleared when set to empty", "", testDateEdit.DateTextBox.Text);
				}

				testDateEdit.DateTextBox.Focus();
				{
					UserIdleWorker.Flush();
					testDateTimeOffsetEdit.DateTextBox.Text = "INVLD";
					textBox.Focus();

					AssertEquals("Not Valid after invalid input", false, dummy.Z0_DateTimeOffset.IsValid);
					dummy.Z0_DateTimeOffset = ZDateTimeOffset.Empty;

					AssertEquals("Text cleared when set to empty", "", testDateTimeOffsetEdit.DateTextBox.Text);
				}
			}
		}

		#region Copy and Cut

		[DeveloperOnlyTest]
		public void TestCopyAndCut()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			using (var form = new ZChildForm())
			{
				var testDateEdit = new ZDateEdit { TabIndex = 0, BindTo = AutoDummyBizo.Schema.Z0_Date };
				form.Controls.Add(testDateEdit);
				form.SetDataBinding(dummy, "");
				form.Show();

				testDateEdit.Focus();
				SafeClipboard.Clear();
				testDateEdit.DateTextBox.Text = "2009 NOV 30";
				var mes = new Message();
				var action = new Action(() =>
				{
					testDateEdit.ProcessCmdKeyExposed(ref mes, Keys.Control | Keys.C);
					Application.DoEvents();
				});
				action.Invoke();
				System.Threading.Thread.Sleep(50);

				var text = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
				AssertEquals("2009 NOV 30", text);
				AssertEquals("2009 NOV 30", testDateEdit.DateTextBox.Text);

				SafeClipboard.Clear();
				action = new Action(() =>
				{
					testDateEdit.ProcessCmdKeyExposed(ref mes, Keys.Control | Keys.X);
					Application.DoEvents();
				}).Invoke;
				action.Invoke();
				System.Threading.Thread.Sleep(50);

				text = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
				AssertEquals("2009 NOV 30", text);
				AssertEquals("", testDateEdit.DateTextBox.Text);
			}
		}

		#endregion
	}
}
