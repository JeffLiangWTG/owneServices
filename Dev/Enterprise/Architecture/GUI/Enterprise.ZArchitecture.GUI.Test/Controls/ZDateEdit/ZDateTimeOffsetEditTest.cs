using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDateTimeOffsetEditTest : TestCaseWithFactory
	{
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestDateTimeOffsetToString()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;

			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-AU")))
			using (RawDataRegistry.Instance.DisplayUtcOffset.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dateTimeOffset = new ZDateTimeOffset(2018, 1, 2, 5, 26, 0, TimeSpan.FromHours(10));
				using var dateTimeOffsetEdit = new ZDateTimeOffsetEdit();
				dateTimeOffsetEdit.DateTimeOffsetValue = dateTimeOffset;

				AssertEquals("Display offset", "02-JAN-18 05:26 GMT+10:00", dateTimeOffsetEdit.Text);
				AssertEquals("Hide offset", "02-JAN-18 05:26", dateTimeOffsetEdit.DateTimeOffsetToString(dateTimeOffset, true));

				dateTimeOffsetEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
				AssertEquals("Display date only", "02-JAN-18", dateTimeOffsetEdit.DateTimeOffsetToString(dateTimeOffset, true));
				AssertEquals("Display date only", "02-JAN-18", dateTimeOffsetEdit.DateTimeOffsetToString(dateTimeOffset));
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestTimeOffSetEditIsEditableAndTextSetsOffsetCorrectly()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-AU")))
			using (RawDataRegistry.Instance.DisplayUtcOffset.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var validDateTimeOffset = new ZDateTimeOffset(2018, 1, 2, 5, 26, 0, TimeSpan.FromHours(10));
				using (var dateTimeOffsetEdit = new ZDateTimeOffsetEdit())
				{
					dateTimeOffsetEdit.DateTimeOffsetValue = validDateTimeOffset;
					AssertEquals("02-JAN-18 05:26 GMT+10:00", dateTimeOffsetEdit.DateTextBox.Text);

					dateTimeOffsetEdit.DateTextBox.Text = "02-JAN-18 05:26 GMT-10:00";
					AssertEquals("Setting text field of the offset edit to a valid offset should set the date edit offset", new TimeSpan(-10, 0, 0), dateTimeOffsetEdit.DateTimeOffsetValue.Offset);

					dateTimeOffsetEdit.DateTextBox.Text = "02-JAN-18 05:26 GMT+99:00";
					AssertEquals("Setting text field of the offset edit to an invalid offset should set the date edit offset to invalid", ZDateTimeOffset.Invalid, dateTimeOffsetEdit.DateTimeOffsetValue);
				}
			}
		}

		public void TestOpenCalendarFormDuringDbTransaction()
		{
			using (new DisposableAction(() => Db.Connection.BeginTransaction(), () => Db.Connection.RollbackTransaction()))
			{
				using (new ZDateTimeOffsetEdit()) { }

				Assert($"Error reported: {ErrorReporter.LastMessageReported}", ErrorReporter.LastMessageReported.IsNullOrEmpty());
			}
		}
	}
}
