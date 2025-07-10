using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CIQRequiredDocumentAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCIQRequiredDocumentTypes()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var testItems2 = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			testItems.JobDeclaration.JE_MessageType = "IMP";
			testItems2.JobDeclaration.JE_MessageType = "IMP";
			var testList = testItems.EntryInstruction.CIQRequiredDocuments.AddNew().AddInfoLookups.CIQRequiredDocumentTypes;
			var testList2 = testItems2.EntryInstruction.CIQRequiredDocuments.AddNew().AddInfoLookups.CIQRequiredDocumentTypes;
			AssertSame("Should be cached", testList, testList2);
			var fullList = new CIQRequiredDocumentTypeList();
			AssertEquals(fullList.Count - 3, testList.Count);
			Assert("Should not contain code 20", !testList.ContainsCode("20"));
			Assert("Should not contain code 22", !testList.ContainsCode("22"));
			Assert("Should not contain code 22", !testList.ContainsCode("96"));
			testItems.JobDeclaration.JE_MessageType = "EXP";
			testItems2.JobDeclaration.JE_MessageType = "EXP";
			testList = testItems.EntryInstruction.CIQRequiredDocuments.AddNew().AddInfoLookups.CIQRequiredDocumentTypes;
			testList2 = testItems2.EntryInstruction.CIQRequiredDocuments.AddNew().AddInfoLookups.CIQRequiredDocumentTypes;
			AssertSame("Should be cached", testList, testList2);
			AssertEquals(fullList.Count - 3, testList.Count);
			Assert("Should not contain code 21", !testList.ContainsCode("21"));
			Assert("Should not contain code 21", !testList.ContainsCode("24"));
			Assert("Should not contain code 21", !testList.ContainsCode("95"));
		}
	}
}
