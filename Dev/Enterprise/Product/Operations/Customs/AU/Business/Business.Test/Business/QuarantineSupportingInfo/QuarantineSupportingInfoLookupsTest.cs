using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineSupportingInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDescriptionList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("NDECT", "NDECT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "NDECT", "Test1", "Test1234", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, "NDECT", "Test2", "Test5678", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			var supportingInfo = invoiceHeader.QuarantineExDocHeader.SupportingInfos.AddNew();
			AssertEquals("Test1, Test2", supportingInfo.Lookups.DeclarationCodeList.CodesAsString);
		}
	}
}
