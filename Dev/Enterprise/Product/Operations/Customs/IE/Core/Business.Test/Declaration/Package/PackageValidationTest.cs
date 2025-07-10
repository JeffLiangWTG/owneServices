using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class PackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCW_PackType()
		{
			TestDataHelper.SetUpPackageTypes(Factory);
			var declaration = Factory.New<JobDeclaration>();
			var package = (Package)declaration.Packages.AddNew();
			package.Validation.ValidateCW_PackType();
			AssertHasMessageErrorContaining("CW_PackType mandatory.", package.CW_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);
			package.CW_PackQty = 1;
			package.CW_PackType = "V!";
			AssertNoMessageErrorContaining("CW_PackType not empty", package.CW_PackTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(package.CW_PackTypeInfo, ListValidation.InvalidCodeMessageError);
			package.CW_PackType = "VG";
			AssertNoMessageErrors("CW_PackType mandatory(validation passes).", package.CW_PackTypeInfo);
		}
	}
}
