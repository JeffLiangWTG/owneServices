using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(TradeChainPartner))]
	sealed class TradeChainPartnerTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<TradeChainPartner>
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		public void TestCA_CSAID()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";

			var orgAddress1 = Factory.New<OrgAddress>();
			orgAddress1.OA_OH = org.PK;
			orgAddress1.Address1 = "Test Address 1";

			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = org.PK;
			orgAddress2.Address1 = "Test Address 2";

			var orgCCC = org.CustomsCodes.AddNew();
			orgCCC.OK_OH = org.PK;
			orgCCC.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCCC.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			orgCCC.OK_CustomsRegNo = "ascvb9";
			orgCCC.OK_OA_PremisesAddress = orgAddress1.PK;

			var orgDUN1 = org.CustomsCodes.AddNew();
			orgDUN1.OK_OH = org.PK;
			orgDUN1.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			orgDUN1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			orgDUN1.OK_CustomsRegNo = "645321789";
			orgDUN1.OK_OA_PremisesAddress = orgAddress1.PK;

			Factory.Save();

			var orgImpAddInfo = OrgImpAddInfo.Get(org);

			var tcp1 = orgImpAddInfo.TradeChainPartners.AddNew();
			tcp1.CA_Org = org.PK;
			tcp1.CA_Address = orgAddress1.PK;
			tcp1.CA_CSAIDType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			AssertEquals("645321789", tcp1.CA_CSAID);

			var tcp2 = orgImpAddInfo.TradeChainPartners.AddNew();
			tcp2.CA_Org = org.PK;
			tcp2.CA_Address = orgAddress1.PK;
			tcp2.CA_CSAIDType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			AssertEquals(string.Empty, tcp2.CA_CSAID);
		}

		public void TestCA_CSAIDReadOnly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var tcp = OrgImpAddInfo.Get(org).TradeChainPartners.AddNew();
			Assert("Is ReadOnly by default", tcp.CA_CSAIDInfo.ReadOnly);

			tcp.CA_CSAIDType = OrgCusCode.CACodeTypes.BusinessNumberForImportExport;
			Assert("Is ReadOnly when CSA ID type is not 'CSA'", tcp.CA_CSAIDInfo.ReadOnly);

			tcp.CA_CSAIDType = OrgCusCode.CACodeTypes.CSAReferenceID;
			Assert("Is editable when CSA ID type is 'CSA'", !tcp.CA_CSAIDInfo.ReadOnly);
		}

		public void TestCA_Action()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tcp = OrgImpAddInfo.Get(org).TradeChainPartners.AddNew();
			AssertEquals(CSAActionTypeList.Codes.ReqAdd, tcp.CA_Action);
		}

		public void TestCA_CSAStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tcp = OrgImpAddInfo.Get(org).TradeChainPartners.AddNew();
			AssertEquals(CSAStatusList.Codes.New, tcp.CA_CSAStatus);

			tcp.CA_Action = CSAActionTypeList.Codes.ReqAdd;
			tcp.CA_CSAStatus = CSAStatusList.Codes.Added;
			AssertEquals(ZString.Empty, tcp.CA_Action);
		}

		public void TestCA_CSAStatusReadOnly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tcp = OrgImpAddInfo.Get(org).TradeChainPartners.AddNew();
			tcp.CA_CSAStatus = CSAStatusList.Codes.Added;
			AssertEquals(false, tcp.CA_CSAStatusInfo.ReadOnly);

			tcp.CA_CSAStatus = CSAStatusList.Codes.Deleted;
			AssertEquals(true, tcp.CA_CSAStatusInfo.ReadOnly);

			tcp.CA_CSAStatus = CSAStatusList.Codes.New;
			AssertEquals(false, tcp.CA_CSAStatusInfo.ReadOnly);

			tcp.CA_CSAStatus = CSAStatusList.Codes.AwaitingAdd;
			AssertEquals(true, tcp.CA_CSAStatusInfo.ReadOnly);

			tcp.CA_CSAStatus = ZString.Empty;
			AssertEquals(false, tcp.CA_CSAStatusInfo.ReadOnly);
		}

		public void TestCA_ActionReadOnly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tcp = OrgImpAddInfo.Get(org).TradeChainPartners.AddNew();
			tcp.CA_CSAStatus = CSAStatusList.Codes.AwaitingAdd;
			AssertEquals(true, tcp.CA_ActionInfo.ReadOnly);

			tcp.CA_CSAStatus = CSAStatusList.Codes.Added;
			AssertEquals(false, tcp.CA_ActionInfo.ReadOnly);

			tcp.CA_CSAStatus = CSAStatusList.Codes.AwaitingDelete;
			AssertEquals(true, tcp.CA_ActionInfo.ReadOnly);

			tcp.CA_CSAStatus = CSAStatusList.Codes.Deleted;
			AssertEquals(false, tcp.CA_ActionInfo.ReadOnly);

			tcp.CA_CSAStatus = CSAStatusList.Codes.ErrorAdded;
			AssertEquals(false, tcp.CA_ActionInfo.ReadOnly);

			tcp.CA_CSAStatus = CSAStatusList.Codes.ErrorDeleted;
			AssertEquals(false, tcp.CA_ActionInfo.ReadOnly);

			tcp.CA_CSAStatus = CSAStatusList.Codes.New;
			AssertEquals(false, tcp.CA_ActionInfo.ReadOnly);

			tcp.CA_CSAStatus = ZString.Empty;
			AssertEquals(false, tcp.CA_ActionInfo.ReadOnly);
		}

		public void TestStatusDescription()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tcp = OrgImpAddInfo.Get(org).TradeChainPartners.AddNew();
			tcp.CA_CSAStatus = "DEN";
			AssertEquals(ZString.Empty, tcp.StatusDescription);
			Assert(tcp.StatusDescriptionReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return tradeChainPartner;
		}
		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return tradeChainPartner;
		}

		protected override IEnumerable<TradeChainPartner> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			var orgImpAddInfo = OrgImpAddInfo.Get(org);
			yield return orgImpAddInfo.TradeChainPartners.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgImpAddInfo = OrgImpAddInfo.Get(org);
			tradeChainPartner = orgImpAddInfo.TradeChainPartners.AddNew();
		}
		TradeChainPartner tradeChainPartner;
	}
}
