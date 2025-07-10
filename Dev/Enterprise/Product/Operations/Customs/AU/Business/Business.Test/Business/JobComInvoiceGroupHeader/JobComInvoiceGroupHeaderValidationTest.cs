using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceGroupHeaderValidationTest : BaseJobComInvoiceGroupHeaderValidationTest
	{
		public void TestMessageValidationType()
		{
			JobComInvoiceGroupHeaderValidation validation = new JobComInvoiceGroupHeaderValidation(testInvoiceGroupHeader);
			AssertEquals("External message validation type", typeof(ExternalMessageValidation), validation.MessageValidation.GetType());
		}

		public void TestApportionGroupChargeWhenNoInvoices()
		{
			testInvoiceGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 100, USD.RX_Code);
			AssertEquals("An amount should make no notification if there are no Invoices entered yet", false, testInvoiceGroupHeader.Charges[0].J7_ChargeTypeInfo.HasNotifications());
		}

		public void TestMandatoryChargeValidationGetsRefreshedWhenGroupChargeIsEntered()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceHeader invoiceHeader = testInvoiceGroupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostFreightWithAmpersand;

			AssertEquals("OFT is required", true, invoiceHeader.JZ_IncoTermInfo.HasNotifications());

			testInvoiceGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 120, JobDeclaration.LocalCurrencyConstantCode);
			invoiceHeader.Validation.ValidateJZ_IncoTerm();
			testJobDeclaration.ResumeApportionment();
			AssertEquals("OFT is now there", false, invoiceHeader.JZ_IncoTermInfo.HasNotifications());
		}

		public void TestValidateExcludedGroupCharge()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceHeader invoiceHeader = testInvoiceGroupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostFreightWithAmpersand;

			AssertEquals("OFT is required", true, invoiceHeader.JZ_IncoTermInfo.HasNotifications());

			testJobDeclaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 120, JobDeclaration.LocalCurrencyConstantCode);
			invoiceHeader.Validation.ValidateJZ_IncoTerm();
			testJobDeclaration.ResumeApportionment();
			AssertEquals("OFT is now there", false, invoiceHeader.JZ_IncoTermInfo.HasNotifications());

			BaseJobComInvHeaderCharge oNS = invoiceHeader.GroupCharges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100, JobDeclaration.LocalCurrencyConstantCode);
			oNS.J7_IsIncludedInITOT = true;
			AssertHasMessageError(oNS.J7_IsIncludedInITOTInfo, "C&F cannot include this charge in lines.");

			oNS.J7_IsIncludedInITOT = false;
			AssertNoMessageError(oNS.J7_IsIncludedInITOTInfo, "C&F cannot include this charge in lines.");
		}

		public void TestGroupLandingChargesInvalidWhereNoInvoicesAboveCIF()
		{
			testJobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testJobDeclaration.AutoCreateChargesBasedOnIncoTerm = false;
			testJobDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceHeader testInvoiceHeader = testInvoiceGroupHeader.JobComInvoiceHeaders.AddNew();
			testInvoiceGroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100, JobDeclaration.LocalCurrencyConstantCode);

			testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("No invoice to apportion LCH", 0, testInvoiceHeader.GroupCharges.Count);

			testInvoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.LandedIntoStore;
			testInvoiceHeader.JZ_InvoiceAmount = 1000;
			testInvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			testJobDeclaration.ResumeApportionment();
			AssertEquals("Group Landing Charges is relavant", 1, testInvoiceHeader.GroupCharges.Count);
		}

		#region Implementation

		protected RefCurrency fUSD;
		protected RefCurrency USD
		{
			get
			{
				if (fUSD == null)
				{
					fUSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
					SetCurrencyValid(fUSD);
				}
				return fUSD;
			}
		}

		protected override Customs.Business.JobComInvoiceGroupHeaderValidation GetNewValidationProvider(JobComInvoiceGroupHeader groupHeader)
		{
			return new JobComInvoiceGroupHeaderValidation(groupHeader);
		}

		void SetCurrencyValid(RefCurrency currency)
		{
			ZQuery filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, currency.RX_Code);
			var rate = Factory.LoadTop1<RefExchangeRate>(filter);
			rate.RE_ExpiryDate = ZDateTime.Today;
		}

		#endregion
	}
}
