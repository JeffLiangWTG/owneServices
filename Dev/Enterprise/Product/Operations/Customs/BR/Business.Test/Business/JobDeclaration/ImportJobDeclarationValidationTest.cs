using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportJobDeclarationValidationTest : JobDeclarationValidationTest
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

		public override void TestCheckBRTransportMode()
		{
			ValidationTestHelper.AssertWarningIfNotEntered(declaration.BRTransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			ValidationTestHelper.AssertErrorIfInvalidCode(declaration.BRTransportModeInfo, "XXX", BRTransportModeList.Codes.SEA);
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

		public void TestCheckOperationType()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.OperationTypeInfo, TypeOfOperationExportList.Codes._1003, TypeOfOperationImportList.Codes.AccountAndOrder);
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

			declaration.JE_OH_Importer = orgWithCMT.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "Importer Registration Number not found.");
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "The Importer Registration Number differs from CJN - Business Taxpayer Registration.");

			declaration.JE_OH_Importer = orgWithCJN.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, "Importer Registration Number not found.");
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, "The Importer Registration Number differs from CJN - Business Taxpayer Registration.");
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

			declaration.OperationType = TypeOfOperationImportList.Codes.AccountAndOrder;

			declaration.JE_OH_Consignee = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_ConsigneeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_OH_Consignee = ZGuid.Missing;
			AssertHasError(declaration.JE_OH_ConsigneeInfo, "The selected Acquirer is not valid. Please choose a new Acquirer or amend it using F3.");

			declaration.JE_OH_Consignee = orgWithCMT.PK;
			AssertHasMessageError(declaration.JE_OH_ConsigneeInfo, "Acquirer Registration Number not found.");
			AssertHasMessageError(declaration.JE_OH_ConsigneeInfo, "The Acquirer Registration Number differs from CJN - Business Taxpayer Registration.");

			declaration.JE_OH_Consignee = orgWithCJN.PK;
			AssertNoMessageError(declaration.JE_OH_ConsigneeInfo, "Acquirer Registration Number not found.");
			AssertNoMessageError(declaration.JE_OH_ConsigneeInfo, "The Acquirer Registration Number differs from CJN - Business Taxpayer Registration.");

			declaration.OperationType = TypeOfOperationImportList.Codes.OnItsOwn;

			declaration.JE_OH_Consignee = ZGuid.Empty;
			AssertNoMessageError(declaration.JE_OH_ConsigneeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_OH_Consignee = orgWithCMT.PK;
			AssertNoMessageError(declaration.JE_OH_ConsigneeInfo, "Acquirer Registration Number not found.");
			AssertNoMessageError(declaration.JE_OH_ConsigneeInfo, "The Acquirer Registration Number differs from CJN - Business Taxpayer Registration.");
		}

		public void TestCheckJE_GoodsOrigin()
		{
			foreach (var transportType in declaration.Lookups.TransportTypeList.GetAllCodes().Except(new[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Road }))
			{
				declaration.JE_TransportMode = transportType;
				CombineAssertions($"JE_GoodsOriginInfo must NOT have notification when JE_TransportMode is ${transportType}", () =>
				{
					AssertNoNotifications("JE_GoodsOriginInfo must NOT have notification", declaration.JE_GoodsOriginInfo);
				});
			}

			foreach (var transportType in new[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Road })
			{
				declaration.JE_TransportMode = transportType;
				CombineAssertions($"JE_GoodsOriginInfo must have notification when JE_TransportMode is ${transportType}", () =>
				{
					ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_GoodsOriginInfo, "XX", Core.Constants.CountryCodes.Brazil);
				});
			}

			declaration.JE_DispatchModality = DispatchModalityCodes.Codes.Normal;
			foreach (var transportType in new[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Road })
			{
				declaration.JE_TransportMode = transportType;
				CombineAssertions($"JE_GoodsOriginInfo must NOT have notification when JE_TransportMode is ${transportType}", () =>
				{
					AssertNoNotifications("JE_GoodsOriginInfo must NOT have notification", declaration.JE_GoodsOriginInfo);
				});
			}
		}

		public void TestCheckJE_DispatchModality()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_DispatchModalityInfo, "X", DispatchModalityCodes.Codes.Normal);
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_DispatchModalityInfo, "X", DispatchModalityCodes.Codes.Normal);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_MessageSubType = "";
			AssertNoNotifications("JE_DispatchModality must NOT have notification", declaration.JE_DispatchModalityInfo);
		}

		public override void TestPackagesActualPackageCount_Validation()
		{
			AssertNoNotifications("PackagesActualPackageCountInfo should not have notification", declaration.PackagesActualPackageCountInfo);

			declaration.JE_TotalNoOfPacks = 10;
			AssertNoNotifications("PackagesActualPackageCountInfo should not have notification", declaration.PackagesActualPackageCountInfo);
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.Import;
	}
}
