using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegPremises))]
sealed class CusTempStorageRegPremisesTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookup()
	{
		AssertType<CusTempStorageRegPremisesLookups>(premises.Lookups);
	}

	public void TestAuthorizationType()
	{
		var cusTempStorageRegPremisesForTest = Factory.New<CusTempStorageRegPremisesForTest>();
		AssertType<CusAuthorizationUsage>(cusTempStorageRegPremisesForTest.AuthorizationExposed);
	}

	public void TestDefaultAuthorizationCode()
	{
		var cusTempStorageRegPremisesForTest = Factory.New<CusTempStorageRegPremisesForTest>();
		AssertEquals("DefaultAuthorizationCode = TST", AuthorizationTypeList.Codes.TST, cusTempStorageRegPremisesForTest.DefaultAuthorizationCodeExposed);
	}

	public void TestDefaultSRP_OA_PremisesAddress()
	{
		CombineAssertions(() =>
		{
			var newFactory = new BusinessObjectFactory();
			var orgHeader = newFactory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = newFactory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";
			var orgHeader2 = newFactory.New<OrgHeader>();
			orgHeader2.OH_Code = "BBB";
			var orgAddress2 = newFactory.New<OrgAddress>();
			orgAddress2.OA_OH = orgHeader2.PK;
			orgAddress2.OA_Address1 = "Address2";

			var authHeader = newFactory.New<CusAuthorisationHeader>();
			authHeader.CPH_Type = AuthorizationTypeList.Codes.TST;
			authHeader.CPH_Number = "AH30001";
			authHeader.CPH_OH_PermitHolder = orgHeader.PK;
			authHeader.CPH_OA_AppliesTo = orgAddress.PK;

			var authHeader2 = newFactory.New<CusAuthorisationHeader>();
			authHeader2.CPH_Type = AuthorizationTypeList.Codes.TST;
			authHeader2.CPH_Number = "AH30002";
			authHeader2.CPH_OH_PermitHolder = orgHeader2.PK;
			authHeader2.CPH_OA_AppliesTo = orgAddress2.PK;
			newFactory.Save();

			premises.AuthorizationNumber = "AH30001";
			AssertEquals("Defaulted by the logic in CusAuthorizationUsage", orgHeader.PK, premises.AuthorizationOwner);

			AssertEquals("Defaulted Address when AuthNumber and AuthOwner", orgAddress.PK, premises.SRP_OA_PremisesAddress);

			premises.AuthorizationNumber = "AH30002";
			AssertEquals("Not Defaulted again by the logic in CusAuthorizationUsage", orgHeader.PK, premises.AuthorizationOwner);

			AssertEquals("Not Defaulted Address when it is not previously empty", orgAddress.PK, premises.SRP_OA_PremisesAddress);

			premises.AuthorizationOwner = ZGuid.Empty;
			premises.SRP_OA_PremisesAddress = ZGuid.Empty;
			premises.AuthorizationNumber = "AH30003";

			AssertEquals("Not Defaulted Owner there is no existing Auth with AH30003 number", ZGuid.Empty, premises.AuthorizationOwner);

			AssertEquals("Not Defaulted Address there is no existing Auth with AH30003 number", ZGuid.Empty, premises.SRP_OA_PremisesAddress);
		});
	}

	public void TestAuthorizationNumber()
	{
		CombineAssertions(() =>
		{
			AssertResourceStringData(premises.AuthorizationNumberInfo, "Authorization Number", "Authorization No.", "Auth. No.", "Number of Authorization");

			AssertEquals("MaxLength", 35, premises.AuthorizationNumberInfo.MaxLength);
		});
	}

	public void TestAuthorizationPropertiesGetter()
	{
		CombineAssertions(() =>
		{
			var authorization = Factory.New<CusAuthorizationUsage>();
			authorization.AGC_ParentID = premises.PK;
			authorization.AGC_ParentTableCode = premises.TablePrefix;
			authorization.AGC_Number = "testnumber";
			authorization.AGC_OH_Owner = ZGuid.BrettsGuid;
			AssertEquals("Has authorization, AuthorizationNumber", "testnumber", premises.AuthorizationNumber);
			AssertEquals("Has authorization, AuthorizationOwner", ZGuid.BrettsGuid, premises.AuthorizationOwner);

			authorization.Delete();
			AssertEquals("No authorization, AuthorizationNumber", ZString.Empty, premises.AuthorizationNumber);
			AssertEquals("No authorization, AuthorizationOwner", ZGuid.Empty, premises.AuthorizationOwner);
		});
	}

	public void TestAuthorizationPropertiesSetter()
	{
		CusAuthorizationUsage RetrieveLinkedCusAuthorizationUsage()
		{
			var authorizationQuery = new ZQuery(new ZQuery(CusAuthorizationUsageSchema.AGC_ParentID, premises.PK));
			_ = authorizationQuery.AddToFilter(new ZQuery(CusAuthorizationUsageSchema.AGC_ParentTableCode, premises.TablePrefix));
			return Factory.Load<CusAuthorizationUsage>(authorizationQuery).SingleOrDefault();
		}

		var existingAuthorization = Factory.New<CusAuthorizationUsage>();
		existingAuthorization.AGC_ParentID = premises.PK;
		existingAuthorization.AGC_ParentTableCode = premises.TablePrefix;

		CombineAssertions(() =>
		{
			premises.AuthorizationNumber = "testnumber";
			premises.AuthorizationOwner = ZGuid.BrettsGuid;

			var loadedAuthorization = RetrieveLinkedCusAuthorizationUsage();
			AssertEquals("Update existing authorization", existingAuthorization.PK, loadedAuthorization.PK);
			AssertEquals("Existing authorization, AuthorizationNumber", "testnumber", loadedAuthorization.AGC_Number);
			AssertEquals("Existing authorization, AuthorizationOwner", ZGuid.BrettsGuid, loadedAuthorization.AGC_OH_Owner);

			existingAuthorization.Delete();
			premises.AuthorizationNumber = "testnumber";
			premises.AuthorizationOwner = ZGuid.BrettsGuid;
			var createdAuthorization = RetrieveLinkedCusAuthorizationUsage();
			AssertNotEquals("Authorization is created when needed", existingAuthorization.PK, createdAuthorization.PK);
			AssertEquals("New authorization, AuthorizationNumber", "testnumber", createdAuthorization.AGC_Number);
			AssertEquals("New authorization, AuthorizationOwner", ZGuid.BrettsGuid, createdAuthorization.AGC_OH_Owner);
		});
	}

	public void TestAuthorizationOwner()
	{
		CombineAssertions(() =>
		{
			AssertResourceStringData(premises.AuthorizationOwnerInfo, "Owner", "Owner", "Owner", "Authorization Owner");
		});
	}

	public void TestCustomsNumberProvider()
	{
		CombineAssertions(() =>
		{
			var provider = ((ICustomsNumberViewStmNumsParent)premises).CustomsNumberProvider;
			AssertEquals("Default Provider", "Enterprise.Customs.EU.TemporaryStorage.GUI.TSCustomsNumberViewStmNumsGuiProvider", provider.GetType().FullName);
		});
	}

	public void TestCreateTSCustomsNumberViewStmNumsBusinessProviderFactory()
	{
		var cusTempStorageRegPremises = Factory.New<CusTempStorageRegPremisesForTest>();
		AssertType<TSCustomsNumberViewStmNumsBusinessProviderFactory>(cusTempStorageRegPremises.NumberProviderFactory_Exposed);
	}

	public void TestDefaultCustomsNumberViewStmNumsBusinessProvider()
	{
		var cusTempStorageRegPremises = Factory.New<CusTempStorageRegPremisesForTest>();
		var provider = cusTempStorageRegPremises.NumberProviderFactory_Exposed
			.GetProvider(cusTempStorageRegPremises.CustomsNumberProviderKey, cusTempStorageRegPremises.PK);
		AssertEquals("Default Provider", "Enterprise.Customs.EU.TemporaryStorage.GUI.TSCustomsNumberViewStmNumsGuiProvider", provider.GetType().FullName);
	}

	void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption, string shortCaption, string fullDescription)
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info);
		AssertEquals("Caption", caption, captionResourceString.Caption);
		AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
		AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
		AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetPremises(factory);

	static CusTempStorageRegPremises GetPremises(BusinessObjectFactory factory)
	{
		var premises = factory.New<CusTempStorageRegPremises>();
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";

		var orgHeader = factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		return premises;
	}

	protected override void SetUp()
	{
		premises = Factory.New<CusTempStorageRegPremises>();
	}

	CusTempStorageRegPremises premises;

	class CusTempStorageRegPremisesForTest : CusTempStorageRegPremises
	{
		public CusTempStorageRegPremisesForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public EFTA.TemporaryStorageRegister.Business.TSCustomsNumberViewStmNumsBusinessProviderFactory NumberProviderFactory_Exposed => NumberProviderFactory;

		public CusAuthorizationUsage AuthorizationExposed => Authorization;

		public ZString DefaultAuthorizationCodeExposed => DefaultAuthorizationCode;
	}
}
