using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business
{
	public static class AccountingExtension
	{
		#region GetTaxCalculationParameters

		public static AccChargeTaxOverrideMatcher.TaxCalculationParameters GetTaxCalculationParameters(this Job job)
		{
			// Currently Singapore Customs Declarations do not have Job Invoicing support.
			// Therefore stand-alone declarations are done through the shipment system.
			// This causes the tax overrides to not work correctly, as the shipment tax override is chosen
			// instead of the brokerage tax override.
			//
			// To solve this in the short-term, the below code manually uses the brokerage tax override
			// based on the department specified on the JobHeader. If the department is a customs department,
			// this logic is used.
			JobInvoicingConsumerType consumerType = job.JobType;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Singapore &&
				job.Department != null &&
				job.Department.GE_CustomsBrokerage &&
				consumerType == JobInvoicingConsumerTypes.Shipment)
			{
				consumerType = JobInvoicingConsumerTypes.Brokerage;
			}

			ILocation origin = job.PlugInData?.InvoicingSupporter?.Origin;
			ILocation destination = job.PlugInData?.InvoicingSupporter?.Destination;
			var customsStatus = job.PlugInData?.InvoicingSupporter?.CustomsEntryNumberType ?? ZString.Empty;
			var communityTransitStatus = job.PlugInData?.InvoicingSupporter?.CommunityTransitStatus ?? ZString.Empty;

			var incoTermInfo = job.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue);
			var incoTerm = incoTermInfo != null && incoTermInfo.InfoType == PaymentTermType.Incoterm ? incoTermInfo.Value : null;

			var jobTypeCode = consumerType != null ? consumerType.Code : string.Empty;

			return new AccChargeTaxOverrideMatcher.TaxCalculationParameters
			{
				IncoTerm = incoTerm,
				JobType = jobTypeCode,
				Direction = job.MovementDirection,
				TransportMode = job.TransportMode,
				Origin = origin,
				Destination = destination,
				CustomsStatus = customsStatus,
				CommunityTransitStatus = communityTransitStatus,
				FixedPlaceOfSupply = job.FixedPlaceOfSupply,
			};
		}

		public static AccChargeTaxOverrideMatcher.TaxCalculationParameters GetTaxCalculationParameters(this IJobCostingPlugIn consol)
		{
			return new AccChargeTaxOverrideMatcher.TaxCalculationParameters
			{
				CostOrSell = CostSell.Cost,
				JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code,
				Direction = consol.CostSupporter.Direction,
				TransportMode = consol.TransportMode,
				Origin = consol.LoadPort,
				Destination = consol.DischargePort,
				Branch = GlbBranch.CurrentBranch,
			};
		}

		#endregion

		public static TFilter WithMaxLengthOf<TFilter>(this TFilter filter, SchemaColumn column) where TFilter : ModuleFilter
		{
			filter.MaxLength = column.MaxLength;
			return filter;
		}

		public static TFilter WithCategory<TFilter>(this TFilter filter, FilterCategory category) where TFilter : ModuleFilter
		{
			filter.Category = category;
			return filter;
		}

		public static ChargeComparison Compare(this IReadOnlyCollection<JobPaymentBasis> thisPaymentBasisCollection, IReadOnlyCollection<JobPaymentBasis> otherPaymentBasisCollection)
		{
			if (!thisPaymentBasisCollection.Any() || !otherPaymentBasisCollection.Any())
			{
				return ChargeComparison.Unknown;
			}

			var hasSameRate = thisPaymentBasisCollection.EqualIgnoringOrder(otherPaymentBasisCollection, JobPaymentBasisComparer.SameRate);
			var hasSameChargeable = thisPaymentBasisCollection.EqualIgnoringOrder(otherPaymentBasisCollection, JobPaymentBasisComparer.SameChargeable);

			if (hasSameRate)
			{
				return hasSameChargeable ? ChargeComparison.Same : ChargeComparison.SameRateButDifferentChargeable;
			}

			return hasSameChargeable ? ChargeComparison.SameChargeableButDifferentRate : ChargeComparison.Different;
		}

		public static string GetExceptionMessageAndStackTrace(this Exception ex)
		{
			return ex != null
				? string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{1}{3}{1}{4}", ex.Message, System.Environment.NewLine, ex.GetType().FullName, ex.StackTrace, GetExceptionMessageAndStackTrace(ex.InnerException))
				: string.Empty;
		}

		public static string ToMTDCompliantFormat(this ZDate date)
			=> date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

		public class InvoicesAndMessage
		{
			public InvoicesAndMessage(IEnumerable<InvoicingBase> invoices, string message)
			{
				Invoices = invoices;
				Message = message;
			}

			public IEnumerable<InvoicingBase> Invoices { get; }
			public string Message { get; }
		}

		public static InvoicesAndMessage GetInvoicingBasesThatCanNotBePrintedDueToCompliance(this IEnumerable<InvoicingBase> invoicesToPrint) =>
			GetTransactionsThatCannotBePrintedDueToCompliance
			(
				invoicesToPrint,
				Res.GetString("cc00bb61-a45e-4809-86a0-8b05abad4f65", "Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, hence re-printing of following transactions is not allowed through this module. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.")
			);

		public static InvoicesAndMessage GetTransactionsThatCannotBeMarkedAsNotPrintedDueToCompliance(this IEnumerable<InvoicingBase> invoicesToPrint) =>
			GetTransactionsThatCannotBePrintedDueToCompliance
			(
				invoicesToPrint,
				Res.GetString("0d163ad6-7cf2-4a79-9822-e336b0ec74a8", "You cannot mark following transaction(s) as 'Not Printed', as they were printed at least once before. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.")
			);

		static InvoicesAndMessage GetTransactionsThatCannotBePrintedDueToCompliance(this IEnumerable<InvoicingBase> invoicesToPrint, string message)
		{
			var errorMessage = string.Empty;
			var invoicesThatCannotBePrinted = GetInvoicingBasesThatCannotBePrinted(invoicesToPrint);
			if (invoicesThatCannotBePrinted.Any())
			{
				var errorMessageBuilder = new ZStringBuilder(message);
				invoicesThatCannotBePrinted
					.Select(r => FormattableString.Invariant($"{r.AH_Ledger} {r.AH_TransactionType} {r.AH_TransactionNum}"))
					.ForEach(s => errorMessageBuilder.Append(s));
				errorMessage = errorMessageBuilder.ToStringWithNewLineBetweenAppends();
			}
			return new InvoicesAndMessage(invoicesThatCannotBePrinted, errorMessage);
		}

		static InvoicingBase[] GetInvoicingBasesThatCannotBePrinted(IEnumerable<InvoicingBase> invoicesToPrint)
		{
			if (invoicesToPrint?.Any() ?? false)
			{
				return invoicesToPrint.OfType<InvoicingBase>().Where(i => !i.CheckCanPrintPostedInvoicingBase().Result).ToArray();
			}
			return Enumerable.Empty<InvoicingBase>().ToArray();
		}

		public static long NextLong(this Random self, long min, long max)
		{
			var buf = new byte[sizeof(ulong)];
			self.NextBytes(buf);
			ulong n = BitConverter.ToUInt64(buf, 0);

			double normalised = n / (ulong.MaxValue + 1.0);

			double range = (double)max - min;
			return (long)(normalised * range) + min;
		}

		public static string TrimToFit(this string text, int maxLength, string trimMessage = "...trimmed to fit")
		{
			return (maxLength > 0 && text.Length > maxLength) ? (text.Substring(0, maxLength - trimMessage.Length) + trimMessage) : text;
		}

		public static ConfigurationMatcherHelper.ConfigurationMatcherParameters GetConfigurationMatcherParameters(this Job job, CostSell costOrSell)
		{
			return new ConfigurationMatcherHelper.ConfigurationMatcherParameters
			{
				CostOrSell = costOrSell,
				JobType = job.JobType?.Code ?? string.Empty,
				Direction = job.MovementDirection,
				TransportMode = job.TransportMode,
				Origin = job.PlugInData?.InvoicingSupporter?.Origin,
				Destination = job.PlugInData?.InvoicingSupporter?.Destination,
				Branch = GlbBranch.CurrentBranch,
			};
		}

		public static ConfigurationMatcherHelper.ConfigurationMatcherParameters GetConfigurationMatcherParameters(this IJobCostingPlugIn consol, CostSell costOrSell)
		{
			return new ConfigurationMatcherHelper.ConfigurationMatcherParameters
			{
				CostOrSell = costOrSell,
				JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code,
				Direction = consol.CostSupporter.Direction,
				TransportMode = consol.TransportMode,
				Origin = consol.LoadPort,
				Destination = consol.DischargePort,
				Branch = GlbBranch.CurrentBranch,
			};
		}
	}
}
