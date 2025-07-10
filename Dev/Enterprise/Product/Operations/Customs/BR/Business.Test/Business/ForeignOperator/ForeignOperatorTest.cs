using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ForeignOperator))]
	public class ForeignOperatorTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var foreignOperator = Factory.New<ForeignOperator>();
			AssertEquals("Foreign Operator", foreignOperator.HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			var foreignOperator = Factory.New<ForeignOperator>();
			AssertEquals(CustomsPostedStatusList.Codes.Active, foreignOperator.CGI_CustomsStatus);
			AssertEquals(CusGoodsCatalogProductionInfoTypeList.Codes.FOR, foreignOperator.CGI_Type);
		}

		public void TestLookups()
		{
			var catalogProductionInfo = Factory.New<ForeignOperator>();
			AssertType<ForeignOperatorLookups>(catalogProductionInfo.Lookups);
		}

		public void TestCountryCode()
		{
			var foreignOperatorOrg = Factory.New<OrgHeader>();
			foreignOperatorOrg.OH_Code = "BRB";
			foreignOperatorOrg.OH_FullName = "TEST FOREIGN OPERATOR";
			foreignOperatorOrg.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Comoros)).RL_Code;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Comoros;

			var goodsCatalog = Factory.New<CusGoodsCatalog>();

			var foreignOperator = goodsCatalog.ForeignOperators.AddNew();
			foreignOperator.CountryCode = Core.Constants.CountryCodes.Brazil;

			var cusBRForeignOperator = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator.BFR_OH_ForeignOperator = foreignOperatorOrg.PK;

			AssertEquals("CountryCode Max Length", 2, foreignOperator.CountryCodeInfo.MaxLength);

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(Core.Constants.CountryCodes.Brazil, foreignOperator.CountryCode);
				AssertEquals("CountryCode should not be read only", false, foreignOperator.CountryCodeInfo.ReadOnly);
			}

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				foreignOperator.CGI_BFR_ForeignOperator = cusBRForeignOperator.PK;
				AssertEquals(Core.Constants.CountryCodes.Comoros, foreignOperator.CountryCode);
				AssertEquals("CountryCode should not be read only", true, foreignOperator.CountryCodeInfo.ReadOnly);

				foreignOperator.CGI_BFR_ForeignOperator = ZGuid.Empty;
				foreignOperator.CGI_Reference = Core.Constants.CountryCodes.Brazil;
				AssertEquals(Core.Constants.CountryCodes.Brazil, foreignOperator.CountryCode);
				AssertEquals("CountryCode should not be read only", false, foreignOperator.CountryCodeInfo.ReadOnly);
			}
		}

		public void TestCGI_Reference()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();

			var foreignOperator = goodsCatalog.ForeignOperators.AddNew();
			foreignOperator.CGI_Reference = Core.Constants.CountryCodes.Brazil;

			AssertEquals("CGI_Reference Max Length", 2, foreignOperator.CGI_ReferenceInfo.MaxLength);
			AssertEquals(Core.Constants.CountryCodes.Brazil, foreignOperator.CGI_Reference);
		}

		public void TestIsKnow()
		{
			var foreignOperator = Factory.New<ForeignOperator>();
			AssertEquals("IsKnow", false, foreignOperator.IsKnow);

			foreignOperator.CountryCode = "1";
			AssertEquals("IsKnow", false, foreignOperator.IsKnow);

			foreignOperator.AuthorityCode = "1";
			AssertEquals("IsKnow", true, foreignOperator.IsKnow);

			foreignOperator.CountryCode = ZString.Empty;
			foreignOperator.AuthorityCode = ZString.Empty;
			foreignOperator.CGI_BFR_ForeignOperator = Factory.New<CusBRForeignOperator>().PK;
			AssertEquals("IsKnow", true, foreignOperator.IsKnow);
		}

		public void TestIsActive()
		{
			var catalogProductionInfo = Factory.New<ForeignOperator>();

			catalogProductionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.DeletePending;
			Assert("IsActive should be FALSE", !catalogProductionInfo.IsActive);

			catalogProductionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
			Assert("IsActive should be FALSE", !catalogProductionInfo.IsActive);

			catalogProductionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Active;
			Assert("IsActive should be TRUE", catalogProductionInfo.IsActive);
		}

		public void TestAuthorityCode()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();

			var foreignOperator = goodsCatalog.ForeignOperators.AddNew();
			foreignOperator.CGI_Reference = Core.Constants.CountryCodes.Brazil;
			foreignOperator.AuthorityCode = "1";

			var cusBRForeignOperator = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator.BFR_AuthorityIdentifier = "123";

			AssertEquals("AuthorityCode Max Length", 35, foreignOperator.AuthorityCodeInfo.MaxLength);

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("1", foreignOperator.AuthorityCode);
				AssertEquals("AuthorityCode should not be read only", false, foreignOperator.AuthorityCodeInfo.ReadOnly);
			}

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(ZString.Empty, foreignOperator.AuthorityCode);
				foreignOperator.CGI_BFR_ForeignOperator = cusBRForeignOperator.PK;
				AssertEquals("123", foreignOperator.AuthorityCode);
				AssertEquals("AuthorityCode should be read only", true, foreignOperator.AuthorityCodeInfo.ReadOnly);
			}
		}

		public void TestReadOnly()
		{
			var catalogProductionInfo = Factory.New<ForeignOperator>();
			Assert("Should NOT be readonly", !catalogProductionInfo.ReadOnly);

			catalogProductionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.DeletePending;
			Assert("Should be readonly", catalogProductionInfo.ReadOnly);

			catalogProductionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Active;
			Assert("Should NOT be readonly", !catalogProductionInfo.ReadOnly);

			catalogProductionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
			Assert("Should be readonly", catalogProductionInfo.ReadOnly);
		}

		public void TestCGI_CustomsStatusDescription()
		{
			var catalogProductionInfo = Factory.New<ForeignOperator>();
			AssertEquals("CGI_CustomsStatusDescription", CustomsPostedStatusList.Descriptions.Active, catalogProductionInfo.CGI_CustomsStatusDescription);

			catalogProductionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.DeletePending;
			AssertEquals("CGI_CustomsStatusDescription", CustomsPostedStatusList.Descriptions.DeletePending, catalogProductionInfo.CGI_CustomsStatusDescription);

			catalogProductionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
			AssertEquals("CGI_CustomsStatusDescription", CustomsPostedStatusList.Descriptions.Accepted, catalogProductionInfo.CGI_CustomsStatusDescription);
		}

		public void TestCanDelete()
		{
			var catalogProductionInfo = Factory.New<ForeignOperator>();

			AssertEquals("ReasonForNotAbleToDelete", "Current Customs Status is Deletion Pending. Foreign Operator will only be deleted when message is sent to Customs.", ((ICanDelete)catalogProductionInfo).ReasonForNotAbleToDelete);
			Assert("Can Delete when CGI_CustomsStatus is ACT", catalogProductionInfo.CanDelete);

			catalogProductionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.DeletePending;
			Assert("Cannot Delete when CGI_CustomsStatus is DPD", !catalogProductionInfo.CanDelete);

			catalogProductionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
			Assert("Can Delete when CGI_CustomsStatus is ACC", catalogProductionInfo.CanDelete);
		}

		public void TestIsForeignOperatorModuleEnabled()
		{
			var foreignOperator = Factory.New<ForeignOperator>();
			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert("IsForeignOperatorModuleEnabled should be true", foreignOperator.IsForeignOperatorModuleEnabled);
			}

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert("IsForeignOperatorModuleEnabled should be false", !foreignOperator.IsForeignOperatorModuleEnabled);
			}
		}

		public void TestUpdateForeignOperatorValues()
		{
			var owner = Factory.New<OrgHeader>();

			var foreignOperatorOrg = Factory.New<OrgHeader>();
			foreignOperatorOrg.OH_Code = "BRB";
			foreignOperatorOrg.OH_FullName = "TEST FOREIGN OPERATOR";
			foreignOperatorOrg.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Comoros;

			var cusGoodsCatalog = Factory.New<CusGoodsCatalog>();
			cusGoodsCatalog.CGC_OH_Owner = owner.PK;

			var cusBRForeignOperator = Factory.New<CusBRForeignOperator>();
			cusBRForeignOperator.BFR_OH_Owner = owner.PK;

			var foreignOperator = cusGoodsCatalog.ForeignOperators.AddNew();
			AssertEquals(ZString.Empty, foreignOperator.CountryCode);
			AssertEquals(ZString.Empty, foreignOperator.AuthorityCode);

			foreignOperator.CountryCode = Core.Constants.CountryCodes.Afghanistan;
			foreignOperator.AuthorityCode = "234";
			foreignOperator.CGI_BFR_ForeignOperator = cusBRForeignOperator.PK;

			AssertEquals("CountryCode cleared", ZString.Empty, foreignOperator.CountryCode);
			AssertEquals("AuthorityCode cleared", ZString.Empty, foreignOperator.AuthorityCode);

			cusBRForeignOperator.BFR_RN_NKCountryCode = Core.Constants.CountryCodes.Kyrgyzstan;
			cusBRForeignOperator.BFR_AuthorityIdentifier = "123";

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(Core.Constants.CountryCodes.Kyrgyzstan, foreignOperator.CountryCode);
				AssertEquals("123", foreignOperator.AuthorityCode);
			}
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateNewCusGoodsCatalogProductionInfo(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CreateNewCusGoodsCatalogProductionInfo(factory);

		protected override BusinessObject GetNewBusinessObject() => CreateNewCusGoodsCatalogProductionInfo(Factory);

		ForeignOperator CreateNewCusGoodsCatalogProductionInfo(BusinessObjectFactory factory)
		{
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = "BRB";
			orgHeader.OH_FullName = "TEST CONSIGNEE";
			var catalog = factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog.CGC_OH_Owner = orgHeader.PK;
			var productionInfo = factory.New<ForeignOperator>();
			productionInfo.CGI_CGC_Catalog = catalog.PK;
			productionInfo.CountryCode = "1";
			return productionInfo;
		}
	}
}
