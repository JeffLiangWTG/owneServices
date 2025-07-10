using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEDocListAndCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();

			var docManagerInfo = declaration.DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "PDF");
			var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], "Test2.txt", "TXT");

			var eDocList = supportingDocument.Lookups.EDocList;
			AssertEquals(2, supportingDocument.Lookups.EDocList.Count);
			AssertEquals(eDoc1.UniqueKey, ((AvailableEDocList)eDocList)[0].PK);
			AssertEquals(eDoc2.UniqueKey, ((AvailableEDocList)eDocList)[1].PK);
		}

		public void TestCodeList()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType("ILDOC", "Doc. LIst", Core.Constants.CountryCodes.Israel);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "ILDOC", "830", "Test code2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "ILDOC", "380", "Test code1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();

			var codeList = supportingDocument.Lookups.CodeList as CodeDescriptionPairList;
			AssertEquals(2, codeList.Count);
			AssertEquals("list should be sorted", "380, 830", codeList.CodesAsString);
		}
	}
}
