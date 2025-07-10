using System;
using System.Globalization;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business.eHub;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.CusTaskNames.Testing
{
	[TestedType(typeof(ScavengingSetting))]
	sealed class ScavengingSettingTest : RegistryBusinessObjectTemplateTestCase<ScavengingSetting>
	{
		public void TestValidation_TaskNameExists()
		{
			var setting = new ScavengingSetting();
			setting.ValidateTaskName();
			Assert(setting.HasErrors);
			AssertHasError(setting.TaskNameInfo, "Should have Task Name.");
		}

		public void TestValidation_TaskNameUnique()
		{
			var collection = new ScavengingSettingCollection(null, null);
			var setting1 = collection.AddNew();
			setting1.TaskName = "Task1";
			var setting2 = collection.AddNew();
			setting2.TaskName = "Task1";

			Assert(setting2.HasErrors);
			AssertHasError(setting2.TaskNameInfo, "Task Name already exists.");
		}

		[TestDate(2017, 11, 16, 16, 35, 00)]
		public void TestTryParseDateTimeStringWithinQueueIntervalMonths_WithFutureStringValues_ReturnsEmpty()
		{
			var possibleExistingDateTimeStrings = new[]
			{
				"2017/11/16 9:31:12 PM",
				"2018-11-17 1:31:12 AM",
				"17-12-2017 11:31:12 PM"
			};

			CombineAssertions(() =>
			{
				using (SetTemporaryAUCulture())
				{
					var setting = new ScavengingSetting();
					foreach (var dateTimeString in possibleExistingDateTimeStrings)
					{
						Assert(!setting.TryParseDateTimeStringWithinQueueIntervalMonths(dateTimeString, out var zDateTime));
						Assert(dateTimeString + " is in the future, should return empty", zDateTime.IsEmpty);
					}
				}
			});
		}

		[TestDate(2017, 11, 16)]
		public void TestTryParseDateTimeStringWithinQueueIntervalMonths_BeyondQueueIntervalMonthsStringValues_ReturnsEmpty()
		{
			var possibleExistingDateTimeStrings = new[]
			{
				"2016/11/15 1:31:12 AM",
				"2015-11-15 1:31:12 AM",
				"17-01-2017 11:31:12 PM"
			};

			CombineAssertions(() =>
			{
				using (SetTemporaryAUCulture())
				{
					var setting = new ScavengingSetting();
					foreach (var dateTimeString in possibleExistingDateTimeStrings)
					{
						Assert(!setting.TryParseDateTimeStringWithinQueueIntervalMonths(dateTimeString, out var zDateTime));
						Assert(dateTimeString + " is beyond 3 months, should return empty", zDateTime.IsEmpty);
					}
				}
			});
		}

		[TestDate(2017, 11, 16)]
		public void TestTryParseDateTimeStringWithinQueueIntervalMonths_WithPossibleExistingStringValues_ReturnsValidDateTime()
		{
			var possibleExistingDateTimeStrings = new[]
			{
				"2017/11/15 1:31:12 AM",
				"2017-11-15 1:31:12 AM",
				"07-11-2017 11:31:12 PM"
			};

			CombineAssertions(() =>
			{
				using (SetTemporaryAUCulture())
				{
					var setting = new ScavengingSetting();
					foreach (var dateTimeString in possibleExistingDateTimeStrings)
					{
						setting.TryParseDateTimeStringWithinQueueIntervalMonths(dateTimeString, out var zDateTime);
						Assert(dateTimeString + " should be a valid date time string", zDateTime.IsValid);
						AssertEquals("Result date time is UTC kind", DateTimeKind.Utc, zDateTime.Kind);
					}
				}
			});
		}

		public void TestTryParseUtcDateTimeString_WithUtcFormatStringValues_ReturnsValidDateTime()
		{
			var utcDateTimeStrings = new[]
			{
				"2004-11-25 15:25:59Z",
				"2004-11-25 15:25:59Z",
				"2008-06-15 21:15:07Z",
				"2017-11-07 15:25:59Z"
			};

			CombineAssertions(() =>
			{
				using (SetTemporaryAUCulture())
				{
					var setting = new ScavengingSetting();
					foreach (var dateTimeString in utcDateTimeStrings)
					{
						Assert(setting.TryParseUtcDateTimeString(dateTimeString, ScavengingSetting.GlobalizedUtcDateTimeFormat, out var zDateTime));
						Assert(dateTimeString + " should be a valid Utc date time string", zDateTime.IsValid);
						AssertEquals("Result date time is UTC kind", DateTimeKind.Utc, zDateTime.Kind);
					}
				}
			});
		}

		[TestDate(2017, 11, 16)]
		public void TestParseDateTimeString_WithInvalidFormatStringValues_ReturnsEmpty()
		{
			var invalidDateTimeStrings = new[]
			{
				"11/15/2017 1:31:12 PM",
				"11-15-2017 1:31:12 PM",
				"2004-11-25T15:25:59",
				"11/14/2017 4:52:55 AM",
				"11/15/2017 1:07:00 PM",
				"2008-04-10T06:30:00",
				"Monday, June 16, 2008 4:15:07 AM",
				"Sun, 15 Jun 2008 21:15:07 GMT",
				"Sunday, June 15, 2008 9:15:07 PM",
				"2008-06-15T21:15:07",
				"11-07-2017 1:31:12 AM"
			};

			CombineAssertions(() =>
			{
				using (SetTemporaryAUCulture())
				{
					var setting = new ScavengingSetting();
					foreach (var dateTimeString in invalidDateTimeStrings)
					{
						var zDateTime = setting.ParseDateTimeString(dateTimeString, ScavengingSetting.GlobalizedUtcDateTimeFormat);
						Assert(dateTimeString + " should return empty", zDateTime.IsEmpty);
					}
				}
			});
		}

		public void TestPeriodStartAndEnd_WithChangedRegions_RetrunsConsistentValue()
		{
			using (SetTemporaryAUCulture())
			{
				var settings = eHubMessagingRegistry.Instance.ScavengingTaskSettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				settings.Remove(settings.Get("Billing"));
				eHubMessagingRegistry.Instance.ScavengingTaskSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

				var originalBillingSetting = settings.AddNew();
				var utcNow = ZDateTime.UtcNow;
				utcNow = utcNow.AddMilliseconds(-utcNow.Millisecond);

				originalBillingSetting.TaskName = "Billing";
				originalBillingSetting.PeriodStart = utcNow;
				originalBillingSetting.PeriodEnd = utcNow.AddDays(7);
				eHubMessagingRegistry.Instance.ScavengingTaskSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

				Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

				var updatedSettings =
					eHubMessagingRegistry.Instance.ScavengingTaskSettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

				var updatedBillingSetting = updatedSettings.Get("Billing");
				Assert(originalBillingSetting.PeriodStart.Equals(updatedBillingSetting.PeriodStart));

				Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");

				updatedSettings =
					eHubMessagingRegistry.Instance.ScavengingTaskSettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

				updatedBillingSetting = updatedSettings.Get("Billing");
				Assert(originalBillingSetting.PeriodStart.Equals(updatedBillingSetting.PeriodStart));
			}
		}

		[TestDate(2017, 11, 16, 16, 35, 10)]
		public void TestToUtcDateTimeString()
		{
			var timeUtcNow = ZDateTime.UtcNow;
			var setting = new ScavengingSetting();
			var utcDateTimeString = setting.ToUtcDateTimeString(timeUtcNow);

			AssertEquals("2017-11-16 16:35:10Z", utcDateTimeString);
		}

		public void TestInvalidPeriodStartEnd_ResetPeriodEnd_ToQueueIntervalMonthsAgo()
		{
			const string testScavengingSetting =
				@"<?xml version='1.0' encoding='utf-16'?><ArrayOfScavengingSetting><ScavengingSetting><TaskName>Billing</TaskName><PeriodStart>10/18/2016 5:01:10 PM</PeriodStart><PeriodEnd>10/28/2016 3:01:10 AM</PeriodEnd></ScavengingSetting></ArrayOfScavengingSetting>";

			var timeQueueIntervalMonthsAgo = ZDateTime.UtcNow.AddMonths(-ScavengingSetting.PopulateQueueIntervalMonths);

			var settings = eHubMessagingRegistry.Instance.ScavengingTaskSettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			settings.RemoveAll();
			eHubMessagingRegistry.Instance.ScavengingTaskSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			WriteTestScavengingTaskSettings(testScavengingSetting);

			using (SetTemporaryAUCulture())
			{
				var updatedSettings =
					eHubMessagingRegistry.Instance.ScavengingTaskSettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

				var updatedBillingSetting = updatedSettings.Get("Billing");
				Assert(timeQueueIntervalMonthsAgo > updatedBillingSetting.PeriodEnd);
			}
		}

		#region ScavengingSettingCollectionProperty

		ScavengingSettingCollection ScavengingSettingCollectionProperty
		{
			get
			{
				if (scavengingSettingCollection == null)
				{
					scavengingSettingCollection =
						new ScavengingSettingCollection(
							new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
				}
				return scavengingSettingCollection;
			}
		}

		ScavengingSettingCollection scavengingSettingCollection;

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ScavengingSetting GetBusinessObjectToClone()
		{
			return new ScavengingSetting(
				new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory,
				ScavengingSettingCollectionProperty);
		}

		protected override ScavengingSetting GetBusinessObjectToSerialise()
		{
			return new ScavengingSetting() { TaskName = "TestTask", PeriodEnd = new ZDateTime(2011, 12, 11, 10, 50, 00), PeriodStart = new ZDateTime(2011, 12, 11, 10, 55, 00) };
		}

		#endregion

		#region Helpers

		IDisposable SetTemporaryAUCulture()
		{
			var savedCurrentCulture = Thread.CurrentThread.CurrentCulture;

			var testCulture = new CultureInfo("en-AU")
			{
				DateTimeFormat = new DateTimeFormatInfo
				{
					AMDesignator = "AM",
					DateSeparator = "/",
					FullDateTimePattern = "dddd, d MMMM yyyy h:mm:ss tt",
					LongDatePattern = "dddd, d MMMM yyyy",
					LongTimePattern = "h:mm:ss tt",
					MonthDayPattern = "d MMMM",
					PMDesignator = "PM",
					ShortDatePattern = "d/MM/yyyy",
					ShortTimePattern = "h:mm tt",
					TimeSeparator = ":",
					YearMonthPattern = "MMMM yyyy"
				}
			};

			Thread.CurrentThread.CurrentCulture = testCulture;

			return new DisposableAction(delegate { Thread.CurrentThread.CurrentCulture = savedCurrentCulture; });
		}

		void WriteTestScavengingTaskSettings(string settingsXml)
		{
			var query = @"
IF EXISTS (SELECT NULL FROM dbo.StmData WHERE SD_Name = @Name)
	UPDATE dbo.StmData SET SD_BinaryValue = @Value WHERE SD_Name = @Name
ELSE
	INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_BinaryValue) VALUES(NEWID(), 'ScavengingTaskSettings', 'BIN', @Value)
			";

			using (var cmd = TestConnection.Command(query))
			{
				var bytes = Encoding.Unicode.GetBytes(settingsXml);
				cmd.AddParameterBasedOnDbColumn("@Value", bytes, StmDataSchema.SD_BinaryValue);
				cmd.AddParameterBasedOnDbColumn("@Name", "ScavengingTaskSettings", StmDataSchema.SD_Name);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
