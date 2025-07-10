using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TradeChainPartnerAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCA_CSAID()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";

			var impAddInfo = OrgImpAddInfo.Get(org);

			var tcp = impAddInfo.TradeChainPartners.AddNew();
			tcp.CA_CSAIDType = CSAConsigneeIDTypeList.Codes.CSA;
			tcp.AddInfoValidation.ValidateCA_CSAID();
			AssertHasErrorContaining(tcp.CA_CSAIDInfo, "");

			tcp.CA_CSAIDType = CSAConsigneeIDTypeList.Codes.BRM;
			tcp.AddInfoValidation.ValidateCA_CSAID();
			AssertNoErrors(tcp.CA_CSAIDInfo);

			var tcp1 = impAddInfo.TradeChainPartners.AddNew();
			tcp1.CA_CSAID = "000AISSSZ";
			tcp1.AddInfoValidation.ValidateCA_CSAID();
			var tcp2 = impAddInfo.TradeChainPartners.AddNew();
			tcp2.CA_CSAID = "000AISSSZ";
			tcp2.AddInfoValidation.ValidateCA_CSAID();
			AssertHasMessageErrorContaining(tcp2.CA_CSAIDInfo, "Duplicate CSAIDs");
		}

		public void TestCheckCA_CSAIDType()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";

			var impAddInfo = OrgImpAddInfo.Get(org);

			var tcp = impAddInfo.TradeChainPartners.AddNew();
			tcp.AddInfoValidation.ValidateCA_CSAIDType();
			AssertHasMessageErrorContaining(tcp.CA_CSAIDTypeInfo, "You have not entered a value.");

			tcp.CA_Type = TradeChainPartnersTypeList.Codes.C;
			tcp.CA_CSAIDType = CSAConsigneeIDTypeList.Codes.BRM;
			tcp.AddInfoValidation.ValidateCA_CSAIDType();
			AssertNoMessageErrorContaining(tcp.CA_CSAIDTypeInfo, "You have not entered a value.");

			tcp.CA_Type = TradeChainPartnersTypeList.Codes.V;
			tcp.CA_CSAIDType = CSAConsigneeIDTypeList.Codes.BRM;
			tcp.AddInfoValidation.ValidateCA_CSAIDType();
			AssertHasMessageErrorContaining(tcp.CA_CSAIDTypeInfo, "The code you have selected is not in the list.");

			tcp.CA_Type = TradeChainPartnersTypeList.Codes.C;
			tcp.CA_CSAIDType = CSAVendorIDTypeList.Codes.CCC;
			tcp.AddInfoValidation.ValidateCA_CSAIDType();
			AssertHasMessageErrorContaining(tcp.CA_CSAIDTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckCA_Type()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";

			var impAddInfo = OrgImpAddInfo.Get(org);

			var tcp = impAddInfo.TradeChainPartners.AddNew();
			tcp.AddInfoValidation.ValidateCA_Type();
			AssertHasMessageErrorContaining(tcp.CA_TypeInfo, "You have not entered a value.");

			tcp.CA_Type = TradeChainPartnersTypeList.Codes.C;
			tcp.AddInfoValidation.ValidateCA_Type();
			AssertNoMessageErrors(tcp.CA_TypeInfo);

			tcp.CA_Type = "X";
			tcp.AddInfoValidation.ValidateCA_Type();
			AssertHasMessageErrorContaining(tcp.CA_TypeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckCA_Status()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";

			var impAddInfo = OrgImpAddInfo.Get(org);

			var tcp = impAddInfo.TradeChainPartners.AddNew();
			AssertNoMessageErrors(tcp.CA_CSAStatusInfo);

			tcp.CA_CSAStatus = "XXX";
			tcp.AddInfoValidation.ValidateCA_CSAStatus();
			AssertHasMessageErrorContaining(tcp.CA_CSAStatusInfo, "The code you have selected is not in the list.");

			tcp.CA_CSAStatus = CSAStatusList.Codes.Added;
			tcp.AddInfoValidation.ValidateCA_CSAStatus();
			AssertNoMessageErrors(tcp.CA_CSAStatusInfo);
		}

		public void TestCheckCA_Action()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";

			var impAddInfo = OrgImpAddInfo.Get(org);

			var tcp = impAddInfo.TradeChainPartners.AddNew();
			AssertEquals("ReqAdd", tcp.CA_Action);

			tcp.CA_CSAStatus = CSAStatusList.Codes.Added;
			tcp.CA_Action = CSAActionTypeList.Codes.ReqAdd;
			AssertHasWarningContaining(tcp.CA_ActionInfo, "This TCP record is already added at CBSA and can not be resent");

			tcp.CA_CSAStatus = CSAStatusList.Codes.New;
			tcp.CA_Action = CSAActionTypeList.Codes.ReqDel;
			AssertHasWarningContaining(tcp.CA_ActionInfo, "This TCP record has not been added by CBSA and can not be deleted");

			tcp.CA_Action = "DDA";
			AssertHasErrorContaining(tcp.CA_ActionInfo, "Actions should be Request Add or Request Delete.");

			tcp.CA_Action = ZString.Empty;
			AssertNoNotifications(tcp.CA_ActionInfo);
		}
	}
}
