using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class UpdateNodeTest : BaseUpdateNodeTest
	{
		#region TestSetFields
		[TestDate(2000, 01, 01)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestSetFields()
		{
			TestDateAttribute.UseUNLOCO = true;
			ZDateTime now = ZDateTime.Now;
			IOperationalActionFieldValuePair[] fieldValuePairs = new IOperationalActionFieldValuePair[] { new DummyOperationalActionFieldValuePair(Field(DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value")), new DummyOperationalActionFieldValuePair(Field(DummyBizoSchema.Z0_Date, "Date Field"), now.AddDays(5)), new DummyOperationalActionFieldValuePair(Field(DummyBizoSchema.Z0_Time, "Time Field"), new ZTime(1,2)), new DummyOperationalActionFieldValuePair(Field(DummyBizoSchema.Z0_DateTimeOffset, "DateTimeOffset Field"), new ZDateTimeOffset(now.AddDays(5))), new DummyOperationalActionFieldValuePair(Field(DummyBizoSchema.Z0_Geography, "Geography Field"), new ZGeography("POINT (-121 48)")), };
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_VarCharMax = "Old Value1";
			dummy1.Z0_Date = now.AddDays(-2);
			dummy1.Z0_Time = new TimeSpan(1,2,3);
			dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(now.AddDays(-2), TimeSpan.FromHours(5));
			dummy1.Z0_Geography = new ZGeography("POINT (-120 44)");
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_VarCharMax = "Old Value2";
			dummy2.Z0_Date = now.AddDays(-1);
			dummy2.Z0_Time = new TimeSpan(4, 5, 6);
			dummy2.Z0_DateTimeOffset = ZDateTimeOffset.Empty;
			dummy2.Z0_Geography = ZGeography.Empty;
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Z0_VarCharMax = "Old Value3";
			dummy3.Z0_Date = now;
			dummy3.Z0_Time = ZTime.Empty;
			dummy3.Z0_DateTimeOffset = ZDateTimeOffset.Empty;
			dummy3.Z0_Geography = ZGeography.Empty;
			RootUpdateNode root = new RootUpdateNode(dummy1.GetType());
			root.AddRange(fieldValuePairs);
			root.Apply(new BusinessObject[] { dummy1, dummy2 });
			AssertEquals("dummy1.Z0_VarCharMax", "New Value", dummy1.Z0_VarCharMax);
			AssertEquals("dummy1.Z0_Date", now.AddDays(5), dummy1.Z0_Date);
			AssertEquals("dummy1.Z0_Time", new TimeSpan(1,2,0), dummy1.Z0_Time);
			AssertEquals("dummy1.Z0_DateTimeOffset", new ZDateTimeOffset(now.AddDays(5), TimeSpan.FromHours(5)), dummy1.Z0_DateTimeOffset);
			AssertEquals("dummy1.Z0_Geography", new ZGeography("POINT (-121 48)"), dummy1.Z0_Geography);
			AssertEquals("dummy2.Z0_VarCharMax", "New Value", dummy2.Z0_VarCharMax);
			AssertEquals("dummy2.Z0_Date", now.AddDays(5), dummy2.Z0_Date);
			AssertEquals("dummy2.Z0_Time", new TimeSpan(1, 2, 0), dummy2.Z0_Time);
			AssertEquals("dummy2.Z0_DateTimeOffset", new ZDateTimeOffset(now.AddDays(5), TimeSpan.FromHours(11)), dummy2.Z0_DateTimeOffset);
			AssertEquals("dummy2.Z0_Geography", new ZGeography("POINT (-121 48)"), dummy2.Z0_Geography);
			AssertEquals("dummy3.Z0_VarCharMax", "Old Value3", dummy3.Z0_VarCharMax);
			AssertEquals("dummy3.Z0_Date", now.AddDays(0), dummy3.Z0_Date);
			AssertEquals("dummy3.Z0_Time", ZTime.Empty, dummy3.Z0_Time);
			AssertEquals("dummy3.Z0_DateTimeOffset", ZDateTimeOffset.Empty, dummy3.Z0_DateTimeOffset);
			AssertEquals("dummy3.Z0_Geography", ZGeography.Empty, dummy3.Z0_Geography);
		}

		#endregion
		#region TestSetCustomFields
		[TestDate(2000, 01, 01)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestSetCustomFields()
		{
			var now = ZDateTime.Now;
			var fieldValuePairs = new IOperationalActionFieldValuePair[] { new DummyOperationalActionFieldValuePair(new OperationalActionBooleanFieldSupporter("__CUSTOMFIELDBOOLEAN__prop__ZBool", false), ZBool.True), new DummyOperationalActionFieldValuePair(new OperationalActionDateFieldSupporter("__CUSTOMFIELDDATETIME__prop__ZDateTime", false), now), new DummyOperationalActionFieldValuePair(new OperationalActionNumericFieldSupporter("__CUSTOMFIELDDECIMAL__prop__ZDecimal", false, decimal.MinValue, decimal.MaxValue, 9, 3), new ZDecimal(100.11)), new DummyOperationalActionFieldValuePair(new OperationalActionNumericFieldSupporter("__CUSTOMFIELDINTEGER__prop__ZInt", false, int.MinValue, int.MaxValue, 10, 0), new ZInt(100)), new DummyOperationalActionFieldValuePair(new OperationalActionTextFieldSupporter("__CUSTOMFIELDSTRING__prop__ZString", false, 100), new ZString("New Value")), };
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_Name = "Template";
			var customField1 = template.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "customFieldBoolean";
			customField1.XC_Type = AddOnColumnDataType.Codes.Boolean;
			var customField2 = template.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "customFieldDatetime";
			customField2.XC_Type = AddOnColumnDataType.Codes.Datetime;
			var customField3 = template.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "customFieldDecimal";
			customField3.XC_Type = AddOnColumnDataType.Codes.Decimal;
			var customField4 = template.GenCustomColumnDefinitions.AddNew();
			customField4.XC_Name = "customFieldInteger";
			customField4.XC_Type = AddOnColumnDataType.Codes.Integer;
			var customField5 = template.GenCustomColumnDefinitions.AddNew();
			customField5.XC_Name = "customFieldString";
			customField5.XC_Type = AddOnColumnDataType.Codes.String;
			var dummy = Factory.New<DummyWithCustomFields>();
			Factory.Save();
			var root = new RootUpdateNode(dummy.GetType());
			root.AddRange(fieldValuePairs);
			root.Apply(new BusinessObject[] { dummy });
			var customBizo = dummy.GetCustomBusinessObject();
			AssertEquals(ZBool.True, customBizo["__CUSTOMFIELDBOOLEAN__prop__ZBool"]);
			AssertEquals(now, customBizo["__CUSTOMFIELDDATETIME__prop__ZDateTime"]);
			AssertEquals(new ZDecimal(100.11), customBizo["__CUSTOMFIELDDECIMAL__prop__ZDecimal"]);
			AssertEquals(new ZInt(100), customBizo["__CUSTOMFIELDINTEGER__prop__ZInt"]);
			AssertEquals("New Value", customBizo["__CUSTOMFIELDSTRING__prop__ZString"]);
		}

		#endregion
		#region TestSetFields_ReadOnlyFieldShouldNotBeSet
		public void TestSetFields_ReadOnlyFieldShouldNotBeSet()
		{
			IOperationalActionFieldValuePair[] fieldValuePairs = new IOperationalActionFieldValuePair[] { new DummyOperationalActionFieldValuePair(Field(DummyBizoSchema.Z0_VarCharMax, "Text Field"), new ZString("New Value")), new DummyOperationalActionFieldValuePair(Field(DummyBizoSchema.Z0_Description, "Text Field"), new ZString("New Description")), };
			DummyWithReadOnly dummy1 = Factory.New<DummyWithReadOnly>();
			dummy1.Z0_VarCharMax = "Old Value1";
			dummy1.Z0_Description = "dummy1";
			DummyWithReadOnly dummy2 = Factory.New<DummyWithReadOnly>();
			dummy2.Z0_VarCharMax = "Old Value2";
			dummy2.Z0_Description = "dummy2";
			dummy2.Z0_Description_ReadOnly = true;
			RootUpdateNode root = new RootUpdateNode(typeof(DummyWithReadOnly));
			root.AddRange(fieldValuePairs);
			root.Apply(new BusinessObject[] { dummy1 });
			Assert("No error on dummy1", !dummy1.HasErrors);
			AssertEquals("dummy1.Z0_VarCharMax", "New Value", dummy1.Z0_VarCharMax);
			AssertEquals("dummy1.Z0_Description", "New Description", dummy1.Z0_Description);
			root.Apply(new BusinessObject[] { dummy2 });
			Assert("Error on dummy2", dummy2.HasErrors);
			AssertEquals("dummy2.Z0_VarCharMax", "New Value", dummy2.Z0_VarCharMax);
			AssertEquals("dummy2.Z0_Description", "dummy2", dummy2.Z0_Description);
		}

		#endregion
		#region Setting Status Change Mode on Process Tasks
		public void TestSetFields_ShouldSetChangeModeToOperationalAction_WhenSettingStatusOnProcessTasks()
		{
			var task1 = Factory.NewWithValidTestData<ProcessTask>();
			var task2 = Factory.NewWithValidTestData<ProcessTask>();
			task1.P9_Status = "ASN";
			task2.P9_Status = "WRK";
			Factory.Save();
			var task1LastLogReference = task1.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			var task2LastLogReference = task2.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.Other}", task1LastLogReference);
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.Other}", task2LastLogReference);
			var fieldValuePairs = new IOperationalActionFieldValuePair[] { new DummyOperationalActionFieldValuePair(Field(ProcessTasksSchema.P9_Status, "Status"), new ZString("CLS")), };
			var root = new RootUpdateNode(task1.GetType());
			root.AddRange(fieldValuePairs);
			root.Apply(new BusinessObject[] { task1, task2 });
			Factory.Save();
			AssertEquals("CLS", task1.P9_Status);
			AssertEquals("CLS", task2.P9_Status);
			task1LastLogReference = task1.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			task2LastLogReference = task2.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.OperationalAction}", task1LastLogReference);
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.OperationalAction}", task2LastLogReference);
		}

		public void TestSetFields_ShouldNotSetChangeMode_WhenSettingOtherFieldsOnProcessTasks()
		{
			var task1 = Factory.NewWithValidTestData<ProcessTask>();
			var task2 = Factory.NewWithValidTestData<ProcessTask>();
			task1.P9_Status = "ASN";
			task2.P9_Status = "WRK";
			Factory.Save();
			var task1LastLogReference = task1.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			var task2LastLogReference = task2.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.Other}", task1LastLogReference);
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.Other}", task2LastLogReference);
			var fieldValuePairs = new IOperationalActionFieldValuePair[] { new DummyOperationalActionFieldValuePair(Field(ProcessTasksSchema.P9_Description, "Description"), new ZString("New description")), };
			var root = new RootUpdateNode(task1.GetType());
			root.AddRange(fieldValuePairs);
			root.Apply(new BusinessObject[] { task1, task2 });
			Factory.Save();
			AssertEquals("New description", task1.P9_Description);
			AssertEquals("New description", task2.P9_Description);
			task1LastLogReference = task1.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			task2LastLogReference = task2.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			AssertContains("Should not change change mode", $"CHM={ProcessTaskStatusChangeModeCodeList.Codes.Other}", task1LastLogReference);
			AssertContains("Should not change change mode", $"CHM={ProcessTaskStatusChangeModeCodeList.Codes.Other}", task2LastLogReference);
		}

		#endregion
		#region TestSetFields_NoExceptionThrown
		public void TestSetFields_NoExceptionThrown()
		{
			var dummy = Factory.New<DummyWithNoZPropertyInfo>();
			dummy.Name = "Old Value";
			var root = new RootUpdateNode(typeof(DummyWithNoZPropertyInfo));
			root.Add(new DummyOperationalActionFieldValuePair(new OperationalActionTextFieldSupporter("Name", false, 10), new ZString("New Value")));
			AssertNoExceptionThrown(() => root.Apply(new BusinessObject[] { dummy }));
			Assert(!dummy.HasErrors);
			AssertEquals("New Value", dummy.Name);
		}

		public void TestSetFields_SelectsFromLookup()
		{
			var dummy = Factory.New<DummyWithListAttribute>();
			dummy.Z0_Code = "AAA";
			var root = new RootUpdateNode(typeof(DummyWithListAttribute));
			root.Add(new DummyOperationalActionFieldValuePair(new OperationalActionTextFieldSupporter("Z0_Code", false, 3), new ZString("bbb")));
			AssertNoExceptionThrown(() => root.Apply(new BusinessObject[] { dummy }));
			Assert(!dummy.HasErrors);
			AssertEquals("BBB", dummy.Z0_Code);
		}
		#endregion
	}
}
