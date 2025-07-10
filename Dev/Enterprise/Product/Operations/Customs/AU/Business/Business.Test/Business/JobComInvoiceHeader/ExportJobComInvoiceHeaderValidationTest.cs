using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class ExportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		public void TestExternalMessageValidationType()
		{
			JobComInvoiceHeaderValidation validation = new JobComInvoiceHeaderValidation(InvoiceHeader);
			AssertEquals("External message validation with better message for exchange rates", typeof(ExternalMessageValidation), validation.MessageValidation.GetType());
		}

		public void TestJZ_ITOTIncotermForExport()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			InvoiceHeader.JZ_IncoTerm = EdificeIncoTermAndCustomsChargeFactory.ErrorIncoTermCode;
			AssertEquals("ITOT incoterm is XXX", EdificeIncoTermAndCustomsChargeFactory.ErrorIncoTermCode, InvoiceHeader.JZ_ITOTIncoTerm);
			AssertEquals("ITOT incoterm is not visible to users", false, InvoiceHeader.JZ_ITOTIncoTermInfo.HasMessageErrors());
		}

		public void TestJZ_ITOTIncotermForImport()
		{
			InvoiceHeader.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_IncoTerm = EdificeIncoTermAndCustomsChargeFactory.ErrorIncoTermCode;
			InvoiceHeader.RunPreSaveValidation();
			AssertEquals("ITOT incoterm is XXX", EdificeIncoTermAndCustomsChargeFactory.ErrorIncoTermCode, InvoiceHeader.JZ_ITOTIncoTerm);
			AssertEquals("ITOT incoterm is XXX", true, InvoiceHeader.JZ_ITOTIncoTermInfo.HasMessageErrors());
		}

		public void TestInvoiceNumberValidation()
		{
			InvoiceHeader.JZ_InvoiceNumber = "";
			AssertHasMessageErrorContaining(InvoiceHeader.JZ_InvoiceNumberInfo, "Please enter an " + InvoiceHeader.JZ_InvoiceNumberInfo.Description + ".");

			InvoiceHeader.JZ_JE = ZGuid.Empty;
			InvoiceHeader.Validation.ValidateJZ_InvoiceNumber();
			Assert("Has no message error when detached from declaration", !InvoiceHeader.JZ_InvoiceNumberInfo.HasMessageErrors());
		}

		public void TestValidateInvoiceCurrencyCode()
		{
			const string messageError = "The entered currency is invalid for EXD messages";
			InvoiceHeader.JZ_InvoiceAmount = 1000m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;

			InvoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertHasMessageErrorContaining("No Invoice Currency should be a message error", InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, messageError);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			AssertNoMessageErrorContaining("AUD should be a valid Invoice Currency", InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, messageError);
			AssertEquals("Invoice Currency has no other errors", false, InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.HasNotifications());
		}

		public void TestValidateInvoiceCurrencyCode_Quarantine()
		{
			const string messageError = "The entered currency is invalid for EXD messages";
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			InvoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			InvoiceHeader.JZ_InvoiceAmount = 0.0m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
				InvoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
				AssertHasMessageErrorContaining("No Invoice Currency should be a message error", InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, messageError);

				InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = false;
				InvoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
				AssertHasMessageErrorContaining("No Invoice Currency on EXDOC should be a message error", InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, messageError);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
				InvoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
				AssertHasMessageErrorContaining("No Invoice Currency should be a message error", InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, messageError);

				InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = false;
				InvoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
				AssertNoMessageErrorContaining("No Invoice Currency on NEXDOC should not be a message error when permit not required", InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, messageError);
			}
		}

		public void TestCurrencyWithNoCurrentExchangeRateErrors()
		{
			InvoiceHeader.JZ_InvoiceAmount = 123m;
			RefCurrency frenchCurrency = Factory.New<RefCurrency>();
			frenchCurrency.RX_Code = "FRF";
			// A new Currency object is used to allow no exchange rate to be found
			InvoiceHeader.JZ_RX_NKInvoice_Currency = frenchCurrency.RX_Code;
			Assert("Error expected : No exchange rate for New Currency", InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.HasNotifications());
		}

		public void TestValidateExchangeRateExistWhenNotExpectingWarning()
		{
			RefExchangeRate exRate = Factory.New<RefExchangeRate>();
			exRate.RE_RX_NKExCurrency = expiredCurrency.RX_Code;
			exRate.RE_ExRateType = "CUS";
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_StartDate = ZDateTime.Now.Date.AddDays(-1);
			exRate.RE_ExpiryDate = ZDateTime.Now.Date.AddMinutes(-1);
			exRate.RE_SellRate = 0.8m;
			InvoiceHeader.JZ_InvoiceAmount = 123m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = expiredCurrency.RX_Code;
			Assert("No Warning expected : No exchange rate for New Currency", !InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.HasWarnings());
		}

		public void TestValidateExchangeRateExistWhenAmountIsZero()
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = expiredCurrency.RX_Code;
			Assert("No error expected : Amount is zero", !InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.HasNotifications());
		}

		public void TestExpiredCurrencyGivesMessageError()
		{
			InvoiceHeader.JZ_InvoiceAmount = 1;
			RefCurrency frenchCurrency = Factory.New<RefCurrency>();
			frenchCurrency.RX_Code = "FRF";
			InvoiceHeader.JZ_RX_NKInvoice_Currency = frenchCurrency.RX_Code;
			Assert("Expired currency should raise notifications", InvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.HasNotifications());
		}

		public void TestValidateJZ_ValuationDateOverride()
		{
			InvoiceHeader.JobDeclaration.JE_DateOfFirstArrival = new ZDateTime(2007, 04, 23, 10, 45, 01);
			InvoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2007, 04, 23, 10, 45, 00);
			AssertNoMessageErrors(InvoiceHeader.JZ_ValuationDateOverrideInfo);
			InvoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2007, 04, 24, 01, 00, 00);
			AssertHasMessageError(InvoiceHeader.JZ_ValuationDateOverrideInfo, "Valuation Date must not be after the first Arrival Date");
			InvoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2007, 04, 22, 23, 00, 00);
			AssertNoMessageErrors(InvoiceHeader.JZ_ValuationDateOverrideInfo);
		}

		public void TestCheckJZ_RX_NKInvoice_Currency()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader testInvoiceHeader = declaration.Invoices.AddNew();
			testInvoiceHeader.JZ_InvoiceAmount = 100m;
			testInvoiceHeader.JZ_RX_NKInvoice_Currency = "AED";
			AssertHasMessageError(testInvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, "The entered currency is invalid for EXD messages");
			testInvoiceHeader.JZ_InvoiceAmount = 100m;
			testInvoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			AssertNoMessageError(testInvoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, "The entered currency is invalid for EXD messages");

			JobComInvoiceHeader testInvoiceHeader2 = declaration.Invoices.AddNew();
			testInvoiceHeader2.JZ_JE = testInvoiceHeader.JZ_JE;
			testInvoiceHeader2.JZ_RX_NKInvoice_Currency = "USD";
			AssertNoMessageErrors(testInvoiceHeader2.JZ_RX_NKInvoice_CurrencyInfo);

			testInvoiceHeader2.JZ_RX_NKInvoice_Currency = "AUD";
			AssertHasMessageError(testInvoiceHeader2.JZ_RX_NKInvoice_CurrencyInfo, ExportJobComInvoiceHeaderValidation.MultipleCurrencyError);
		}

		public override void TestValidateJZ_OH_Supplier()
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)GetInvoiceHeader();
			JobDeclaration declaration = invoiceHeader.JobDeclaration;
			AssertNotNull("Precondition: invoiceHeader.JobDeclaration", declaration);
			invoiceHeader.JZ_JE = ZGuid.Empty;
			new Customs.Business.FakeDeclarationCreatorForInvoice(invoiceHeader);
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertHasErrors(invoiceHeader.JZ_OH_SupplierInfo);
			invoiceHeader.JZ_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			AssertNoErrors(invoiceHeader.JZ_OH_SupplierInfo);

			invoiceHeader.JZ_JE = declaration.PK;
			invoiceHeader.Validation.ValidateJZ_OH_Supplier();
			AssertNoErrors(invoiceHeader.JZ_OH_SupplierInfo);
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			AssertNoErrors(invoiceHeader.JZ_OH_SupplierInfo);
		}

		public void TestValidationObjectIsNotNull()
		{
			AssertNotNull(GetNewValidationProvider(Factory.New<JobComInvoiceHeader>()));
		}

		public void TestInvoiceAmountNotCheckedTwice()
		{
			InvoiceHeader.JZ_InvoiceNumber = "X";
			InvoiceHeader.JZ_RX_NKInvoice_Currency = USDCurrency.RX_Code;
			InvoiceHeader.JZ_InvoiceAmount = -1m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = AUDCurrency.RX_Code;
			AssertEquals("JZ_InvoiceAmount should have an error", true, InvoiceHeader.JZ_InvoiceAmountInfo.HasMessageErrors());
		}

		public void TestCheckJZ_InvoiceAmount()
		{
			const string messageError = "Invoice Total must be greater than zero";
			InvoiceHeader.JZ_InvoiceNumber = "INV1";
			InvoiceHeader.JZ_InvoiceAmount = 0.0m;

			InvoiceHeader.JobDeclaration.IsAQISCertificateRequest = false;
			InvoiceHeader.Validation.ValidateJZ_InvoiceAmount();
			AssertHasMessageErrorContaining(InvoiceHeader.JZ_InvoiceAmountInfo, messageError);

			InvoiceHeader.JobDeclaration.IsAQISCertificateRequest = true;
			InvoiceHeader.Validation.ValidateJZ_InvoiceAmount();
			AssertHasMessageErrorContaining("IsAQISCertificateRequest is ignored", InvoiceHeader.JZ_InvoiceAmountInfo, messageError);

			InvoiceHeader.JZ_InvoiceAmount = 1.0m;
			InvoiceHeader.Validation.ValidateJZ_InvoiceAmount();
			AssertNoMessageErrorContaining(InvoiceHeader.JZ_InvoiceAmountInfo, messageError);
		}

		public void TestCheckJZ_InvoiceAmount_Quarantine()
		{
			const string messageError = "Invoice Total must be greater than zero";
			InvoiceHeader.JZ_InvoiceNumber = "INV1";
			InvoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			InvoiceHeader.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			InvoiceHeader.JZ_InvoiceAmount = 0.0m;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				InvoiceHeader.JobDeclaration.IsAQISCertificateRequest = false;
				InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
				InvoiceHeader.Validation.ValidateJZ_InvoiceAmount();
				AssertHasMessageErrorContaining(InvoiceHeader.JZ_InvoiceAmountInfo, messageError);

				InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = false;
				InvoiceHeader.Validation.ValidateJZ_InvoiceAmount();
				AssertHasMessageErrorContaining(InvoiceHeader.JZ_InvoiceAmountInfo, messageError);

				InvoiceHeader.JobDeclaration.IsAQISCertificateRequest = true;
				InvoiceHeader.Validation.ValidateJZ_InvoiceAmount();
				AssertNoMessageErrorContaining("Invoice Amount is not required when IsAQISCertificateRequest", InvoiceHeader.JZ_InvoiceAmountInfo, messageError);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				InvoiceHeader.JobDeclaration.IsAQISCertificateRequest = false;
				InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
				InvoiceHeader.Validation.ValidateJZ_InvoiceAmount();
				AssertHasMessageErrorContaining(InvoiceHeader.JZ_InvoiceAmountInfo, messageError);

				InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = false;
				InvoiceHeader.Validation.ValidateJZ_InvoiceAmount();
				AssertNoMessageErrorContaining("Invoice Amount is not required for NEXDOC when NOT ObtainExportCustomsPermit", InvoiceHeader.JZ_InvoiceAmountInfo, messageError);

				InvoiceHeader.JobDeclaration.IsAQISCertificateRequest = true;
				InvoiceHeader.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
				InvoiceHeader.Validation.ValidateJZ_InvoiceAmount();
				AssertNoMessageErrorContaining("Invoice Amount is not required when IsAQISCertificateRequest", InvoiceHeader.JZ_InvoiceAmountInfo, messageError);
			}
		}

		#region Implementation
		protected RefCurrency invalidCurrency;
		protected string invalidCurrencyCode = "XOF"; //in terms of AUCustoms

		RefCurrency fAUDCurrency;
		protected RefCurrency AUDCurrency
		{
			get
			{
				if (fAUDCurrency == null)
				{
					fAUDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia);
				}
				return fAUDCurrency;
			}
		}

		RefCurrency fUSDCurrency;
		protected RefCurrency USDCurrency
		{
			get
			{
				if (fUSDCurrency == null)
				{
					fUSDCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
				}
				return fUSDCurrency;
			}
		}

		protected void ClearAllExchangeRates()
		{
			RefExchangeRateCollection rates = new RefExchangeRateCollection(Factory, new ZQuery());
			rates.DeleteAll();
		}

		protected override JobComInvoiceHeaderValidation GetNewValidationProvider(JobComInvoiceHeader invoiceHeader)
		{
			return new ExportJobComInvoiceHeaderValidation(invoiceHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			invalidCurrency = RefCurrency.LoadFromCurrencyCode(Factory, invalidCurrencyCode);
			if (invalidCurrency == null)
			{
				invalidCurrency = Factory.New<RefCurrency>();
				invalidCurrency.RX_Code = invalidCurrencyCode;
			}
			InvoiceHeader.JobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		}

		protected override Type GetTypeForTest()
		{
			return typeof(ExportJobComInvoiceHeaderValidation);
		}

		protected override Customs.Business.BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)base.GetInvoiceHeader();
			return invoiceHeader;
		}

		#endregion
	}
}
