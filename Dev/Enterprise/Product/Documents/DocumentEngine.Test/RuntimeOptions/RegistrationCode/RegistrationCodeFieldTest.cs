using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(RegistrationCodeField))]
	sealed class RegistrationCodeFieldTest : FitlerFieldTestWithClearValues
	{
		public void TestValidateCodeCountry()
		{
			var field = GetNewFieldWithTestData();
			field.CodeCountry = "";
			AssertNoErrors(field.CodeCountryInfo);
			field.CodeCountry = "XXX";
			AssertHasError(field.CodeCountryInfo, "Enter a valid selection.");
			field.CodeCountry = Core.Constants.CountryCodes.Australia;
			AssertNoErrors(field.CodeCountryInfo);
		}

		public void TestValidateCustomType()
		{
			var field = GetNewFieldWithTestData();
			field.CodeCountry = "";
			field.CustomType = "";
			AssertNoErrors(field.CustomTypeInfo);

			field.CodeCountry = "XXX";
			AssertNoErrors(field.CustomTypeInfo);

			field.CodeCountry = "";
			field.CustomType = "ZZZ";
			AssertHasError(field.CustomTypeInfo, "Enter a valid selection.");

			field.CodeCountry = Core.Constants.CountryCodes.Australia;
			field.CustomType = "";
			AssertNoErrors(field.CustomTypeInfo);

			field.CustomType = "XXX";
			AssertHasError(field.CustomTypeInfo, "Enter a valid selection.");

			field.CustomType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			AssertNoErrors(field.CustomTypeInfo);

			field.CustomType = OrgCusCode.SpainCodeTypes.DNI;
			AssertHasError(field.CustomTypeInfo, "Enter a valid selection.");

			field.CodeCountry = Core.Constants.CountryCodes.Spain;
			AssertNoErrors(field.CustomTypeInfo);
		}

		public void TestWhereClause()
		{
			var field = GetNewFieldWithTestData();
			field.CodeCountry = "";
			field.CustomType = "";
			AssertEquals("", field.WhereClause());

			field.CodeCountry = Core.Constants.CountryCodes.Australia;
			var whereClauseMatch = Regex.Match(field.WhereClause(), @"ACodeCountry = (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like [ACodeCountry = @p1] but was: " + field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 value", Core.Constants.CountryCodes.Australia, field.SqlParameters()[0].Value.ToString());
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, field.SqlParameters()[0].ToString());

			field.CustomType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			whereClauseMatch = Regex.Match(field.WhereClause(), @"ACodeCountry = (@p[0-9]+) AND ACustomType = (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like [ACodeCountry = @p1 AND ACustomType = @p2] but was: " + field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 value", Core.Constants.CountryCodes.Australia, field.SqlParameters()[0].Value.ToString());
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, field.SqlParameters()[0].ToString());
			AssertEquals("Param 2 value", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, field.SqlParameters()[1].Value.ToString());
			AssertEquals("Param 2 name", whereClauseMatch.Groups[2].Value, field.SqlParameters()[1].ToString());

			field.CodeCountry = "";
			whereClauseMatch = Regex.Match(field.WhereClause(), @"ACustomType = (@p[0-9]+)", RegexOptions.IgnoreCase);
			Assert("Where clause should be like [ACustomType = @p2] but was: " + field.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 2 value", OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, field.SqlParameters()[1].Value.ToString());
			AssertEquals("Param 2 name", whereClauseMatch.Groups[1].Value, field.SqlParameters()[1].ToString());
		}

		public void TestJsonConverter()
		{
			var field = GetNewFieldWithTestData();

			var result = JsonConverterHelper.Serialize(field);
			var deserialisedField = JsonConverterHelper.Deserialize<RegistrationCodeField>(result);

			AssertEquals(field.DisplayName, deserialisedField.DisplayName);
			AssertEquals(field.FieldName, deserialisedField.FieldName);
			AssertEquals(field.FieldNameCodeCountry, deserialisedField.FieldNameCodeCountry);
			AssertEquals(field.FieldNameCustomType, deserialisedField.FieldNameCustomType);
			AssertEquals(field.CodeCountry, deserialisedField.CodeCountry);
			AssertEquals(field.CustomType, deserialisedField.CustomType);
		}

		public override void TestSafeCopyValuesFrom()
		{
			var source = GetNewFieldWithTestData();
			var destination = new RegistrationCodeField(Factory);
			destination.SafeCopyValuesFrom(source);
			AssertEquals(source.CodeCountry, destination.CodeCountry);
			AssertEquals(source.CustomType, destination.CustomType);
		}

		public override void TestClearValues()
		{
			var field = GetNewFieldWithTestData();
			field.ClearValues();
			AssertEquals(ZString.Empty, field.CodeCountry);
			AssertEquals(ZString.Empty, field.CustomType);
		}

		public void TestValueAsObjectTranslatable()
		{
			var field = GetNewFieldWithTestData();
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("32e91d71-490d-421f-a4e1-b863222c122c", new ResourceStringData("32e91d71-490d-421f-a4e1-b863222c122c", "国家代码: {0}, 注册代码: {1}"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals($"国家代码: {field.CodeCountry.ToString()}, 注册代码: {field.CustomType.ToString()}", field.ValueAsObject);
				}
			}
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			var field = GetNewFieldWithTestData();

			return new FilterFieldWithUTSupport[]
			{
				field,
			};
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get { return 1; }
		}

		RegistrationCodeField GetNewFieldWithTestData()
		{
			var field = new RegistrationCodeField(new BusinessObjectFactory());
			field.DisplayName = "Registration Code";
			field.FieldNameCodeCountry = "ACodeCountry";
			field.FieldNameCustomType = "ACustomType";
			field.CodeCountry = Core.Constants.CountryCodes.Australia;
			field.CustomType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			return field;
		}
	}
}
