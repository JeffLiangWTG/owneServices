using System;
using System.Reflection;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Registry.Business.HVLVPreScreeningRule;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPreScreeningField))]
	sealed class HVLVPreScreeningFieldTest : RegistryBusinessObjectTemplateTestCase<HVLVPreScreeningField>
	{
		public void TestPropertiesValidation()
		{
			CombineAssertions("Validation", () =>
			{
				AssertNoErrors(BizObj.FieldDescriptionInfo);
				AssertNoErrors(BizObj.ValidationRuleInfo);

				BizObj.FieldDescription = "";
				AssertHasErrors(BizObj.FieldDescriptionInfo);
				BizObj.FieldDescription = "AAA";
				AssertHasErrors(BizObj.FieldDescriptionInfo);

				BizObj.ValidationRule = "";
				AssertHasErrors(BizObj.ValidationRuleInfo);
				BizObj.ValidationRule = "AAA";
				AssertHasErrors(BizObj.ValidationRuleInfo);
			});
		}

		public void TestFieldDescriptionReadOnlyWhenSpecialCharactersIsNotNull()
		{
			SetUpTestingRule();

			ValidationField.FieldDescription = "Special Characters";
			Assert(!ValidationField.FieldDescriptionInfo.ReadOnly);

			var specialCharacter = ValidationField.SpecialCharacters.AddNew();
			specialCharacter.CharacterValue = "AAA";
			Assert(ValidationField.FieldDescriptionInfo.ReadOnly);

			specialCharacter.CharacterValue = "";
			Assert(!ValidationField.FieldDescriptionInfo.ReadOnly);
		}

		public void TestFieldDescriptionReadOnlyWhenScreeningValueIsNotNull()
		{
			SetUpTestingRule();

			Assert(!ValidationField.FieldDescriptionInfo.ReadOnly);

			var screeningValue = ValidationField.ScreeningValues.AddNew();
			screeningValue.ScreeningValue = "AAA";
			Assert(ValidationField.FieldDescriptionInfo.ReadOnly);

			screeningValue.ScreeningValue = "";
			Assert(!ValidationField.FieldDescriptionInfo.ReadOnly);
		}

		public void TestFieldDescriptionReadOnlyWhenMacrosScriptIsNotNull()
		{
			SetUpTestingRule();

			ValidationField.FieldDescription = "User Defined";
			Assert(!ValidationField.FieldDescriptionInfo.ReadOnly);

			ValidationField.MacrosScript = "AAA";
			Assert(ValidationField.FieldDescriptionInfo.ReadOnly);

			ValidationField.MacrosScript = "";
			Assert(!ValidationField.FieldDescriptionInfo.ReadOnly);
		}

		public void TestDestinationCountryMandatoryWhenFieldDescriptionIsGoodsValue()
		{
			SetUpTestingRule();

			ValidationRule.TransportMode = "AIR";
			AssertNoErrors(ValidationRule.DestinationCountryCodeInfo);

			ValidationField.FieldDescription = "Goods Value";
			AssertHasErrors(ValidationRule.DestinationCountryCodeInfo);

			ValidationRule.DestinationCountryCode = "US";
			var screeningValue = ValidationField.ScreeningValues.AddNew();
			screeningValue.ScreeningValue = "800";
			AssertNoErrors(ValidationRule.DestinationCountryCodeInfo);
		}

		public void TestCheckDuplicateFields()
		{
			var preScreeningRule = new HVLVPreScreeningRule();
			var field1 = preScreeningRule.Fields.AddNew();
			field1.FieldDescription = "Consignee";
			var field2 = preScreeningRule.Fields.AddNew();
			field2.FieldDescription = "Consignee";

			field1.RunPreSaveValidation();

			AssertHasError(field1.FieldDescriptionInfo, "The Field Description has been duplicated and must be unique.");
			AssertHasError(field2.FieldDescriptionInfo, "The Field Description has been duplicated and must be unique.");

			field1.FieldDescription = "Shipper";
			field1.RunPreSaveValidation();
			field2.RunPreSaveValidation();

			AssertNoErrors(field1.FieldDescriptionInfo);
			AssertNoErrors(field2.FieldDescriptionInfo);

			field1.FieldDescription = "User Defined";
			field2.FieldDescription = "User Defined";
			field1.RunPreSaveValidation();
			field2.RunPreSaveValidation();

			Assert(!field1.FieldDescriptionInfo.HasErrors());
			Assert(!field2.FieldDescriptionInfo.HasErrors());
		}

		public void TestFieldList()
		{
			SetUpTestingRule();

			CombineAssertions("FieldList", () =>
			{
				AssertEquals("Field List Count", 31, ValidationField.FieldDescriptionList.Count);
				Assert("Consignee", ValidationField.FieldDescriptionList.ContainsCode("Consignee"));
				Assert("Consignee Contact", ValidationField.FieldDescriptionList.ContainsCode("Consignee Contact"));
				Assert("Consignee Address 1", ValidationField.FieldDescriptionList.ContainsCode("Consignee Address 1"));
				Assert("Consignee Address 2", ValidationField.FieldDescriptionList.ContainsCode("Consignee Address 2"));
				Assert("Goods Description", ValidationField.FieldDescriptionList.ContainsCode("Goods Description"));
				Assert("Consignee City", ValidationField.FieldDescriptionList.ContainsCode("Consignee City"));
				Assert("Consignee Postcode", ValidationField.FieldDescriptionList.ContainsCode("Consignee Postcode"));
				Assert("Consignee State", ValidationField.FieldDescriptionList.ContainsCode("Consignee State"));
				Assert("Consignee Email", ValidationField.FieldDescriptionList.ContainsCode("Consignee Email"));
				Assert("Consignee Mobile", ValidationField.FieldDescriptionList.ContainsCode("Consignee Mobile"));
				Assert("Consignee Phone", ValidationField.FieldDescriptionList.ContainsCode("Consignee Phone"));
				Assert("Consignee Fax", ValidationField.FieldDescriptionList.ContainsCode("Consignee Fax"));
				Assert("Goods Value", ValidationField.FieldDescriptionList.ContainsCode("Goods Value"));
				Assert("Shipper Address 1", ValidationField.FieldDescriptionList.ContainsCode("Shipper Address 1"));
				Assert("Shipper", ValidationField.FieldDescriptionList.ContainsCode("Shipper"));
				Assert("Shipper Address 2", ValidationField.FieldDescriptionList.ContainsCode("Shipper Address 2"));
				Assert("Shipper City", ValidationField.FieldDescriptionList.ContainsCode("Shipper City"));
				Assert("Shipper Contact", ValidationField.FieldDescriptionList.ContainsCode("Shipper Contact"));
				Assert("Shipper Email", ValidationField.FieldDescriptionList.ContainsCode("Shipper Email"));
				Assert("Shipper Fax", ValidationField.FieldDescriptionList.ContainsCode("Shipper Fax"));
				Assert("Shipper Mobile", ValidationField.FieldDescriptionList.ContainsCode("Shipper Mobile"));
				Assert("Shipper Phone", ValidationField.FieldDescriptionList.ContainsCode("Shipper Phone"));
				Assert("Shipper Postcode", ValidationField.FieldDescriptionList.ContainsCode("Shipper Postcode"));
				Assert("Shipper State", ValidationField.FieldDescriptionList.ContainsCode("Shipper State"));
				Assert("Origin HS Code", ValidationField.FieldDescriptionList.ContainsCode("Origin HS Code"));
				Assert("Destination HS Code", ValidationField.FieldDescriptionList.ContainsCode("Destination HS Code"));
				Assert("User Defined", ValidationField.FieldDescriptionList.ContainsCode("User Defined"));
				Assert("Special Characters", ValidationField.FieldDescriptionList.ContainsCode("Special Characters"));
				Assert("Consignee Instructions", ValidationField.FieldDescriptionList.ContainsCode("Consignee Instructions"));
				Assert("Total Lines Customs Value", ValidationField.FieldDescriptionList.ContainsCode("Total Lines Customs Value"));
				Assert("Total Lines Intrinsic Value", ValidationField.FieldDescriptionList.ContainsCode("Total Lines Intrinsic Value"));
			});
		}

		public void TestValidationRuleList()
		{
			SetUpTestingRule();

			CombineAssertions("ValidationRuleList", () =>
			{
				AssertEquals(3, ValidationField.ValidationRuleList.Count);
				Assert(ValidationField.ValidationRuleList.ContainsCode("NON"));
				Assert(ValidationField.ValidationRuleList.ContainsCode("ERR"));
				Assert(ValidationField.ValidationRuleList.ContainsCode("WRN"));
			});
		}

		public void TestDestinationCountryAndFieldsNameReadOnlyWhenOverrideDeminimus()
		{
			SetUpTestingRule();

			ValidationRule.DestinationCountryCode = "US";
			Assert(!ValidationRule.DestinationCountryCodeInfo.ReadOnly);

			var testField = ValidationRule.Fields.AddNew();
			testField.FieldDescription = BizObj.FieldDescriptionList.GetCodeFromDescription(HVLVConsignmentSchema.Constants.HVC_GoodsValue);
			Assert(!testField.FieldDescriptionInfo.ReadOnly);

			testField.IsDeminimusValueOverride = true;
			Assert(ValidationRule.DestinationCountryCodeInfo.ReadOnly);
			Assert(testField.FieldDescriptionInfo.ReadOnly);
		}

		public void TestDeminimusCurrencyIsReadOnlyWhenOverrideDeminimusIsNotTicked()
		{
			SetUpTestingRule();

			var testField = ValidationRule.Fields.AddNew();
			Assert(testField.DeminimusCurrencyInfo.ReadOnly);

			testField.IsDeminimusValueOverride = true;
			Assert(!testField.DeminimusCurrencyInfo.ReadOnly);
		}

		public void TestDeminimusAndCurrencyWhenFieldNameSetToGoodsValue()
		{
			SetUpTestingRule();

			CreateTaxOrFee(Factory, "US", 800.000m, "DEM");
			CreateTaxOrFee(Factory, "AU", 1000.000m, "DEM");
			Factory.Save();

			ValidationRule.DestinationCountryCode = "US";

			ValidationField.FieldDescription = BizObj.FieldDescriptionList.GetCodeFromDescription(HVLVConsignmentSchema.Constants.HVC_GoodsValue);
			AssertEquals(800.00m, ValidationField.DeminimusValue);
			AssertEquals("USD", ValidationField.DeminimusCurrency);

			ValidationRule.DestinationCountryCode = "AU";
			AssertEquals(1000.00m, ValidationField.DeminimusValue);
			AssertEquals("AUD", ValidationField.DeminimusCurrency);

			ValidationRule.DestinationCountryCode = "NZ";
			AssertEquals(ZDecimal.Zero, ValidationField.DeminimusValue);
			AssertEquals("NZD", ValidationField.DeminimusCurrency);

			ValidationRule.DestinationCountryCode = "KK";
			AssertEquals(ZDecimal.Zero, ValidationField.DeminimusValue);
			AssertEquals("", ValidationField.DeminimusCurrency);
		}

		public void TestMacrosScriptWhenFieldSetToUserDefined()
		{
			SetUpTestingRule();

			ValidationField.FieldDescription = "User Defined";

			AssertHasErrors("Macros can not be empty", ValidationField.MacrosScriptInfo);

			ValidationField.MacrosScript = "\"<HVC_Consignment>\"====\"A\"";
			AssertHasError(ValidationField.MacrosScriptInfo, "Error evaluating \"\"====\"A\"");

			ValidationField.MacrosScript = "\"<HVC_Consignment>\"";
			AssertHasError(ValidationField.MacrosScriptInfo, "Result of \"\" is not a True/False expression");

			ValidationField.MacrosScript = "\"<HVC_ConsigneeName>\".Contains(\"AA\")";
			AssertNoErrors(BizObj.MacrosScriptInfo);

			ValidationField.MacrosScript = "\"<HVC_ConsigneeName>\"==\"A\"";
			AssertNoErrors(BizObj.MacrosScriptInfo);

			ValidationField.MacrosScript = "\"<Left(\"<HVC_ConsignmentId>\", 3)>\"==\"HVC\"";
			AssertNoErrors(ValidationField.MacrosScriptInfo);
		}

		public void TestDifferentConditionsVisibleWhenChooseDifferentFieldName()
		{
			SetUpTestingRule();

			Assert(ValidationField.IsScreeningValuesVisible);
			Assert(!ValidationField.IsHSCodeVisible);
			Assert(!ValidationField.IsDeminimusVisible);
			Assert(!ValidationField.IsMacrosVisible);
			Assert(!ValidationField.IsSpecialCharactersField);

			ValidationField.FieldDescription = "User Defined";
			Assert(!ValidationField.IsScreeningValuesVisible);
			Assert(!ValidationField.IsHSCodeVisible);
			Assert(!ValidationField.IsDeminimusVisible);
			Assert(ValidationField.IsMacrosVisible);
			Assert(!ValidationField.IsSpecialCharactersField);

			ValidationField.FieldDescription = "Goods Value";
			Assert(!ValidationField.IsScreeningValuesVisible);
			Assert(!ValidationField.IsHSCodeVisible);
			Assert(ValidationField.IsDeminimusVisible);
			Assert(!ValidationField.IsMacrosVisible);
			Assert(!ValidationField.IsSpecialCharactersField);

			ValidationField.FieldDescription = "Special Characters";
			Assert(!ValidationField.IsScreeningValuesVisible);
			Assert(!ValidationField.IsHSCodeVisible);
			Assert(!ValidationField.IsDeminimusVisible);
			Assert(!ValidationField.IsMacrosVisible);
			Assert(ValidationField.IsSpecialCharactersField);

			ValidationField.FieldDescription = "Total Lines Customs Value";
			Assert(!ValidationField.IsScreeningValuesVisible);
			Assert(!ValidationField.IsHSCodeVisible);
			Assert(ValidationField.IsDeminimusVisible);
			Assert(!ValidationField.IsMacrosVisible);
			Assert(!ValidationField.IsSpecialCharactersField);

			ValidationField.FieldDescription = "Total Lines Intrinsic Value";
			Assert(!ValidationField.IsScreeningValuesVisible);
			Assert(!ValidationField.IsHSCodeVisible);
			Assert(ValidationField.IsDeminimusVisible);
			Assert(!ValidationField.IsMacrosVisible);
			Assert(!ValidationField.IsSpecialCharactersField);

			ValidationField.FieldDescription = "Origin HS Code";
			Assert(!ValidationField.IsScreeningValuesVisible);
			Assert(ValidationField.IsHSCodeVisible);
			Assert(!ValidationField.IsDeminimusVisible);
			Assert(!ValidationField.IsMacrosVisible);
			Assert(!ValidationField.IsSpecialCharactersField);

			ValidationField.FieldDescription = "Destination HS Code";
			Assert(!ValidationField.IsScreeningValuesVisible);
			Assert(ValidationField.IsHSCodeVisible);
			Assert(!ValidationField.IsDeminimusVisible);
			Assert(!ValidationField.IsMacrosVisible);
			Assert(!ValidationField.IsSpecialCharactersField);
		}

		public void TestCheckSameConsigneeVisible()
		{
			SetUpTestingRule();

			ValidationRule.ModuleType = ModuleTypeCodes.HVLVBookingHeader;

			Assert("The checkbox should be visible", ValidationField.IsSameConsigneeCheckBoxVisible);

			ValidationRule.ModuleType = ModuleTypeCodes.HVLVShipment;

			Assert("The checkbox should be visible", ValidationField.IsSameConsigneeCheckBoxVisible);
		}

		public void TestMessageTextMandatoryWhenFieldIsUserDefined()
		{
			SetUpTestingRule();
			AssertNoErrors(ValidationField.MessageTextInfo);

			ValidationField.FieldDescription = "Consignee";
			AssertNoErrors(ValidationField.MessageTextInfo);

			ValidationField.FieldDescription = "User Defined";
			AssertHasErrors(ValidationField.MessageTextInfo);

			ValidationField.MessageText = "Shawn";
			AssertNoErrors(ValidationField.MessageTextInfo);
		}

		void CreateTaxOrFee(BusinessObjectFactory factory, ZString countryCode, ZDecimal value, string code)
		{
			var assembly = Assembly.Load("Enterprise.Customs.Universal.Test");
			var type = assembly.GetType("Enterprise.Customs.Universal.Testing.UniversalReferenceTestDataHelper");
			var dataHelperObj = Activator.CreateInstance(type, factory);
			var parametersTypes = new Type[] { typeof(ZString), typeof(ZDecimal), typeof(ZString), typeof(ZDecimal), typeof(ZDecimal), typeof(ZString), typeof(ZDateTime), typeof(ZDateTime), typeof(string) };
			var createTaxOrFee = type.GetMethod("CreateTaxOrFee", parametersTypes);
			var parameters = new object[] { (ZString)code, value, countryCode, (ZDecimal)0, (ZDecimal)0, (ZString)"", new ZDateTime(2019, 02, 20), new ZDateTime(2079, 06, 05), "Deminimus" };
			createTaxOrFee.Invoke(dataHelperObj, parameters);
		}

		public void TestReadElementsWithHVLVPreScreeningFieldXml_InRandomOrder()
		{
			var xmlString1 = @"
<HVLVPreScreeningField>
    <FieldDescription />
    <ValidationRule>111</ValidationRule>
    <DeminimusValue>1.1</DeminimusValue>
    <IsDeminimusValueOverride>true</IsDeminimusValueOverride>
    <CheckSameConsignee>false</CheckSameConsignee>
    <MessageText>test</MessageText>
    <IsMandatory>false</IsMandatory>
    <ArrayOfHVLVPreScreeningValue>
        <HVLVPreScreeningValue>
            <ScreeningValue>12</ScreeningValue>
            <ScreeningComparisonOperatorCode>Contains</ScreeningComparisonOperatorCode>
            <MessageTextPerValue />
        </HVLVPreScreeningValue>
    </ArrayOfHVLVPreScreeningValue>
    <ArrayOfHVLVPreScreeningSpecialCharacterValue />
    <DeminimusCurrency>USD</DeminimusCurrency>
</HVLVPreScreeningField>";
			AssertReadElementsWithHVLVPreScreeningFieldXml(xmlString1);

			var xmlString2 = @"
<HVLVPreScreeningField>
    <FieldDescription />
    <ValidationRule>111</ValidationRule>
    <DeminimusValue>1.1</DeminimusValue>
    <DeminimusCurrency>USD</DeminimusCurrency>
    <IsMandatory>false</IsMandatory>
    <IsDeminimusValueOverride>true</IsDeminimusValueOverride>
    <MessageText>test</MessageText>
    <CheckSameConsignee>false</CheckSameConsignee>
    <ArrayOfHVLVPreScreeningSpecialCharacterValue />
    <ArrayOfHVLVPreScreeningValue>
        <HVLVPreScreeningValue>
            <ScreeningValue>12</ScreeningValue>
            <ScreeningComparisonOperatorCode>Contains</ScreeningComparisonOperatorCode>
            <MessageTextPerValue />
        </HVLVPreScreeningValue>
    </ArrayOfHVLVPreScreeningValue>
</HVLVPreScreeningField>";
			AssertReadElementsWithHVLVPreScreeningFieldXml(xmlString2);

			void AssertReadElementsWithHVLVPreScreeningFieldXml(string xmlString)
			{
				var dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(ExpectedBusinessObjectType);
				var deserialisedBusinessObject = (HVLVPreScreeningField)dummyDataType.Deserialise(Encoding.UTF8.GetBytes(xmlString));
				CombineAssertions(() =>
				{
					Assert(!deserialisedBusinessObject.IsMandatory);
					Assert(!deserialisedBusinessObject.CheckSameConsignee);
					Assert(deserialisedBusinessObject.IsDeminimusValueOverride);
					AssertEquals("", deserialisedBusinessObject.FieldDescription);
					AssertEquals("111", deserialisedBusinessObject.ValidationRule);
					AssertEquals("test", deserialisedBusinessObject.MessageText);
					AssertEquals("USD", deserialisedBusinessObject.DeminimusCurrency);
					AssertNotNull(deserialisedBusinessObject.ScreeningValues);
					AssertNotNull(deserialisedBusinessObject.SpecialCharacters);
				});
			}
		}

		void SetUpTestingRule()
		{
			ValidationRule = new HVLVPreScreeningRule();
			ValidationField = ValidationRule.Fields.AddNew();
		}

		#region Implementation

		protected override HVLVPreScreeningField GetBusinessObjectToClone()
		{
			return (HVLVPreScreeningField)GetNewBusinessObject();
		}

		protected override HVLVPreScreeningField GetBusinessObjectToSerialise()
		{
			return (HVLVPreScreeningField)GetNewBusinessObject();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		static HVLVPreScreeningRule ValidationRule;

		static HVLVPreScreeningField ValidationField;

		#endregion
	}
}
