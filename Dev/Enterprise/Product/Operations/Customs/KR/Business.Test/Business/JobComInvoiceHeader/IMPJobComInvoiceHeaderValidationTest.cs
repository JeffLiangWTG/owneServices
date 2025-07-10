using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class IMPJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJZ_Incoterm()
		{
			invoice.Validation.ValidateJZ_IncoTerm();
			AssertHasMessageErrorContaining(invoice.JZ_IncoTermInfo, jz_IncotermErrorMessage);

			invoice.JZ_IncoTerm = "XXX";
			AssertNoMessageErrorContaining(invoice.JZ_IncoTermInfo, jz_IncotermErrorMessage);
			AssertHasErrorContaining(invoice.JZ_IncoTermInfo, ListValidation.InvalidCodeError);

			invoice.JZ_IncoTerm = IncotermList.Codes.FreeOnBoard;
			AssertNoErrorContaining(invoice.JZ_IncoTermInfo, ListValidation.InvalidCodeError);

			invoice.JZ_PaymentTerms = InvoicePaymentTermCodeList.Codes.GN;
			invoice.JZ_IncoTerm = ZString.Empty;
			AssertNoMessageErrors(invoice.JZ_IncoTermInfo);

			invoice.JZ_PaymentTerms = ZString.Empty;
			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._71;
			invoice.Validation.ValidateJZ_IncoTerm();
			AssertNoMessageErrors(invoice.JZ_IncoTermInfo);
		}

		public void TestJZ_InvoiceAmount()
		{
			invoice.JZ_InvoiceAmount = -1;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.ValueCannotBeNegative);

			invoice.JZ_InvoiceAmount = 0;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, jz_InvoiceAmountErrorMessageGreaterThanZero);
			invoice.JZ_InvoiceAmount = 1;
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, jz_InvoiceAmountErrorMessageGreaterThanZero);

			invoice.JZ_PaymentTerms = InvoicePaymentTermCodeList.Codes.GN;
			invoice.JZ_InvoiceAmount = 1;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, jz_InvoiceAmountErrorMessageMustBeZero);
			invoice.JZ_InvoiceAmount = 0;
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, jz_InvoiceAmountErrorMessageMustBeZero);

			invoice.JZ_PaymentTerms = ZString.Empty;
			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._71;
			invoice.JZ_InvoiceAmount = 1;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, jz_InvoiceAmountErrorMessageMustBeZero);
			invoice.JZ_InvoiceAmount = 0;
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, jz_InvoiceAmountErrorMessageMustBeZero);
		}

		public void TestJZ_RX_NKInvoice_Currency()
		{
			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			RefExchangeRate rate = Factory.New<RefExchangeRate>();
			rate.RE_GC = GlbCompany.CurrentCompany.PK;
			rate.RE_RX_NKExCurrency = uSD.RX_Code;
			rate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			rate.RE_SellRate = 0.8573m;
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			Factory.Save();

			invoice.Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertHasMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_RX_NKInvoice_Currency = "XXX";
			AssertNoMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, ListValidation.InvalidCodeMessageError);

			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			AssertNoMessageErrors(invoice.JZ_RX_NKInvoice_CurrencyInfo);

			invoice.JZ_RX_NKInvoice_Currency = "USD";
			AssertNoMessageErrors(invoice.JZ_RX_NKInvoice_CurrencyInfo);

			invoice.JZ_PaymentTerms = InvoicePaymentTermCodeList.Codes.GN;
			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			AssertHasMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, jz_RX_NKInvoice_CurrencyErrorMessage);

			invoice.JZ_RX_NKInvoice_Currency = "USD";
			AssertNoMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, jz_RX_NKInvoice_CurrencyErrorMessage);

			invoice.JZ_PaymentTerms = ZString.Empty;
			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._72;
			invoice.JZ_RX_NKInvoice_Currency = "KRW";
			AssertNoMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, jz_RX_NKInvoice_CurrencyErrorMessage);

			invoice.JZ_RX_NKInvoice_Currency = "USD";
			AssertNoMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, jz_RX_NKInvoice_CurrencyErrorMessage);
		}

		public void TestJZ_PaymentTerms()
		{
			invoice.JZ_PaymentTerms = ZString.Empty;
			AssertHasMessageErrorContaining(invoice.JZ_PaymentTermsInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_PaymentTerms = "12";
			AssertHasMessageErrorContaining(invoice.JZ_PaymentTermsInfo, ListValidation.InvalidCodeMessageError);

			invoice.JZ_PaymentTerms = InvoicePaymentTermCodeList.Codes.CD;
			AssertNoMessageErrors(invoice.JZ_PaymentTermsInfo);
		}

		public void TestJZ_InvoiceCurrExRate()
		{
			invoice.JZ_InvoiceCurrExRate = 0;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceCurrExRateInfo, jz_InvoiceCurrExRateErrorMessage);

			invoice.JZ_InvoiceCurrExRate = 1;
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceCurrExRateInfo, jz_InvoiceCurrExRateErrorMessage);

			invoice.JZ_InvoiceCurrExRate = -1;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceCurrExRateInfo, jz_InvoiceCurrExRateErrorMessage);
		}

		public void TestHouseBillNumber()
		{
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationPlan = "D";
			var invoice = declaration.Invoices.AddNew();
			invoice.Validation.ValidateJZ_CU_RelatedHouseBill();
			AssertHasMessageErrorContaining(invoice.JZ_CU_RelatedHouseBillInfo, "A house bill number is mandatory for the Declaration Plan Code, 'C', 'D', 'E', or 'F'");

			declaration.JE_DeclarationPlan = "H";
			invoice.Validation.ValidateJZ_CU_RelatedHouseBill();
			AssertNoMessageErrors(invoice.JZ_CU_RelatedHouseBillInfo);

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "1";

			invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
			AssertHasMessageErrorContaining(invoice.JZ_CU_RelatedHouseBillInfo, "The Declaration Plan Code is H. Please do not enter a house bill number");

			declaration.JE_DeclarationPlan = "C";
			invoice.Validation.ValidateJZ_CU_RelatedHouseBill();
			AssertNoMessageErrors(invoice.JZ_CU_RelatedHouseBillInfo);
		}

		public void TestCheckJZ_OH_Supplier()
		{
			var supplier1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "");
			var supplier2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "Test Supplier2");
			supplier2.OH_RL_NKClosestPort = "KRSEL";
			var supplierID = new IDNumberAndType[]
			{
				new IDNumberAndType { Type = IdentificationType.ForeignCompanyID, Number = "ZZZ1101022931" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier2, supplierID);

			validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrorContaining(invoice.JZ_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_OH_Supplier = supplier1.PK;
			AssertHasMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.MissingCompanyNameMessage);
			AssertHasMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.MissingCountryCodeMessage);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.A;
			validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.MissingCompanyNameMessage);
			AssertHasMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.MissingCountryCodeMessage);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.C;
			validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.MissingCompanyNameMessage);
			AssertHasMessageErrorContaining(invoice.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation.MissingCountryCodeMessage);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._91;
			validation.ValidateJZ_OH_Supplier();
			AssertNoMessageErrors(invoice.JZ_OH_SupplierInfo);

			invoice.JZ_OH_Supplier = supplier2.PK;
			AssertNoMessageErrors(invoice.JZ_OH_SupplierInfo);
		}

		public void TestCheckImportSupplierID()
		{
			var supplier1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "");
			var supplier2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "Test Supplier");
			supplier2.OH_RL_NKClosestPort = "KRSEL";
			var supplierID = new IDNumberAndType[]
			{
				new IDNumberAndType { Type = IdentificationType.ForeignCompanyID, Number = "ZZZ1101022931" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(supplier2, supplierID);

			invoice.JZ_OH_Supplier = supplier1.PK;
			validation.ValidateImportSupplierID();
			AssertHasMessageErrorContaining(invoice.ImportSupplierIDInfo, "There is no Supplier ID for this organization. Please press F3 here and add a number of type '07' in Config > Registration Numbers/Codes on the Organization form.");

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.A;
			validation.ValidateImportSupplierID();
			AssertHasMessageErrorContaining(invoice.ImportSupplierIDInfo, "There is no Supplier ID for this organization. Please press F3 here and add a number of type '07' in Config > Registration Numbers/Codes on the Organization form.");

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.C;
			validation.ValidateImportSupplierID();
			AssertEquals(SupplierDefaultCode.SupplierID, invoice.ImportSupplierID);
			AssertNoMessageErrors(invoice.ImportSupplierIDInfo);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._91;
			validation.ValidateImportSupplierID();
			AssertNoMessageErrors(invoice.ImportSupplierIDInfo);

			invoice.JZ_OH_Supplier = supplier2.PK;
			validation.ValidateImportSupplierID();
			AssertNoMessageErrors(invoice.ImportSupplierIDInfo);
		}

		public void TestCheckJZ_OA_SellerAddress()
		{
			var seller1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "");
			var seller2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "Test Online Seller");

			AssertNoMessageErrors(invoice.JZ_OA_SellerAddressInfo);

			invoice.JZ_OA_SellerAddress = seller1.MainAddress.PK;
			AssertHasMessageErrorContaining(invoice.JZ_OA_SellerAddressInfo, MandatoryValidation.DoNotEntered);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._15;
			validation.ValidateJZ_OA_SellerAddress();
			AssertHasMessageErrorContaining(invoice.JZ_OA_SellerAddressInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_OnlineTradeType = OnlineTradeTypeCodeList.Codes.A;
			validation.ValidateJZ_OA_SellerAddress();
			AssertHasMessageErrorContaining(invoice.JZ_OA_SellerAddressInfo, JobComInvoiceHeaderValidation.MissingCompanyNameMessage);

			var sellerID = new IDNumberAndType[]
			{
				new IDNumberAndType { Type = IdentificationType.OnlineTradeSellerID, Number = "ZZZ1101022931" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(seller1, sellerID);
			validation.ValidateJZ_OA_SellerAddress();
			AssertNoMessageErrors(invoice.JZ_OA_SellerAddressInfo);

			invoice.JZ_OA_SellerAddress = seller2.MainAddress.PK;
			AssertNoMessageErrors(invoice.JZ_OA_SellerAddressInfo);
		}

		public void TestCheckJZ_OH_SellingAgent()
		{
			var agent = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "Test Online Selling Agent");
			AssertNoMessageErrors(invoice.JZ_OH_SellingAgentInfo);

			invoice.JZ_OH_SellingAgent = agent.PK;
			AssertHasMessageErrorContaining(invoice.JZ_OH_SellingAgentInfo, MandatoryValidation.DoNotEntered);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._15;
			validation.ValidateJZ_OH_SellingAgent();
			AssertNoMessageErrors(invoice.JZ_OH_SellingAgentInfo);
		}

		public void TestJZ_OA_DistributorAddress()
		{
			var orgHeader1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "(주)레디코리아");
			var orgHeaderCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ECommerceCompanyID, Number = "123456789" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(orgHeader1, orgHeaderCodes);

			var orgHeader2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "(주)레디코리아");

			var orgHeader3 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK3", "");

			var orgHeader4 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK4", "");
			TestOrgDataSetUpHelper.AddCustomsCode(orgHeader4, orgHeaderCodes);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._15;
			invoice.JZ_OnlineTradeType = ImportOnlineTypeCodeList.Codes.B;
			AssertTradeTypeIs15AndOnlineTradeTypeIsBOrC();
			invoice.JZ_OnlineTradeType = ImportOnlineTypeCodeList.Codes.C;
			AssertTradeTypeIs15AndOnlineTradeTypeIsBOrC();

			invoice.JZ_OnlineTradeType = ImportOnlineTypeCodeList.Codes.A;
			AssertTradeTypeIs15AndOnlineTradeTypeIsAOrZ();
			invoice.JZ_OnlineTradeType = ImportOnlineTypeCodeList.Codes.Z;
			AssertTradeTypeIs15AndOnlineTradeTypeIsAOrZ();

			void AssertTradeTypeIs15AndOnlineTradeTypeIsBOrC()
			{
				invoice.JZ_OA_DistributorAddress = ZGuid.Empty;
				AssertHasMessageErrorContaining(invoice.JZ_OA_DistributorAddressInfo, MandatoryValidation.YouHaveNotEntered);
				invoice.JZ_OA_DistributorAddress = orgHeader1.MainAddress.PK;
				AssertNoMessageErrors(invoice.JZ_OA_DistributorAddressInfo);
				invoice.JZ_OA_DistributorAddress = orgHeader2.MainAddress.PK;
				AssertNoMessageErrors(invoice.JZ_OA_DistributorAddressInfo);
				invoice.JZ_OA_DistributorAddress = orgHeader3.MainAddress.PK;
				AssertHasMessageErrorContaining(invoice.JZ_OA_DistributorAddressInfo, "The name of this company is missing. Press F3 here and enter the company name.");
				invoice.JZ_OA_DistributorAddress = orgHeader4.MainAddress.PK;
				AssertNoMessageErrors(invoice.JZ_OA_DistributorAddressInfo);
			}

			void AssertTradeTypeIs15AndOnlineTradeTypeIsAOrZ()
			{
				invoice.JZ_OA_DistributorAddress = ZGuid.Empty;
				AssertNoMessageErrors(invoice.JZ_OA_DistributorAddressInfo);
				invoice.JZ_OA_DistributorAddress = orgHeader1.MainAddress.PK;
				AssertHasMessageErrorContaining(invoice.JZ_OA_DistributorAddressInfo, MandatoryValidation.DoNotEntered);
				invoice.JZ_OA_DistributorAddress = orgHeader2.MainAddress.PK;
				AssertHasMessageErrorContaining(invoice.JZ_OA_DistributorAddressInfo, MandatoryValidation.DoNotEntered);
				invoice.JZ_OA_DistributorAddress = orgHeader3.MainAddress.PK;
				AssertHasMessageErrorContaining(invoice.JZ_OA_DistributorAddressInfo, MandatoryValidation.DoNotEntered);
				invoice.JZ_OA_DistributorAddress = orgHeader4.MainAddress.PK;
				AssertHasMessageErrorContaining(invoice.JZ_OA_DistributorAddressInfo, MandatoryValidation.DoNotEntered);
			}
		}

		public void TestJZ_ImportCargoManagementNumber()
		{
			invoice.Validation.ValidateJZ_ImportCargoManagementNumber();
			AssertHasMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_ImportCargoManagementNumber = "123";
			AssertHasMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, "The length of the value should be 2, 15 or 19.");

			invoice.JZ_ImportCargoManagementNumber = "12";
			AssertHasMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, "Only 'NO' is accepted when the length of the value is 2.");

			invoice.JZ_ImportCargoManagementNumber = "NO";
			AssertNoMessageErrors(invoice.JZ_ImportCargoManagementNumberInfo);

			invoice.JZ_ImportCargoManagementNumber = "01234567890123A";
			AssertHasMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, "The last 4 digits should be numeric.");

			invoice.JZ_ImportCargoManagementNumber = "012345678901234";
			AssertNoMessageErrors(invoice.JZ_ImportCargoManagementNumberInfo);

			invoice.JZ_ImportCargoManagementNumber = "012345678901234567B";
			AssertHasMessageErrorContaining(invoice.JZ_ImportCargoManagementNumberInfo, "The last 8 digits should be numeric.");

			invoice.JZ_ImportCargoManagementNumber = "0123456789012345678";
			AssertNoMessageErrors(invoice.JZ_ImportCargoManagementNumberInfo);
		}

		public void TestJZ_BlanketValuationDeclarationNumber()
		{
			invoice.JZ_BlanketValuationDeclarationNumber = ZString.Empty;
			AssertNoMessageErrors(invoice.JZ_BlanketValuationDeclarationNumberInfo);

			invoice.JZ_ValuationDecAttachCode = ValueDeclarationAttachedCodeList.Codes.P;
			invoice.JZ_BlanketValuationDeclarationNumber = ZString.Empty;
			AssertHasMessageErrorContaining(invoice.JZ_BlanketValuationDeclarationNumberInfo, "You have indicated that this entry is covered by a periodic valuation declaration. Please enter its declaration number.");

			invoice.JZ_BlanketValuationDeclarationNumber = "TESTNUMBER";
			AssertNoMessageErrors(invoice.JZ_BlanketValuationDeclarationNumberInfo);
		}

		public void TestJZ_COOStatus()
		{
			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.C;
			invoice.JZ_COOStatus = "";
			AssertNoMessageErrors(invoice.JZ_COOStatusInfo);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.A;
			invoice.Validation.ValidateJZ_COOStatus();
			AssertHasMessageErrorContaining(invoice.JZ_COOStatusInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_COOStatus = "A";
			AssertHasMessageErrorContaining(invoice.JZ_COOStatusInfo, ListValidation.InvalidCodeMessageError);

			invoice.JZ_COOStatus = ImportCertificateOfOriginIssuedCodeList.Codes.Y;
			AssertNoMessageErrors(invoice.JZ_COOStatusInfo);
		}

		public void TestJZ_ValuationDecAttachCode()
		{
			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.C;
			invoice.JZ_ValuationDecAttachCode = "";
			AssertNoMessageErrors(invoice.JZ_ValuationDecAttachCodeInfo);

			declaration.JE_MessageSubType = ImportDeclarationTypeCodeList.Codes.A;
			invoice.Validation.ValidateJZ_ValuationDecAttachCode();
			AssertHasMessageErrorContaining(invoice.JZ_ValuationDecAttachCodeInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.JZ_ValuationDecAttachCode = "A";
			AssertHasMessageErrorContaining(invoice.JZ_ValuationDecAttachCodeInfo, ListValidation.InvalidCodeMessageError);

			invoice.JZ_ValuationDecAttachCode = ValueDeclarationAttachedCodeList.Codes.Y;
			AssertNoMessageErrors(invoice.JZ_ValuationDecAttachCodeInfo);
		}

		public void TestCheckJZ_OnlineTradeType()
		{
			AssertNoMessageErrors(invoice.JZ_OnlineTradeTypeInfo);

			invoice.JZ_OnlineTradeType = OnlineTradeTypeCodeList.Codes.A;
			AssertHasMessageErrorContaining(invoice.JZ_OnlineTradeTypeInfo, MandatoryValidation.DoNotEntered);

			declaration.JE_TradeType = ImportDealingTypeCodeList.Codes._15;
			invoice.Validation.ValidateJZ_OnlineTradeType();
			AssertNoMessageErrors(invoice.JZ_OnlineTradeTypeInfo);
			invoice.JZ_OnlineTradeType = "X";
			invoice.Validation.ValidateJZ_OnlineTradeType();
			AssertHasMessageErrorContaining(invoice.JZ_OnlineTradeTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJZ_COOLabelType()
		{
			invoice.JZ_COOLabelType = "";
			AssertNoMessageErrors(invoice.JZ_COOLabelTypeInfo);
			invoice.JZ_COOLabelLocation = CountryOfOriginLabelLocationCodeList.Codes.B;
			invoice.Validation.ValidateJZ_COOLabelType();
			AssertHasMessageErrorContaining(invoice.JZ_COOLabelTypeInfo, MandatoryValidation.YouHaveNotEntered);
			invoice.JZ_COOLabelType = CountryOfOriginLabelTypeCodeList.Codes.A;
			AssertHasMessageErrorContaining(invoice.JZ_COOLabelTypeInfo, "If Country Of Origin Label Location is 'B', then Country Of Origin Label Type cannot be 'A', 'C' or 'E'");
			invoice.JZ_COOLabelType = CountryOfOriginLabelTypeCodeList.Codes.B;
			AssertNoMessageErrors(invoice.JZ_COOLabelTypeInfo);
			invoice.JZ_COOLabelLocation = CountryOfOriginLabelLocationCodeList.Codes.N;
			invoice.JZ_COOLabelType = "";
			AssertNoMessageErrors(invoice.JZ_COOLabelTypeInfo);
		}

		public void TestCheckJZ_COOExemptionReason()
		{
			invoice.JZ_COOExemptionReason = "";
			AssertNoMessageErrors(invoice.JZ_COOExemptionReasonInfo);
			invoice.JZ_COOLabelLocation = CountryOfOriginLabelLocationCodeList.Codes.E;
			invoice.Validation.ValidateJZ_COOExemptionReason();
			AssertHasMessageErrorContaining(invoice.JZ_COOExemptionReasonInfo, MandatoryValidation.YouHaveNotEntered);
			invoice.JZ_COOExemptionReason = "16";
			AssertHasMessageErrorContaining(invoice.JZ_COOExemptionReasonInfo, ListValidation.InvalidCodeMessageError);
			invoice.JZ_COOExemptionReason = CountryOfOriginExemptionReasonCodeList.Codes._12;
			AssertNoMessageErrors(invoice.JZ_COOExemptionReasonInfo);
		}

		public void TestJZ_DeductionType()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SettingRate(usd, 999.67m, Core.Constants.ExchangeRateTypes.Code.CustomsRate);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			invoice.JZ_DeductionType = "X";
			AssertEquals(ZDecimal.Zero, invoice.GeneralCost);
			AssertNoMessageErrors(invoice.JZ_DeductionTypeInfo);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;
			validation.ValidateJZ_DeductionType();
			AssertNoMessageErrors(invoice.JZ_DeductionTypeInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			validation.ValidateJZ_DeductionType();
			AssertHasMessageErrorContaining(invoice.JZ_DeductionTypeInfo, MandatoryValidation.DoNotEntered);

			invoice.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B405, 222.22m, Core.Constants.CurrencyCodes.UnitedStates);
			AssertEquals("222.22 * 999.67", 222147m, invoice.GeneralCost);
			validation.ValidateJZ_DeductionType();
			AssertHasMessageErrorContaining(invoice.JZ_DeductionTypeInfo, ListValidation.InvalidCodeMessageError);

			invoice.JZ_DeductionType = CostRateCodeList.Codes._1;
			AssertNoMessageErrors(invoice.JZ_DeductionTypeInfo);
		}

		public void TestJZ_DeductionRate()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SettingRate(usd, 999.67m, Core.Constants.ExchangeRateTypes.Code.CustomsRate);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			invoice.JZ_DeductionRate = 100m;
			invoice.JZ_DeductionType = CostRateCodeList.Codes._1;
			AssertEquals(ZDecimal.Zero, invoice.GeneralCost);
			AssertNoMessageErrors(invoice.JZ_DeductionRateInfo);

			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;
			validation.ValidateJZ_DeductionRate();
			AssertNoMessageErrors(invoice.JZ_DeductionRateInfo);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			validation.ValidateJZ_DeductionRate();
			AssertHasMessageErrorContaining(invoice.JZ_DeductionRateInfo, "If General Cost is 0 then Cost Rate must be 0");
			AssertNoMessageErrorContaining(invoice.JZ_DeductionRateInfo, "You have entered Cost Rate Code without its rate.");
			AssertNoMessageErrorContaining(invoice.JZ_DeductionRateInfo, "You have entered Cost Rate without its rate code.");

			invoice.Charges.AddNew(ImportChargeMethodFourCodeList.Codes.B405, 222.22m, Core.Constants.CurrencyCodes.UnitedStates);
			AssertEquals("222.22 * 999.67", 222147m, invoice.GeneralCost);
			invoice.JZ_DeductionType = ZString.Empty;
			validation.ValidateJZ_DeductionRate();
			AssertHasMessageErrorContaining(invoice.JZ_DeductionRateInfo, "You have entered Cost Rate without its rate code.");

			invoice.JZ_DeductionType = CostRateCodeList.Codes._1;
			invoice.JZ_DeductionRate = ZDecimal.Zero;
			AssertHasMessageErrorContaining(invoice.JZ_DeductionRateInfo, "You have entered Cost Rate Code without its rate.");

			invoice.JZ_DeductionRate = 100m;
			AssertNoMessageErrors(invoice.JZ_DeductionRateInfo);
		}

		public void TestCheckJZ_ProvAdditionalRateIsValidZDecimal()
		{
			invoice.JZ_ProvAdditionalRate = -10m;
			AssertHasMessageErrorContaining(invoice.JZ_ProvAdditionalRateInfo, "Percentage value should be between 0 and 100");

			invoice.JZ_ProvAdditionalRate = 60m;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalRateInfo, "Percentage value should be between 0 and 100");

			invoice.JZ_ProvAdditionalRate = 1000m;
			AssertHasMessageErrorContaining(invoice.JZ_ProvAdditionalRateInfo, "Percentage value should be between 0 and 100");
		}

		public void TestCheckJZ_SpecificUseProductType()
		{
			invoice.JZ_SpecificUseProductType = SpecificUseProductTypeList.Codes._10;
			AssertNoMessageErrors(invoice.JZ_SpecificUseProductTypeInfo);

			invoice.JZ_SpecificUseProductType = SpecificUseProductTypeList.Codes._11;
			AssertNoMessageErrors(invoice.JZ_SpecificUseProductTypeInfo);

			invoice.JZ_SpecificUseProductType = SpecificUseProductTypeList.Codes._99;
			AssertNoMessageErrors(invoice.JZ_SpecificUseProductTypeInfo);

			invoice.JZ_SpecificUseProductType = "12";
			AssertHasMessageErrorContaining(invoice.JZ_SpecificUseProductTypeInfo, ListValidation.InvalidCodeMessageError);

			invoice.JZ_SpecificUseProductType = "";
			AssertNoMessageErrors(invoice.JZ_SpecificUseProductTypeInfo);
		}

		public void TestCheckJZ_ScheduledReExportCustomsOffice()
		{
			SetCustomsOffice();
			invoice.JZ_ScheduledReExportCustomsOffice = "X";
			AssertHasMessageErrorContaining(invoice.JZ_ScheduledReExportCustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

			invoice.JZ_ScheduledReExportCustomsOffice = "010";
			AssertNoMessageErrors(invoice.JZ_ScheduledReExportCustomsOfficeInfo);
		}

		public void TestCheckJZ_JurisdictionalCusOffice()
		{
			SetCustomsOffice();
			invoice.JZ_JurisdictionalCusOffice = "X";
			AssertHasMessageErrorContaining(invoice.JZ_JurisdictionalCusOfficeInfo, ListValidation.InvalidCodeMessageError);

			invoice.JZ_JurisdictionalCusOffice = "010";
			AssertNoMessageErrors(invoice.JZ_JurisdictionalCusOfficeInfo);
		}

		public void TestCheckJZ_RN_NKReExportDestinationCountry()
		{
			invoice.JZ_RN_NKReExportDestinationCountry = "X";
			AssertHasMessageErrorContaining(invoice.JZ_RN_NKReExportDestinationCountryInfo, ListValidation.InvalidCodeMessageError);

			invoice.JZ_RN_NKReExportDestinationCountry = "KR";
			AssertNoMessageErrors(invoice.JZ_RN_NKReExportDestinationCountryInfo);
		}

		void SetCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();
		}

		public void TestCheckValuationQuestion_IMP()
		{
			invoice.ValuationQuestion7A_IMP = ZString.Empty;
			invoice.ValuationQuestion8A = ZString.Empty;
			invoice.ValuationQuestion8B = ZString.Empty;
			invoice.ValuationQuestion9A = ZString.Empty;
			invoice.ValuationQuestion9B = ZString.Empty;

			AssertNoMessageErrors(invoice.ValuationQuestion7A_IMPInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion8AInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion8BInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion9AInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion9BInfo);

			declaration.ValidationMode = ValidationModes.ValuationDeclaration;
			validation.ValidateValuationQuestion7A_IMP();
			AssertHasMessageErrorContaining(invoice.ValuationQuestion7A_IMPInfo, MandatoryValidation.YouHaveNotEntered);
			validation.ValidateValuationQuestion8A();
			AssertHasMessageErrorContaining(invoice.ValuationQuestion8AInfo, MandatoryValidation.YouHaveNotEntered);
			validation.ValidateValuationQuestion8B();
			AssertHasMessageErrorContaining(invoice.ValuationQuestion8BInfo, MandatoryValidation.YouHaveNotEntered);
			validation.ValidateValuationQuestion9A();
			AssertHasMessageErrorContaining(invoice.ValuationQuestion9AInfo, MandatoryValidation.YouHaveNotEntered);
			validation.ValidateValuationQuestion9B();
			AssertHasMessageErrorContaining(invoice.ValuationQuestion9BInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.ValuationQuestion7A_IMP = "X";
			invoice.ValuationQuestion8A = "X";
			invoice.ValuationQuestion8B = "X";
			invoice.ValuationQuestion9A = "X";
			invoice.ValuationQuestion9B = "X";
			AssertHasMessageErrorContaining(invoice.ValuationQuestion7A_IMPInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoice.ValuationQuestion8AInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoice.ValuationQuestion8BInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoice.ValuationQuestion9AInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoice.ValuationQuestion9BInfo, ListValidation.InvalidCodeMessageError);

			invoice.ValuationQuestion7A_IMP = YesNoList.Codes.No;
			invoice.ValuationQuestion8A = YesNoList.Codes.No;
			invoice.ValuationQuestion8B = YesNoList.Codes.No;
			invoice.ValuationQuestion9A = YesNoList.Codes.No;
			invoice.ValuationQuestion9B = YesNoList.Codes.No;
			AssertNoMessageErrors(invoice.ValuationQuestion7A_IMPInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion8AInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion8BInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion9AInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion9BInfo);

			invoice.ValuationQuestion7A_IMP = YesNoList.Codes.Yes;
			declaration.ValidationMode = ValidationModes.None;

			invoice.ValuationQuestion7B_IMP = ZString.Empty;
			invoice.ValuationQuestion7C = ZString.Empty;
			invoice.ValuationQuestion7D = ZString.Empty;
			invoice.ValuationQuestion7EA = ZString.Empty;

			AssertNoMessageErrors(invoice.ValuationQuestion7B_IMPInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion7CInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion7DInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion7EAInfo);

			declaration.ValidationMode = ValidationModes.ValuationDeclaration;
			validation.ValidateValuationQuestion7B_IMP();
			AssertHasMessageErrorContaining(invoice.ValuationQuestion7B_IMPInfo, MandatoryValidation.YouHaveNotEntered);
			validation.ValidateValuationQuestion7C();
			AssertHasMessageErrorContaining(invoice.ValuationQuestion7CInfo, MandatoryValidation.YouHaveNotEntered);
			validation.ValidateValuationQuestion7D();
			AssertHasMessageErrorContaining(invoice.ValuationQuestion7DInfo, MandatoryValidation.YouHaveNotEntered);
			validation.ValidateValuationQuestion7EA();
			AssertHasMessageErrorContaining(invoice.ValuationQuestion7EAInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.ValuationQuestion7B_IMP = "XX";
			invoice.ValuationQuestion7C = "X";
			invoice.ValuationQuestion7D = "X";
			invoice.ValuationQuestion7EA = "X";
			AssertHasMessageErrorContaining(invoice.ValuationQuestion7B_IMPInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoice.ValuationQuestion7CInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoice.ValuationQuestion7DInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoice.ValuationQuestion7EAInfo, ListValidation.InvalidCodeMessageError);

			invoice.ValuationQuestion7B_IMP = SpecialRelationshipCodeList.Codes._01;
			invoice.ValuationQuestion7C = YesNoList.Codes.No;
			invoice.ValuationQuestion7D = YesNoList.Codes.No;
			invoice.ValuationQuestion7EA = PricingCodeList.Codes._01;
			AssertNoMessageErrors(invoice.ValuationQuestion7B_IMPInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion7CInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion7DInfo);
			AssertNoMessageErrors(invoice.ValuationQuestion7EAInfo);

			AssertNoMessageErrors(invoice.ValuationQuestion7EBInfo);
			invoice.ValuationQuestion7EA = PricingCodeList.Codes._99;

			declaration.ValidationMode = ValidationModes.None;
			invoice.ValuationQuestion7EB = ZString.Empty;
			AssertNoMessageErrors(invoice.ValuationQuestion7EBInfo);

			declaration.ValidationMode = ValidationModes.ValuationDeclaration;
			validation.ValidateValuationQuestion7EB();
			AssertHasMessageErrorContaining(invoice.ValuationQuestion7EBInfo, MandatoryValidation.YouHaveNotEntered);

			invoice.ValuationQuestion7EB = "Test";
			AssertNoMessageErrors(invoice.ValuationQuestion7EBInfo);
		}

		void SettingRate(RefCurrency currency, ZDecimal rate, string rateType)
		{
			var result = Factory.New<RefExchangeRate>();
			result.RE_GC = GlbCompany.CurrentCompany.PK;
			result.RE_RX_NKExCurrency = currency.RX_Code;
			result.RE_StartDate = ZDateTime.Today;
			result.RE_ExpiryDate = ZDateTime.Today;
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;

			Factory.Save();
		}

		public void Test5SMFormD_JZ_ValuationCode()
		{
			invoice.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);
			invoice.JZ_ValuationDecAttachCode = "N";
			invoice.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);
			invoice.JZ_ValuationDecAttachCode = "Y";
			invoice.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);
			invoice.JZ_ValuationCode = "9";
			AssertHasMessageErrorContaining(invoice.JZ_ValuationCodeInfo, ListValidation.InvalidCodeMessageError);
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);

			invoice.JobDeclaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);
			invoice.JZ_ValuationDecAttachCode = ZString.Empty;
			invoice.JZ_ValuationCode = ZString.Empty;
			AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);
			invoice.JZ_ValuationDecAttachCode = "N";
			invoice.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);
			invoice.JZ_ValuationDecAttachCode = "Y";
			invoice.Validation.ValidateJZ_ValuationCode();
			AssertHasMessageErrorContaining(invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			invoice.JZ_ValuationCode = "9";
			AssertHasMessageErrorContaining(invoice.JZ_ValuationCodeInfo, ListValidation.InvalidCodeMessageError);
			invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
			AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);
		}

		public void Test934ProvisionalPricingReasonsValidation()
		{
			invoice.Validation.ValidateJZ_ProvPricingYN();
			AssertNoMessageErrorContaining(invoice.JZ_ProvPricingYNInfo, IMPJobComInvoiceHeaderValidation.SelectProvisionalPricingReasonMessage);
			AssertNoMessageErrorContaining(invoice.JZ_ProvPricingYNInfo, IMPJobComInvoiceHeaderValidation.DoNotSelectProvisionalPricingReasonMessage);

			invoice.ProvisionalPricingReason101 = true;
			AssertNoMessageErrorContaining(invoice.JZ_ProvPricingYNInfo, IMPJobComInvoiceHeaderValidation.SelectProvisionalPricingReasonMessage);
			AssertNoMessageErrorContaining(invoice.JZ_ProvPricingYNInfo, IMPJobComInvoiceHeaderValidation.DoNotSelectProvisionalPricingReasonMessage);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);

			invoice.Validation.ValidateJZ_ProvPricingYN();
			AssertNoMessageErrorContaining(invoice.JZ_ProvPricingYNInfo, IMPJobComInvoiceHeaderValidation.SelectProvisionalPricingReasonMessage);
			AssertHasMessageErrorContaining(invoice.JZ_ProvPricingYNInfo, IMPJobComInvoiceHeaderValidation.DoNotSelectProvisionalPricingReasonMessage);

			invoice.JZ_ProvPricingYN = YesNoList.Codes.Yes;
			AssertNoMessageErrorContaining(invoice.JZ_ProvPricingYNInfo, IMPJobComInvoiceHeaderValidation.SelectProvisionalPricingReasonMessage);
			AssertNoMessageErrorContaining(invoice.JZ_ProvPricingYNInfo, IMPJobComInvoiceHeaderValidation.DoNotSelectProvisionalPricingReasonMessage);

			invoice.ProvisionalPricingReason101 = false;
			invoice.Validation.ValidateJZ_ProvPricingYN();
			AssertHasMessageErrorContaining(invoice.JZ_ProvPricingYNInfo, IMPJobComInvoiceHeaderValidation.SelectProvisionalPricingReasonMessage);
			AssertNoMessageErrorContaining(invoice.JZ_ProvPricingYNInfo, IMPJobComInvoiceHeaderValidation.DoNotSelectProvisionalPricingReasonMessage);

			invoice.JZ_ProvPricingYN = YesNoList.Codes.No;
			AssertNoMessageErrorContaining(invoice.JZ_ProvPricingYNInfo, IMPJobComInvoiceHeaderValidation.SelectProvisionalPricingReasonMessage);
			AssertNoMessageErrorContaining(invoice.JZ_ProvPricingYNInfo, IMPJobComInvoiceHeaderValidation.DoNotSelectProvisionalPricingReasonMessage);
		}

		public void Test934DateTypeValidation()
		{
			invoice.Validation.ValidateJZ_EstimatedDateOfFinalPrice();
			AssertNoMessageErrorContaining(invoice.JZ_EstimatedDateOfFinalPriceInfo, MandatoryValidation.DoNotEntered);

			invoice.Validation.ValidateJZ_ImpContractExpiryDate();
			AssertNoMessageErrorContaining(invoice.JZ_ImpContractExpiryDateInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_EstimatedDateOfFinalPrice = new ZDateTime(2022, 02, 01);
			AssertNoMessageErrorContaining(invoice.JZ_EstimatedDateOfFinalPriceInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ImpContractExpiryDate = new ZDateTime(2022, 12, 01);
			AssertNoMessageErrorContaining(invoice.JZ_ImpContractExpiryDateInfo, MandatoryValidation.DoNotEntered);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);

			invoice.JZ_EstimatedDateOfFinalPrice = ZDateTime.Empty;
			AssertNoMessageErrorContaining(invoice.JZ_EstimatedDateOfFinalPriceInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ImpContractExpiryDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(invoice.JZ_ImpContractExpiryDateInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_EstimatedDateOfFinalPrice = new ZDateTime(2022, 02, 02);
			AssertHasMessageErrorContaining(invoice.JZ_EstimatedDateOfFinalPriceInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ImpContractExpiryDate = new ZDateTime(2022, 12, 02);
			AssertHasMessageErrorContaining(invoice.JZ_ImpContractExpiryDateInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvPricingYN = YesNoList.Codes.Yes;

			invoice.JZ_EstimatedDateOfFinalPrice = ZDateTime.Empty;
			AssertNoMessageErrorContaining(invoice.JZ_EstimatedDateOfFinalPriceInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ImpContractExpiryDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(invoice.JZ_ImpContractExpiryDateInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_EstimatedDateOfFinalPrice = new ZDateTime(2022, 02, 03);
			AssertNoMessageErrorContaining(invoice.JZ_EstimatedDateOfFinalPriceInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ImpContractExpiryDate = new ZDateTime(2022, 12, 03);
			AssertNoMessageErrorContaining(invoice.JZ_ImpContractExpiryDateInfo, MandatoryValidation.DoNotEntered);
		}

		public void Test934NumberTypeValidation()
		{
			invoice.Validation.ValidateJZ_ProvAdditionalRate();
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalRateInfo, MandatoryValidation.DoNotEntered);

			invoice.Validation.ValidateJZ_ProvAdditionalAmount();
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalAmountInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalAmount = -1;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalAmountInfo, MandatoryValidation.ValueCannotBeNegative);

			invoice.JZ_ProvAdditionalRate = ZDecimal.Zero;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalRateInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalAmount = ZDecimal.Zero;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalAmountInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalRate = 1m;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalRateInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalAmount = 1m;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalAmountInfo, MandatoryValidation.DoNotEntered);

			declaration.SetValidationModeOnElectronicMessaging(ElectronicDocumentTypeList.Codes._934);

			invoice.JZ_ProvAdditionalRate = -1;
			AssertHasMessageErrorContaining(invoice.JZ_ProvAdditionalRateInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalAmount = -1;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalAmountInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertHasMessageErrorContaining(invoice.JZ_ProvAdditionalAmountInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalRate = ZDecimal.Zero;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalRateInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalAmount = ZDecimal.Zero;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalAmountInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalRate = 1m;
			AssertHasMessageErrorContaining(invoice.JZ_ProvAdditionalRateInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalAmount = 1m;
			AssertHasMessageErrorContaining(invoice.JZ_ProvAdditionalAmountInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvPricingYN = YesNoList.Codes.Yes;

			invoice.JZ_ProvAdditionalRate = -1;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalRateInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalAmount = -1;
			AssertHasMessageErrorContaining(invoice.JZ_ProvAdditionalAmountInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalAmountInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalRate = ZDecimal.Zero;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalRateInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalAmount = ZDecimal.Zero;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalAmountInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalRate = 1m;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalRateInfo, MandatoryValidation.DoNotEntered);

			invoice.JZ_ProvAdditionalAmount = 1m;
			AssertNoMessageErrorContaining(invoice.JZ_ProvAdditionalAmountInfo, MandatoryValidation.DoNotEntered);
		}

		readonly string jz_IncotermErrorMessage = "If Invoice Payment Term is not [GN] and Trade Type is not [71,80~97,100] then Incoterm is mandatory. Please enter a value.";
		readonly string jz_InvoiceAmountErrorMessageGreaterThanZero = "If Invoice Payment Term is not [GN] and Trade Type is not [71,80~97,100] then Total Invoice Amount must be greater than zero.";
		readonly string jz_InvoiceAmountErrorMessageMustBeZero = "If Invoice Payment Term is [GN] and or Trade Type is [71,80~97,100] then Total Invoice Amount must be zero.";
		readonly string jz_RX_NKInvoice_CurrencyErrorMessage = "If Invoice Payment Term is [GN] or Trade Type is [71,80~97,100] then Invoice Amount Currency must be [USD].";
		readonly string jz_InvoiceCurrExRateErrorMessage = "Please enter an 'Exchange Rate' greater than 0.";

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			validation = (IMPJobComInvoiceHeaderValidation)invoice.Validation;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "86420");

			Factory.Save();
		}
		IMPJobComInvoiceHeaderValidation validation;
		JobComInvoiceHeader invoice;
		JobDeclaration declaration;
	}
}
