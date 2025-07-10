using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusAuthorizationUsage))]
	sealed class CusAuthorisationUsageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var cusAuthorizationUsage = CreateAuthorizationUsage();
			AssertType<CusAuthorizationUsageLookups>(cusAuthorizationUsage.Lookups);
		}

		public void TestValidation()
		{
			var cusAuthorizationUsage = CreateAuthorizationUsage();
			AssertType<EU.Business.CusAuthorizationUsageValidation>(cusAuthorizationUsage.Validation);
		}

		public void TestAGC_Code_Caption()
		{
			var cusAuthorizationUsage = CreateAuthorizationUsage();
			NCTSTestHelper.AssertCaptions(cusAuthorizationUsage.AGC_CodeInfo, "Code", string.Empty, string.Empty);
		}

		public void TestAGC_Number_Caption()
		{
			var cusAuthorizationUsage = CreateAuthorizationUsage();
			NCTSTestHelper.AssertCaptions(cusAuthorizationUsage.AGC_NumberInfo, "Authorization Number", "Auth. Number", "Auth. No.");
		}

		public void TestAGC_OH_Owner_Caption()
		{
			var cusAuthorizationUsage = CreateAuthorizationUsage();
			NCTSTestHelper.AssertCaptions(cusAuthorizationUsage.AGC_OH_OwnerInfo, "Authorization Owner", "Auth. Owner", "Owner");
		}

		public void TestValidation_Default()
		{
			var testNctsHeader = Factory.New<NctsHeader>();
			testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var cusAuthorizationUsage = testNctsHeader.CusAuthorizationUsages.AddNew();

			AssertType<EU.Business.CusAuthorizationUsageValidation>("Default validation type", cusAuthorizationUsage.Validation);
		}

		public void TestValidation_Phase5()
		{
			var testNctsHeader = Factory.New<NctsHeader>();
			testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var cusAuthorizationUsage = testNctsHeader.CusAuthorizationUsages.AddNew();

			AssertType<CusAuthorizationUsagePhase5Validation>("Phase5 validation", cusAuthorizationUsage.Validation);
		}

		public void TestAGC_Code_ShouldPopulateNumberAndOwner_WhenSingleAuthorizationWithSameCodeExist()
		{
			// Arrange
			const string authorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var testNctsHeader = CreateNCTS5DepartureHeader();
			testNctsHeader.Principal.OrganisationPK = organization.PK;

			var cusAuthorizationHeader = CreateCusAuthorizationHeader(authorizationType, organization);

			var cusAuthorizationUsage = testNctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();

			AssertEquals("Precondition", string.Empty, cusAuthorizationUsage.AGC_Number);
			AssertEquals("Precondition", ZGuid.Empty, cusAuthorizationUsage.AGC_OH_Owner);

			// Act
			cusAuthorizationUsage.AGC_Code = authorizationType;

			// Assert
			AssertEquals(cusAuthorizationHeader.CPH_Number, cusAuthorizationUsage.AGC_Number);
			AssertEquals(organization.PK, cusAuthorizationUsage.AGC_OH_Owner);
		}

		public void TestAGC_Code_ShouldPopulateOwner_WhenManyAuthorizationsWithSameCodeExist()
		{
			// Arrange
			const string authorizationType = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var testNctsHeader = CreateNCTS5DepartureHeader();
			testNctsHeader.Principal.OrganisationPK = organization.PK;

			CreateCusAuthorizationHeader(authorizationType, organization);
			CreateCusAuthorizationHeader(authorizationType, organization);

			var cusAuthorizationUsage = testNctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();

			AssertEquals("Precondition", ZGuid.Empty, cusAuthorizationUsage.AGC_OH_Owner);

			// Act
			cusAuthorizationUsage.AGC_Code = authorizationType;

			// Assert
			AssertEquals(organization.PK, cusAuthorizationUsage.AGC_OH_Owner);
		}

		public void TestAGC_Code_ShouldPopulateOwner_WhenNoAuthorizationsWithSameCodeExist()
		{
			// Arrange
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var testNctsHeader = CreateNCTS5DepartureHeader();
			testNctsHeader.Principal.OrganisationPK = organization.PK;

			var cusAuthorizationUsage = testNctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();

			AssertEquals("Precondition", ZGuid.Empty, cusAuthorizationUsage.AGC_OH_Owner);

			// Act
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;

			// Assert
			AssertEquals(organization.PK, cusAuthorizationUsage.AGC_OH_Owner);
		}

		public void TestAGC_Code_ShouldNotChangeOwner_WhenPrincipalIsEmpty()
		{
			// Arrange
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var testNctsHeader = CreateNCTS5DepartureHeader();
			testNctsHeader.Principal.OrganisationPK = ZGuid.Empty;

			var cusAuthorizationUsage = testNctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_OH_Owner = organization.PK;

			// Act
			cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;

			// Assert
			AssertEquals(organization.PK, cusAuthorizationUsage.AGC_OH_Owner);
		}

		NctsHeader CreateNCTS5DepartureHeader()
		{
			var testNctsHeader = Factory.New<NctsHeader>();
			testNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			return testNctsHeader;
		}

		CusAuthorisationHeader CreateCusAuthorizationHeader(string type, OrgHeader organization)
		{
			var cusAuthorizationHeaderACR = Factory.New<CusAuthorisationHeader>();
			cusAuthorizationHeaderACR.CPH_Type = type;
			cusAuthorizationHeaderACR.CPH_OH_PermitHolder = organization.PK;
			cusAuthorizationHeaderACR.CPH_Number = "123";
			cusAuthorizationHeaderACR.CPH_StartDate = ZDate.Today.AddDays(-1);
			cusAuthorizationHeaderACR.CPH_EndDate = ZDate.Today.AddDays(1);
			return cusAuthorizationHeaderACR;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusAuthorizationUsage = CreateAuthorizationUsage();
			cusAuthorizationUsage.AGC_Code = "abc";
			cusAuthorizationUsage.AGC_Number = "123";
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
			return cusAuthorizationUsage;
		}

		CusAuthorizationUsage CreateAuthorizationUsage()
		{
			var testNctsHeader = Factory.New<NctsHeader>();
			testNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var authorization = testNctsHeader.CusAuthorizationUsages.AddNew();
			return authorization;
		}
	}
}
