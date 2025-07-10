using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusExit.Testing
{
	[TestedType(typeof(CusExitDetail))]
	class CusExitDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSiretNumber()
		{
			AssertEquals("39433691100042", SetupExitDetailWithDeclarant().SiretNumber);
		}

		public void TestEoriNumber()
		{
			AssertEquals("40433691100042", SetupExitDetailWithDeclarant().EORINumber);
		}

		public void TestAgentOrDeclarant()
		{
			AssertEquals("TESTSRT", SetupExitDetailWithDeclarant().AgentOrDeclarant.OH_Code);
		}

		CusExitDetail SetupExitDetailWithDeclarant()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var declarantAddress = orgHeader.MainAddress;
			orgHeader.OH_Code = "TESTSRT";
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.FranceCodeTypes.Siret, "39433691100042", Core.Constants.CountryCodes.France);
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "404336911", Core.Constants.CountryCodes.France);
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00042", Core.Constants.CountryCodes.France);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var addressPK = declarantAddress.PK;
			declaration.JE_OA_DeclarantAddress = addressPK;
			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = "JE";
			exitHeader.CEH_ParentID = declaration.PK;
			exitHeader.CEH_OA_Agent = addressPK;
			var cusExitDetail = Factory.New<CusExitDetail>();
			cusExitDetail.CED_CEH = exitHeader.PK;
			return cusExitDetail;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var exitHeader = Factory.New<CusExitControlHeader>();
			return exitHeader.CusExitDetails.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var exitHeader = factory.NewWithValidTestData<CusExitControlHeader>();
			return exitHeader.CusExitDetails.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	}
}
