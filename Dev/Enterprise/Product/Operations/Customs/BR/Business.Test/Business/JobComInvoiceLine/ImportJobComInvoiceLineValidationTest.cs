using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		public void TestCheckJI_CustomsSecondUnitQty()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsSecondUnitQtyInfo, "XX", ContainerTypeList.Codes.ReturnableGlassBottle);
		}

		public void TestCheckJI_CustomsThirdUnitQty()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_CustomsThirdUnitQtyInfo, "X", CapacityUnitList.Codes.L);
		}

		public void TestCheckMercosulForeignDeclarationType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.MercosulForeignDeclarationTypeInfo, "XXXXX", CertificateTypeList.Codes.CCPTC);

			invoiceLine.MercosulForeignDeclarations.AddNew();
			invoiceLine.MercosulForeignDeclarationType = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLine.MercosulForeignDeclarationTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.MercosulForeignDeclarationType = CertificateTypeList.Codes.CCPTC;
			AssertNoMessageErrorContaining(invoiceLine.MercosulForeignDeclarationTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_Tariff()
		{
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "88888888", 40m, preference: RatePreferenceType.Normal);
			ReferenceTestDataHelper.CreateTariffAndRates(Factory, "99999999", 50m, preference: RatePreferenceType.Normal);

			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			goodsCatalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
			goodsCatalog.CGC_Tariff = "88888888";
			goodsCatalog.CGC_AuthorityVersion = "1";
			goodsCatalog.CGC_AuthorityIdentifier = "123";
			goodsCatalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.Codes.Active;

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			invoiceLine.JI_Tariff = "99999999";
			invoiceLine.JI_PrimaryPreference = RatePreferenceType.ExTariff;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffInfo, "There is no applicable DTY rate");

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "There is no applicable DTY rate");
			AssertNoMessageError(invoiceLine.JI_TariffInfo, "Tariff does not match with the catalog file.");

			invoiceLine.JI_CGC_Catalog = goodsCatalog.PK;
			invoiceLine.JI_Tariff = "99999999";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, "Tariff does not match with the catalog file.");

			invoiceLine.JI_Tariff = "88888888";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, "Tariff does not match with the catalog file.");

			invoiceLine.JI_Tariff = ZString.Empty;
			AssertNoMessageError(invoiceLine.JI_TariffInfo, "Tariff does not match with the catalog file.");

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			invoiceLine.JI_JZ = invoice.PK;
			AssertNull(invoiceLine.Declaration);
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffInfo, "There is no applicable DTY rate");
		}

		public void TestCheckICMSFCPRateValue()
		{
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.ICMSFCPRateValueInfo);
		}

		public void TestCheckComplementaryDescription()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = BRIncoTermList.Codes.OCV;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.ComplementaryDescription = ZString.Empty;
			invoiceLine.Validation.ValidateComplementaryDescription();

			AssertHasMessageErrorContaining(invoiceLine.ComplementaryDescriptionInfo, "The Incoterm chosen is OCV - Other Condition of Sale, but you have not entered a Complement.");

			invoice.JZ_IncoTerm = BRIncoTermList.Codes.OCV;
			invoiceLine.ComplementaryDescription = "Comp123";
			AssertNoMessageErrorContaining(invoiceLine.ComplementaryDescriptionInfo, "The Incoterm chosen is OCV - Other Condition of Sale, but you have not entered a Complement.");

			invoice.JZ_IncoTerm = BRIncoTermList.Codes.CIF;
			invoiceLine.ComplementaryDescription = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.ComplementaryDescriptionInfo, "The Incoterm chosen is OCV - Other Condition of Sale, but you have not entered a Complement.");
		}

		public void TestCheckJI_CGC_Catalog()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "XXX";
			supplier.OH_FullName = "TEST COMPANY";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";
			product.RelatedOrganisations.AddSupplier(supplier);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			pivot.CI_TariffNum = "56049000";
			pivot.CI_OH = supplier.PK;
			Factory.Save();

			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			goodsCatalog.CGC_AuthorityVersion = "1";
			goodsCatalog.CGC_AuthorityIdentifier = "123";
			goodsCatalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.Codes.Active;

			var altGoodsCatalog = Factory.New<CusGoodsCatalog>();
			altGoodsCatalog.CGC_AuthorityVersion = "2";
			altGoodsCatalog.CGC_AuthorityIdentifier = "456";
			altGoodsCatalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.Codes.Active;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "XXX";
			consignee.OH_FullName = "TEST COMPANY";
			consignee.PrimaryRegistrationNumber.Number = "00.124.968/0002-36";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "YYY";
			importer.OH_FullName = "IMPORTER COMPANY";
			importer.PrimaryRegistrationNumber.Number = "00.567.968/1112-55";

			goodsCatalog.CGC_OH_Owner = consignee.PK;
			invoiceLine.JI_CGC_Catalog = goodsCatalog.PK;

			declaration.JE_OH_Importer = importer.PK;

			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertNoMessageError(invoiceLine.JI_CGC_CatalogInfo, "The Goods Catalog is not Active.");

			invoiceLine.JI_CGC_Catalog = goodsCatalog.PK;
			AssertNoMessageError(invoiceLine.JI_CGC_CatalogInfo, "The Goods Catalog is not Active.");

			goodsCatalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.Codes.Inactive;
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertHasMessageError(invoiceLine.JI_CGC_CatalogInfo, "The Goods Catalog is not Active.");

			goodsCatalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.Codes.Draft;
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertHasMessageError(invoiceLine.JI_CGC_CatalogInfo, "The Goods Catalog is not Active.");

			goodsCatalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.Codes.Active;
			var validationMessage = "The Root CNPJ of the Owner set to this Goods Catalog does not match the Root CNPJ of the Importer of this job.";
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertHasMessageErrorContaining(invoiceLine.JI_CGC_CatalogInfo, validationMessage);

			importer.PrimaryRegistrationNumber.Number = "00.124.968/0002-36";
			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.NotSent;
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertHasMessageError(invoiceLine.JI_CGC_CatalogInfo, "This Goods Catalog should not be used because there might be messages that need to be sent (Latest Message Status is currently NST - Not Sent).");

			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Rejected;
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertHasMessageError(invoiceLine.JI_CGC_CatalogInfo, "This Goods Catalog should not be used due to its last message being rejected (Latest Message Status is currently REJ - Rejected).");

			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertHasMessageError(invoiceLine.JI_CGC_CatalogInfo, "This Goods Catalog should not be used because there is a message waiting for response (Latest Message Status is currently AWA - Awaiting Response).");

			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Accepted;
			goodsCatalog.CGC_AuthorityIdentifier = "";
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertHasMessageError(invoiceLine.JI_CGC_CatalogInfo, "This Goods Catalog does not contain an Authority Code.");

			goodsCatalog.CGC_AuthorityIdentifier = "1";
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertHasMessageError(invoiceLine.JI_CGC_CatalogInfo, "This Catalog does not contain a valid known or unknown Foreign Operator (Customs Status is currently different from Accepted).");

			goodsCatalog.ForeignOperators.AddNew().CGI_CustomsStatus = CustomsPostedStatusList.Codes.Active;

			var foreignOperator = goodsCatalog.ForeignOperators.AddNew();
			foreignOperator.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Active;

			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertHasMessageError(invoiceLine.JI_CGC_CatalogInfo, "This Catalog does not contain a valid known or unknown Foreign Operator (Customs Status is currently different from Accepted).");

			foreignOperator.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertNoNotifications(invoiceLine.JI_CGC_CatalogInfo);

			invoiceLine.JI_CGC_Catalog = ZGuid.Empty;
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertHasMessageErrorContaining(invoiceLine.JI_CGC_CatalogInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CGC_Catalog = goodsCatalog.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_CGC_CatalogInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(invoiceLine.JI_CGC_CatalogInfo, "Catalog does not match with the product file.");

			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertNoMessageError(invoiceLine.JI_CGC_CatalogInfo, "Catalog does not match with the product file.");

			pivot.CI_CGC_Catalog = altGoodsCatalog.PK;
			invoiceLine.JI_CGC_Catalog = goodsCatalog.PK;
			AssertHasMessageError(invoiceLine.JI_CGC_CatalogInfo, "Catalog does not match with the product file.");

			pivot.CI_CGC_Catalog = goodsCatalog.PK;
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertNoMessageError(invoiceLine.JI_CGC_CatalogInfo, "Catalog does not match with the product file.");

			goodsCatalog.CGC_AuthorityVersion = "1";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var line = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = line.PK;

			entryHeader.CH_AuthorityVersion = "1";

			invoiceLine.JI_CGC_Catalog = goodsCatalog.PK;
			invoiceLine.JI_CatalogAuthorityVersion = "2";
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertNoMessageError("Case 1, Catalog Version and JI_CGC_CatalogInfo are not equal but CH_AuthorityVersion = 1", invoiceLine.JI_CGC_CatalogInfo, "The version stored in this invoice line differs from the Goods Catalog registration.");

			entryHeader.CH_AuthorityVersion = "0";
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertHasMessageError("Case 2, Catalog Version and JI_CGC_CatalogInfo are not equal and CH_AuthorityVersion = 0", invoiceLine.JI_CGC_CatalogInfo, "The version stored in this invoice line differs from the Goods Catalog registration.");

			entryHeader.CH_AuthorityVersion = ZString.Empty;
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertHasMessageError("Case 2, Catalog Version and JI_CGC_CatalogInfo are not equal and CH_AuthorityVersion = 0", invoiceLine.JI_CGC_CatalogInfo, "The version stored in this invoice line differs from the Goods Catalog registration.");

			invoiceLine.JI_CatalogAuthorityVersion = "1";
			invoiceLine.Validation.ValidateJI_CGC_Catalog();
			AssertNoMessageError("Case 3, Catalog Version and JI_CGC_CatalogInfo are equal and CH_AuthorityVersion = 0", invoiceLine.JI_CGC_CatalogInfo, "The version stored in this invoice line differs from the Goods Catalog registration.");
		}

		public void TestCheckJI_OA_ManufacturerAddress()
		{
			var importer = Factory.New<OrgHeader>();
			var manufacturer1 = Factory.New<OrgHeader>();
			var manufacturer2 = Factory.New<OrgHeader>();

			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_OH_Owner = importer.PK;
			foreignOperator.BFR_OH_ForeignOperator = manufacturer1.PK;

			const string manufacturerIsNotForeignOperatorMessage = "Manufacturer choose is not a Foreign Operator.";
			const string foreignOperatorHasNoIdentifierMessage = "Manufacturer choose is a Foreign Operator but has no Authority Identifier.";
			const string foreignOperatorHasNotBeenSentMessage = "Manufacturer should not be used because there might be messages that need to be sent (Message Status is currently NST – Not Sent).";
			const string foreignOperatorIsAwaitingMessage = "Manufacturer choose is a Foreign Operator but there are pending messages (Message Status is currently AWA - Awaiting Response).";

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				declaration.JE_OH_Importer = importer.PK;
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, manufacturerIsNotForeignOperatorMessage);

				invoiceLine.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
				AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, manufacturerIsNotForeignOperatorMessage);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorHasNoIdentifierMessage);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorHasNotBeenSentMessage);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorIsAwaitingMessage);

				invoiceLine.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, manufacturerIsNotForeignOperatorMessage);
				AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorHasNoIdentifierMessage);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorHasNotBeenSentMessage);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorIsAwaitingMessage);

				foreignOperator.BFR_AuthorityIdentifier = "1";
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, manufacturerIsNotForeignOperatorMessage);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorHasNoIdentifierMessage);
				AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorHasNotBeenSentMessage);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorIsAwaitingMessage);

				foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.AwaitingResponse;
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, manufacturerIsNotForeignOperatorMessage);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorHasNoIdentifierMessage);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorHasNotBeenSentMessage);
				AssertHasMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorIsAwaitingMessage);

				foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Accepted;
				invoiceLine.Validation.ValidateJI_OA_ManufacturerAddress();
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, manufacturerIsNotForeignOperatorMessage);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorHasNoIdentifierMessage);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorHasNotBeenSentMessage);
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, foreignOperatorIsAwaitingMessage);
			}

			using (Registry.BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				declaration.JE_OH_Importer = importer.PK;
				invoiceLine.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
				AssertNoMessageError(invoiceLine.JI_OA_ManufacturerAddressInfo, manufacturerIsNotForeignOperatorMessage);
			}
		}

		public void TestCheckJI_ManufacturerIndicator()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "TEST1";
			supplier.Addresses.AddNew();

			invoiceLine.JI_ManufacturerIndicator = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_ManufacturerIndicatorInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.InvoiceHeader.JZ_OA_SupplierAddress = supplier.Addresses[0].PK;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_ManufacturerIndicatorInfo, "X", ManufacturerIndicatorList.Codes._2);
		}

		public void TestCheckJI_ICMSRate()
		{
			ValidationTestHelper.AssertValueCannotBeZeroMessageError(invoiceLine.JI_ICMSRateInfo);

			declaration.MakeNonPersistent();
			invoiceLine.InvoiceHeader.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			invoiceLine.JI_ICMSRate = ZDecimal.Zero;
			AssertNoMessageErrorContaining(invoiceLine.JI_ICMSRateInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckJI_ICMSBaseValueReductionPercentage()
		{
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			invoiceLine.JI_ICMSBaseValueReductionPercentage = ZDecimal.Zero;
			invoiceLine.JI_ICMSTotalAmountReductionPercentage = ZDecimal.Zero;
			invoiceLine.Validation.ValidateAll();
			AssertNoErrorContaining(invoiceLine.JI_ICMSBaseValueReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			invoiceLine.Validation.ValidateAll();
			AssertNoErrorContaining(invoiceLine.JI_ICMSBaseValueReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			invoiceLine.JI_ICMSRate = 10m;
			invoiceLine.Validation.ValidateAll();
			AssertHasMessageErrorContaining(invoiceLine.JI_ICMSBaseValueReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.JI_ICMSBaseValueReductionPercentageInfo);

			invoiceLine.JI_ICMSBaseValueReductionPercentage = 10m;
			invoiceLine.Validation.ValidateAll();
			AssertNoErrorContaining(invoiceLine.JI_ICMSBaseValueReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");

			invoiceLine.JI_ICMSBaseValueReductionPercentage = ZDecimal.Zero;
			invoiceLine.JI_ICMSTotalAmountReductionPercentage = 10m;
			invoiceLine.Validation.ValidateAll();
			AssertNoErrorContaining(invoiceLine.JI_ICMSBaseValueReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");
		}

		public void TestCheckJI_ICMSTotalAmountReductionPercentage()
		{
			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.FullCollection;
			invoiceLine.JI_ICMSTotalAmountReductionPercentage = 150m;
			AssertNoErrorContaining(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");
			AssertNoErrorContaining(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo, "Percentage should be a value between 0 and 100.");

			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo);
			BRValidationTestHelper.CheckPercentageProperties(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo);

			invoiceLine.JI_ICMSBaseValueReductionPercentage = ZDecimal.Zero;
			invoiceLine.JI_ICMSTotalAmountReductionPercentage = ZDecimal.Zero;
			AssertNoErrorContaining(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");

			invoiceLine.JI_ICMSBaseValueReductionPercentage = 10m;
			AssertNoErrorContaining(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");

			invoiceLine.JI_ICMSBaseValueReductionPercentage = ZDecimal.Zero;
			AssertHasMessageErrorContaining(invoiceLine.JI_ICMSTotalAmountReductionPercentageInfo, "You must enter at least Base Amount reduction (%) or Total Amount reduction (%) in Reduction calculation.");
		}

		public void TestCheckJI_ICMSFormula()
		{
			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			invoiceLine.JI_ICMSFormula = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_ICMSFormulaInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_ICMSBaseValueReductionPercentage = 10m;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_ICMSFormulaInfo, "X", ICMSFormulaList.Codes.BC);
		}

		public void TestCheckJI_GoodsApplication()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_GoodsApplicationInfo, "X", ImportGoodsApplicationTypeList.Codes.Industrialization);
		}

		public void TestCheckJI_GoodsCondition()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_GoodsConditionInfo, "X", ImportGoodsConditionTypeList.Codes.Used);
		}

		public void TestCheckJI_ManufacturerAuthorityIdentifier()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_ManufacturerAuthorityIdentifierInfo);

			invoiceLine.JI_ManufacturerIndicator = "3";
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_ManufacturerAuthorityIdentifierInfo);

			declaration.MakeNonPersistent();
			invoiceLine.InvoiceHeader.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_ManufacturerAuthorityIdentifierInfo);
		}

		public void TestCheckJI_ManufacturerAuthorityVersion()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_ManufacturerAuthorityVersionInfo);

			invoiceLine.JI_ManufacturerIndicator = "3";
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_ManufacturerAuthorityVersionInfo);

			declaration.MakeNonPersistent();
			invoiceLine.InvoiceHeader.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceLine.JI_ManufacturerAuthorityVersionInfo);
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.Import;
	}
}
