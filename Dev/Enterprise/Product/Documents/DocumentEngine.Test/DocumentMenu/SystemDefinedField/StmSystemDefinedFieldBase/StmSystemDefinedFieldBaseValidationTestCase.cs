using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	abstract class StmSystemDefinedFieldBaseValidationTestCase : StmSystemDefinedFieldValidationTestCase
	{
		public void TestCheckS1_Category()
		{
			AssertNoErrors("Precondition: S1_Category should not have errors.", Parent.S1_CategoryInfo);

			Parent.S1_Category = "";
			AssertHasError(Parent.S1_CategoryInfo, "Please enter a Category.");

			Parent.S1_Category = "Category";
			AssertNoErrors(Parent.S1_CategoryInfo);
		}

		public void TestCheckS1_DisplayEditRule()
		{
			AssertNoErrors("Precondition: S1_DisplayEditRule should not have errors.", Parent.S1_DisplayEditRuleInfo);
			AssertNotNullOrEmpty("Precondition: DisplayEditRules[0].Code should not be empty.", Parent.Lookups.DisplayEditRules[0].Code);

			Parent.S1_DisplayEditRule = "!@#";
			AssertHasError(Parent.S1_DisplayEditRuleInfo, "Enter a valid " + Parent.S1_DisplayEditRuleInfo.Description + ".");

			Parent.S1_DisplayEditRule = "";
			AssertHasError(Parent.S1_DisplayEditRuleInfo, "Please enter a " + Parent.S1_DisplayEditRuleInfo.Description + ".");

			Parent.S1_DisplayEditRule = Parent.Lookups.DisplayEditRules[0].Code;
			AssertNoErrors(Parent.S1_DisplayEditRuleInfo);
		}

		public void TestCheckS1_Hint()
		{
			AssertNoErrors("Precondition: S1_Hint should not have errors.", Parent.S1_HintInfo);

			Parent.S1_Hint = "";
			AssertHasError(Parent.S1_HintInfo, "Please enter a Hint.");

			Parent.S1_Hint = "Hint";
			AssertNoErrors(Parent.S1_HintInfo);
		}

		public void TestCheckS1_Name()
		{
			StmSystemDefinedFieldCollection collection = new StmSystemDefinedFieldCollection(Factory);

			StmSystemDefinedField field1 = collection.AddNew();
			StmSystemDefinedField field2 = collection.AddNew();

			AssertNoErrors("Precondition: Field1.S1_Name should not have errors.", field1.S1_NameInfo);
			AssertNoErrors("Precondition: Field2.S1_Name should not have errors.", field2.S1_NameInfo);

			field1.S1_Name = "";
			AssertHasError(field1.S1_NameInfo, "Please enter a Name.");

			field1.S1_Name = "Name";
			AssertNoErrors(field1.S1_NameInfo);

			field2.S1_Name = "Name";
			AssertHasError(field2.S1_NameInfo, "The Name has been duplicated and must be unique.");

			field2.S1_Name = "Something";
			AssertNoErrors(field2.S1_NameInfo);
		}

		public void TestCheckS1_Precision()
		{
			AssertNoErrors("Precondition: S1_Precision should not have errors.", Parent.S1_PrecisionInfo);

			Parent.S1_Type = "TXT";
			Parent.S1_Precision = 1m;
			AssertHasError(Parent.S1_PrecisionInfo, "Please do not enter a Precision.");

			Parent.S1_Precision = 0m;
			AssertNoErrors(Parent.S1_PrecisionInfo);

			Parent.S1_Type = "DEC";
			Parent.S1_Precision = 0.9m;
			AssertHasError(Parent.S1_PrecisionInfo, "Please enter a 'Precision' within the range 1.0 to 9.3.");

			Parent.S1_Precision = 10m;
			AssertHasError(Parent.S1_PrecisionInfo, "Please enter a 'Precision' within the range 1.0 to 9.3.");

			Parent.S1_Precision = 8.4m;
			AssertHasError(Parent.S1_PrecisionInfo, "Please enter a fractional part within the range .0 to .3.");

			Parent.S1_Precision = 2.3m;
			AssertHasError(Parent.S1_PrecisionInfo, "Please enter a Precision that is larger than its fractional part.");

			Parent.S1_Precision = 9.3m;
			AssertNoErrors(Parent.S1_PrecisionInfo);

			Parent.S1_Precision = 1.0m;
			AssertNoErrors(Parent.S1_PrecisionInfo);
		}

		public void TestCheckS1_Type()
		{
			AssertNoErrors("Precondition: S1_Type should not have errors.", Parent.S1_TypeInfo);
			AssertNotNullOrEmpty("Precondition: Types[0].Code should not be empty.", Parent.Lookups.Types[0].Code);

			Parent.S1_Type = "";
			AssertHasError(Parent.S1_TypeInfo, "Please enter a Type.");

			Parent.S1_Type = "!@#";
			AssertHasError(Parent.S1_TypeInfo, "Enter a valid Type.");

			Parent.S1_Type = Parent.Lookups.Types[0].Code;
			AssertNoErrors(Parent.S1_TypeInfo);
		}

		public void TestCheckS1_Validation()
		{
			AssertNoErrors("Precondition: S1_Validation should not have errors.", Parent.S1_ValidationInfo);

			Parent.S1_Validation = "";
			AssertHasError(Parent.S1_ValidationInfo, "Please enter a Validation Rule.");

			Parent.S1_Validation = "!@#";
			AssertHasError(Parent.S1_ValidationInfo, "Enter a valid Validation Rule.");

			Parent.S1_Type = "DAT";
			Parent.S1_Validation = "RVL";
			AssertHasError(Parent.S1_ValidationInfo, "'Range Value Required' is only valid if Type is 'Text', 'Integer' or 'Decimal'.");

			Parent.S1_Validation = "VAL";
			AssertNoErrors(Parent.S1_ValidationInfo);
		}

		public void TestSettingTypeValidatesOtherProperties()
		{
			Parent.S1_Type = "DEC";

			Parent.S1_Validation = "RVL";
			Parent.S1_Precision = 1.1m;
			Parent.S1_LowerValue = 2.22m;
			Parent.S1_UpperValue = 11.1m;

			AssertNoErrors(Parent.S1_ValidationInfo);
			AssertHasErrors(Parent.S1_LowerValueInfo);
			AssertHasErrors(Parent.S1_UpperValueInfo);

			Parent.S1_Type = "INT";
			AssertNoErrors(Parent.S1_ValidationInfo);
			AssertNoErrors(Parent.S1_LowerValueInfo);
			AssertNoErrors(Parent.S1_UpperValueInfo);

			Parent.S1_Type = "GRD";
			Parent.S1_Type = "DAT";
			AssertHasErrors(Parent.S1_ValidationInfo);
			AssertNoErrors(Parent.S1_LowerValueInfo);
			AssertNoErrors(Parent.S1_UpperValueInfo);
		}

		#region Order

		protected void TestCheckOrder(ZPropertyInfo propertyInfo)
		{
			AssertNoErrors(string.Format("Precondition: {0} should not have errors.", propertyInfo.Description), propertyInfo);
			string propertyName = propertyInfo.Name;

			propertyInfo.Value = (ZShort)0;
			AssertHasError(propertyInfo, "Please enter an 'Order' greater than 0.");

			propertyInfo.Value = (ZShort)1;
			AssertNoErrors(propertyInfo);

			BusinessObjectCollection collection = GetNewCollection();

			collection.Add(Parent);
			StmSystemDefinedFieldBase field1 = (StmSystemDefinedFieldBase)collection.AddNew();
			StmSystemDefinedFieldBase field2 = (StmSystemDefinedFieldBase)collection.AddNew();
			StmSystemDefinedFieldBase field3 = (StmSystemDefinedFieldBase)collection.AddNew();

			propertyInfo.Value = (ZShort)1;
			field1[propertyName] = (ZShort)1;
			field2[propertyName] = (ZShort)2;
			field3[propertyName] = (ZShort)4;

			AssertNoErrors(propertyInfo);
			AssertHasError(field1.ZPropertyInfoHash[propertyName], "Please enter a unique Order.");
			AssertNoErrors(field2.ZPropertyInfoHash[propertyName]);
			AssertHasError(field3.ZPropertyInfoHash[propertyName], "The Order is out of sequence as there is no Order number 3. Please adjust the Order numbers so that they are in sequence.");
		}

		public abstract void TestCheckOrder();
		protected abstract BusinessObjectCollection GetNewCollection();

		#endregion

		#region S1_Default

		public void TestCheckS1_Default()
		{
			AssertNoErrors("Precondition: S1_Default should not have errors.", Parent.S1_DefaultInfo);

			Parent.Lookups.Defaults.AddPair("STR", "ZString");
			Parent.Lookups.Defaults.AddPair("BYT", "ZByte");
			Parent.Lookups.Defaults.AddPair("DAT", "ZDateTime");
			Parent.Lookups.Defaults.AddPair("DEC", "ZDecimal");
			Parent.Lookups.Defaults.AddPair("INT", "ZInt");
			Parent.Lookups.Defaults.AddPair("SHR", "ZShort");
			Parent.Lookups.Defaults.AddPair("BOL", "ZBool");

			Parent.S1_Type = "TXT";
			Parent.S1_Default = "!@#";
			AssertHasError(Parent.S1_DefaultInfo, "Enter a valid Default Value.");
			AssertValidS1_Default(Parent, new string[] { "STR", "BYT", "DAT", "DEC", "INT", "SHR", "BOL", }, System.Array.Empty<string>(), "");

			Parent.S1_Type = "DAT";
			AssertValidS1_Default(Parent, new string[] { "DAT" }, new string[] { "STR", "BYT", "DEC", "INT", "SHR", "BOL", }, "Please select a Date/Time field.");

			Parent.S1_Type = "DTM";
			AssertValidS1_Default(Parent, new string[] { "DAT" }, new string[] { "STR", "BYT", "DEC", "INT", "SHR", "BOL", }, "Please select a Date/Time field.");

			Parent.S1_Type = "INT";
			AssertValidS1_Default(Parent, new string[] { "BYT", "INT", "SHR" }, new string[] { "STR", "DAT", "DEC", "BOL", }, "Please select a Byte, Integer or Short field.");

			Parent.S1_Type = "DEC";
			AssertValidS1_Default(Parent, new string[] { "DEC" }, new string[] { "STR", "DAT", "BYT", "INT", "SHR", "BOL", }, "Please select a Decimal field.");

			Parent.S1_Type = "BLN";
			AssertValidS1_Default(Parent, new string[] { "BOL" }, new string[] { "STR", "DAT", "BYT", "INT", "SHR", "DEC" }, "Please select a Boolean field.");
		}

		void AssertValidS1_Default(AutoStmSystemDefinedField field, string[] validValues, string[] invalidValues, string errorMessage)
		{
			foreach (string value in validValues)
			{
				field.S1_Default = value;
				AssertNoErrors(field.S1_DefaultInfo);
			}

			foreach (string value in invalidValues)
			{
				field.S1_Default = value;
				AssertHasError(field.S1_DefaultInfo, errorMessage);
			}

			field.S1_Default = "";
			AssertNoErrors(field.S1_DefaultInfo);
		}

		#endregion

		#region S1_Lower/UpperValue

		public void TestCheckS1_LowerValueIsWithinRangeAndPrecision()
		{
			TestRangeValueIsWithinRangeAndPrecision(Parent.S1_LowerValueInfo, "a", "Lower Value");
		}

		public void TestCheckS1_ValueIsWithinRangeAndPrecision()
		{
			TestRangeValueIsWithinRangeAndPrecision(Parent.S1_UpperValueInfo, "an", "Upper Value");
		}

		public void TestCheckS1_LowerValueIsLessThanS1_UpperValue()
		{
			AssertNoErrors("Precondition: S1_LowerValue should not have errors.", Parent.S1_LowerValueInfo);
			AssertNoErrors("Precondition: S1_UpperValue should not have errors.", Parent.S1_UpperValueInfo);

			Parent.S1_Validation = "RVL";
			string[] typesWithValidation = new string[] { "TXT", "INT", "DEC" };

			foreach (string type in typesWithValidation)
			{
				Parent.S1_Type = type;
				Parent.S1_Precision = 9.2m;

				using (Parent.SuspendValidationTesting())
				{
					Parent.S1_LowerValue = 0m;
					Parent.S1_UpperValue = 0m;

					Parent.S1_LowerValueInfo.ClearAllNotifications();
					Parent.S1_UpperValueInfo.ClearAllNotifications();
				}

				Parent.S1_LowerValue = 3m;
				AssertHasError(Parent.S1_LowerValueInfo, "Please enter a value less than the Upper Value.");
				AssertNoErrors(Parent.S1_UpperValueInfo);

				Parent.S1_UpperValue = 3m;
				AssertHasError(Parent.S1_LowerValueInfo, "Please enter a value less than the Upper Value.");
				AssertHasError(Parent.S1_UpperValueInfo, "Please enter a value greater than the Lower Value.");

				Parent.S1_LowerValue = 4m;
				AssertHasError(Parent.S1_LowerValueInfo, "Please enter a value less than the Upper Value.");
				AssertHasError(Parent.S1_UpperValueInfo, "Please enter a value greater than the Lower Value.");

				Parent.S1_LowerValue = 2m;
				AssertNoErrors(Parent.S1_LowerValueInfo);
				AssertNoErrors(Parent.S1_UpperValueInfo);

				Parent.S1_UpperValue = 1m;
				AssertNoErrors(Parent.S1_LowerValueInfo);
				AssertHasError(Parent.S1_UpperValueInfo, "Please enter a value greater than the Lower Value.");

				Parent.S1_UpperValue = 4m;
				AssertNoErrors(Parent.S1_LowerValueInfo);
				AssertNoErrors(Parent.S1_UpperValueInfo);
			}

			string[] typesWithNoValidation = new string[] { "DTM", "DAT", "GRD", "COL" };

			foreach (string type in typesWithNoValidation)
			{
				Parent.S1_Type = type;
				Parent.S1_LowerValue = 0m;
				Parent.S1_UpperValue = 0m;
				AssertNoErrors(Parent.S1_LowerValueInfo);
				AssertNoErrors(Parent.S1_UpperValueInfo);
			}
		}

		void TestRangeValueIsWithinRangeAndPrecision(ZPropertyInfo propertyInfo, string article, string description)
		{
			AssertNoErrors(string.Format("Precondition: {0} should not have errors.", propertyInfo.Name), propertyInfo);

			Parent.S1_Type = "DAT";
			Parent.S1_Validation = "VAL";
			propertyInfo.Value = (ZDecimal)1m;
			AssertHasError(propertyInfo, string.Format("Please do not enter {0} {1}.", article, description));

			Parent.S1_Validation = "RVL";
			propertyInfo.Value = (ZDecimal)3m;
			AssertHasError(propertyInfo, string.Format("Please do not enter {0} {1}.", article, description));

			Parent.S1_Type = "TXT";
			propertyInfo.Value = (ZDecimal)1234567m;
			AssertHasError(propertyInfo, string.Format("The number 1,234,567 is too large, the maximum value allowed for {0} is 999,999.999.", propertyInfo.Description));

			Parent.S1_Type = "INT";
			propertyInfo.Value = (ZDecimal)1234567m;
			AssertHasError(propertyInfo, string.Format("The number 1,234,567 is too large, the maximum value allowed for {0} is 999,999.999.", propertyInfo.Description));

			Parent.S1_Type = "DEC";
			Parent.S1_Precision = 9.9m;
			propertyInfo.Value = (ZDecimal)0m;
			AssertHasError(propertyInfo, "Please fix the errors on Precision first before changing this value.");

			Parent.S1_Precision = 9.3m;
			propertyInfo.Value = new ZDecimal(-1);
			AssertHasError(propertyInfo, string.Format("Please enter {0} '{1}' greater than or equal to 0.", article, description));

			Parent.S1_Precision = 7.3m;
			propertyInfo.Value = (ZDecimal)12345m;
			AssertHasError(propertyInfo, string.Format("The number 12,345 is too large, the maximum value allowed for {0} is 9,999.999.", propertyInfo.Description));
		}

		#endregion
	}
}
