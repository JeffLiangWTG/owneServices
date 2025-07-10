namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	abstract class FitlerFieldTestWithClearValues : FilterFieldTest
	{
		public void TestClearValueForUnitTest()
		{
			var fields = GetFieldsWithValidClearValueTestCases();
			AssertEquals("You have supplied the wrong number of test cases", ExpectedNumberOfClearValueTestCases, fields.Length);
			foreach (var field in fields)
			{
				AssertEquals("Field is not empty", false, field.IsEmpty);
				field.ClearValueForUnitTest();
				AssertEquals("Field is now empty", true, field.IsEmpty);
			}
		}

		public void TestJsonSerializable()
		{
			var fields = GetFieldsWithValidClearValueTestCases();
			var field = fields[0];

			if (string.IsNullOrEmpty(field.DisplayName))
			{
				field.DisplayName = "Test Display Name";
			}

			if (string.IsNullOrEmpty(field.FieldName))
			{
				field.FieldName = "Test Field Name";
			}

			var jsonData = JsonConverterHelper.Serialize(field);
			AssertNotNullOrEmpty("FilterField should be serialized to Json", jsonData);

			var fieldType = FilterField.GetType();
			var newField = JsonConverterHelper.Deserialize(jsonData, fieldType) as FilterFieldWithUTSupport;
			AssertType(fieldType, newField);

			CombineAssertions("JsonData should be deserialized to FilterField", () =>
			{
				AssertEquals("DisplayName", field.DisplayName, newField.DisplayName);
				AssertEquals("FieldName", field.FieldName, newField.FieldName);
			});
		}

		public void TestSetJsonData()
		{
			var fields = GetFieldsWithValidClearValueTestCases();
			var field = fields[0];
			field.DisplayName = "Test Display Name";
			field.FieldName = "Test Field Name";

			var jsonData = ((IJsonSerializable)field).GetJsonData() as BaseFieldJsonData;

			CombineAssertions("GetJsonData method should call SetJsonData", () =>
			{
				AssertEquals("DisplayName", "Test Display Name", jsonData.DisplayName);
				AssertEquals("FieldName", "Test Field Name", jsonData.FieldName);
			});
		}

		public abstract void TestSafeCopyValuesFrom();

		public abstract void TestClearValues();

		public abstract FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases();

		public abstract int ExpectedNumberOfClearValueTestCases { get; }
	}
}
