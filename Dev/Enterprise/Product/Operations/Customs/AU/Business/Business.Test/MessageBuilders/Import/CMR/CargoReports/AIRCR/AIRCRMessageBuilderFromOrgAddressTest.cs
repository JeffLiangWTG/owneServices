using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AIRCRMessageBuilderFromOrgAddressTest : AIRCRMessageBuilderTest
	{
		public void TestIdentificationFromAddress()
		{
			var consignee = SetupOrgHeader();
			consignee.OH_Code = "TSTCN";
			consignee.OH_FullName = "CONSIGNEE";
			consignee.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "112345678901");
			consignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "1987654321");

			var otherConsigneeAddress = consignee.Addresses.AddNew();
			otherConsigneeAddress.OA_Address1 = "Address3";
			otherConsigneeAddress.OA_Address2 = "Address4";
			otherConsigneeAddress.OA_City = "CITY";
			otherConsigneeAddress.OA_State = "NSW";
			otherConsigneeAddress.OA_PostCode = "12345";
			otherConsigneeAddress.OA_Phone = "1234567890";
			otherConsigneeAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var cusCodeOnConsigneePremises = consignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "22222333339999");
			cusCodeOnConsigneePremises.OK_OA_PremisesAddress = otherConsigneeAddress.PK;
			hawb.CS_OA_ConsigneeAddress = otherConsigneeAddress.PK;

			var consignor = SetupOrgHeader();
			consignor.OH_Code = "TSTCZ";
			consignor.OH_FullName = "CONSIGNOR";
			consignor.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "212345678901");
			consignor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "2987654321");

			var otherConsignorAddress = consignor.Addresses.AddNew();
			otherConsignorAddress.OA_Address1 = "Address5";
			otherConsignorAddress.OA_Address2 = "Address6";
			otherConsignorAddress.OA_City = "CITY";
			otherConsignorAddress.OA_State = "NSW";
			otherConsignorAddress.OA_PostCode = "12345";
			otherConsignorAddress.OA_Phone = "1234567890";
			otherConsignorAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var cusCodeOnConsignorPremises = consignor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "44444555559999");
			cusCodeOnConsignorPremises.OK_OA_PremisesAddress = otherConsignorAddress.PK;
			hawb.CS_OA_ConsignorAddress = otherConsignorAddress.PK;

			var generatedMessage = GeneratedMessage;
			AssertContains("NAD+CN++CONSIGNEE::ADDRESS3 ADDRESS4 CITY NSW 12345 AU'", generatedMessage);
			AssertContains("NAD+CZ++CONSIGNOR::ADDRESS5 ADDRESS6 CITY NSW 12345 AU'", generatedMessage);
			AssertContains("Has Consignee CID", "NAD+IM+22222333339::95'", generatedMessage);
			AssertContains("Has Consignor CID", "NAD+SU+44444555559::95'", generatedMessage);
			AssertContains("Has ABN", "NAD+VN+212345678901::95'", generatedMessage);
		}

		protected override void SetConsignee(CusHAWB house, OrgHeader consignee)
		{
			house.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
		}

		protected override void SetConsignor(CusHAWB house, OrgHeader consignor)
		{
			house.CS_OA_ConsignorAddress = consignor.MainAddress.PK;
		}
	}
}
