using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// Updates Invoice Type on a Charge based on the Invoice Posting Option of the Debtor, 
	/// the Charge Code Group, Charge Code Type and Sell Currency.
	/// Uses the Invoice Type specified on the Charge Code Type Ovverides if it is there.
	/// </summary>
	public class InvoiceTypeCalculator
	{
		public InvoiceTypeCalculator(BaseCharge charge)
		{
			this.Charge = charge;
		}

		readonly BaseCharge Charge;

		#region Invoice Type

		public void UpdateInvoiceType()
		{
			ZString resultingInvoiceType = "";

			if (Charge.SellAccount != null && Charge.ChargeCode != null)
			{
				var typeOverride = Job.GetChargeTypeInformation(Charge.ChargeCode, Charge.InvoicingJob);
				if (Charge.SellAccount.CompanyData.OB_ARCustomerSelfBillsRevenue)
				{
					resultingInvoiceType = InvoiceTypesList.Codes.SelfBillingInvoice;
				}
				else if (typeOverride != null && typeOverride.HasOveriddenInvoiceType)
				{
					resultingInvoiceType = typeOverride.AN_InvoiceType;
				}
				else if (Charge.Job != null)
				{
					string invoicePostingStyle = GetInvoicePostingStyle();

					string invoiceTypeOverride = null;
					if (Charge.InvoicingJob.JobType != null)
					{
						invoiceTypeOverride = Charge.InvoicingJob.JobType.GetOverriddenInvoiceType(Charge.InvoicingJob.PlugInData, Charge.ChargeCode, Charge.SellCurrency, invoicePostingStyle, Charge.JR_InvoiceType);
					}

					if (invoiceTypeOverride != null)
					{
						resultingInvoiceType = invoiceTypeOverride;
					}
					else
					{
						if (invoicePostingStyle != null)
						{
							resultingInvoiceType = GetInvoiceTypeForPostingStyle(invoicePostingStyle);
						}
					}
				}

				resultingInvoiceType = Charge.HasDeferredConfiguration ? (ZString)InvoiceTypeCalculationProvider.ConvertNonDeferredInvoiceTypeToDeferredOne(resultingInvoiceType) : resultingInvoiceType;
				typeOverride = null;
			}
			else
			{
				if (Charge.SellAccount == null && Charge.InvoicingJob != null && Charge.InvoicingJob.JobType != null)
				{
					resultingInvoiceType = Charge.InvoicingJob.JobType.GetInvoiceTypeWithNoDebtor(Charge.JR_InvoiceType);
				}
			}

			Charge.JR_InvoiceType = resultingInvoiceType;
		}

		public string GetInvoicePostingStyle()
		{
			if (Charge.SellAccount != null && Charge.ChargeCode != null)
			{
				var jobTypeCode = Charge.InvoicingJob.JobType != null ? Charge.InvoicingJob.JobType.Code : string.Empty;
				var serviceDirection = GetServiceDirectionCode();
				var mode = GetMode();

				return new OrgInvoiceRollupOrGroup.Loader(Charge.SellAccount, Charge.Branch, Charge.Department).GetInvoicePostingStyle(serviceDirection, Charge.InvoicingJob.TransportMode, mode, jobTypeCode);
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		string GetInvoiceTypeForPostingStyle(string invoicePostingStyle)
		{
			switch (invoicePostingStyle)
			{
				case InvoicePostingOptionsList.Codes.FinalInvoiceOnly:
					return InvoiceTypesList.Codes.FinalInvoice;

				case InvoicePostingOptionsList.Codes.DisbursementInvoiceOnly:
					return InvoiceTypesList.Codes.DisbursementInvoice;

				case InvoicePostingOptionsList.Codes.DisbursementAndFinal:
					return InvoiceTypeForDisbursementAndFinal;

				case InvoicePostingOptionsList.Codes.FreightAndFinal:
					return InvoiceTypeForFreightInForeignAndFinal;

				case InvoicePostingOptionsList.Codes.DisbursementFreightAndFinal:
					return InvoiceTypeForDisbursementFreightAndFinal;

				case InvoicePostingOptionsList.Codes.ForeignCurrencyInvoiceAndFinal:
					return InvoiceTypeForForeignCurrencyAndFinal;

				case InvoicePostingOptionsList.Codes.DisbursementInvoiceForeignAndFinal:
					return InvoiceTypeForDisbursementInvoiceForeignAndFinal;

				case InvoicePostingOptionsList.Codes.InvoicePerTaxCode:
					return InvoiceTypesList.Codes.InvoicePerTaxCode;

				case InvoicePostingOptionsList.Codes.DisbursementForeignAndFinal:
					return InvoiceTypeForDisbursementForeignAndFinal;

				case InvoicePostingOptionsList.Codes.DisbursementStandardAndFinal:
					return InvoiceTypeForDisbursementStandardAndFinal;

				case InvoicePostingOptionsList.Codes.DisbursementFreightForeignAndFinal:
					return InvoiceTypeForDisbursementFreightForeignAndFinal;

				case InvoicePostingOptionsList.Codes.DisbursementForeignOnly:
					return DisbursementInvoiceTypeForForeignOrLocalCurrency;

				case InvoicePostingOptionsList.Codes.DisbursementFreightAsDisbursementAndFinal:
					return InvoiceTypeForDisbursementFreightAsDisbursementAndFinal;

				case InvoicePostingOptionsList.Codes.DisbursementFreightAsForeignDisbursementAndStandardAndFinal:
					return InvoiceTypeForDisbursementFreightAsForeignDisbursementAndStandardAndFinal;

				default:
					return string.Empty;
			}
		}

		public string GetDefaultSellInvoiceCurrency()
		{
			var result = string.Empty;
			if (!Charge.JR_InvoiceType.IsEmpty && InvoiceTypeCalculationProvider.BillInLocalCurrency(Charge.JR_InvoiceType)
				&& Charge.SellAccount != null && Charge.InvoicingJob != null)
			{
				var jobTypeCode = Charge.InvoicingJob.JobType != null ? Charge.InvoicingJob.JobType.Code : string.Empty;
				var serviceDirection = GetServiceDirectionCode();
				var mode = GetMode();

				result = new OrgInvoiceRollupOrGroup.Loader(Charge.SellAccount, Charge.Branch, Charge.Department).GetInvoicePostingCurrency(serviceDirection, Charge.InvoicingJob.TransportMode, mode, jobTypeCode);
			}
			return result;
		}

		string GetMode()
		{
			string mode = Charge.InvoicingJob.TransportMode;
			string containerMode = Charge.InvoicingJob.ContainerMode;
			if (containerMode == OrgConstants.ModesForGroupOrSubTotal.Codes.FCL || containerMode == OrgConstants.ModesForGroupOrSubTotal.Codes.LCL)
			{
				mode = containerMode;
			}
			return mode;
		}

		string GetServiceDirectionCode()
		{
			return Charge.InvoicingJob != null ? Charge.InvoicingJob.ServiceDirection : OrgConstants.ServiceDirection.Code.Unknown;
		}

		#endregion

		#region Implementation

		string InvoiceTypeForDisbursementAndFinal
		{
			get { return Charge.IsDisbursementCharge ? InvoiceTypesList.Codes.DisbursementInvoice : InvoiceTypesList.Codes.FinalInvoice; }
		}

		string InvoiceTypeForDisbursementFreightAndFinal
		{
			get { return Charge.IsDisbursementCharge ? InvoiceTypesList.Codes.DisbursementInvoice : InvoiceTypeForFreightInForeignAndFinal; }
		}

		string InvoiceTypeForFreightInForeignAndFinal
		{
			get { return Charge.IsFreight && Charge.IsSellForeign ? InvoiceTypesList.Codes.FreightInvoice : InvoiceTypesList.Codes.FinalInvoice; }
		}

		string InvoiceTypeForDisbursementForeignAndFinal
		{
			get { return Charge.IsDisbursementCharge ? DisbursementInvoiceTypeForForeignOrLocalCurrency : InvoiceTypesList.Codes.FinalInvoice; }
		}

		string InvoiceTypeForDisbursementStandardAndFinal
		{
			get { return Charge.IsDisbursementCharge ? DisbursementInvoiceTypeForForeignOrLocalCurrency : InvoiceTypeForForeignCurrencyAndFinal; }
		}

		string InvoiceTypeForDisbursementFreightForeignAndFinal
		{
			get { return Charge.IsDisbursementCharge ? DisbursementInvoiceTypeForForeignOrLocalCurrency : (Charge.IsFreight ? InvoiceTypesList.Codes.FreightInvoice : InvoiceTypesList.Codes.FinalInvoice); }
		}

		string InvoiceTypeForDisbursementFreightAsDisbursementAndFinal
		{
			get { return (Charge.IsDisbursementCharge || Charge.IsFreight) ? InvoiceTypesList.Codes.DisbursementInvoice : InvoiceTypesList.Codes.FinalInvoice; }
		}

		string InvoiceTypeForDisbursementFreightAsForeignDisbursementAndStandardAndFinal
		{
			get { return (Charge.IsDisbursementCharge || Charge.IsFreight) ? DisbursementInvoiceTypeForForeignOrLocalCurrency : InvoiceTypeForForeignCurrencyAndFinal; }
		}

		string InvoiceTypeForDisbursementInvoiceForeignAndFinal
		{
			get {  return Charge.IsDisbursementCharge ? InvoiceTypesList.Codes.DisbursementInvoice : InvoiceTypeForForeignCurrencyAndFinal; }
		}

		string InvoiceTypeForForeignCurrencyAndFinal
		{
			get { return Charge.IsSellForeign ? InvoiceTypesList.Codes.ForeignCurrencyInvoice : InvoiceTypesList.Codes.FinalInvoice; }
		}

		string DisbursementInvoiceTypeForForeignOrLocalCurrency
		{
			get { return Charge.IsSellForeign ? InvoiceTypesList.Codes.DisbursementInForeignCurrency : InvoiceTypesList.Codes.DisbursementInvoice; }
		}

		#endregion
	}
}
