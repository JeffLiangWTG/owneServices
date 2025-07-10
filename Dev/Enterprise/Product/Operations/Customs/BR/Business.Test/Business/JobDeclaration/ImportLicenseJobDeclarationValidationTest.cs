using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public override void TestCheckJE_CustomsOffice()
		{
			base.TestCheckJE_CustomsOffice();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_CustomsOfficeInfo);
		}

		public void TestCheckEntranceOfficeCode()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.EntranceOfficeCodeInfo);
		}

		public void TestCheckJE_GoodsOrigin()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_GoodsOriginInfo, "XX", Enterprise.Core.Constants.CountryCodes.Brazil);
		}

		public void TestCheckJE_OH_Importer()
		{
			var orgWithCMT = Factory.New<OrgHeader>();
			orgWithCMT.OH_Code = "TEST1";
			orgWithCMT.OH_IsConsignee = true;
			orgWithCMT.PrimaryRegistrationNumber.NumberTypeForDisplay = BrazilOrgCusCodeInfo.OrgCusCodes.CMT;

			var orgWithCJN = Factory.New<OrgHeader>();
			orgWithCJN.OH_Code = "TEST2";
			orgWithCJN.OH_IsConsignee = true;
			orgWithCJN.PrimaryRegistrationNumber.NumberTypeForDisplay = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			orgWithCJN.PrimaryRegistrationNumber.Number = "01001001000303";

			var orgWithCPF = Factory.New<OrgHeader>();
			orgWithCPF.OH_Code = "TEST3";
			orgWithCPF.OH_IsConsignee = true;
			orgWithCPF.PrimaryRegistrationNumber.NumberTypeForDisplay = BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration;
			orgWithCPF.PrimaryRegistrationNumber.Number = "01001001000303";

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			declaration.DeclarantType = ZString.Empty;
			declaration.JE_OH_Importer = orgWithCMT.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "Importer Registration Number not found.");
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "The Importer Registration Number differs from CJN - Business Taxpayer Registration.");

			declaration.JE_OH_Importer = orgWithCPF.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, "Importer Registration Number not found.");
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "The Importer Registration Number differs from CJN - Business Taxpayer Registration.");

			declaration.JE_OH_Importer = orgWithCJN.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, "Importer Registration Number not found.");
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, "The Importer Registration Number differs from CJN - Business Taxpayer Registration.");
		}

		public override void TestCheckJE_MessageTypeIsEnteredOrValid()
		{
			base.TestCheckJE_MessageTypeIsEnteredOrValid();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.LPCO;
			declaration.Validation.ValidateJE_MessageType();
			AssertHasError(declaration.JE_MessageTypeInfo, "This Shipment Type can only be used on the Licenses module (Operate > Customs > License).");

			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.Validation.ValidateJE_MessageType();
			AssertNoError(declaration.JE_MessageTypeInfo, "This Shipment Type can only be used on the Licenses module (Operate > Customs > License).");
		}

		public void TestCheckJE_MessageType_LIC_NoOrders()
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			AssertNoErrorContaining(declaration.JE_MessageTypeInfo, "Orders should not be attached to Import License jobs. Please either detach the Order(s) or change the Shipment Type.");

			declaration.AttachedOrders.AddNew();
			declaration.Validation.ValidateJE_MessageType();
			AssertHasErrorContaining(declaration.JE_MessageTypeInfo, "Orders should not be attached to Import License jobs. Please either detach the Order(s) or change the Shipment Type.");
		}

		public void TestCheckJE_MessageSubType()
		{
			declaration.Validation.ValidateJE_MessageSubType();
			AssertNoNotifications(declaration.JE_MessageSubTypeInfo);
		}

		public void TestCheckJE_RL_NKOrigin()
		{
			declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoNotifications(declaration.JE_RL_NKOriginInfo);
		}

		public void TestCheckJE_RL_NKFinalDestination()
		{
			declaration.Validation.ValidateJE_RL_NKFinalDestination();
			AssertNoNotifications(declaration.JE_RL_NKFinalDestinationInfo);
		}

		public void TestCheckJE_RL_NKPortOfLoading()
		{
			declaration.Validation.ValidateJE_RL_NKPortOfLoading();
			AssertNoNotifications(declaration.JE_RL_NKPortOfLoadingInfo);
		}

		public void TestCheckJE_RL_NKPortOfArrival()
		{
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertNoNotifications(declaration.JE_RL_NKPortOfArrivalInfo);
		}

		public override void TestCheckJE_ContainerMode_Mandatory()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.Validation.ValidateJE_ContainerMode();
			AssertNoNotifications(declaration.JE_ContainerModeInfo);
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.ImportLicense;
	}
}
