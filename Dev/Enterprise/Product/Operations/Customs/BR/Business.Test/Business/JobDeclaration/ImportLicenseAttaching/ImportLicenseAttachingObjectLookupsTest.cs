using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseAttachingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestImportLicenseFeeTypeList()
		{
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);
			var entryInstruction = Factory.New<CusEntryInstruction>();

			var importLicenseAttachingObject = new ImportLicenseAttachingObject(entryInstruction);
			var feeTypeList1 = importLicenseAttachingObject.Lookups.FeeTypeList;
			AssertEquals("F1D5, F1ND", feeTypeList1.CodesAsString);

			var feeTypeList2 = new ImportLicenseAttachingObjectLookups(importLicenseAttachingObject);
			AssertSame("List should be cached", feeTypeList1, feeTypeList2.FeeTypeList);
		}
	}
}
