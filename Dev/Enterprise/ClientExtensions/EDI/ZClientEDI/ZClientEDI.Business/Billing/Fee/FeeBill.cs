using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Fee
{
	public class FeeBill : SystemBill
	{
		public FeeBill(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Charge Codes

		public override ZString GetAmountChargeCodeName(SystemUsage usage)
		{
			throw new System.InvalidOperationException();
		}

		public override ZString GetDiscountChargeCodeName(SystemUsage usage)
		{
			throw new System.InvalidOperationException();
		}

		public override void CreateRevenueBreakdown(ARInvoice invoice, ZDecimal signedProcessingFeePercentage)
		{
			var dateForExchangeRate = BillingInvoicingHelper.GetDateForExchangeRate(invoice);

			foreach (FeeSystemUsage systemUsage in SystemUsages.Where(x => x.IsBilled))
			{
				foreach (ClientLicenceFee licenceFee in systemUsage.Fees)
				{
					var preDiscount = licenceFee.L8_Amount;
					var currency = licenceFee.L8_RX_NKCurrency;
					var billed = invoice.Factory.New<EdiBilledUsage>();
					var processingFee = licenceFee.L8_IsDiscountable ? signedProcessingFeePercentage / 100m * preDiscount : 0m;
					var postDiscount = preDiscount + processingFee;
					billed.BU9_AC_AmountChargeCode = BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, licenceFee.L8_ChargeCode);
					if (preDiscount != postDiscount)
					{
						billed.BU9_AC_DiscountChargeCode = BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
					}
					billed.BU9_AH_Invoice = invoice.PK;
					billed.BU9_BillingModel = BillingConstants.PriceHeaderType.ODM;
					billed.BU9_LC = systemUsage.User.LicenceCompanyPK;
					billed.BU9_LCC = systemUsage.User.ClientCompanyPK;
					billed.BU9_LD = systemUsage.User.DatabasePK;
					billed.BU9_LocalAmountPostDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(postDiscount, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_LocalAmountPreDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(preDiscount, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_LocalProcessingAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(processingFee, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_PeriodStart = PeriodStart.Date;
					billed.BU9_PriceCurrency = currency;
					billed.BU9_TransactionAmountPostDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(postDiscount, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_TransactionAmountPreDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(preDiscount, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_TransactionProcessingAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(processingFee, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
					billed.BU9_UnitCount = 1;
					billed.BU9_UnitPrice = preDiscount;
					billed.BU9_UsageCode = SystemCode;
					billed.BU9_UsageSubCode = licenceFee.L8_Type;
				}
			}
		}

		#endregion

		#region Add Amount Line

		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			var feeTypes = EDIDataRegistry.Instance.LicenceFeeTypes.Value;

			foreach (FeeSystemUsage systemUsage in SystemUsages.Where(x => x.IsBilled))
			{
				var tax = new TaxGroup(systemUsage.InvoiceDelivery);
				foreach (ClientLicenceFee licenceFee in systemUsage.Fees)
				{
					ZString description = licenceFee.L8_DescriptionMultilingual;
					if (systemUsage.IsRemitTo)
					{
						description = "Remit: " + description;
					}

					const string DateFormat = "MMM yyyy";
					description += string.Format(CultureInfo.InvariantCulture, "\r\n{0}", systemUsage.PeriodStart.ToString(DateFormat, CultureInfo.InvariantCulture));
					if (licenceFee.L8_RenewalMonths != 1)
					{
						description += string.Format(CultureInfo.InvariantCulture, " to {0}",
							systemUsage.PeriodStart.AddMonths(licenceFee.L8_RenewalMonths - 1).ToString(DateFormat, CultureInfo.InvariantCulture));
					}

					var amount = !systemUsage.IsRemitTo ? (decimal)licenceFee.L8_Amount : -(licenceFee.L8_Amount);
					var taxDate = licenceFee.CalculateTaxDate(systemUsage.PeriodStart);
					var line = new BillLine(amount, tax, systemUsage.CurrencyCode, licenceFee.L8_ChargeCode, description, lines.Count, SystemCode, ZString.Empty, ZGuid.Empty, taxDate: taxDate);
					if (feeTypes.GetBoolFromCode(licenceFee.L8_Type))
					{
						line.IsProcessingFeeExempt = true;
					}
					lines.Add(line);
				}
			}
		}

		#endregion

		#region On Invoice Saving

		protected override void SetChargeableUsagesInvoiceCore(ARInvoice invoice)
		{
			foreach (FeeSystemUsage systemUsage in SystemUsages.Where(x => x.Billing != null))
			{
				ClientChargeableUsage[] chargeableUsages = null;
				if (systemUsage.ChargeableUsagePKs.Count > 0)
				{
					chargeableUsages = invoice.Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.PK, systemUsage.ChargeableUsagePKs));
				}

				foreach (ClientLicenceFee fee in systemUsage.Fees)
				{
					ClientChargeableUsage chargeableUsage = chargeableUsages != null ? chargeableUsages.FirstOrDefault(s => s.U1_Parent == fee.PK) : null;
					ZGuid chargeableUsagePk = chargeableUsage != null ? chargeableUsage.PK : ZGuid.Empty;
					CreateOrUpdateChargeableUsage(invoice, fee, chargeableUsagePk, systemUsage.PeriodStart, systemUsage.IsRemitTo);
				}
			}
		}

		internal static ClientChargeableUsage CreateOrUpdateChargeableUsage(ARInvoice invoice,
			ClientLicenceFee fee,
			ZGuid chargeableUsagePk,
			ZDateTime periodStart,
			bool isRemitTo)
		{
			ClientChargeableUsage usage = null;
			if (!chargeableUsagePk.IsEmpty)
			{
				usage = invoice.Factory.Load<ClientChargeableUsage>(chargeableUsagePk);
			}

			return CreateOrUpdateChargeableUsage(invoice, fee, usage, periodStart, isRemitTo);
		}

		internal static ClientChargeableUsage CreateOrUpdateChargeableUsage(ARInvoice invoice,
			ClientLicenceFee fee,
			ClientChargeableUsage usage,
			ZDateTime periodStart,
			bool isRemitTo)
		{
			if (usage == null)
			{
				usage = invoice.Factory.New<ClientChargeableUsage>();
			}
			else
			{
				usage = usage.GetInAnotherFactory(invoice.Factory);
			}

			if (!isRemitTo)
			{
				usage.U1_LC = fee.L8_LC;
			}
			else
			{
				var remitToOrg = invoice.Factory.Load<EDIOrgHeader>(fee.L8_OH_RemitToOrg);
				if (remitToOrg != null && remitToOrg.LicCompany != null)
				{
					usage.U1_LC = remitToOrg.LicCompany.PK;
				}
			}

			usage.U1_AH_Invoice = invoice.PK;
			usage.U1_Code = BillingConstants.BillingSystem.Fee;
			usage.U1_InvoicedUnitCount = 1;
			usage.U1_Parent = fee.PK;
			usage.U1_PeriodStart = periodStart;
			usage.U1_SubCode = fee.L8_Type;
			usage.U1_UnitCount = 1;
			usage.U1_UnitPrice = fee.L8_Amount;
			usage.U1_LD = fee.L8_LD;
			usage.U1_UpdateTime = ZDateTime.UtcNow;
			return usage;
		}

		#endregion
	}
}

