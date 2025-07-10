using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class ImportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		public void TestJZ_ValuationDateOverride()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, expectedExportDateErrorText);
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Now.AddDays(1);
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, expectedExportDateErrorText);
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Now.AddDays(-1);
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, expectedExportDateErrorText);
		}
		const string expectedExportDateErrorText = "The valuation date override cannot be in the future.";

		public void TestJZ_ValuationDateOverrideFoirExwarehouse()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.AUDIsRequired);
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.ValuationDateIsRequired);
			line2.AddInfo.ZA_TILV = "10AUD";
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.ValuationDateIsRequired);
			line2.AddInfo.ZA_TILV = "10USD";
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.ValuationDateIsRequired);
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2009, 10, 20);
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.ValuationDateIsRequired);
			line2.AddInfo.ZA_TILV = ZString.Empty;
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Empty;
			InvoiceLineCharge oFT = line2.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 1m;
			oFT.J7_RX_NKCurrency = "USD";
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.ValuationDateIsRequired);
			oFT.J7_RX_NKCurrency = "AUD";
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.ValuationDateIsRequired);
			InvoiceLineCharge oTH = line2.Charges.AddNew();
			oTH.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			oTH.J7_Amount = 1m;
			oTH.J7_RX_NKCurrency = "USD";
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.ValuationDateIsRequired);
			InvoiceLineCharge oNS = line1.Charges.AddNew();
			oNS.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			oNS.J7_Amount = 1m;
			oNS.J7_RX_NKCurrency = "AUD";
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.ValuationDateIsRequired);
			oNS.J7_RX_NKCurrency = "USD";
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.ValuationDateIsRequired);
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2009, 10, 20);
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.ValuationDateIsRequired);

			line1.AddInfo.ZA_TILV = "10USD";
			line1.AddInfo.ZA_WRN = "WRN1";
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.AUDIsRequired);
			line2.AddInfo.ZA_WRN = "WRN1";
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.AUDIsRequired);
			line2.AddInfo.ZA_WRN = "WRN2";
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.AUDIsRequired);
			oNS.J7_RX_NKCurrency = "AUD";
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.AUDIsRequired);
			line1.AddInfo.ZA_TILV = "10AUD";
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.AUDIsRequired);
		}

		public void TestWhenThereIsUnbalanceBetweenHeaderTILVAndLineTILVs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.AddInfo.ZA_TILV = "100AUD";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.AddInfo.ZA_TILV = "200AUD";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20000m;
			invoiceLine2.AddInfo.ZA_TILV = "150AUD";

			declaration.ResumeApportionment();

			AssertHasMessageErrorContaining(invoice.JZ_Calc_TNIInfo, "The total T&I of lines");

			invoiceLine.AddInfo.ZA_TILV = "";
			invoiceLine2.AddInfo.ZA_TILV = "";
			declaration.ResumeApportionment();
			AssertNoMessageErrorContaining(invoice.JZ_Calc_TNIInfo, "The total T&I of lines");
		}

		public void TestAddInfoTAndI()
		{
			InvoiceHeader.JobDeclaration.JobComInvoiceGroupHeaders[0].Charges.AddNew("OFT", 2000m, "AUD");
			InvoiceHeader.AddInfo.ZA_TILV = "250AUD";

			JobComInvoiceHeader invoice2 = InvoiceHeader.JobDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JobComInvoiceLines.AddNew().JI_LinePrice = 10000m;

			InvoiceHeader.Validation.ValidateJZ_Calc_TNI();
			Declaration.ResumeApportionment();
			AssertHasMessageError(InvoiceHeader.JZ_Calc_TNIInfo, ImportJobComInvoiceHeaderValidation.AllInvoicesShouldHaveTILV);

			invoice2.AddInfo.ZA_TILV = "0AUD";
			InvoiceHeader.Validation.ValidateJZ_Calc_TNI();
			AssertNoMessageError(InvoiceHeader.JZ_Calc_TNIInfo, ImportJobComInvoiceHeaderValidation.AllInvoicesShouldHaveTILV);
		}

		public void TestImporterValidation()
		{
			AssertEquals("Importer has no warnings", false, InvoiceHeader.JZ_OH_BuyerInfo.HasWarnings());

			OrgHeader header = OrgHeader.New(Factory);
			InvoiceHeader.JZ_OH_Buyer = header.PK;
			AssertEquals("Importer has a warning", true, InvoiceHeader.JZ_OH_BuyerInfo.HasWarnings());

			InvoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			AssertEquals("Importer has no warnings", false, InvoiceHeader.JZ_OH_BuyerInfo.HasWarnings());
		}

		public void TestSupplierDoesNotGetValidatedForExWarehouse()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			InvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertNoNotifications("Supplier should have no notifications", InvoiceHeader.JZ_OH_SupplierInfo);
		}

		public void TestSupplierCodeValidation()
		{
			var header = OrgHeader.New(Factory);
			InvoiceHeader.JZ_OH_Supplier = header.PK;
			InvoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrors(InvoiceHeader.JZ_OH_SupplierInfo);

			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "1234567890");
			var cusCode = header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CustomsClientID, Core.Constants.CountryCodes.Australia);
			cusCode.OK_OA_PremisesAddress = header.MainAddress.PK;
			InvoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrors(InvoiceHeader.JZ_OH_SupplierInfo);

			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");
			InvoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageErrors(InvoiceHeader.JZ_OH_SupplierInfo);

			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			InvoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrorContaining(InvoiceHeader.JZ_OH_SupplierInfo, "The INVOICE HEADER Supplier must have a Customs Client ID");
		}

		public void TestSupplierValidation()
		{
			var header = OrgHeader.New(Factory);
			InvoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrorContaining(InvoiceHeader.JZ_OH_SupplierInfo, "ID0417");
			InvoiceHeader.JZ_OH_Supplier = header.PK;
			AssertNoMessageErrorContaining(InvoiceHeader.JZ_OH_SupplierInfo, "ID0417");
		}

		public void TestErrorIfInvalidWeightUQ()
		{
			InvoiceHeader.JZ_Weight = 100m;
			InvoiceHeader.JZ_WeightUQ = "ZZ";
			Assert("HasMessageErrors", InvoiceHeader.JZ_WeightUQInfo.HasMessageErrors());
			InvoiceHeader.JZ_WeightUQ = "KG";
			Assert("!HasMessageErrors", !InvoiceHeader.JZ_WeightUQInfo.HasMessageErrors());
		}

		public void TestNoErrorForEmptyIncoTermNature30()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			InvoiceHeader.Validation.ValidateJZ_IncoTerm();
			AssertEquals("Notifications", false, InvoiceHeader.JZ_IncoTermInfo.HasNotifications());
		}

		public void TestValidateJZ_Calc_CIFAmount()
		{
			InvoiceHeader.JZ_InvoiceAmount = 1000m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceHeader.RunPreSaveValidation();
			AssertHasMessageError(InvoiceHeader.JZ_Calc_CIFAmountInfo, InvoiceHeaderValidation.ZeroFreightInsuranceWarning);

			InvoiceHeader.JobDeclaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			InvoiceHeader.JobDeclaration.ResumeApportionment();
			AssertNoMessageError(InvoiceHeader.JZ_Calc_CIFAmountInfo, InvoiceHeaderValidation.ZeroFreightInsuranceWarning);

			InvoiceHeader.JobDeclaration.JobComInvoiceGroupHeaders[0].Charges.RemoveAndDeleteAll();
			Declaration.ResumeApportionment();
			AssertEquals("CIF = FOB", InvoiceHeader.JZ_Calc_CIFAmount, InvoiceHeader.JZ_Calc_FOBAmount);

			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			InvoiceHeader.RunPreSaveValidation();
			Declaration.ResumeApportionment();
			AssertNoMessageError(InvoiceHeader.JZ_Calc_CIFAmountInfo, InvoiceHeaderValidation.ZeroFreightInsuranceWarning);
			AssertHasWarning(InvoiceHeader.JZ_Calc_CIFAmountInfo, InvoiceHeaderValidation.ZeroFreightInsuranceWarning);

			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			InvoiceHeader.RunPreSaveValidation();
			Declaration.ResumeApportionment();
			AssertNoMessageError(InvoiceHeader.JZ_Calc_CIFAmountInfo, InvoiceHeaderValidation.ZeroFreightInsuranceWarning);
			AssertHasWarning(InvoiceHeader.JZ_Calc_CIFAmountInfo, InvoiceHeaderValidation.ZeroFreightInsuranceWarning);

			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceHeader.RunPreSaveValidation();
			Declaration.ResumeApportionment();
			AssertNoMessageError(InvoiceHeader.JZ_Calc_CIFAmountInfo, InvoiceHeaderValidation.ZeroFreightInsuranceWarning);
			AssertHasWarning(InvoiceHeader.JZ_Calc_CIFAmountInfo, InvoiceHeaderValidation.ZeroFreightInsuranceWarning);
		}

		public void TestMissingMandatoryChargesForExwarehouse()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AssertEquals("Notifications", false, InvoiceHeader.JZ_IncoTermInfo.HasNotifications());
		}

		public void TestChargeMandatoryForUnpackedIncoTerm()
		{
			InvoiceHeader.JZ_InvoiceAmount = 1000m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
			AssertEquals("Packing cost is required", true, InvoiceHeader.JZ_IncoTermInfo.HasNotifications());

			invoiceGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100, aUDCurrency.RX_Code);
			AssertEquals("FIFT is required", true, InvoiceHeader.JZ_IncoTermInfo.HasNotifications());

			invoiceGroupHeader.Charges.RemoveAndDeleteAll();
			InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("This incoterm doesn't require packing costs", false, InvoiceHeader.JZ_IncoTermInfo.HasNotifications());

			InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
			AssertEquals("PC/FIFT is required", true, InvoiceHeader.JZ_IncoTermInfo.HasNotifications());
		}

		public void TestChargeMandatoryForPAF()
		{
			InvoiceHeader.JZ_InvoiceAmount = 1000m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;

			AssertEquals("ForeignInland Freight is required", true, InvoiceHeader.JZ_IncoTermInfo.HasNotifications());

			JobComInvoiceGroupHeader groupHeader = InvoiceHeader.Master as JobComInvoiceGroupHeader;
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 0m, aUDCurrency.RX_Code);
			Declaration.ResumeApportionment();
			AssertEquals("Apportioned Charge", 1, InvoiceHeader.GroupCharges.Count);
		}

		public void TestChargeMandatoryForC_I()
		{
			InvoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.CostAndInsurance;
			AssertEquals("ONS is required", true, InvoiceHeader.JZ_IncoTermInfo.HasNotifications());
		}

		public void TestChargeMandatoryForC_F()
		{
			InvoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.CostAndFreight;
			AssertEquals("OFS is required", true, InvoiceHeader.JZ_IncoTermInfo.HasNotifications());
		}

		public void TestChargeMandatoryForCIF()
		{
			InvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			InvoiceHeader.JZ_InvoiceAmount = 1000;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			AssertEquals("Two charges added", 2, InvoiceHeader.Charges.Count);
		}

		public void TestZeroTNIWarning()
		{
			InvoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.FreeOnBoard;
			InvoiceHeader.JZ_InvoiceAmount = 1000m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			Declaration.ResumeApportionment();
			InvoiceHeader.RunPreSaveValidation();
			AssertHasMessageError("Zero TNI Warning", InvoiceHeader.JZ_Calc_TNIInfo, ImportJobComInvoiceHeaderValidation.ZeroTNIWarning);
			InvoiceCharge oFT = InvoiceHeader.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 100m;
			oFT.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			Declaration.ResumeApportionment();
			AssertNoMessageError("No Zero TNI Warning", InvoiceHeader.JZ_Calc_TNIInfo, ImportJobComInvoiceHeaderValidation.ZeroTNIWarning);
			oFT.J7_Amount = 0m;
			Declaration.ResumeApportionment();
			InvoiceHeader.Validation.ValidateJZ_Calc_TNI();

			AssertHasMessageError("Zero TNI Warning", InvoiceHeader.JZ_Calc_TNIInfo, ImportJobComInvoiceHeaderValidation.ZeroTNIWarning);
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			Declaration.ResumeApportionment();
			InvoiceHeader.Validation.ValidateJZ_Calc_TNI();
			AssertNoMessageError("No Zero TNI Warning", InvoiceHeader.JZ_Calc_TNIInfo, ImportJobComInvoiceHeaderValidation.ZeroTNIWarning);
		}

		#region Implementation
		protected RefCurrency aUDCurrency;

		protected override void SetUp()
		{
			base.SetUp();
			aUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			InvoiceHeader.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			InvoiceHeader.JobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}

		protected void AssertChargeRelevancy(ZPropertyInfo chargeInfo, bool expectingError, string incoTerm)
		{
			if (expectingError)
			{
				Assert(chargeInfo.Value + " is not relevant for this " + incoTerm, chargeInfo.HasNotifications());
			}
			else
			{
				Assert(chargeInfo.Value + " is relevant for this " + incoTerm, !chargeInfo.HasNotifications());
			}
		}

		protected override JobComInvoiceHeaderValidation GetNewValidationProvider(JobComInvoiceHeader invoiceHeader)
		{
			return new ImportJobComInvoiceHeaderValidation(invoiceHeader);
		}

		protected override Type GetTypeForTest()
		{
			return typeof(ImportJobComInvoiceHeaderValidation);
		}
		#endregion
	}
}
