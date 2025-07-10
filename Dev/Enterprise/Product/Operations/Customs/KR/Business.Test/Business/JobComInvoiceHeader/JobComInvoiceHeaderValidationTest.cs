using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		protected override Type GetTypeForTest()
		{
			return typeof(JobComInvoiceHeaderValidation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
		}
		JobComInvoiceHeader invoice;

		public void TestJZ_InvoiceAmount()
		{
			invoice.JobDeclaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			invoice.JZ_InvoiceAmount = -1;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.ValueCannotBeNegative);
			invoice.JZ_InvoiceAmount = 0;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.ValueCannotBeZero);
			invoice.JZ_InvoiceAmount = 1;
			AssertNoMessageErrors(invoice.JZ_InvoiceAmountInfo);

			invoice.JobDeclaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			invoice.JZ_InvoiceAmount = -1;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.ValueCannotBeNegative);

			invoice.JobDeclaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			invoice.JZ_InvoiceAmount = -1;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceAmountInfo, MandatoryValidation.ValueCannotBeNegative);
			invoice.JZ_InvoiceAmount = 0;
			AssertNoMessageErrors(invoice.JZ_InvoiceAmountInfo);
			invoice.JZ_InvoiceAmount = 1;
			AssertNoMessageErrors(invoice.JZ_InvoiceAmountInfo);
		}

		public override void TestValidateJZ_OH_Buyer()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			Assert("Initially no errors", !invoice.JZ_OH_BuyerInfo.HasErrors());
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			invoice.Validation.ValidateJZ_OH_Buyer();
			Assert("In error", invoice.JZ_OH_BuyerInfo.HasErrors());

			invoice.JZ_JE = declaration.PK;
			invoice.Validation.ValidateJZ_OH_Buyer();
			Assert("No error when attached to a declaration", !invoice.JZ_OH_BuyerInfo.HasErrors());
		}

		protected override BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			invoice.JobDeclaration.JE_MessageType = ZString.Empty;
			return invoice;
		}

		public override void TestValidateAbsenceOfOFTOrONS()
		{
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = invoice.JobDeclaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = IncotermList.Codes.FreeOnBoard;
			invoice.RunPreSaveValidation();
			AssertEquals(false, invoice.JZ_Calc_CIFAmountInfo.HasNotifications());
		}

		public void TestJZ_InvoiceCurrExRate()
		{
			RefCurrency usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			var resultUSD = Factory.New<RefExchangeRate>();
			resultUSD.RE_GC = GlbCompany.CurrentCompany.PK;
			resultUSD.RE_RX_NKExCurrency = usd.RX_Code;
			resultUSD.RE_StartDate = ZDateTime.Today;
			resultUSD.RE_ExpiryDate = ZDateTime.Today;
			resultUSD.RE_SellRate = 0.8m;
			resultUSD.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			RefCurrency isk = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Iceland);
			var resultISK = Factory.New<RefExchangeRate>();
			resultISK.RE_GC = GlbCompany.CurrentCompany.PK;
			resultISK.RE_RX_NKExCurrency = isk.RX_Code;
			resultISK.RE_StartDate = ZDateTime.Today;
			resultISK.RE_ExpiryDate = ZDateTime.Today;
			resultISK.RE_SellRate = 0.8m;
			resultISK.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "86420");

			declaration.JE_ProcedureType = "E";
			invoice.JZ_InvoiceCurrExRate = 0;
			AssertNoMessageErrors(invoice.JZ_InvoiceCurrExRateInfo);

			declaration.JE_ProcedureType = "B";
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertNoMessageErrors(invoice.JZ_InvoiceCurrExRateInfo);

			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceCurrExRateInfo, ExtensionMethods.ExchangeRatePublishedMessage);

			invoice.JZ_InvoiceCurrExRate = 100;
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceCurrExRateInfo, ExtensionMethods.ExchangeRatePublishedMessage);

			declaration.JE_ProcedureType = "E";
			invoice.Validation.ValidateJZ_InvoiceCurrExRate();
			AssertNoMessageErrors(invoice.JZ_InvoiceCurrExRateInfo);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			invoice.JZ_InvoiceCurrExRate = 0;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceCurrExRateInfo, ExtensionMethods.ExchangeRatePublishedMessage);

			invoice.JZ_InvoiceCurrExRate = 100;
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceCurrExRateInfo, ExtensionMethods.ExchangeRatePublishedMessage);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			invoice.JZ_InvoiceCurrExRate = 0;
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceCurrExRateInfo, ExtensionMethods.ExchangeRatePublishedMessage);

			invoice.JZ_InvoiceCurrExRate = 100;
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceCurrExRateInfo, ExtensionMethods.ExchangeRatePublishedMessage);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Iceland;
			invoice.JZ_InvoiceCurrExRate = 0;
			AssertHasMessageErrorContaining(invoice.JZ_InvoiceCurrExRateInfo, ExtensionMethods.ExchangeRateNonPublishedMessage);

			invoice.JZ_InvoiceCurrExRate = 100;
			AssertNoMessageErrorContaining(invoice.JZ_InvoiceCurrExRateInfo, ExtensionMethods.ExchangeRateNonPublishedMessage);
		}

		public void TestTotalInvoiceLinesWeightInKG()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			CombineAssertions("Check TotalInvoiceLinesWeightInKG", () =>
			{
				invoiceLine.JI_Weight = -1m;
				AssertEquals(-1m, invoice.TotalInvoiceLinesWeightInKG);
				invoice.Validation.ValidateAll();
				AssertHasMessageErrorContaining(invoice.TotalInvoiceLinesWeightInKGInfo, "Please enter a 'Total Gross Weight' greater than 0.");

				invoiceLine.JI_Weight = 0m;
				AssertEquals(0m, invoice.TotalInvoiceLinesWeightInKG);
				invoice.Validation.ValidateAll();
				AssertHasMessageErrorContaining(invoice.TotalInvoiceLinesWeightInKGInfo, "Please enter a 'Total Gross Weight' greater than 0.");
			});
		}

		public void TestJZ_ValuationCode()
		{
			invoice.JobDeclaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			AssertNotWorkIfInvalidCodeOrEmpty();

			invoice.JobDeclaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertNotWorkIfInvalidCodeOrEmpty();

			invoice.JobDeclaration.JE_MessageType = KRJobMessageTypeList.Codes.Carnet;
			AssertNotWorkIfInvalidCodeOrEmpty();

			invoice.JobDeclaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			AssertWorkIfInvalidCodeOrEmpty();

			invoice.JobDeclaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertNotWorkIfInvalidCodeOrEmpty();

			void AssertNotWorkIfInvalidCodeOrEmpty()
			{
				invoice.JZ_ValuationCode = ZString.Empty;
				AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);

				invoice.JZ_ValuationCode = "XX";
				AssertHasMessageErrorContaining(invoice.JZ_ValuationCodeInfo, ListValidation.InvalidCodeMessageError);
			}

			void AssertWorkIfInvalidCodeOrEmpty()
			{
				invoice.JZ_ValuationCode = ZString.Empty;
				AssertHasMessageErrorContaining(invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				invoice.JZ_ValuationCode = "XX";
				AssertHasMessageErrorContaining(invoice.JZ_ValuationCodeInfo, ListValidation.InvalidCodeMessageError);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
				AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;
				AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodThree;
				AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;
				AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourB;
				AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodFive;
				AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);

				invoice.JZ_ValuationCode = ValuationCodeList.Codes.MethodSix;
				AssertNoMessageErrors(invoice.JZ_ValuationCodeInfo);
			}
		}
	}
}
