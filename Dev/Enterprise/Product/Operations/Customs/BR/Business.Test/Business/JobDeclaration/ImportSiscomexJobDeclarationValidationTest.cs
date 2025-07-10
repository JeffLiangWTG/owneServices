using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportSiscomexJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public override void TestCheckJE_CustomsOffice()
		{
			base.TestCheckJE_CustomsOffice();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_CustomsOfficeInfo);
		}

		public override void TestCheckJE_LocationOfGoods()
		{
			base.TestCheckJE_LocationOfGoods();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_LocationOfGoodsInfo);
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
			declaration.DeclarantType = DeclarantTypeList.Codes.LegalPerson;
			declaration.JE_OH_Importer = orgWithCMT.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "Importer Registration Number not found.");
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "The Importer Registration Number differs from CJN - Business Taxpayer Registration.");

			declaration.JE_OH_Importer = orgWithCPF.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, "Importer Registration Number not found.");
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "The Importer Registration Number differs from CJN - Business Taxpayer Registration.");

			declaration.JE_OH_Importer = orgWithCJN.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, "Importer Registration Number not found.");
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, "The Importer Registration Number differs from CJN - Business Taxpayer Registration.");

			declaration.DeclarantType = DeclarantTypeList.Codes.NaturalPerson;
			declaration.JE_OH_Importer = orgWithCMT.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "Importer Registration Number not found.");
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "The Importer Registration Number differs from CPF - Individual Taxpayer Registration.");

			declaration.JE_OH_Importer = orgWithCPF.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, "Importer Registration Number not found.");
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, "The Importer Registration Number differs from CPF - Individual Taxpayer Registration.");

			declaration.JE_OH_Importer = orgWithCJN.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, "Importer Registration Number not found.");
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "The Importer Registration Number differs from CPF - Individual Taxpayer Registration.");
		}

		public void TestCheckJE_SubLocationOfGoods()
		{
			ReferenceTestDataHelper.CreateWarehousingSectorsCodes(Factory);

			declaration.JE_CustomsOffice = ZString.Empty;
			declaration.JE_LocationOfGoods = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_CustomsOffice = "CO00001";
			declaration.JE_LocationOfGoods = "CE00001";
			AssertHasMessageErrorContaining(declaration.JE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_SubLocationOfGoods = "001";
			AssertNoMessageErrorContaining(declaration.JE_SubLocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_MessageSubType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_MessageSubTypeInfo, "XX", MessageSubTypeList.Codes._01);
		}

		public void TestCheckJE_OH_Consignee()
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
			declaration.DeclarantType = DeclarantTypeList.Codes.DoorToDoor;
			declaration.JE_OH_Consignee = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_ConsigneeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_OH_Consignee = orgWithCMT.PK;
			AssertHasMessageError(declaration.JE_OH_ConsigneeInfo, "Consignee Registration Number not found.");
			AssertHasMessageError(declaration.JE_OH_ConsigneeInfo, "The Consignee Registration Number differs from CJN - Business Taxpayer Registration.");

			declaration.JE_OH_Consignee = orgWithCPF.PK;
			AssertNoMessageError(declaration.JE_OH_ConsigneeInfo, "Consignee Registration Number not found.");
			AssertHasMessageError(declaration.JE_OH_ConsigneeInfo, "The Consignee Registration Number differs from CJN - Business Taxpayer Registration.");

			declaration.JE_OH_Consignee = orgWithCJN.PK;
			AssertNoMessageError(declaration.JE_OH_ConsigneeInfo, "Consignee Registration Number not found.");
			AssertNoMessageError(declaration.JE_OH_ConsigneeInfo, "The Consignee Registration Number differs from CJN - Business Taxpayer Registration.");

			declaration.JE_OH_Consignee = ZGuid.Missing;
			AssertHasError(declaration.JE_OH_ConsigneeInfo, "The selected Consignee is not valid. Please choose a new Consignee or amend it using F3.");

			foreach (var declarantType in new string[] { DeclarantTypeList.Codes.LegalPerson, DeclarantTypeList.Codes.NaturalPerson })
			{
				declaration.DeclarantType = declarantType;
				declaration.OperationType = TypeOfOperationImportList.Codes.AccountAndOrder;
				declaration.JE_OH_Consignee = ZGuid.Empty;
				AssertHasMessageErrorContaining(declaration.JE_OH_ConsigneeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_OH_Consignee = orgWithCMT.PK;
				AssertHasMessageError(declaration.JE_OH_ConsigneeInfo, "Acquirer Registration Number not found.");
				AssertHasMessageError(declaration.JE_OH_ConsigneeInfo, "The Acquirer Registration Number differs from CJN - Business Taxpayer Registration.");

				declaration.JE_OH_Consignee = orgWithCPF.PK;
				AssertNoMessageError(declaration.JE_OH_ConsigneeInfo, "Acquirer Registration Number not found.");
				AssertHasMessageError(declaration.JE_OH_ConsigneeInfo, "The Acquirer Registration Number differs from CJN - Business Taxpayer Registration.");

				declaration.JE_OH_Consignee = orgWithCJN.PK;
				AssertNoMessageError(declaration.JE_OH_ConsigneeInfo, "Acquirer Registration Number not found.");
				AssertNoMessageError(declaration.JE_OH_ConsigneeInfo, "The Acquirer Registration Number differs from CJN - Business Taxpayer Registration.");
			}
		}

		public void TestCheckOperationType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.OperationTypeInfo, TypeOfOperationExportList.Codes._1003, TypeOfOperationImportList.Codes.AccountAndOrder);
		}

		public void TestCheckDeclarantType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.DeclarantTypeInfo, "X", DeclarantTypeList.Codes.LegalPerson);
		}

		public override void TestCheckBRTransportMode()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.BRTransportModeInfo);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.BRTransportModeInfo);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.BRTransportModeInfo);
		}

		public void TestCheckJE_VesselName()
		{
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VesselName = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_VesselNameInfo);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckVesselCountry()
		{
			var refVesselOk = Factory.NewWithValidTestData<RefVessel>();
			refVesselOk.RV_Code = "V_OK";
			refVesselOk.RV_RN_NKCountryOfReg = "AU";

			var refVesselError = Factory.NewWithValidTestData<RefVessel>();
			refVesselError.RV_Code = "V_ERROR";
			refVesselError.RV_RN_NKCountryOfReg = ZString.Empty;

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VesselName = refVesselError.RV_Code;
			AssertNoMessageErrorContaining(declaration.VesselCountryInfo, "Vessel Country not found.");

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = refVesselError.RV_Code;
			AssertHasMessageErrorContaining(declaration.VesselCountryInfo, "Vessel Country not found.");

			declaration.JE_VesselName = refVesselOk.RV_Code;
			AssertNoMessageErrorContaining(declaration.VesselCountryInfo, "Vessel Country not found.");
		}

		public void TestCheckJE_OH_ShippingLine()
		{
			var supplierOrg = Factory.New<OrgHeader>();
			supplierOrg.OH_FullName = "Supplier Org";
			supplierOrg.MainAddress.OA_Address1 = "Main Address Supp Org";

			AssertNoMessageErrorContaining(declaration.JE_OH_ShippingLineInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			AssertNoMessageErrorContaining(declaration.JE_OH_ShippingLineInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			declaration.JE_TransportMode = "OWN";
			AssertNoMessageErrorContaining(declaration.JE_OH_ShippingLineInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			declaration.JE_TransportMode = "SEA";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_OH_ShippingLineInfo);

			declaration.JE_OH_ShippingLine = supplierOrg.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_ShippingLineInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_ExportDate()
		{
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = "MAI";
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			declaration.JE_TransportMode = "MAI";
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = "SEA";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_ExportDateInfo);

			declaration.JE_ExportDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_RL_NKPortOfLoading()
		{
			AssertNoMessageErrorContaining(declaration.JE_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			declaration.JE_TransportMode = "SEA";
			AssertNoMessageErrorContaining(declaration.JE_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			declaration.JE_TransportMode = "MAI";
			AssertNoMessageErrorContaining(declaration.JE_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			declaration.JE_TransportMode = "SEA";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_ExportDateInfo);

			declaration.JE_RL_NKPortOfLoading = "BR6MO";
			AssertNoMessageErrorContaining(declaration.JE_RL_NKPortOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_HouseBill()
		{
			var subTypeListForMessageError = new[] { MessageSubTypeList.Codes._01, MessageSubTypeList.Codes._02, MessageSubTypeList.Codes._03, MessageSubTypeList.Codes._04, MessageSubTypeList.Codes._05,
				MessageSubTypeList.Codes._06, MessageSubTypeList.Codes._07, MessageSubTypeList.Codes._08, MessageSubTypeList.Codes._09, MessageSubTypeList.Codes._10, MessageSubTypeList.Codes._12,
				MessageSubTypeList.Codes._13, MessageSubTypeList.Codes._14, MessageSubTypeList.Codes._15 };

			var transportModeListForMessageError = new[] { TransportTypeList.Codes.Road };

			CombineAssertions(() =>
			{
				foreach (var subType in declaration.Lookups.MessageSubTypeList.GetAllCodes())
				{
					declaration.JE_MessageSubType = subType;
					foreach (var transportMode in declaration.Lookups.TransportTypeList.GetAllCodes())
					{
						declaration.JE_TransportMode = transportMode;
						if (subTypeListForMessageError.Contains(subType) && transportModeListForMessageError.Contains(transportMode))
						{
							declaration.JE_HouseBill = ZString.Empty;
							AssertHasMessageErrorContaining($"Must contain message error when Declaration Type = {subType} and Mode Transport Mode = {transportMode}", declaration.JE_HouseBillInfo, MandatoryValidation.YouHaveNotEntered);
							declaration.JE_HouseBill = "123";
							AssertNoNotifications(declaration.JE_HouseBillInfo);
						}
						else
						{
							declaration.JE_HouseBill = ZString.Empty;
							AssertNoNotifications($"Must NOT contain message error when Declaration Type = {subType} and Mode Transport Mode = {transportMode}", declaration.JE_HouseBillInfo);
						}
					}
				}
			});
		}

		public void TestCheckJE_UCR()
		{
			var subTypeListForMessageError = new[] { MessageSubTypeList.Codes._01, MessageSubTypeList.Codes._02, MessageSubTypeList.Codes._03, MessageSubTypeList.Codes._04, MessageSubTypeList.Codes._05,
				MessageSubTypeList.Codes._06, MessageSubTypeList.Codes._07, MessageSubTypeList.Codes._08, MessageSubTypeList.Codes._09, MessageSubTypeList.Codes._10, MessageSubTypeList.Codes._12,
				MessageSubTypeList.Codes._13, MessageSubTypeList.Codes._14, MessageSubTypeList.Codes._15 };

			var transportModeListForMessageError = new[] { TransportTypeList.Codes.Mail, TransportTypeList.Codes.Sea, TransportTypeList.Codes.River, TransportTypeList.Codes.Lake };

			CombineAssertions(() =>
			{
				foreach (var subType in declaration.Lookups.MessageSubTypeList.GetAllCodes())
				{
					declaration.JE_MessageSubType = subType;
					foreach (var transportMode in declaration.Lookups.TransportTypeList.GetAllCodes())
					{
						declaration.JE_TransportMode = transportMode;
						if (subTypeListForMessageError.Contains(subType) && transportModeListForMessageError.Contains(transportMode))
						{
							declaration.JE_UCR = ZString.Empty;
							AssertHasMessageErrorContaining($"Must contain message error when Declaration Type = {subType} and Mode Transport Mode = {transportMode}", declaration.JE_UCRInfo, MandatoryValidation.YouHaveNotEntered);
							declaration.JE_UCR = "123";
							AssertNoNotifications(declaration.JE_UCRInfo);
						}
						else
						{
							declaration.JE_UCR = ZString.Empty;
							AssertNoNotifications($"Must NOT contain message error when Declaration Type = {subType} and Mode Transport Mode = {transportMode}", declaration.JE_UCRInfo);
						}
					}
				}
			});
		}

		public void TestCheckPaymentBankAccountPK()
		{
			var message = "You have not entered a Bank Account in Registry > Brazil > Import (SISCOMEX Web) > Tax and Fee Payment Bank Account.";

			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_FullAccountNumber = "12345";
			bankAccount.AB_BSB = "XXX";
			bankAccount.AB_AccountNum = "123";

			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Importer;
			AssertNoMessageErrorContaining(declaration.PaymentBankAccountPKInfo, message);

			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			AssertHasMessageErrorContaining(declaration.PaymentBankAccountPKInfo, message);

			using (BRCustomsDataRegistry.Instance.TaxFeeCustomsPaymentBankAccount.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, bankAccount.PK.ToGuid()))
			{
				declaration.Validation.ValidatePaymentBankAccount();
				AssertNoMessageErrorContaining(declaration.PaymentBankAccountPKInfo, message);
			}
		}

		public void TestCheckJE_PaymentMethod()
		{
			var oOrgHeader1 = Factory.New<OrgHeader>();
			oOrgHeader1.OH_Code = "C1";
			var oOrgHeader2 = Factory.New<OrgHeader>();
			oOrgHeader2.OH_Code = "C2";
			var oBRAddinfo = BROrgImpAddInfo.Get(oOrgHeader2);
			oBRAddinfo.ZO_BSBNumber = "111";
			oBRAddinfo.ZO_BankCode = "XXX";
			oBRAddinfo.ZO_AccountNumber = "777";
			declaration.JE_OH_Importer = oOrgHeader1.PK;
			AssertNoMessageError("Should NOT have error message", declaration.JE_PaymentMethodInfo, "No Bank Account has been configured against the Importer Bank Details.");

			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Importer;
			AssertHasMessageError("Should have error message because importer do not have bank account configure", declaration.JE_PaymentMethodInfo, "No Bank Account has been configured against the Importer Bank Details.");

			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Broker;
			AssertNoMessageError("Should NOT have error message, because payment method is configure as broker", declaration.JE_PaymentMethodInfo, "No Bank Account has been configured against the Importer Bank Details.");

			declaration.JE_OH_Importer = oOrgHeader2.PK;
			declaration.JE_PaymentMethod = PaymentPartyCodeDescriptionList.Codes.Importer;
			AssertNoMessageError("Should NOT have error message because importer do have bank account configure", declaration.JE_PaymentMethodInfo, "No Bank Account has been configured against the Importer Bank Details.");

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_PaymentMethodInfo, "XXX", PaymentPartyCodeDescriptionList.Codes.Importer);
		}

		public void TestCheckJE_DispatchModality()
		{
			declaration.JE_MessageSubType = "";
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_DispatchModalityInfo);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._21;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_DispatchModalityInfo);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._22;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_DispatchModalityInfo, "X", DispatchModalityCodes.Codes.Normal);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_MessageSubType = "";
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_DispatchModalityInfo);
		}

		public void TestCheckJE_CargoArrivalDocumentType()
		{
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertNoNotifications("JE_CargoArrivalDocumentType must NOT have notification", declaration.JE_CargoArrivalDocumentTypeInfo);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_CargoArrivalDocumentTypeInfo, "X", BRCargoArrivalDocList.Codes.DTA);

			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			declaration.JE_CargoArrivalDocumentType = "X";
			AssertNoNotifications("JE_CargoArrivalDocumentType must NOT have notification", declaration.JE_CargoArrivalDocumentTypeInfo);
		}

		public void TestCheckJE_CargoArrivalDocumentNumber()
		{
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertNoNotifications("JE_CargoArrivalDocumentNumber must NOT have notification", declaration.JE_CargoArrivalDocumentNumberInfo);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_CargoArrivalDocumentNumberInfo);

			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			declaration.JE_CargoArrivalDocumentNumber = "";
			AssertNoNotifications("JE_CargoArrivalDocumentNumber must NOT have notification", declaration.JE_CargoArrivalDocumentNumberInfo);
		}

		public void TestCheckJE_CargoArrivalDocumentUtilization()
		{
			declaration.JE_MessageSubType = MessageSubTypeList.Codes._11;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertNoNotifications("JE_CargoArrivalDocumentUtilization must NOT have notification", declaration.JE_CargoArrivalDocumentUtilizationInfo);

			declaration.JE_MessageSubType = MessageSubTypeList.Codes._01;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_CargoArrivalDocumentUtilizationInfo, "X", BRUtilizationList.Codes.Total);

			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
			declaration.JE_CargoArrivalDocumentUtilization = "X";
			AssertNoNotifications("JE_CargoArrivalDocumentUtilization must NOT have notification", declaration.JE_CargoArrivalDocumentUtilizationInfo);
		}

		public void TestCheckJE_TotalWeight()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_TotalWeightInfo);
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.ImportSiscomex;
	}
}
