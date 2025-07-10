using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(OrgHeaderWrapper))]
	sealed class OrgHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertNotNull(wrapper);
		}

		public void TestHasCCIDWithSameAddress()
		{
			var headerWrapper = wrapper as OrgHeaderWrapper;
			header.CustomsCodes.RemoveAndDeleteAll();
			AssertEquals("HasCCIDWithSameAddress", false, headerWrapper.HasCCIDWithSameAddress);
			var address = Factory.New<OrgAddress>();
			address.Address1 = "Address1";
			address.Address2 = "Address2";
			address.City = "Sydney";
			address.Postcode = "1111";
			var orgCusCode1 = header.CustomsCodes.AddNew();
			orgCusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			orgCusCode1.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			orgCusCode1.OK_CustomsRegNo = "1234";
			var dataProvider = headerWrapper.CLREGInfoProvider;
			dataProvider.ZA_Bsn1 = ZString.Empty;
			dataProvider.ZA_Bsn2 = ZString.Empty;
			dataProvider.ZA_BsnCity = ZString.Empty;
			dataProvider.ZA_BsnPostCode = ZString.Empty;
			AssertEquals("HasCCIDWithSameAddress when has matched empty address", true, headerWrapper.HasCCIDWithSameAddress);

			var orgCusCode2 = header.CustomsCodes.AddNew();
			orgCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			orgCusCode2.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			orgCusCode2.OK_CustomsRegNo = "5678";
			orgCusCode2.OK_OA_PremisesAddress = address.PK;
			dataProvider.ZA_Bsn1 = "Address1";
			dataProvider.ZA_Bsn2 = "Address2";
			dataProvider.ZA_BsnCity = "Sydney";
			dataProvider.ZA_BsnPostCode = "1111";
			AssertEquals("HasCCIDWithSameAddress when has matched non-empty address", true, headerWrapper.HasCCIDWithSameAddress);
		}

		public void TestGoodsOwnerPartyID()
		{
			AssertEquals("GoodsOwnerPartyID", ZString.Empty, wrapper.GoodsOwnerPartyID);
			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "54321");
			var cusCode = header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
			AssertEquals("GoodsOwnerPartyID", "54321", wrapper.GoodsOwnerPartyID);
			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			AssertEquals("GoodsOwnerPartyID", ZString.Empty, wrapper.GoodsOwnerPartyID);
			header.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345");
			AssertEquals("GoodsOwnerPartyID", "12345", wrapper.GoodsOwnerPartyID);
		}

		public void TestIsCompanyOrg()
		{
			Assert("!Wrapper.IsCompanyOrg", !wrapper.IsCompanyOrg);
			wrapper = new OrgHeaderWrapper(GlbCompany.CurrentCompany.OrgProxy);
			Assert("Wrapper.IsCompanyOrg", wrapper.IsCompanyOrg);
		}

		[ExpectNoExceptions]
		public void TestIsCompanyOrgForNullOrgHeaderProxy()
		{
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			Assert("!Wrapper.IsCompanyOrg", !wrapper.IsCompanyOrg);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<OrgHeader>();
			return new OrgHeaderWrapper(header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<OrgHeader>();
			wrapper = new OrgHeaderWrapper(header);
		}

		OrgHeader header;
		IOrgHeaderWrapper wrapper;
	}
}
