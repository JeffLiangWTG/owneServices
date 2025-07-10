using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CFIARegistrationNumberLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var number = Factory.New<CFIARegistrationNumber>();
			AssertEquals("CFIARegTypes type", typeof(ZZRefCusCodeListCombinedCollection), number.Lookups.CFIARegTypes.GetType());
			AssertEquals("CY_DataList type", typeof(CodeDescriptionPairList), number.Lookups.CY_DataList.GetType());
		}

		public void TestCY_DataList()
		{
			var importerbuyer = Factory.New<OrgHeader>();
			importerbuyer.OH_Code = "TE#1";
			var impAddInfo = OrgImpAddInfo.Get(importerbuyer);
			var sfLicense1 = impAddInfo.SafeFoodLicenses.AddNew();
			sfLicense1.CY_Code = "BANANA";
			sfLicense1.CY_Data = "BANANADESC";
			var sfLicense2 = impAddInfo.SafeFoodLicenses.AddNew();
			sfLicense2.CY_Code = "APPLE";
			sfLicense2.CY_Data = "APPLEDESC";

			var importerNoLicense = Factory.New<OrgHeader>();
			importerNoLicense.OH_Code = "TE#2";

			var importer3 = Factory.New<OrgHeader>();
			importer3.OH_Code = "TE#3";
			var impAddInfo2 = OrgImpAddInfo.Get(importer3);
			var sfLicense21 = impAddInfo2.SafeFoodLicenses.AddNew();
			sfLicense21.CY_Code = "CORN";
			sfLicense21.CY_Data = "CORNDESC";
			var sfLicense22 = impAddInfo2.SafeFoodLicenses.AddNew();
			sfLicense22.CY_Code = "BAGEL";
			sfLicense22.CY_Data = "BAGELDESC";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importerbuyer.PK;

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var number = invoiceLine.CFIARegistrationNumbers.AddNew();
			number.CY_Code = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			AssertEquals("2 licenses", 2, number.Lookups.CY_DataList.Count);
			AssertEquals("BANANA, APPLE", number.Lookups.CY_DataList.CodesAsString);

			declaration.JE_OH_Importer = importerNoLicense.PK;
			AssertEquals("no licenses", 0, number.Lookups.CY_DataList.Count);

			declaration.JE_OH_Importer = importer3.PK;
			AssertEquals("2 licenses", 2, number.Lookups.CY_DataList.Count);
			AssertEquals("CORN, BAGEL", number.Lookups.CY_DataList.CodesAsString);
		}
	}
}
