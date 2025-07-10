using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ImportLicenseInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var parent = Factory.New<ImportLicenseInfo>();
			var importLicenseTypeList = parent.Lookups.CodeList as CodeDescriptionPairList;
			AssertEquals("1, 2", importLicenseTypeList.CodesAsString);
		}

		public void TestImportLicenseFeeTypeList()
		{
			var parent = Factory.New<ImportLicenseInfo>();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			ReferenceTestDataHelper.CreateReferenceDataForILFFeeTypeList(Factory);
			var importLicenseFeeTypeList = invoiceLine.ImportLicenseSupportingInfo.Lookups.FeeTypeList;
			AssertEquals("F1D5, F1ND", importLicenseFeeTypeList.CodesAsString);
			var importLicenseFeeTypeList2 = new ImportLicenseInfoLookups(parent);
			AssertSame("List should be cached", importLicenseFeeTypeList, importLicenseFeeTypeList2.FeeTypeList);
		}
	}
}
