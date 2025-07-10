using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		protected override Type GetTypeForTest() => typeof(ImportJobComInvoiceHeaderValidation);

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
		}

		public void TestCheckJZ_OA_SupplierAddress()
		{
			var supplierOrg = Factory.New<OrgHeader>();
			supplierOrg.OH_FullName = "Supplier Org";
			supplierOrg.MainAddress.OA_Address1 = "356 Main Address Supp Org";
			supplierOrg.OH_Code = "XXX";

			invoiceHeader.Validation.ValidateJZ_OA_SupplierAddress();
			AssertHasMessageError("Empty Supplier", invoiceHeader.JZ_OA_SupplierAddressInfo, MandatoryValidation.YouHaveNotEnteredMessage("Supplier Address"));
			AssertHasMessageError("Empty Supplier", invoiceHeader.SupplierOrgPKInfo, MandatoryValidation.YouHaveNotEnteredMessage(JobComInvoiceHeader.SupplierCaption.Caption));

			invoiceHeader.JZ_OA_SupplierAddress = supplierOrg.MainAddress.PK;
			AssertNoMessageError("Valid Supplier", invoiceHeader.JZ_OA_SupplierAddressInfo, MandatoryValidation.YouHaveNotEnteredMessage("Supplier Address"));
			AssertNoMessageError("Valid Supplier", invoiceHeader.SupplierOrgPKInfo, MandatoryValidation.YouHaveNotEnteredMessage(JobComInvoiceHeader.SupplierCaption.Caption));
		}

		public void TestCheckJZ_OA_SupplierAddress_StatusAndStreetNumber()
		{
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AddressValidationHelperTest.TestCheckAddressStatusAndStreetNumber(Factory, invoiceHeader.JZ_OA_SupplierAddressInfo, true);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AddressValidationHelperTest.TestCheckAddressStatusAndStreetNumber(Factory, invoiceHeader.JZ_OA_SupplierAddressInfo, false);
		}

		public void TestCheckSupplierOrgPK()
		{
			var importer = Factory.New<OrgHeader>();
			var supplier1 = Factory.New<OrgHeader>();
			var supplier2 = Factory.New<OrgHeader>();

			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = importer.PK;
			foreignOperator.BFR_OH_ForeignOperator = supplier1.PK;

			const string supplierIsNotForeignOperatorMessage = "Supplier is not a Foreign Operator.";
			const string foreignOperatorHasNoIdentifierMessage = "Supplier choose is a Foreign Operator but has no Authority Identifier.";
			const string foreignOperatorHasNotBeenSentMessage = "Supplier should not be used because there might be messages that need to be sent (Message Status is currently NST – Not Sent).";
			const string foreignOperatorIsAwaitingMessage = "Supplier choose is a Foreign Operator but there are pending messages (Message Status is currently AWA - Awaiting Response).";

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				declaration.JE_OH_Importer = importer.PK;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.SupplierOrgPKInfo);

				invoiceHeader.JZ_OA_SupplierAddress_ZAddress.ValidateOrgPK();
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, supplierIsNotForeignOperatorMessage);

				invoiceHeader.JZ_OH_Supplier = supplier2.PK;
				AssertHasMessageError(invoiceHeader.SupplierOrgPKInfo, supplierIsNotForeignOperatorMessage);
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorHasNoIdentifierMessage);
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorHasNotBeenSentMessage);
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorIsAwaitingMessage);

				invoiceHeader.JZ_OH_Supplier = supplier1.PK;
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, supplierIsNotForeignOperatorMessage);
				AssertHasMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorHasNoIdentifierMessage);
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorHasNotBeenSentMessage);
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorIsAwaitingMessage);

				foreignOperator.BFR_AuthorityIdentifier = "1";
				invoiceHeader.JZ_OA_SupplierAddress_ZAddress.ValidateOrgPK();
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, supplierIsNotForeignOperatorMessage);
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorHasNoIdentifierMessage);
				AssertHasMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorHasNotBeenSentMessage);
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorIsAwaitingMessage);

				foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
				invoiceHeader.JZ_OA_SupplierAddress_ZAddress.ValidateOrgPK();
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, supplierIsNotForeignOperatorMessage);
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorHasNoIdentifierMessage);
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorHasNotBeenSentMessage);
				AssertHasMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorIsAwaitingMessage);

				foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Accepted;
				invoiceHeader.JZ_OA_SupplierAddress_ZAddress.ValidateOrgPK();
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, supplierIsNotForeignOperatorMessage);
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorHasNoIdentifierMessage);
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorHasNotBeenSentMessage);
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, foreignOperatorIsAwaitingMessage);
			}

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.SupplierOrgPKInfo);

				declaration.JE_OH_Importer = importer.PK;
				invoiceHeader.JZ_OH_Supplier = supplier2.PK;
				AssertNoMessageError(invoiceHeader.SupplierOrgPKInfo, supplierIsNotForeignOperatorMessage);
			}
		}

		public void TestCheckJZ_RelatedIndicator()
		{
			invoiceHeader.JZ_RelatedIndicator = "X";
			AssertNoMessageError("Invalid JZ_RelatedIndicator", invoiceHeader.JZ_RelatedIndicatorInfo, "You have not entered a Buy-Seller Relationship.");
			AssertHasMessageErrorContaining(invoiceHeader.JZ_RelatedIndicatorInfo, ListValidation.InvalidCodeMessageError);

			invoiceHeader.JZ_RelatedIndicator = RelatedIndicatorList.Codes.BuyerSellerRelationNoInfluence;
			AssertNoMessageError("Valid JZ_RelatedIndicator", invoiceHeader.JZ_RelatedIndicatorInfo, "You have not entered a Buy-Seller Relationship.");
			AssertNoMessageErrorContaining(invoiceHeader.JZ_RelatedIndicatorInfo, ListValidation.InvalidCodeMessageError);

			invoiceHeader.JZ_RelatedIndicator = "";
			AssertHasMessageError("Valid JZ_RelatedIndicator", invoiceHeader.JZ_RelatedIndicatorInfo, "You have not entered a Buy-Seller Relationship.");
			AssertNoMessageErrorContaining(invoiceHeader.JZ_RelatedIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJZ_ValuationCode()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceHeader.JZ_ValuationCodeInfo, "X", ValuationCodeList.Codes._04);
		}

		public void TestCheckJZ_SupplierAuthorityIdentifier()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_SupplierAuthorityIdentifierInfo);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.JZ_SupplierAuthorityIdentifierInfo);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.JZ_SupplierAuthorityIdentifierInfo);
		}

		public void TestCheckJZ_SupplierAuthorityVersion()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_SupplierAuthorityVersionInfo);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.JZ_SupplierAuthorityVersionInfo);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.JZ_SupplierAuthorityVersionInfo);
		}

		public void TestCheckJZ_AdditionalTerms()
		{
			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(invoiceHeader.JZ_AdditionalTermsInfo, invoiceHeader.JZ_IncoTermInfo, (ZString)BRIncoTermList.Codes.OCV, false,  "The selected Incoterm is OCV - Other Condition of Sale, but no Complement has been entered.");
		}

		public void TestCheckExchangeRateDate()
		{
			var date = ZDateTime.Today;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var header1 = declaration.Invoices.AddNew();
			header1.JZ_RX_NKInvoice_Currency = "USD";
			header1.IsJZ_InvoiceCurrExRateUserEnterable = true;
			header1.ExchangeRateDate = date.AddDays(-5);

			var header2 = declaration.Invoices.AddNew();
			header2.JZ_RX_NKInvoice_Currency = "USD";
			header2.IsJZ_InvoiceCurrExRateUserEnterable = true;
			header2.ExchangeRateDate = date;
			header2.Validation.ValidateExchangeRateDate();
			AssertHasMessageError(header2.ExchangeRateDateInfo, "The Exchange Rate Date is different from the other invoices of this declaration.");

			header2.ExchangeRateDate = date.AddDays(-5);
			AssertNoMessageError(header2.ExchangeRateDateInfo, "The Exchange Rate Date is different from the other invoices of this declaration.");
		}
	}
}
