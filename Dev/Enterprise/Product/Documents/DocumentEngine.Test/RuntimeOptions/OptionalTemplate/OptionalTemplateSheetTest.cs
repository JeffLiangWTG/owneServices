using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(OptionalTemplateSheet))]
	sealed class OptionalTemplateSheetTest : FilterFieldTest
	{
		public void TestSettingSelectedShouldRunValidation()
		{
			var sheet = new OptionalTemplateSheet(Factory, "Test Sheet");
			sheet.Validators.Add(new DummyValidatorThatIsAlwaysInvalid());
			AssertEquals("Pre-condition: sheet.SelectedInfo.HasError(\"There was an error with filter field [Test Sheet].\")", false, sheet.SelectedInfo.HasError("There was an error with filter field [Test Sheet]."));
			sheet.Selected = true;
			AssertEquals("sheet.SelectedInfo.HasError(\"There was an error with filter field [Test Sheet].\")", true, sheet.SelectedInfo.HasError("There was an error with filter field [Test Sheet]."));
		}

		public void TestJsonConverter()
		{
			var sheet = new OptionalTemplateSheet(Factory, "Fred");
			sheet.Selected = true;
			sheet.FieldName = "MyFieldName";
			sheet.DisplayName = "FredDysplayName";

			var result = JsonConverterHelper.Serialize(sheet);
			var deserialisedField = JsonConverterHelper.Deserialize<OptionalTemplateSheet>(result);

			AssertEquals("Fred", deserialisedField.Name);
			AssertEquals("FredDysplayName", deserialisedField.DisplayName);
			AssertEquals(true, deserialisedField.Selected);
			AssertEquals("MyFieldName", deserialisedField.FieldName);
		}

		public void TestNameDiplayNameAndSelected()
		{
			OptionalTemplateSheet sheet = new OptionalTemplateSheet(Factory, "Fred");
			AssertEquals("Sheet name is fred", "Fred", sheet.Name);
			AssertEquals("Sheet Display name is fred", "Fred", sheet.DisplayName);
			AssertEquals("Sheet defaults to unselected", false, sheet.Selected);

			sheet.Selected = true;
			AssertEquals("Sheet returns selected value", true, sheet.Selected);

			sheet.Selected = false;
			AssertEquals("Sheet returns selected value", false, sheet.Selected);
		}

		#region Implementation

		class DummyValidatorThatIsAlwaysInvalid : FilterCollectionValidator
		{
			public override bool IsValid(FilterField filterToValidate)
			{
				return false;
			}

			public override string GetErrorMessage(FilterField filterToValidate)
			{
				return string.Format("There was an error with filter field [{0}].", filterToValidate.DisplayNameLocalized);
			}
		}

		#endregion
	}
}
