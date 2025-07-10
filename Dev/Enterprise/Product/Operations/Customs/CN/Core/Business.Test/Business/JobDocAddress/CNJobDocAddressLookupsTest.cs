using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	public class CNJobDocAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGovRegNumTypes()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_MessageSubType = "CUS";
			var docAddress = declaration.ImporterDocumentaryAddress;
			var testList = docAddress.Lookups.GovRegNumTypes;

			AssertEquals("Should have 6 items.", 6, testList.Count);
			AssertEquals("AEO", true, testList.Contains(new CodeDescriptionPair("AEO", "Authorized Economic Operator")));
			AssertEquals("MMR", true, testList.Contains(new CodeDescriptionPair("MMR", "Meat Manufacturer Registration Number")));
			AssertEquals("SMR", true, testList.Contains(new CodeDescriptionPair("SMR", "Seafood Manufacturer Registration Number")));
			AssertEquals("CCD", true, testList.Contains(new CodeDescriptionPair("CCD", "Customs Client Code")));
			AssertEquals("USC", true, testList.Contains(new CodeDescriptionPair("USC", "Unified Social Credit Identifier")));
			AssertEquals("CIQ", true, testList.Contains(new CodeDescriptionPair("CIQ", "China Import-Export Inspection and Quarantine Code")));

			var anotherList = declaration.SupplierDocumentaryAddress.Lookups.GovRegNumTypes;
			AssertSame("Should have cahced GovRegNumTypes", testList, anotherList);
		}

		public void TestOverseasPartyCodes()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_MessageSubType = "CUS";
			var docAddress = declaration.ImporterDocumentaryAddress;
			var testList = docAddress.Lookups.OverseasPartyCodes;

			AssertEquals("Should have 3 items in OverseasPartyCodes", 3, testList.Count);
			AssertEquals("AEO", true, testList.Contains(new CodeDescriptionPair("AEO", "Authorized Economic Operator")));
			AssertEquals("MMR", true, testList.Contains(new CodeDescriptionPair("MMR", "Meat Manufacturer Registration Number")));
			AssertEquals("SMR", true, testList.Contains(new CodeDescriptionPair("SMR", "Seafood Manufacturer Registration Number")));
		}
	}
}
