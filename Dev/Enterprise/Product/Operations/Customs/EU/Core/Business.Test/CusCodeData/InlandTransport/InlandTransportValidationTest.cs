using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class InlandTransportValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckNationality()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(inlandTransport.NationalityInfo, "X1", Core.Constants.CountryCodes.Australia);
			});
		}

		public void TestCheckCY_Code_Unique_TrainNumber()
		{
			AssertCheckCY_CodeUnique(ExportInlandTransportTypeList.Codes._21);
		}

		public void TestCheckCY_Code_Unique_RoadVehicleRegistrationNumber()
		{
			AssertCheckCY_CodeUnique(ExportInlandTransportTypeList.Codes._30);
		}

		public void TestCheckCY_Data_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(inlandTransport.CY_DataInfo);
		}

		public void TestCheckCY_Data_UpperCase()
		{
			const string message = "Transport ID cannot have lower case letters.";
			var typesWithUpperCaseDataOnly = new HashSet<string>
			{
				ExportInlandTransportTypeList.Codes._10,
				ExportInlandTransportTypeList.Codes._20,
				ExportInlandTransportTypeList.Codes._21,
				ExportInlandTransportTypeList.Codes._30,
				ExportInlandTransportTypeList.Codes._31,
				ExportInlandTransportTypeList.Codes._40,
				ExportInlandTransportTypeList.Codes._41,
				ExportInlandTransportTypeList.Codes._80
			};
			inlandTransport.CY_Data = "#A-(1) 中文 ÄHNLICH ÖÜ_[E]";
			var inlandTransportWithLowerCaseLetters = declaration.InlandTransports.AddNew();
			inlandTransportWithLowerCaseLetters.CY_Data = "Has Lowercase Letters";

			CombineAssertions(() =>
			{
				foreach (var type in new ExportInlandTransportTypeList().GetAllCodes())
				{
					inlandTransport.CY_Code = type;
					inlandTransport.Validation.ValidateCY_Data();
					AssertNoMessageError($"CY_Code '{type}', no lowercase letters", inlandTransport.CY_DataInfo, message);

					inlandTransportWithLowerCaseLetters.CY_Code = type;
					inlandTransportWithLowerCaseLetters.Validation.ValidateCY_Data();
					if (typesWithUpperCaseDataOnly.Contains(type))
					{
						AssertHasMessageError($"CY_Code '{type}', has lowercase letters", inlandTransportWithLowerCaseLetters.CY_DataInfo, message);
					}
					else
					{
						AssertNoMessageError($"CY_Code '{type}', has lowercase letters", inlandTransportWithLowerCaseLetters.CY_DataInfo, message);
					}
				}
			});
		}

		public void TestValidateAll()
		{
			inlandTransport.Validation.ValidateAll();
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("Validation for CY_Data", inlandTransport.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("Validation for Nationality", inlandTransport.NationalityInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			inlandTransport = declaration.InlandTransports.AddNew();
		}
		JobDeclaration declaration;
		InlandTransport inlandTransport;

		void AssertCheckCY_CodeUnique(string code)
		{
			const string uniqueTypeIDMessage = "The Type of ID has been duplicated and must be unique.";
			inlandTransport.CY_Code = code;
			var inlandTransport2 = declaration.InlandTransports.AddNew();
			inlandTransport2.CY_Code = code;

			CombineAssertions(() =>
			{
				AssertHasError("Duplicate", inlandTransport2.CY_CodeInfo, uniqueTypeIDMessage);
				inlandTransport2.CY_Code = ExportInlandTransportTypeList.Codes._10;
				AssertNoError("Unique", inlandTransport2.CY_CodeInfo, uniqueTypeIDMessage);
			});
		}
	}
}
