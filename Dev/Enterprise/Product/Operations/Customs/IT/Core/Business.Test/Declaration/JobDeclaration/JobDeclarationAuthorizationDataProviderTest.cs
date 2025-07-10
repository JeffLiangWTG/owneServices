using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using AuthTypeList = Enterprise.Customs.IT.Business.CusAuthorizationHeaderTypeList.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationAuthorizationDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when declaration parameter is null", () => new JobDeclarationAuthorizationDataProvider(null));
	}

	public void TestAsIAuthorizationListDataProviderEligibleHoldersMember()
	{
		var orgProxyPK = GlbCompany.CurrentCompany.OrgProxy.PK;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		var asIAuthorizationListDataProvider = new JobDeclarationAuthorizationDataProvider(declaration) as IAuthorizationListDataProvider;

		var party1 = Factory.New<OrgHeader>();
		var party2 = Factory.New<OrgHeader>();
		var party3 = Factory.New<OrgHeader>();
		var party4 = Factory.New<OrgHeader>();
		var party5 = Factory.New<OrgHeader>();

		declaration.JE_OH_Importer = party1.PK;
		declaration.JE_OH_Supplier = party2.PK;
		declaration.JE_OA_DeclarantAddress = party3.MainAddress.PK;
		declaration.JE_OH_Forwarder = party4.PK;
		declaration.JE_OA_SellerAddress = party5.MainAddress.PK;
		AssertEquals("[PRE-CONDITION] When importer is set, Representative is defaulted", orgProxyPK, declaration.JE_OA_Representative_ZAddress.OrgPK);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertEligibleHolders(new ZGuid[] { party1.PK, party3.PK, party4.PK, orgProxyPK, party5.PK });

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEligibleHolders(new ZGuid[] { party2.PK, party3.PK, party4.PK, orgProxyPK, party5.PK });

		declaration.JE_MessageType = "XXX";
		AssertEligibleHolders(Array.Empty<ZGuid>());

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = ZGuid.Empty;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_OH_Forwarder = ZGuid.Empty;
		declaration.JE_OA_SellerAddress = ZGuid.Empty;
		declaration.JE_OA_Representative = ZGuid.Empty;
		AssertEligibleHoldersWithCustomMessage($"When involved parties are not set, {nameof(asIAuthorizationListDataProvider.GetEligibleHolders)}", Array.Empty<ZGuid>());

		declaration.JE_OH_Importer = party1.PK;
		declaration.JE_OA_DeclarantAddress = party1.PK;
		declaration.JE_OA_Representative = ZGuid.Empty;
		AssertEligibleHoldersWithCustomMessage($"When involved parties are the same, {nameof(asIAuthorizationListDataProvider.GetEligibleHolders)}", new ZGuid[] { party1.PK });

		void AssertEligibleHolders(ZGuid[] expectedEligibleHolders)
		{
			AssertArrayEqualsByElements($"When JE_MessageType = '{declaration.JE_MessageType}', {nameof(asIAuthorizationListDataProvider.GetEligibleHolders)}", expectedEligibleHolders, asIAuthorizationListDataProvider.GetEligibleHolders().ToArray());
		}

		void AssertEligibleHoldersWithCustomMessage(ZString assertionMessage, ZGuid[] expectedEligibleHolders)
		{
			AssertArrayEqualsByElements(assertionMessage, expectedEligibleHolders, asIAuthorizationListDataProvider.GetEligibleHolders().ToArray());
		}
	}

	public void TestAsIAuthorizationListDataProviderAuthorisationTypeMember()
	{
		var declaration = Factory.New<JobDeclaration>();
		var asIAuthorizationListDataProvider = new JobDeclarationAuthorizationDataProvider(declaration) as IAuthorizationListDataProvider;

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertAuthorizationType(AuthTypeList.ApprovedLocationForImport, AuthTypeList.ApprovedLocationForExport, AuthTypeList.CustomsWarehousingCW1, AuthTypeList.CustomsWarehousingCW2, AuthTypeList.CustomsWarehousingCWP);

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertAuthorizationType(AuthTypeList.ApprovedLocationForExport);

		declaration.JE_MessageType = "XXX";
		AssertAuthorizationType(Array.Empty<ZString>());

		void AssertAuthorizationType(params ZString[] expectedAuthorizationType)
		{
			AssertArrayEqualsByElements($"When JE_MessageType = '{declaration.JE_MessageType}', {nameof(asIAuthorizationListDataProvider.AuthorizationTypes)}", expectedAuthorizationType, asIAuthorizationListDataProvider.AuthorizationTypes.ToArray());
		}
	}

	public void TestIAuthorizationHeaderDataProviderMembers()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationAsIAuthorizationHeaderDataProvider = new JobDeclarationAuthorizationDataProvider(declaration) as IAuthorizationHeaderDataProvider;

		AssertEquals("AuthorizationNumber", ZString.Empty, declarationAsIAuthorizationHeaderDataProvider.AuthorizationNumber);

		declaration.ZG_AuthorisationNumber = "123456";
		AssertEquals("AuthorizationNumber", declaration.ZG_AuthorisationNumber, declarationAsIAuthorizationHeaderDataProvider.AuthorizationNumber);

		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: AuthTypeList.ApprovedLocationForImport, permitHolder: organisation.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisation.PK;
		declaration.ZG_AuthorisationNumber = "1111111";
		AssertArrayEqualsByElements("AuthorizationTypes", new ZString[] { AuthTypeList.ApprovedLocationForImport, AuthTypeList.ApprovedLocationForExport, AuthTypeList.CustomsWarehousingCW1, AuthTypeList.CustomsWarehousingCW2, AuthTypeList.CustomsWarehousingCWP }, declarationAsIAuthorizationHeaderDataProvider.AuthorizationTypes.ToArray());

		AssertEquals("HolderPk", organisation.PK, declarationAsIAuthorizationHeaderDataProvider.HolderPk);
	}

	public void TestAsIAuthorizationHeaderDataProviderAuthorisationTypeMember()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationAsIAuthorizationHeaderDataProvider = new JobDeclarationAuthorizationDataProvider(declaration) as IAuthorizationHeaderDataProvider;

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		AssertAuthorizationType(AuthTypeList.ApprovedLocationForImport, AuthTypeList.ApprovedLocationForExport, AuthTypeList.CustomsWarehousingCW1, AuthTypeList.CustomsWarehousingCW2, AuthTypeList.CustomsWarehousingCWP);

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		AssertAuthorizationType(AuthTypeList.ApprovedLocationForExport);

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertAuthorizationType(Array.Empty<ZString>());

		declaration.JE_MessageType = ZString.Empty;
		AssertAuthorizationType(Array.Empty<ZString>());

		void AssertAuthorizationType(params ZString[] expectedAuthorizationType)
		{
			AssertArrayEqualsByElements($"When JE_MessageType = '{declaration.JE_MessageType}', {nameof(declarationAsIAuthorizationHeaderDataProvider.AuthorizationTypes)}", expectedAuthorizationType, declarationAsIAuthorizationHeaderDataProvider.AuthorizationTypes.ToArray());
		}
	}
}
