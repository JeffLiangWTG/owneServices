using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(SecurityFilterField))]
	sealed class SecurityFilterFieldTest : FilterFieldTest
	{
		SecurityFilterField field;

		SecurityFilterField Field
		{
			get
			{
				if (field == null)
				{
					field = (SecurityFilterField)GetNewBusinessObject();
					field.DisplayName = "Security Right";
					field.FieldName = "SecurityRight";
				}
				return field;
			}
		}

		public void TestIsEmpty()
		{
			Field.FilterContainer.LookupKey = CheckpointLookupKey.Empty;
			Field.GroupBySummary = true;
			Field.HideDeniedRights = false;
			AssertEquals("IsEmpty", true, Field.IsEmpty);

			Field.FilterContainer.LookupKey = new CheckpointLookupKey("x");
			AssertEquals("IsEmpty", false, Field.IsEmpty);

			Field.FilterContainer.LookupKey = CheckpointLookupKey.Empty;
			Field.GroupByStaff = true;
			AssertEquals("IsEmpty", false, Field.IsEmpty);

			Field.GroupBySummary = true;
			Field.HideDeniedRights = true;
			AssertEquals("IsEmpty", false, Field.IsEmpty);
		}

		public void TestFilterContainerIsRegistered()
		{
			AssertEquals("FilterContainer should be a registered child.", true, Field.IsRegisteredEditableChildObject(Field.FilterContainer));
		}

		public void TestGroupByStaffAndSummary()
		{
			AssertEquals("GroupByStaff", false, Field.GroupByStaff);
			AssertEquals("GroupBySummary", true, Field.GroupBySummary);

			Field.GroupBySummary = false;
			AssertEquals("GroupByStaff", true, Field.GroupByStaff);
			AssertEquals("GroupBySummary", false, Field.GroupBySummary);

			Field.GroupBySummary = true;
			AssertEquals("GroupByStaff", false, Field.GroupByStaff);
			AssertEquals("GroupBySummary", true, Field.GroupBySummary);

			Field.GroupByStaff = true;
			AssertEquals("GroupByStaff", true, Field.GroupByStaff);
			AssertEquals("GroupBySummary", false, Field.GroupBySummary);

			Field.GroupByStaff = false;
			AssertEquals("GroupByStaff", false, Field.GroupByStaff);
			AssertEquals("GroupBySummary", true, Field.GroupBySummary);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1136:DoNotUseSystemRuntimeSerializationFormattersBinary", Justification = "WI00700503 - Pending migration")]
		public void TestHideDeniedRightsNotExists()
		{
			var serializedSecurityFilterFieldWithoutHideDeniedRights =
				@"{
					""SecurityRight"": ""Boo"",
					""ItemGuid"": ""43f9fae4-3b92-45cc-b35c-a8f117601cce"",
					""GroupByStaff"": false,
					""DisplayName"": ""Security Right"",
					""FieldName"": ""SecurityRight""
				}";

			var deserialisedField = JsonConverterHelper.Deserialize<SecurityFilterField>(serializedSecurityFilterFieldWithoutHideDeniedRights);
			AssertEquals("", false, deserialisedField.HideDeniedRights);
		}

		public void TestJsonConverter()
		{
			var lookupKey = new CheckpointLookupKey(Env.Security.Forwarding.Code);
			Field.FilterContainer.LookupKey = lookupKey;
			Field.GroupBySummary = true;

			var result = JsonConverterHelper.Serialize(Field);
			var deserialisedField = JsonConverterHelper.Deserialize<SecurityFilterField>(result);

			AssertEquals("DisplayName", "Security Right", deserialisedField.DisplayName);
			AssertEquals("FieldName", "SecurityRight", deserialisedField.FieldName);
			AssertEquals("FilterContainer.LookupKey", lookupKey, deserialisedField.FilterContainer.LookupKey);
			AssertEquals("GroupByStaff", false, deserialisedField.GroupByStaff);
			AssertEquals("GroupBySummary", true, deserialisedField.GroupBySummary);
			AssertEquals("ValueAsObject", Env.Security.Forwarding.HumanReadableName.GetUnresolvedString(), deserialisedField.ValueAsObject);

			Field.GroupByStaff = true;

			result = JsonConverterHelper.Serialize(Field);
			deserialisedField = JsonConverterHelper.Deserialize<SecurityFilterField>(result);

			AssertEquals("DisplayName", "Security Right", deserialisedField.DisplayName);
			AssertEquals("FieldName", "SecurityRight", deserialisedField.FieldName);
			AssertEquals("FilterContainer.LookupKey", lookupKey, deserialisedField.FilterContainer.LookupKey);
			AssertEquals("GroupByStaff", true, deserialisedField.GroupByStaff);
			AssertEquals("GroupBySummary", false, deserialisedField.GroupBySummary);
			AssertEquals("ValueAsObject", Env.Security.Forwarding.HumanReadableName.GetUnresolvedString(), deserialisedField.ValueAsObject);
		}

		public void TestSuggestedUserControlType()
		{
			AssertEquals("SuggestedUserControlType", FilterFieldSuggestedUserControlType.SecurityFilterControl, Field.SuggestedUserControlType);
		}

		public void TestSafeCopyValuesFrom()
		{
			SecurityFilterField source = new SecurityFilterField(Factory);
			source.FilterContainer.LookupKey = new CheckpointLookupKey("ABC", Guid.NewGuid());
			source.GroupByStaff = true;
			SecurityFilterField destination = new SecurityFilterField(Factory);
			((IFilter)destination).SafeCopyValuesFrom(source);
			AssertEquals(source.FilterContainer.LookupKey.Code, destination.FilterContainer.LookupKey.Code);
			AssertEquals(source.FilterContainer.LookupKey.ItemGuid, destination.FilterContainer.LookupKey.ItemGuid);
			AssertEquals(source.GroupByStaff, destination.GroupByStaff);
		}

		public void TestClearValues()
		{
			SecurityFilterField field = new SecurityFilterField(Factory);
			field.FilterContainer.LookupKey = new CheckpointLookupKey("ABC", Guid.NewGuid());
			field.GroupByStaff = true;
			((IFilter)field).ClearValues();
			AssertEquals(CheckpointLookupKey.Empty.Code, field.FilterContainer.LookupKey.Code);
			AssertEquals(CheckpointLookupKey.Empty.ItemGuid, field.FilterContainer.LookupKey.ItemGuid);
			AssertEquals(ZBool.False, field.GroupByStaff);
		}

		public void TestSecurityFilterField()
		{
			var reportFilterData = new ReportFilterData();
			var field = new SecurityFilterField(Factory);
			field.FilterContainer.LookupKey = new CheckpointLookupKey("aBcDeF", Guid.NewGuid());
			field.FillFilterData(reportFilterData);
			AssertEquals("SecurityFilterCollection should have one SecurityFilterField", 1, reportFilterData.SecurityFilterCollection.Count);
			AssertEquals("SelectedValue of SecurityFilterField should have the upper value", "ABCDEF", reportFilterData.SecurityFilterCollection[0].SelectedValue);
		}
	}
}
