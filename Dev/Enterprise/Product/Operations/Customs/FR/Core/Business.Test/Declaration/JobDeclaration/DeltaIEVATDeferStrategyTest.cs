using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class DeltaIEVATDeferStrategyTest : VATDeferStrategyAbstractTest
	{
		public override void TestDefermentAccountNumberShouldBeDefaultedWhenOrganisationChanged()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.MainAddress.Address1 = "ImporterAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.DEF, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.DEF, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declaration = Factory.NewWithValidTestData<DummyJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
			declaration.JE_GB = ZGuid.Empty;

			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("GetPaymentMethodSource couldn't be inferred, because the declaration is import but no importer nor declarant was captured", ZString.Empty, declaration.JE_DefermentAccountNumber);

			declaration.JE_OA_DeclarantAddress = declarant.Addresses[0].PK;
			AssertEquals("GetPaymentMethodSource is the Declarant because the declaration is UCC6-Import and declarant is present.", "DGUA", declaration.JE_DefermentAccountNumber);

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
			AssertEquals("GetPaymentMethodSource is still the declarant because the declaration is UCC6-Import and a declarant was captured.", "DGUA", declaration.JE_DefermentAccountNumber);

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertEquals("GetPaymentMethodSource is importer, because the declaration is UCC6-Import and no declarant is captured, but a importer has been set against the declaration.", "IGUA", declaration.JE_DefermentAccountNumber);
		}

		public override void TestDefermentAccountNumberShouldBeDefaultedWhenPaymentMethodChanged()
		{
			var declaration = Factory.NewWithValidTestData<DummyJobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.DEF, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
			AssertEquals("No payment method has been set, deferment account number should be empty", ZString.Empty, declaration.JE_DefermentAccountNumber);

			declaration.JE_PaymentMethodInfo.ClearValue();
			declaration.JE_DefermentAccountNumberInfo.ClearValue();
			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;
			AssertEquals("Payment method changed, so should JE_DefermentAccountNumber", "DGUA", declaration.JE_DefermentAccountNumber);
		}

		public void TestDefermentAccountNumberShouldBeDefaultedWhenDeclarantTypeChanged()
		{
			var declaration = Factory.NewWithValidTestData<DummyJobDeclaration>();
			declaration.JE_PaymentMethod = MethodOfPaymentList.Codes.R;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMPORTER";
			importer.MainAddress.Address1 = "ImporterAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.DEF, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.MainAddress.Address1 = "DeclarantAddress";
			GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.DEF, "DGUA", declarant.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "DeclarantAddress", ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.France);

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;
			AssertEquals("Deferment Account Number should be empty when Declarant Type is Direct and Importer is not captured.", ZString.Empty, declaration.JE_DefermentAccountNumber);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
			AssertEquals("Deferment Account Number should be empty when Declarant Type is Indirect and Declarant is not captured.", ZString.Empty, declaration.JE_DefermentAccountNumber);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
			AssertEquals("Deferment Account Number should be empty when Declarant Type is Self and Declarant is not captured.", ZString.Empty, declaration.JE_DefermentAccountNumber);

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;
			AssertEquals("Deferment Account Number should be defaulted to Importer when Declarant Type is Direct and Importer is captured.", "IGUA", declaration.JE_DefermentAccountNumber);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
			AssertEquals("Deferment Account Number should be defaulted to Declarant when Declarant Type is Indirect and Declarant is captured.", "DGUA", declaration.JE_DefermentAccountNumber);

			declaration.JE_DeclarantType = RepresentationTypeList.Codes.SEL;
			AssertEquals("Deferment Account Number should be defaulted to Declarant when Declarant Type is Self and Declarant is captured.", "DGUA", declaration.JE_DefermentAccountNumber);
		}

		class DummyJobDeclaration : JobDeclaration
		{
			public DummyJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override EU.Business.Declaration.VATDeferStrategy GetVATDeferStrategyCore() => new DeltaIEVATDeferStrategy(this);
		}
	}
}
