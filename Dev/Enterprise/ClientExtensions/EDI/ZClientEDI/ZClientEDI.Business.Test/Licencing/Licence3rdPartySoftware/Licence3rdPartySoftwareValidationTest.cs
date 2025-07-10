using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	internal class Licence3rdPartySoftwareValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2009, 4, 21)]
		public void TestLicence3rdPartSoftwareValidation()
		{
			Licence3rdPartySoftware testRow = Factory.New<Licence3rdPartySoftware>();
			OrgSupplierPart testPart = Factory.NewWithValidTestData<OrgSupplierPart>();

			testRow.L3_OP_ProductSKU = ZGuid.Empty;
			AssertHasErrors("Product should have error - blank", testRow.L3_OP_ProductSKUInfo);
			testRow.L3_OP_ProductSKU = ZGuid.Invalid;
			AssertHasErrors("Product should have error - Invalid", testRow.L3_OP_ProductSKUInfo);
			testRow.L3_OP_ProductSKU = testPart.PK;
			AssertNoErrors("Product should have no error", testRow.L3_OP_ProductSKUInfo);

			testRow.L3_LicenceType = "";
			AssertHasErrors("LicType should have error - blank", testRow.L3_LicenceTypeInfo);
			testRow.L3_LicenceType = "CPU";
			AssertNoErrors("LicType should have no error", testRow.L3_LicenceTypeInfo);

			testRow.L3_OSType = "";
			AssertHasErrors("OS Type should  have error - blank", testRow.L3_OSTypeInfo);
			testRow.L3_OSType = "x86";
			AssertNoErrors("OS Type should  have no error", testRow.L3_OSTypeInfo);

			testRow.L3_LicenceCount = -1;
			AssertHasErrors("Count cannot be less then 1", testRow.L3_LicenceCountInfo);
			testRow.L3_LicenceCount = 3;
			AssertNoErrors("Count should have no errors", testRow.L3_LicenceCountInfo);

			testRow.L3_UpgradeAssuranceEndsOn = ZDate.Today.AddYears(1);
			AssertHasErrors("Should Have error - Starts date is blank", testRow.L3_UpgradeAssuranceEndsOnInfo);
			testRow.L3_UpgradeAssuranceStartsOn = ZDate.Today.AddYears(2);
			AssertHasErrors("Error - Start date is less then end date", testRow.L3_UpgradeAssuranceStartsOnInfo);
			testRow.L3_UpgradeAssuranceStartsOn = ZDate.Today;
			testRow.L3_UpgradeAssuranceEndsOn = ZDate.Today.AddYears(1);
			AssertNoErrors("Should have no errors", testRow.L3_UpgradeAssuranceEndsOnInfo);
			AssertNoErrors("Should have no errors", testRow.L3_UpgradeAssuranceStartsOnInfo);
		}
	}
}
