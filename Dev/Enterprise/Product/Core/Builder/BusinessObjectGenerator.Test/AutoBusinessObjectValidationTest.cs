using System;

namespace Enterprise.BusinessObjectGenerator.Testing
{
	sealed class AutoBusinessObjectValidationTest : AutoCodeTestCase
	{
		#region Test English-language Validation

		public void TestEnglishCharactersValidationForAllProperties()
		{
			var generatedCode = Validation.SourceCode;

			var expectedEnglishValidationCodeForTT_String = Validation.LinesOfCode(
				"		protected virtual void CheckTT_StringIsWesternEuropean()",
				"		{",
				"			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.TT_StringInfo);",
				"		}");

			var expectedEnglishValidationCodeForTT_Char = Validation.LinesOfCode(
				"		protected virtual void CheckTT_CharIsWesternEuropean()",
				"		{",
				"			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.TT_CharInfo);",
				"		}");

			var expectedEnglishValidationCodeForTT_Code = Validation.LinesOfCode(
				"		protected virtual void CheckTT_CodeIsWesternEuropean()",
				"		{",
				"			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.TT_CodeInfo);",
				"		}");

			var expectedEnglishValidationCodeForTT_SparseString = Validation.LinesOfCode(
				"		protected virtual void CheckTT_SparseStringIsWesternEuropean()",
				"		{",
				"			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.TT_SparseStringInfo);",
				"		}");

			var expectedEnglishValidationCodeForTT_SparseChar = Validation.LinesOfCode(
				"		protected virtual void CheckTT_SparseCharIsWesternEuropean()",
				"		{",
				"			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.TT_SparseCharInfo);",
				"		}");

			AssertEquals("Should have generated CheckTT_StringIsWesternEuropean() method.", true, generatedCode.IndexOf(expectedEnglishValidationCodeForTT_String) != -1);
			AssertEquals("Should have generated CheckTT_CharIsWesternEuropean() method.", true, generatedCode.IndexOf(expectedEnglishValidationCodeForTT_Char) != -1);
			AssertEquals("Should have generated CheckTT_CodeIsWesternEuropean() method.", true, generatedCode.IndexOf(expectedEnglishValidationCodeForTT_Code) != -1);
			AssertEquals("Should have generated CheckTT_SparseStringIsWesternEuropean() method.", true, generatedCode.IndexOf(expectedEnglishValidationCodeForTT_SparseString) != -1);
			AssertEquals("Should have generated CheckTT_SparseCharIsWesternEuropean() method.", true, generatedCode.IndexOf(expectedEnglishValidationCodeForTT_SparseChar) != -1);

			var editedGeneratedCode = generatedCode
				.Replace(expectedEnglishValidationCodeForTT_String, "")
				.Replace(expectedEnglishValidationCodeForTT_Char, "")
				.Replace(expectedEnglishValidationCodeForTT_Code, "")
				.Replace(expectedEnglishValidationCodeForTT_SparseString, "")
				.Replace(expectedEnglishValidationCodeForTT_SparseChar, "");

			AssertEquals("Should not have generated English-Language Validation for any other properties.",
				true, editedGeneratedCode.IndexOf("EnglishCharactersValidation.ErrorIfNotWesternEuropean") == -1);
		}

		public void TestEnglishCharactersValidationCallInGetDelegateMethod()
		{
			string generatedCode = Validation.SourceCode;

			string expectedRunMethodCodeForTT_String = Validation.LinesOfCode(
				"		RunValidationInvoker GetTT_StringValidationInvoker()",
				"		{",
				"			return delegate",
				"			{",
				"				CheckTT_StringIsWesternEuropean();",
				"				CheckTT_String();",
				"			};",
				"		}");

			string expectedRunMethodCodeForTT_Char = Validation.LinesOfCode(
				"		RunValidationInvoker GetTT_CharValidationInvoker()",
				"		{",
				"			return delegate",
				"			{",
				"				CheckTT_CharIsWesternEuropean();",
				"				CheckTT_Char();",
				"			};",
				"		}");

			AssertEquals("Should have generated call to CheckTT_StringIsWesternEuropean() in GetTT_StringValidationInvoker() but was " + System.Environment.NewLine + generatedCode, true, generatedCode.IndexOf(expectedRunMethodCodeForTT_String) != -1);
			AssertEquals("Should have generated call to CheckTT_CharIsWesternEuropean() in GetTT_CharValidationInvoker() but was " + System.Environment.NewLine + generatedCode, true, generatedCode.IndexOf(expectedRunMethodCodeForTT_Char) != -1);
		}

		public void TestEnglishCharactersValidation()
		{
			string expectedCode = Validation.LinesOfCode(
				"		protected virtual void CheckTT_CharIsWesternEuropean()",
				"		{",
				"			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.TT_CharInfo);",
				"		}",
				"");

			AutoProperty property = GetAnyPropertyThatRequiresEnglishCharactersValidation();
			string resultingCode = Validation.CodeForEnglishCharactersValidation(property);
			AssertEquals(expectedCode, resultingCode);

			property = GetAnyPropertyThatDoesNotRequiresEnglishCharactersValidation();
			resultingCode = Validation.CodeForEnglishCharactersValidation(property);
			AssertNull("Should not generate english-validation for AutoProperty with SqlDbType." + property.SqlDbType + ".", resultingCode);
		}

		AutoProperty GetAnyPropertyThatRequiresEnglishCharactersValidation()
		{
			foreach (AutoProperty property in CodeCollection.AutoBusinessObject.AutoProperties.Properties)
			{
				if (property.RequiresEnglishCharactersValidation)
				{
					return property;
				}
			}

			throw new ArgumentException("No properties exist that require english-language validation (the test data probably needs updating).");
		}

		AutoProperty GetAnyPropertyThatDoesNotRequiresEnglishCharactersValidation()
		{
			foreach (AutoProperty property in CodeCollection.AutoBusinessObject.AutoProperties.Properties)
			{
				if (!property.RequiresEnglishCharactersValidation)
				{
					return property;
				}
			}

			throw new ArgumentException("No properties exist that does not require english-language validation (the test data probably needs updating).");
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Validation = CodeCollection.AutoBusinessObjectValidation;
		}

		AutoBusinessObjectValidation Validation;

		#endregion
	}
}
