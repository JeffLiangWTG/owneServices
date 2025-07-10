using System.Text;
using Enterprise.Registry.Business.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPreScreeningValue))]
	sealed class HVLVPreScreeningValueTest : RegistryBusinessObjectTemplateTestCase<HVLVPreScreeningValue>
	{
		public void TestScreeningComparisonOperatorsList_DefaultValueIsContains()
		{
			var validationRule = new HVLVPreScreeningRule();
			var validationField = new HVLVPreScreeningField(validationRule);
			var preScreeningValue = validationField.ScreeningValues.AddNew();

			AssertEquals(HVLVPreScreeningComparisonOperators.Codes.Contains, preScreeningValue.ScreeningComparisonOperatorCode);
		}

		public void TestScreeningComparisonOperatorCode_Validation()
		{
			var validationRule = new HVLVPreScreeningRule();
			var validationField = new HVLVPreScreeningField(validationRule);
			var preScreeningValue = validationField.ScreeningValues.AddNew();

			preScreeningValue.ScreeningComparisonOperatorDescription = "XXX";

			AssertHasErrors("Pre-screening comparison operator should have errors", preScreeningValue.ScreeningComparisonOperatorCodeInfo);
		}

		public void TestFromToHSCodeValidation()
		{
			var validationRule = new HVLVPreScreeningRule();
			var validationField = new HVLVPreScreeningField(validationRule);
			validationField.FieldDescription = "Origin HS Code";

			var screeningValue1 = validationField.ScreeningValues.AddNew();

			screeningValue1.FromHSCode = "";
			AssertHasErrors("Please enter a value.", screeningValue1.FromHSCodeInfo);

			screeningValue1.ToHSCode = "1234.56";
			AssertHasErrors("'To HS Code' can only be entered if 'From HS Code' is also entered.", screeningValue1.FromHSCodeInfo);

			screeningValue1.FromHSCode = "1234.58";
			AssertHasErrors("'From HS Code' must be less than 'To HS Code'.", screeningValue1.FromHSCodeInfo);

			screeningValue1.ToHSCode = "";
			AssertNoErrors(screeningValue1.FromHSCodeInfo);

			screeningValue1.ToHSCode = "1234.59";
			AssertNoErrors(screeningValue1.FromHSCodeInfo);

			screeningValue1.FromHSCode = "1234.56";
			screeningValue1.ToHSCode = "1234.58";
			AssertNoErrors(screeningValue1.FromHSCodeInfo);

			var screeningValue2 = validationField.ScreeningValues.AddNew();
			screeningValue2.FromHSCode = "1234.56";
			screeningValue2.ToHSCode = "1234.58";
			AssertHasErrors("Same 'To HS Code' and 'From HS Code' shared across multiple validations.", screeningValue2.FromHSCodeInfo);
		}

		public void TestChangeScreeningComparisonOperatorDescription_WillChangeScreeningComparisonOperatorCode()
		{
			var validationRule = new HVLVPreScreeningRule();
			var validationField = new HVLVPreScreeningField(validationRule);
			var preScreeningValue = validationField.ScreeningValues.AddNew();

			AssertEquals("Precondition:", HVLVPreScreeningComparisonOperators.Codes.Contains, preScreeningValue.ScreeningComparisonOperatorCode);

			preScreeningValue.ScreeningComparisonOperatorDescription = "Starts With";
			AssertEquals("The ScreeningComparisonOperatorCode should be updated to starts with according to description", HVLVPreScreeningComparisonOperators.Codes.StartsWith, preScreeningValue.ScreeningComparisonOperatorCode);

			preScreeningValue.ScreeningComparisonOperatorDescription = "Ends With";
			AssertEquals("The ScreeningComparisonOperatorCode should be updated to ends with according to description", HVLVPreScreeningComparisonOperators.Codes.EndsWith, preScreeningValue.ScreeningComparisonOperatorCode);
		}

		public void TestScreeningValueReadOnlyWhenFieldNameIsEmpty()
		{
			var validationRule = new HVLVPreScreeningRule();
			var validationField = new HVLVPreScreeningField(validationRule);
			var screeningValue = validationField.ScreeningValues.AddNew();

			Assert(validationField.FieldDescription.IsEmpty);
			Assert(screeningValue.ScreeningValueInfo.ReadOnly);

			validationField.FieldDescription = "Consignee";
			AssertNoErrors(validationField.FieldDescriptionInfo);
			Assert(!screeningValue.ScreeningValueInfo.ReadOnly);

			validationField.FieldDescription = "AA";
			AssertHasErrors(validationField.FieldDescriptionInfo);
			Assert(screeningValue.ScreeningValueInfo.ReadOnly);

			validationField.FieldDescription = "";
			Assert(screeningValue.ScreeningValueInfo.ReadOnly);
		}

		public void TestCheckDuplicateScreeningValues()
		{
			var validationRule = new HVLVPreScreeningRule();
			var validationField = new HVLVPreScreeningField(validationRule);
			var screeningValue1 = validationField.ScreeningValues.AddNew();
			screeningValue1.ScreeningValue = "AAA";
			var screeningValue2 = validationField.ScreeningValues.AddNew();
			screeningValue2.ScreeningValue = "AAA";

			screeningValue1.RunPreSaveValidation();

			AssertHasError(screeningValue1.ScreeningValueInfo, "The Screening Value has been duplicated and must be unique.");
			AssertHasError(screeningValue2.ScreeningValueInfo, "The Screening Value has been duplicated and must be unique.");

			screeningValue2.ScreeningValue = "BBB";
			screeningValue1.RunPreSaveValidation();
			screeningValue2.RunPreSaveValidation();

			AssertNoErrors(screeningValue1.ScreeningValueInfo);
			AssertNoErrors(screeningValue2.ScreeningValueInfo);
		}

		public void TestScreeningValuesFormattedForItemLineTariff()
		{
			var validationRule = new HVLVPreScreeningRule();

			var validationField1 = new HVLVPreScreeningField(validationRule);
			validationField1.FieldDescription = "Origin HS Code";
			var screeningValue1 = validationField1.ScreeningValues.AddNew();
			screeningValue1.FromHSCode = "123456";

			AssertEquals("Should have been formatted", "1234.56", screeningValue1.FromHSCode);

			validationRule.DestinationCountryCode = "AU";
			var validationField2 = new HVLVPreScreeningField(validationRule);
			validationField2.FieldDescription = "Destination HS Code";
			var screeningValue2 = validationField2.ScreeningValues.AddNew();
			screeningValue2.ToHSCode = "123456";

			AssertEquals("Should have been formatted", "1234.56", screeningValue2.ToHSCode);

			var validationField3 = new HVLVPreScreeningField(validationRule);
			validationField3.FieldDescription = "Consignee Phone";
			var screeningValue3 = validationField3.ScreeningValues.AddNew();
			screeningValue3.ScreeningValue = "123456";

			AssertEquals("Should not be formatted", "123456", screeningValue3.ScreeningValue);
		}

		public void TestScreeningCompareOptions_Contains()
		{
			var field = new HVLVPreScreeningField();

			var screeningValue = field.ScreeningValues.AddNew();
			screeningValue.ScreeningValue = "XYZ";
			screeningValue.ScreeningComparisonOperatorCode = HVLVPreScreeningComparisonOperators.Codes.Contains;

			var fieldValue1 = "ABCXYZ";
			var fieldValue2 = "YZ";

			AssertEquals(true, screeningValue.IsMatched(fieldValue1));
			AssertEquals(false, screeningValue.IsMatched(fieldValue2));
		}

		public void TestScreeningCompareOptions_ExactMatch()
		{
			var field = new HVLVPreScreeningField();

			var screeningValue = field.ScreeningValues.AddNew();
			screeningValue.ScreeningValue = "ABC";
			screeningValue.ScreeningComparisonOperatorCode = HVLVPreScreeningComparisonOperators.Codes.ExactMatch;

			var fieldValue1 = "ABC";
			var fieldValue2 = "XYZ";

			AssertEquals(true, screeningValue.IsMatched(fieldValue1));
			AssertEquals(false, screeningValue.IsMatched(fieldValue2));
		}

		public void TestScreeningCompareOptions_StartsWith()
		{
			var field = new HVLVPreScreeningField();

			var screeningValue = field.ScreeningValues.AddNew();
			screeningValue.ScreeningValue = "A";
			screeningValue.ScreeningComparisonOperatorCode = HVLVPreScreeningComparisonOperators.Codes.StartsWith;

			var fieldValue1 = "ABC";
			var fieldValue2 = "XYZ";

			AssertEquals(true, screeningValue.IsMatched(fieldValue1));
			AssertEquals(false, screeningValue.IsMatched(fieldValue2));
		}

		public void TestScreeningCompareOptions_EndsWith()
		{
			var field = new HVLVPreScreeningField();

			var screeningValue = field.ScreeningValues.AddNew();
			screeningValue.ScreeningValue = "C";
			screeningValue.ScreeningComparisonOperatorCode = HVLVPreScreeningComparisonOperators.Codes.EndsWith;

			var fieldValue1 = "ABC";
			var fieldValue2 = "XYZ";

			AssertEquals(true, screeningValue.IsMatched(fieldValue1));
			AssertEquals(false, screeningValue.IsMatched(fieldValue2));
		}

		public void TestFromToHSCodeMatch()
		{
			var field = new HVLVPreScreeningField();

			var screeningValue = field.ScreeningValues.AddNew();
			screeningValue.FromHSCode = "1234.56";
			screeningValue.ToHSCode = "1234.58";

			var fieldValue1 = "1234.56";
			var fieldValue2 = "1234.57";
			var fieldValue3 = "1234.46";
			var fieldValue4 = "1234.66";

			Assert(screeningValue.IsMatchedForHSCode(fieldValue1));
			Assert(screeningValue.IsMatchedForHSCode(fieldValue2));
			Assert(!screeningValue.IsMatchedForHSCode(fieldValue3));
			Assert(!screeningValue.IsMatchedForHSCode(fieldValue4));
		}

		public void TestReadElementsWithHVLVPreScreeningValueXml_InRandomOrder()
		{
			var xmlString1 = @"
<HVLVPreScreeningValue>
    <FromHSCode />
    <ScreeningValue>111</ScreeningValue>
    <ScreeningComparisonOperatorCode>Contains</ScreeningComparisonOperatorCode>
    <ToHSCode />
    <MessageTextPerValue>123</MessageTextPerValue>
</HVLVPreScreeningValue>";
			AssertReadElementsWithHVLVPreScreeningValueXml(xmlString1);

			var xmlString2 = @"
<HVLVPreScreeningValue>
    <ScreeningComparisonOperatorCode>Contains</ScreeningComparisonOperatorCode>
    <ScreeningValue>111</ScreeningValue>
    <ToHSCode />
    <MessageTextPerValue>123</MessageTextPerValue>
    <FromHSCode />
</HVLVPreScreeningValue>";
			AssertReadElementsWithHVLVPreScreeningValueXml(xmlString2);

			void AssertReadElementsWithHVLVPreScreeningValueXml(string xmlString)
			{
				var dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(ExpectedBusinessObjectType);
				var deserialisedBusinessObject = (HVLVPreScreeningValue)dummyDataType.Deserialise(Encoding.UTF8.GetBytes(xmlString));
				CombineAssertions(() =>
				{
					AssertEquals("111", deserialisedBusinessObject.ScreeningValue);
					AssertEquals("", deserialisedBusinessObject.FromHSCode);
					AssertEquals("", deserialisedBusinessObject.ToHSCode);
					AssertEquals("Contains", deserialisedBusinessObject.ScreeningComparisonOperatorCode);
					AssertEquals("123", deserialisedBusinessObject.MessageTextPerValue);
				});
			}
		}

		#region Implementation

		protected override HVLVPreScreeningValue GetBusinessObjectToClone()
		{
			return (HVLVPreScreeningValue)GetNewBusinessObject();
		}

		protected override HVLVPreScreeningValue GetBusinessObjectToSerialise()
		{
			return (HVLVPreScreeningValue)GetNewBusinessObject();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
