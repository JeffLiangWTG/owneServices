using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(REXDISCusTempStorageDec))]
	sealed class REXDISCusTempStorageDecValidationTest : CusTempStorageDecValidationTest<REXDISCusTempStorageDec>
	{
		public void TestSTH_DeclarationSubType()
		{
			var storageDec = GetCusTempStorageDecToTest();
			storageDec.Validation.ValidateSTH_DeclarationSubType();
			AssertHasMessageError(storageDec.STH_DeclarationSubTypeInfo, MandatoryValidation.YouHaveNotEnteredMessage(storageDec.STH_DeclarationSubTypeInfo.GetHumanReadableName()));
			storageDec.STH_DeclarationSubType = "X";
			AssertHasMessageError(storageDec.STH_DeclarationSubTypeInfo, "The code you have selected is not in the list.");
			storageDec.STH_DeclarationSubType = TemporaryStorageProcedureTypeList.Codes.C1;
			AssertNoMessageErrors(storageDec.STH_DeclarationSubTypeInfo);
		}

		public void TestShouldValidateRegistrationNumberLengthAndMrnFormat()
		{
			var validation = GetCusTempStorageDecToTest().Validation;
			AssertEquals(false, validation.ShouldValidateRegistrationNumberLengthAndMrnFormat);
		}

		protected override REXDISCusTempStorageDec GetCusTempStorageDecToTest() => Factory.NewWithValidTestData<REXDISCusTempStorageDec>();
	}
}
