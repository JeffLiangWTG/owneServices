using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegPremisesValidation))]
sealed class CusTempStorageRegPremisesValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckSRP_Code()
	{
		CombineAssertions(() =>
		{
			const string error = "Code must be unique, please enter a different code";
			var premises = Factory.New<CusTempStorageRegPremises>();
			premises.Validation.ValidateSRP_Code();
			var codeInfo = premises.SRP_CodeInfo;

			AssertHasErrorContaining("Code is empty", codeInfo, MandatoryValidation.MustBeEntered);

			premises.SRP_Code = "AA";
			AssertNoErrorContaining(codeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining("Code is not in DB", codeInfo, error);

			var factory = new BusinessObjectFactory();
			var premisesInDB = factory.New<CusTempStorageRegPremises>();
			premisesInDB.SRP_Code = "AA";
			premisesInDB.SRP_Description = "AA";
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			premisesInDB.SRP_OA_PremisesAddress = orgAddress.PK;
			factory.Save();

			premises.Validation.ValidateSRP_Code();
			AssertNoErrorContaining("Code is not empty", codeInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining("Code is in DB", codeInfo, error);

			premises.SRP_Code = "AB";
			AssertNoErrorContaining("Code is not empty", codeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining("New Code is not in DB", codeInfo, error);
		});
	}

	public void TestCheckSRP_Description()
	{
		ValidationTestHelper.AssertErrorIfNotEntered(Factory.New<CusTempStorageRegPremises>().SRP_DescriptionInfo, MandatoryValidation.MustBeEntered);
	}

	public void TestCheckSRP_CustomsLocation()
	{
		ValidationTestHelper.AssertWarningIfNotEntered(Factory.New<CusTempStorageRegPremises>().SRP_CustomsLocationInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckSRP_Type()
	{
		CombineAssertions(() =>
		{
			const string error = "Enter a valid Type.";
			const string emptyTypeError = "Please enter a Type.";

			var premises = Factory.New<CusTempStorageRegPremises>();
			premises.Validation.ValidateSRP_Type();
			var srpTypeInfo = premises.SRP_TypeInfo;
			premises.SRP_Type = "ADT";
			AssertNoErrorContaining("No Error if Type is valid (ADT)", srpTypeInfo, error);

			premises.SRP_Type = "LAM";
			AssertNoErrorContaining("No Error if Type is valid (LAM)", srpTypeInfo, error);

			premises.SRP_Type = "ADX";
			AssertHasErrorContaining("Error if Type is invalid (ADX)", srpTypeInfo, error);

			premises.SRP_Type = "LL";
			AssertHasErrorContaining("Error if Type is invalid (LL)", srpTypeInfo, error);

			premises.SRP_Type = "123";
			AssertHasErrorContaining("Error if Type is invalid (123)", srpTypeInfo, error);

			premises.SRP_Type = " ";
			AssertHasErrorContaining("Error if Type is blank space", srpTypeInfo, emptyTypeError);

			premises.SRP_Type = "";
			AssertHasErrorContaining("Error if Type is empty", srpTypeInfo, emptyTypeError);
		});
	}
}
