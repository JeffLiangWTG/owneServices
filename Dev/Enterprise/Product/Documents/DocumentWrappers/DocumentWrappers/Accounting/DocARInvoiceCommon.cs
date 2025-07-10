using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.Invoicing;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.InvoiceDocLineRollUpper;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;
using Enterprise.DocumentWrappers.ProcessManagement;
using Enterprise.DocumentWrappers.Warehouse;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US.ISF;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public abstract partial class DocARInvoiceCommon : DocARBaseInvoice
	{
		#region Construction

		protected DocARInvoiceCommon(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
				: base(invoicingBase, factoryToWrap)
		{
		}

		#endregion

		#region Description Height / Width

		public ZInt DescriptionHeight
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionHeight, 1); }
		}

		public ZInt DescriptionWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.DescriptionWidth, 1); }
		}

		#endregion

		#region Invoice Lines

		public ZString GroupOrSubtotal
		{
			get
			{
				var (isValid, serviceDirection, transportMode, mode, jobTypes) = GetDirectionTransportModeJobTypes();
				return isValid
					? new OrgInvoiceRollupOrGroup.Loader(InvoicingBase).GetGroupOrSubTotal(serviceDirection, transportMode, mode, jobTypes)
					: ZString.Empty;
			}
		}

		public ZString GroupOrSubtotalStyle
		{
			get
			{
				var (isValid, serviceDirection, transportMode, mode, jobTypes) = GetDirectionTransportModeJobTypes();
				return isValid
					? new OrgInvoiceRollupOrGroup.Loader(InvoicingBase).GetGroupOrSubtotalStyle(serviceDirection, transportMode, mode, jobTypes)
					: ZString.Empty;
			}
		}

		(bool, ZString, ZString, ZString, ZString[]) GetDirectionTransportModeJobTypes()
		{
			if (InvoicingOrgHeader != null)
			{
				if (GenericInvoicingJob != null)
				{
					return
					(
						true,
						GetServiceDirection(GenericInvoicingJob.Origin, GenericInvoicingJob.Destination),
						GenericInvoicingJob.TransportMode,
						Mode,
						new[] { GenericInvoicingJob.JobType }
					);
				}
				else if (JobHeader == null && Consol != null)
				{
					return
					(
						true,
						OrgConstants.ServiceDirection.Code.All,
						OrgConstants.ModesForGroupOrSubTotal.Codes.All,
						OrgConstants.ModesForGroupOrSubTotal.Codes.All,
						new[] { new ZString(JobInvoicingConsumerTypes.Consol.Code), new ZString(JobInvoicingConsumerTypes.ForwardingConsol.Code) }
					);
				}
				else if (JobHeader == null && Consol == null)
				{
					return
					(
						true,
						OrgConstants.ServiceDirection.Code.All,
						OrgConstants.ModesForGroupOrSubTotal.Codes.All,
						OrgConstants.ModesForGroupOrSubTotal.Codes.All,
						new[] { new ZString(OrgInvoiceRollupOrGroupLookups.JobType_List.NonJobRelated.Code) }
					);
				}
			}

			return (false, ZString.Empty, ZString.Empty, ZString.Empty, Array.Empty<ZString>());
		}

		protected override ZString GetInvoiceLineDisplayOption()
		{
			ZString result = base.GetInvoiceLineDisplayOption();

			if (result.IsEmpty && InvoicingOrgHeader != null)
			{
				if (JobHeader == null && Consol != null)
				{
					result = new OrgInvoiceRollupOrGroup.Loader(TransactionHeader).GetInvoiceLineDisplayOption(OrgConstants.ServiceDirection.Code.All,
							OrgConstants.ModesForGroupOrSubTotal.Codes.All,
							OrgConstants.ModesForGroupOrSubTotal.Codes.All,
							JobInvoicingConsumerTypes.Consol.Code, JobInvoicingConsumerTypes.ForwardingConsol.Code);
				}
			}

			return result;
		}

		public DocARInvoiceLineCollection Lines
		{
			get
			{
				if (InvoiceLineCollection == null)
				{
					InvoiceLineCollection = DocARInvoiceLineCollection.New(InvoicingBase.Factory);
					foreach (InvoicingLineBase line in InvoicingBase.Lines)
					{
						DocARInvoiceLine invoiceLineToAdd = Factory.GetCachedValue(line.PK.ToStringKey(), delegate
						{ return DocARInvoiceLine.New(line, Factory); });
						InvoiceLineCollection.Add(invoiceLineToAdd);
						invoiceLineToAdd.HeaderOrganisation = AccountOrg;
						invoiceLineToAdd.HeaderCurrency = Currency;
					}
					InvoiceLineCollection.Sort("Sequence", ListSortDirection.Ascending);
				}
				return InvoiceLineCollection;
			}
		}
		DocARInvoiceLineCollection InvoiceLineCollection;

		protected override DocARInvoiceLineCollection GetLinesToFormat()
		{
			if ((!IsPeriodicInvoice || InvoiceLineByNON.Any()) &&
				(GroupOrSubtotal == OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical || GroupOrSubtotal == OrgConstants.GroupOrSubTotalCharges.Code.Sequence))
			{
				return GroupAndSortLines(Lines);
			}
			else
			{
				return Lines;
			}
		}

		public DocARInvoiceLineCollection LinesForInvoice
		{
			get
			{
				return LinesForInvoiceCore();
			}
		}

		public DocTransactionHeaderCollection MultipleInstallments
		{
			get
			{
				if (fMultipleInstallments == null)
				{
					fMultipleInstallments = GetMultipleInstallments();
				}
				return fMultipleInstallments;
			}
		}

		DocTransactionHeaderCollection fMultipleInstallments;

		public DocInvoiceHasChargeCodeCollection InvoiceHasChargeCode
		{
			get
			{
				if (fInvoiceHasChargeCode == null)
				{
					fInvoiceHasChargeCode = new DocInvoiceHasChargeCodeCollection(LinesForInvoice, Factory);
				}
				return fInvoiceHasChargeCode;
			}
		}

		DocInvoiceHasChargeCodeCollection fInvoiceHasChargeCode;

		protected virtual DocARInvoiceLineCollection LinesForInvoiceCore()
		{
			return GroupAndSortLines(Lines);
		}

		protected DocTransactionHeaderCollection GetMultipleInstallments()
		{
			var result = new DocTransactionHeaderCollection(InvoicingBase.Factory);

			if (InvoicingBase.AH_InvoiceTerm == Core.Constants.InvoiceTerms.MultipleInstallments)
			{
				var journals = InvoicingBase.GetMultipleInstallmentsJournals(Core.Constants.TransactionCategory.Codes.InstalmentJournal);
				foreach (var journal in journals.OrderBy(x => x.AH_DueDate))
				{
					var installment = DocTransactionHeader.New(journal, InvoicingBase.Factory);
					result.Add(installment);
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "columnName")]
		public static class ColumnConstants
		{
			public const string Sequence = "Sequence";
			public const string AgreedPaymentMethod = "AgreedPaymentMethod";
			public const string InvoicedAmount = "InvoicedAmount";
			public const string BalanceDueAmount = "BalanceDueAmount";
			public const string DueDate = "DueDate";
		}

		public ZString MultipleInstallmentsSequencesColumn => GetMultipleInstallmentsColumnCore(ColumnConstants.Sequence);
		public ZString MultipleInstallmentsPaymentMethodsColumn => GetMultipleInstallmentsColumnCore(ColumnConstants.AgreedPaymentMethod);
		public ZString MultipleInstallmentsInvoicedAmountsColumn => GetMultipleInstallmentsColumnCore(ColumnConstants.InvoicedAmount);
		public ZString MultipleInstallmentsBalanceDueAmountsColumn => GetMultipleInstallmentsColumnCore(ColumnConstants.BalanceDueAmount);
		public ZString MultipleInstallmentsDueDatesColumn => GetMultipleInstallmentsColumnCore(ColumnConstants.DueDate);

		protected ZString GetMultipleInstallmentsColumnCore(string columnName)
		{
			if (MultipleInstallments.IsNullOrEmpty())
			{
				return ZString.Empty;
			}

			StringBuilder message = new StringBuilder();
			int sequence = 0;

			foreach (var installment in MultipleInstallments.Cast<DocTransactionHeader>().OrderBy(x => x.DueDate))
			{
				sequence++;
				switch (columnName)
				{
					case ColumnConstants.Sequence:
						message.AppendLine(sequence.ToString());
						break;
					case ColumnConstants.AgreedPaymentMethod:
						message.AppendLine(installment.TransactionARPaymentMethod);
						break;
					case ColumnConstants.InvoicedAmount:
						message.AppendLine(Currency.Code + " " + FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(installment.OSTotal, Currency));
						break;
					case ColumnConstants.BalanceDueAmount:
						message.AppendLine(Currency.Code + " " + FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(installment.OSOutstandingAmount, Currency));
						break;
					case ColumnConstants.DueDate:
						message.AppendLine(installment.DueDate.ToShortDateString());
						break;
				}
			}
			return message.ToString();
		}

		protected DocARInvoiceLineCollection GroupAndSortLines(DocARInvoiceLineCollection sortLines)
		{
			var invoiceDocRollUpperAndSorter = new InvoiceDocRollUpSorter(DocLineRollUpper, this, Factory);
			var result = invoiceDocRollUpperAndSorter.GroupAndSortLines(GroupOrSubtotal, GroupOrSubtotalStyle, sortLines, invoiceDocRollUpperAndSorter.TopLevelLineGroups);
			return result;
		}

		public DocTaxSummaryLineCollection TaxSummaryLines
		{
			get
			{
				var result = new DocTaxSummaryLineCollection(Factory);

				// for lines with tax rate other than type RVS, SUS, NOT, EXT, we want to group by (main tax rate, main tax type, extra tax rate, extra tax type).
				var taxSummaryLines1 = from DocARInvoiceLine line in Lines
									   where line.TaxRate != null
												   && line.TaxRate.Type != AccTaxRate.Types.ReverseRated
												   && line.TaxRate.Type != AccTaxRate.Types.Suspended
												   && line.TaxRate.Type != AccTaxRate.Types.NotReportable
												   && line.TaxRate.Type != AccTaxRate.Types.Exempt
												   && line.TaxRate.Type != AccTaxRate.Types.ReportableUnderBusinessTax
												   && line.TaxRate.Type != AccTaxRate.Types.RatedInAnotherCountry
									   orderby line.Sequence
									   group line by new { line.TaxRateAmount_Raw, line.TaxRate.Type, line.Line.AL_TaxExtraRateNumerator, line.Line.AL_TaxExtraRateDenominator, line.TaxRate.AccTaxRate.AT_ExtraTaxRateType } into taxRates
									   select new TaxSummaryLine
									   (
											   taxRates.Key.Type,
											   taxRates.Key.TaxRateAmount_Raw,
											   taxRates.Key.AT_ExtraTaxRateType,
											   taxRates.Key.AL_TaxExtraRateNumerator,
											   taxRates.Key.AL_TaxExtraRateDenominator,

											   taxRates.Sum(p => p.OSExTaxAmount),
											   taxRates.Sum(p => p.LocalExTaxAmount),

											   taxRates.Sum(p => p.TaxRate.ExtraType == AccTaxRate.ExtraTypes.RegionalTax || p.TaxRate.ExtraType == AccTaxRate.ExtraTypes.VATRemittedByCustomer ? p.OSGSTAmount : p.OSTaxAmount),
											   taxRates.Sum(p => p.CalculatedLocalTaxAmount),

											   taxRates.Sum(p => p.OSAmount),
											   taxRates.Sum(p => p.LocalAmount),

											   taxRates.Sum(p => p.OSExTaxAmount * (p.GetEffectiveExtraRate() / 100) * (p.TaxRate.AccTaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.VATRetention || p.TaxRate.AccTaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.VATRetentionFraction ? -1 : 1)),
											   taxRates.Sum(p => p.LocalExTaxAmount * (p.GetEffectiveExtraRate() / 100) * (p.TaxRate.AccTaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.VATRetention || p.TaxRate.AccTaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.VATRetentionFraction ? -1 : 1))
									   );

				// for lines with tax rate type RVS, SUS, NOT, EXT, we don't care about the main rate and extra tax, so we group by (main type) only.
				// i.e. we merge all the RVS into one line, all the SUS into one line, etc.
				var taxSummaryLines2 = from DocARInvoiceLine line in Lines
									   where line.TaxRate != null &&
												   (line.TaxRate.Type == AccTaxRate.Types.ReverseRated ||
													  line.TaxRate.Type == AccTaxRate.Types.Suspended ||
													  line.TaxRate.Type == AccTaxRate.Types.NotReportable ||
													  line.TaxRate.Type == AccTaxRate.Types.Exempt ||
													  line.TaxRate.Type == AccTaxRate.Types.ReportableUnderBusinessTax ||
													  line.TaxRate.Type == AccTaxRate.Types.RatedInAnotherCountry)
									   orderby line.Sequence
									   group line by new { line.TaxRate.Type } into taxRates
									   select new TaxSummaryLine
									   (
											   taxRates.Key.Type,
											   0,
											   "",
											   0,
											   1,

											   taxRates.Sum(p => p.OSExTaxAmount),
											   taxRates.Sum(p => p.LocalExTaxAmount),

											   taxRates.Sum(p => p.OSTaxAmount),
											   taxRates.Sum(p => p.CalculatedLocalTaxAmount),

											   taxRates.Sum(p => p.OSAmount),
											   taxRates.Sum(p => p.LocalAmount),

											   taxRates.Sum(p => p.OSExTaxAmount * (p.GetEffectiveExtraRate() / 100) * (p.TaxRate.AccTaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.VATRetention || p.TaxRate.AccTaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.VATRetentionFraction ? -1 : 1)),
											   taxRates.Sum(p => p.LocalExTaxAmount * (p.GetEffectiveExtraRate() / 100) * (p.TaxRate.AccTaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.VATRetention || p.TaxRate.AccTaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.VATRetentionFraction ? -1 : 1))
									   );

				var taxSummaryLines = taxSummaryLines1.Union(taxSummaryLines2);

				foreach (TaxSummaryLine line in taxSummaryLines)
				{
					result.Add(DocTaxSummaryLine.New(line, Factory));
					if (line.TaxExtraRateType == AccTaxRate.ExtraTypes.VATRemittedByCustomer)
					{
						result.Add(DocTaxSummaryLine.New(
							new TaxSummaryLine(
								AccTaxRate.ExtraTypes.VATRemittedByCustomer, 0, "",
								0, 1,
								0, 0,
								line.TotalExtraTaxAmountInOSCurrency, line.TotalExtraTaxAmountInLocalCurrency,
								0, 0,
								0, 0
							), Factory));
					}
				}

				return result;
			}
		}

		ZString TotalSummaryByTaxRateCore(ZBool useZeroAmountTaxDescriptionOverride)
		{
			var taxLineBuilder = new ZStringBuilder();
			var taxExemptLineBuilder = new ZStringBuilder();

			var groupedList = from DocARInvoiceLine line in Lines
							  where line.TaxRate != null
							  orderby line.Sequence
							  group line by new { line.TaxRateAmount_Raw, line.TaxRate.Type, line.TaxRate.AccTaxRate.AT_ExtraTaxRateType } into taxRates
							  select new
							  {
								  Rate = taxRates.Key.TaxRateAmount_Raw,
								  TaxType = taxRates.Key.Type,
								  TotalTaxAmount = taxRates.Sum(p => p.OSGSTAmount),
								  TotalAmount = taxRates.Sum(p => p.OSExTaxAmount),
								  ExtraTaxRates =
									  from e in taxRates
									  where e.TaxExtraRateAmount != 0
									  group e by new { e.TaxExtraRateAmount, e.TaxRate.AccTaxRate.AT_ExtraTaxRateType } into extraTaxRates
									  select new
									  {
										  TaxRate = extraTaxRates.Key.TaxExtraRateAmount,
										  TaxType = extraTaxRates.Key.AT_ExtraTaxRateType,
										  TotalTaxAmount = extraTaxRates.Sum(p => Utilities.Round(p.OSExTaxAmount * (p.GetEffectiveExtraRate() / 100), Currency.Decimals)),
										  TotalPrimaryTaxAmount = extraTaxRates.Sum(p => p.OSGSTAmount),
										  TotalExclTaxAmount = extraTaxRates.Sum(p => p.OSExTaxAmount)
									  }
							  };

			foreach (var taxRate in groupedList)
			{
				bool isNonZeroRatedTax = false;
				List<ZString> extraTaxLines = null;

				ZString taxAmountOrDescription, joiningString = new();

				var taxType = taxRate.TaxType.ToString().ToUpperInvariant();
				if (DocTaxRate.IsRatedTax(taxType) || taxType == AccTaxRate.Types.CapitalRated || taxType == AccTaxRate.Types.IntegratedGST)
				{
					if (taxRate.Rate != 0m || taxRate.ExtraTaxRates.Any())
					{
						isNonZeroRatedTax = true;
						taxAmountOrDescription = string.Format("{0}%", taxRate.Rate.ToString("0.00#"));
					}
					else
					{
						taxAmountOrDescription = useZeroAmountTaxDescriptionOverride ? (string)AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.Rated) : Res.GetString("d079f7f6-0305-48f4-8efd-bc7fb5178166", "Zero Rated");
					}
				}
				else if (taxType == AccTaxRate.Types.Exempt)
				{
					taxAmountOrDescription = useZeroAmountTaxDescriptionOverride ? (string)AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.Exempt) : Res.GetString("da39bb99-17e6-4904-850a-c3c4238695b2", "Exempt");
				}
				else if (taxType == AccTaxRate.Types.ReverseRated)
				{
					taxAmountOrDescription = useZeroAmountTaxDescriptionOverride ? (string)AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.ReverseRated) : Res.GetString("3e8e1212-316e-4641-895c-c3d70c67047c", "Tax Shifted");
				}
				else if (taxType == AccTaxRate.Types.Suspended)
				{
					taxAmountOrDescription = useZeroAmountTaxDescriptionOverride ? (string)AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.Suspended) : Res.GetString("5bd77847-6229-4d73-9e76-e436717352bc", "Suspended");
				}
				else if (taxType == AccTaxRate.Types.NotReportable)
				{
					taxAmountOrDescription = useZeroAmountTaxDescriptionOverride ? (string)AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.NotReportable) : Res.GetString("3c0e9e66-202c-49de-8f29-bf459f157e7b", "Not Applicable");
				}
				else if (taxType == AccTaxRate.Types.ReportableUnderBusinessTax)
				{
					taxAmountOrDescription = useZeroAmountTaxDescriptionOverride ? (string)AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.ReportableUnderBusinessTax) : Res.GetString("6eedf5e5-5d5c-4c56-8cdf-5f919c58bead", "Reportable under Business Tax");
				}
				else if (taxType == AccTaxRate.Types.RatedInAnotherCountry)
				{
					isNonZeroRatedTax = true;
					taxAmountOrDescription = string.Format("{0}%", taxRate.Rate.ToString("0.00#"));
				}
				else if (taxType == AccTaxRate.Types.ExcludedFromTheTaxBase)
				{
					taxAmountOrDescription = useZeroAmountTaxDescriptionOverride ? (string)AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.ExcludedFromTheTaxBase) : Res.GetString("bdc5d939-09ba-4bff-a5bd-806d3a9d45d3", "Excluded");
				}
				else
				{
					throw new NotSupportedException(string.Format("Unknown tax rate type {0} is not supported", taxRate.TaxType));
				}

				if (taxRate.ExtraTaxRates != null && taxRate.ExtraTaxRates.Any())
				{
					extraTaxLines = new List<ZString>();
					foreach (var extraTaxRate in taxRate.ExtraTaxRates)
					{
						decimal extraTotalBaseAmount = 0M;
						decimal extraTotalTaxAmount = 0M;
						ZStringBuilder extraTaxLineBuilder = new ZStringBuilder();
						if (AccTaxRate.ExtraTypes.VATRetention.Equals(extraTaxRate.TaxType.ToString(), StringComparison.InvariantCultureIgnoreCase) ||
								AccTaxRate.ExtraTypes.VATRetentionFraction.Equals(extraTaxRate.TaxType.ToString(), StringComparison.InvariantCultureIgnoreCase))
						{
							joiningString = Res.GetString("67de3a4e-92d6-4bfd-addc-6243965bc3ef", "minus");
							extraTotalTaxAmount = -1 * extraTaxRate.TotalTaxAmount;
							extraTotalBaseAmount = -1 * AccTaxRate.GetExtraTaxBaseAmount(extraTaxRate.TaxType, extraTaxRate.TaxRate, extraTaxRate.TotalTaxAmount, extraTaxRate.TotalPrimaryTaxAmount, extraTaxRate.TotalExclTaxAmount);
						}
						else
						{
							joiningString = Res.GetString("69c774b9-ba6e-447a-8e5a-f1d1abbd8e9d", "and");
							extraTotalTaxAmount = extraTaxRate.TotalTaxAmount;
							extraTotalBaseAmount = AccTaxRate.GetExtraTaxBaseAmount(extraTaxRate.TaxType, extraTaxRate.TaxRate, extraTaxRate.TotalTaxAmount, extraTaxRate.TotalPrimaryTaxAmount, extraTaxRate.TotalExclTaxAmount);
						}

						extraTaxLineBuilder.Append(string.Format("{0}@{1}={2}"
								, FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(extraTotalBaseAmount, Currency)
								, string.Format("{0}%", extraTaxRate.TaxRate.ToString("0.00#"))
								, FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(extraTotalTaxAmount, Currency)));
						extraTaxLines.Add(extraTaxLineBuilder.ToString());
					}
				}

				if (isNonZeroRatedTax)
				{
					taxLineBuilder.Append(string.Format(" {0}@{1}={2}"
							, FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(taxRate.TotalAmount, Currency)
							, taxAmountOrDescription
							, FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(taxRate.TotalTaxAmount, Currency)));

					if (taxType == AccTaxRate.Types.RatedInAnotherCountry)
					{
						taxLineBuilder.Append(Res.GetString("1d6f37a3-fb22-4ddd-8144-d61bfcc70f6a", " VAT of another country/region"));
					}

					if (extraTaxLines != null)
					{
						foreach (var extraTaxLine in extraTaxLines)
						{
							taxLineBuilder.Append(string.Format(" {0} {1}", joiningString, extraTaxLine.ToString()));
						}
						extraTaxLines = null;
					}

					taxLineBuilder.Append(",");
				}
				else
				{
					taxExemptLineBuilder.Append(string.Format(" {0} {1}", FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(taxRate.TotalAmount, Currency), taxAmountOrDescription));
				}
			}

			bool hasRatedTax = taxLineBuilder.Length > 0;
			bool hasExemptTax = taxExemptLineBuilder.Length > 0;

			return (hasRatedTax ? taxLineBuilder.ToString().TrimEnd(',') : string.Empty) +
						 (hasRatedTax && hasExemptTax ? System.Environment.NewLine : string.Empty) +
						 (hasExemptTax ? taxExemptLineBuilder.ToStringWithDelimiterBetweenAppends(",") : string.Empty);
		}

		public ZString TotalSummaryByTaxRate
		{
			get
			{
				return TotalSummaryByTaxRateCore(false);
			}
		}

		public ZString TotalSummaryByTaxRateWithDescriptionOverride
		{
			get
			{
				return TotalSummaryByTaxRateCore(true);
			}
		}

		public DocChargeSummaryLineCollection ChargeSummaryLinesBeforeAmendment
		{
			get
			{
				DocChargeSummaryLineCollection result = new DocChargeSummaryLineCollection(Factory);
				if (ParentTransaction != null)
				{
					DocARInvoiceLineCollection combinedLines = new DocARInvoiceLineCollection(Factory);

					InvoicingBase parent = Factory.Load<InvoicingBase>(ParentTransaction.TransactionPK);
					DocARInvoice docParentTransaction = DocARInvoice.New(parent, Factory);
					combinedLines.AddRange(docParentTransaction.Lines);

					InvoicingBase[] relatedToInvoices = docParentTransaction.RelatedToInvoices;
					foreach (InvoicingBase invoice in relatedToInvoices)
					{
						// only add lines from sibling transactions that occured before my amending/reversal date
						if (invoice.AH_PostDate < this.InvoicingBase.AH_PostDate)
						{
							DocARInvoice docInvoice = DocARInvoice.New(invoice, Factory);
							combinedLines.AddRange(docInvoice.Lines);
						}
					}

					result = PrepareChargeSummaryLineCollection(combinedLines);
				}
				return result;
			}
		}

		public DocChargeSummaryLineCollection ChargeSummaryLinesForAmendment
		{
			get
			{
				return PrepareChargeSummaryLineCollection(Lines);
			}
		}

		public DocChargeSummaryLineCollection ChargeSummaryLinesAfterAmendment
		{
			get
			{
				DocChargeSummaryLineCollection result = new DocChargeSummaryLineCollection(Factory);
				if (ParentTransaction != null)
				{
					DocARInvoiceLineCollection combinedLines = new DocARInvoiceLineCollection(Factory);

					InvoicingBase parentTransaction = Factory.Load<InvoicingBase>(ParentTransaction.TransactionPK);
					DocARInvoice docParentTransaction = DocARInvoice.New(parentTransaction, Factory);
					combinedLines.AddRange(docParentTransaction.Lines);

					InvoicingBase[] relatedToInvoices = docParentTransaction.RelatedToInvoices;
					foreach (InvoicingBase invoice in relatedToInvoices)
					{
						// add lines from sibling transactions that occured before my amending/reversal date together with my own transaction
						if (invoice.AH_PostDate < this.InvoicingBase.AH_PostDate)
						{
							DocARInvoice docInvoice = DocARInvoice.New(invoice, Factory);
							combinedLines.AddRange(docInvoice.Lines);
						}
						else if (invoice.PK == this.InvoicingBase.PK)
						{
							combinedLines.AddRange(this.Lines);
						}
					}

					result = PrepareChargeSummaryLineCollection(combinedLines);
				}
				return result;
			}
		}

		DocChargeSummaryLineCollection PrepareChargeSummaryLineCollection(DocARInvoiceLineCollection linesCollection)
		{
			DocChargeSummaryLineCollection result = new DocChargeSummaryLineCollection(Factory);
			var chargeSummaryLines = from DocARInvoiceLine line in linesCollection
									 orderby line.Sequence
									 group line by new { line.LineDescription, line.TaxRate } into linesGrouped
									 select new ChargeSummaryLine
									 (
											 linesGrouped.Key.TaxRate,
											 linesGrouped.First().TaxRateAmount_Raw,
											 linesGrouped.First().TaxExtraRateAmount,

											 linesGrouped.Key.LineDescription,

											 linesGrouped.Sum(p => p.OSExTaxAmount * p.Invoice.RevertSignForARCreditNote),
											 linesGrouped.Sum(p => p.LocalExTaxAmount * p.Invoice.RevertSignForARCreditNote),

											 linesGrouped.Sum(p => p.OSAmount * p.Invoice.RevertSignForARCreditNote),
											 linesGrouped.Sum(p => p.LocalAmount * p.Invoice.RevertSignForARCreditNote),

											 linesGrouped.Sum(p => p.OSTaxAmount * p.Invoice.RevertSignForARCreditNote),
											 linesGrouped.Sum(p => p.CalculatedLocalTaxAmount * p.Invoice.RevertSignForARCreditNote)
									 );

			foreach (ChargeSummaryLine line in chargeSummaryLines)
			{
				result.Add(DocChargeSummaryLine.New(line, Factory));
			}
			return result;
		}

		#region Vietnam Invoice

		public DocARInvoiceLineCollection LinesForVNInvoice
		{
			get
			{
				DocARInvoiceLineCollection invoiceLineCollection = DocARInvoiceLineCollection.New(InvoicingBase.Factory);

				if (Lines.Count <= MaxLinesForVNInvoice)
				{
					invoiceLineCollection.AddRange(Lines);
				}
				else
				{
					invoiceLineCollection.Add(DocARInvoiceLineForRollUp.New(Factory));
					DocARInvoiceLineForRollUp descriptionLine = DocARInvoiceLineForRollUp.New(Factory);
					descriptionLine.SetLineDescription(new ZString(Res.GetString("f0156ca2-4539-4252-a661-7c4c8d4f02bf", "Please refer to detailed attachment")));
					invoiceLineCollection.Add(descriptionLine);
				}

				while (invoiceLineCollection.Count < MaxLinesForVNInvoice)
				{
					invoiceLineCollection.Add(DocARInvoiceLineForRollUp.New(Factory));
				}

				return invoiceLineCollection;
			}
		}

		public ZBool TooManyLinesForVNInvoice
		{
			get { return Lines.Count > MaxLinesForVNInvoice; }
		}

		const int MaxLinesForVNInvoice = 5; // Template can only fit 5 Lines

		public ZString OSTotalInWordsVietnamese
		{
			get
			{
				var oSTotalInWordsVietnamese = new CurrencyToWords_VI_VN().ConvertToWords((double)OSTotal, Currency.Code);
				oSTotalInWordsVietnamese = oSTotalInWordsVietnamese.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture) + oSTotalInWordsVietnamese.Substring(1, oSTotalInWordsVietnamese.Length - 1);
				return oSTotalInWordsVietnamese;
			}
		}

		#endregion

		#region Indonesia Invoice

		public DocARInvoiceLineCollection LinesForIDInvoice
		{
			get
			{
				DocARInvoiceLineCollection invoiceLineCollection = DocARInvoiceLineCollection.New(InvoicingBase.Factory);

				foreach (DocARInvoiceLine line in Lines)
				{
					if (line.GSTVAT != 0m)
					{
						invoiceLineCollection.Add(line);
					}
				}

				return invoiceLineCollection;
			}
		}

		public ZDecimal TotalLocalExTaxForIDInvoice
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoiceLine line in LinesForIDInvoice)
				{
					result += line.CalculatedLocalExTaxAmount;
				}
				return result;
			}
		}

		public ZDecimal TotalLocalTaxForIDInvoice
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoiceLine line in LinesForIDInvoice)
				{
					result += line.CalculatedLocalTaxAmount;
				}
				return result;
			}
		}

		public ZDecimal TotalLocalTax_Raw
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoiceLine line in Lines)
				{
					result += line.LocalTaxAmount_Raw;
				}
				return Utilities.Round(result, GlbCompany.CurrentCompany.GetLocalDecimals());
			}
		}

		public ZDecimal TotalLocalInvoiceAmount_Raw
		{
			get
			{
				return InvoiceLocalSubTotal + TotalLocalTax_Raw;
			}
		}

		public ZString TotalLocalInvoiceAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalLocalInvoiceAmount, Currency);
			}
		}

		public ZString TotalLocalInvoiceAmountRawFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalLocalInvoiceAmount_Raw, Currency);
			}
		}

		public DocIDInvoiceTaxInfoCollection IDInvoiceTaxInfoSummary
		{
			get
			{
				DocIDInvoiceTaxInfoCollection collection = new DocIDInvoiceTaxInfoCollection(Factory);
				SortedDictionary<ZDecimal, IDInvoiceTaxInfo> taxInfo = new SortedDictionary<ZDecimal, IDInvoiceTaxInfo>();

				foreach (DocARInvoiceLine line in LinesForIDInvoice)
				{
					IDInvoiceTaxInfo value;
					var taxRate = line.TaxRateAmount_Raw;
					if (taxInfo.TryGetValue(taxRate, out value))
					{
						value.TotalLocalExTaxAmount += line.CalculatedLocalExTaxAmount;
						value.TotalLocalTaxAmount += line.CalculatedLocalTaxAmount;
					}
					else
					{
						value = new IDInvoiceTaxInfo();
						value.TaxRate = FormatNumberUtil.FormatRateWithCurrentCompanysCulture(taxRate);
						value.TotalLocalExTaxAmount = line.CalculatedLocalExTaxAmount;
						value.TotalLocalTaxAmount = line.CalculatedLocalTaxAmount;
						taxInfo.Add(taxRate, value);
					}
				}

				foreach (IDInvoiceTaxInfo info in taxInfo.Values)
				{
					DocIDInvoiceTaxInfo docInfo = DocIDInvoiceTaxInfo.New(info, Factory);
					collection.Add(docInfo);
				}

				return collection;
			}
		}

		#endregion

		#endregion

		#region Properties

		#region Wrapper Fields

		public override DocJobHeader JobHeader
		{
			get
			{
				if (IsAmendingTransactionForPeriodicInvoiceWithSingleJob && !JobRelatedToAmendingTransactionCreatedForPeriodicInvoice.IsNull)
				{
					return DocJobHeader.New(JobRelatedToAmendingTransactionCreatedForPeriodicInvoice, Factory);
				}
				return DocJobHeader.New(TransactionHeader.Job, Factory);
			}
		}

		JobHeader transactionJobHeader => (IsAmendingTransactionForPeriodicInvoiceWithSingleJob && !JobRelatedToAmendingTransactionCreatedForPeriodicInvoice.IsNull) ? JobRelatedToAmendingTransactionCreatedForPeriodicInvoice : TransactionHeader.Job;

		const int MaximumContainersOnALine = 7;

		public ZString ContainerNumbersForDeclaration
		{
			get
			{
				ZString result = ZString.Empty;
				if (Declaration != null)
				{
					Array arrayofContainers = Declaration.LineOfContainerNumbers.Split(',');

					int count = 0;

					foreach (ZString container in arrayofContainers)
					{
						count++;

						if (count > MaximumContainersOnALine)
						{
							result = result.TrimEndIncludingWhiteSpace(',') + " ...";
							break;
						}

						result += container + ", ";
					}
				}
				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		#region ISF

		public ICusISFHeader ISF
		{
			get
			{
				ICusISFHeader result = null;
				if (IsISFJob())
				{
					result = Factory.Load<ICusISFHeader>(transactionJobHeader.JH_ParentID);
				}
				return result;
			}
		}

		#endregion

		#region Declaration

		public DocBaseJobDeclaration Declaration
		{
			get
			{
				DocBaseJobDeclaration result = null;

				if (IsDeclarationJob())
				{
					BaseJobDeclaration declaration = Factory.Load<BaseJobDeclaration>(transactionJobHeader.JH_ParentID);
					if (declaration != null)
					{
						result = DocBaseJobDeclaration.New(declaration, Factory);
					}
				}

				return result;
			}
		}

		#endregion

		#region WorkItem

		public DocBaseWorkItem WorkItem
		{
			get
			{
				DocBaseWorkItem result = null;

				if (IsWorkItemJob())
				{
					var workItem = Factory.Load<WorkItem>(transactionJobHeader.JH_ParentID);
					if (workItem != null)
					{
						result = DocBaseWorkItem.New(workItem, Factory);
					}
				}
				return result;
			}
		}

		#endregion

		#region AgencyShipment

		public DocAgencyShipment AgencyShipment
		{
			get { return AgencyShipmentCore; }
		}

		protected virtual DocAgencyShipment AgencyShipmentCore
		{
			get
			{
				DocAgencyShipment result = null;
				if (IsShipmentJob())
				{
					ZQuery query = new ZQuery();
					query.AddToFilter(JobShipmentSchema.PK, transactionJobHeader.JH_ParentID);
					query.AddToFilter(JobShipmentSchema.JS_IsShipping, true);

					AgencyShipment shipment = Factory.LoadTop1<AgencyShipment>(query);
					if (shipment != null)
					{
						result = DocAgencyShipment.New(shipment, Factory);
					}
				}
				return result;
			}
		}

		#endregion

		#region Shipment

		public DocForwardingShipment Shipment
		{
			get { return ShipmentCore; }
		}

		protected virtual DocForwardingShipment ShipmentCore
		{
			get
			{
				DocForwardingShipment result = null;
				if (IsShipmentJob())
				{
					ZQuery query = new ZQuery();
					query.AddToFilter(JobShipmentSchema.PK, transactionJobHeader.JH_ParentID);
					query.AddToFilter(JobShipmentSchema.JS_IsShipping, false);

					ForwardingShipment shipment = Factory.LoadTop1<ForwardingShipment>(query);
					if (shipment != null)
					{
						result = DocForwardingShipment.New(shipment, Factory);
					}
				}
				return result;
			}
		}

		#endregion

		#region NctsHeader

		public NctsHeaderDocumentWrapper NctsHeader
		{
			get { return NctsHeaderCore; }
		}

		protected virtual NctsHeaderDocumentWrapper NctsHeaderCore
		{
			get
			{
				NctsHeaderDocumentWrapper result = null;

				if (OpJobHelper.IsNCTSJob)
				{
					NctsHeader nctsHeader = Factory.Load<NctsHeader>(transactionJobHeader.JH_ParentID);
					if (nctsHeader != null)
					{
						result = NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
					}
				}
				return result;
			}
		}

		#endregion

		#region Consol

		public DocForwardingConsol Consol
		{
			get
			{
				DocForwardingConsol result = null;
				if (IsConsolJob())
				{
					var consol = Factory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, StripConsolidatedInvoiceRefOfEndChars());
					if (consol != null)
					{
						result = DocForwardingConsol.New(consol, Factory);
					}
				}
				return result;
			}
		}

		#endregion

		#region Load List

		public DocLoadListConsol LoadList
		{
			get
			{
				DocLoadListConsol result = null;
				if (IsLoadListJob())
				{
					var loadListConsol = Factory.Load<CFSLoadListConsol>(transactionJobHeader.JH_ParentID);
					if (loadListConsol != null)
					{
						result = DocLoadListConsol.New(loadListConsol, Factory);
					}
				}
				return result;
			}
		}

		#endregion

		#region Container Registration

		public DocPackUnpackContainerRego ContainerRegistration
		{
			[DocumentEngineObsoleteField("Container Registration is being deleted")]
			get
			{
				DocPackUnpackContainerRego result = null;
				if (IsContainerRegistrationJob())
				{
					var containerRego = Factory.LoadTop1<CFSContainer>(new ZQuery(JobContainerSchema.JC_ContainerJobID, StripConsolidatedInvoiceRefOfEndChars()));
					if (containerRego != null)
					{
						result = DocPackUnpackContainerRego.New(containerRego, Factory);
					}
				}
				return result;
			}
		}

		#endregion

		#region Cartage

		public DocCommonCartage Cartage
		{
			get
			{
				DocCommonCartage result = null;
				if (!TransactionHeader.IsDeleted && IsCartageJob())
				{
					CommonCartage transport = Factory.Load<CommonCartage>(transactionJobHeader.JH_ParentID);
					if (transport != null)
					{
						result = DocCommonCartage.New(transport, Factory);
					}
				}

				return result;
			}
		}

		#endregion

		#region Warehouse

		public DocWarehouseAdHocServiceJob WarehouseAdHocServiceJob
		{
			get
			{
				DocWarehouseAdHocServiceJob result = null;
				var adHocServiceJob = Factory.Load<WhsAdHocServiceJob>(TransactionHeader?.Job?.JH_ParentID ?? ZGuid.Empty);
				if (adHocServiceJob != null && adHocServiceJob.JobHeader.JH_IsActive)
				{
					result = DocWarehouseAdHocServiceJob.New(adHocServiceJob, Factory);
				}
				return result;
			}
		}

		public DocWhsInvoice WarehouseInvoice
		{
			get
			{
				DocWhsInvoice docInvoice = null;
				if (IsWarehousePeriodicJob())
				{
					ZQuery filter = new ZQuery(JobStorageSchema.ET_StorageJobNumber, StripConsolidatedInvoiceRefOfEndChars());
					var invoice = Factory.LoadTop1<WhsInvoice>(filter);
					if (invoice != null)
					{
						docInvoice = DocWhsInvoice.New(invoice, Factory);
					}
				}
				return docInvoice;
			}
		}

		public DocWhsStocktake WarehouseStocktake
		{
			get
			{
				DocWhsStocktake docStocktake = null;
				if (IsWarehouseStocktakeJob())
				{
					ZQuery filter = new ZQuery(WhsStocktakeSchema.WS_StocktakeNumber, StripConsolidatedInvoiceRefOfEndChars());
					var stocktake = Factory.LoadTop1<WhsStocktake>(filter);
					if (stocktake != null)
					{
						docStocktake = DocWhsStocktake.New(stocktake, Factory);
					}
				}
				return docStocktake;
			}
		}

		public DocWhsDocket WarehouseDocket
		{
			get
			{
				DocWhsDocket docDocket = null;
				if (IsWarehouseOperationsJob())
				{
					ZQuery filter = new ZQuery(WhsDocketSchema.WD_DocketID, StripConsolidatedInvoiceRefOfEndChars());
					var docket = Factory.LoadTop1<WhsDocket>(filter);
					if (docket != null)
					{
						fWarehouseDocketType = docket.WD_DocketType;
						if (docket.WD_DocketType == Enterprise.Warehouse.Transactions.CodeLists.DocketType.Codes.Receive)
						{
							docDocket = DocWhsReceive.New((WhsReceive)docket, Factory);
						}
						else if (docket.WD_DocketType == Enterprise.Warehouse.Transactions.CodeLists.DocketType.Codes.Order)
						{
							docDocket = DocWhsOrder.New((WhsOrder)docket, Factory);
						}
					}
				}
				return docDocket;
			}
		}

		protected ZString fWarehouseDocketType;
		protected ZString WarehouseDocketType
		{
			get
			{
				if (fWarehouseDocketType.IsEmpty)
				{
					if (WarehouseDocket != null)
					{
						fWarehouseDocketType = WarehouseDocket.DocketType;
					}
				}
				return fWarehouseDocketType;
			}
		}

		#endregion

		#endregion

		#region ZInt Fields

		public new ZInt AgePeriod
		{
			get { return TransactionHeader.AH_AgePeriod; }
		}

		public new ZInt PostPeriod
		{
			get { return TransactionHeader.AH_PostPeriod; }
		}

		#endregion

		#region ZBool

		public ZBool ShowLocalGSTAmount
		{
			get
			{
				return IsTaxed
							 && !ShowLocalEquivalentTotalAmounts
							 && Currency.Code != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
							 && (GlbCompany.CurrentCompany.Country.IsPartOfEuropeanUnion
									 || GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.Australia
									 || GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.Norway
									 || GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.Serbia
									 || GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.UnitedArabEmirates
									 || GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.Bahrain
									 || GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.Kuwait
									 || GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.Oman
									 || GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.Qatar
									 || GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.SaudiArabia
									 || GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.NewCaledonia
									 || GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.LaoPeoplesDemocraticRepublic
									 || GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.UnitedKingdom
									 );
			}
		}

		public ZBool ShowLocalEquivalentTotalAmounts
		{
			get
			{
				return Currency.Code != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency &&
							 AccountingConfigurationRegistry.Instance.ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency.Value;
			}
		}

		public ZBool ShowInvoiceTaxDate => ShowTaxOnDocs && InvoicingBase.IsTaxReportable && InvoiceTaxDate.IsValid
			&& PrintTaxDateInARInvoiceDocumentOptions.Contains(AccountingConfigurationRegistry.Instance.PrintTaxDateInARInvoiceDocument.Value);

		string[] PrintTaxDateInARInvoiceDocumentOptions => [AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInHeaderEarliest.Code, AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInHeaderLatest.Code];

		public ZBool ShowInvoiceLineTaxDate => ShowTaxOnDocs
			&& AccountingConfigurationRegistry.Instance.PrintTaxDateInARInvoiceDocument.Value == AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInBody.Code;

		public ZString InvoiceLineTaxDateHeading => Res.GetString("ddd42ef3-7c87-4aa8-a5d4-1b9cf7ed7524", "TAX DATE");

		public ZBool PostToGL
		{
			get { return TransactionHeader.AH_PostToGL == "Y"; }
		}

		public ZString PostedBy => TransactionHeader.PostedBy;

		public ZBool PrintAgentReferenceOnCustomInvoice
		{
			get { return PrintAgentReferenceOnCustomInvoiceCore; }
		}

		protected virtual ZBool PrintAgentReferenceOnCustomInvoiceCore
		{
			get { return false; }
		}

		public ZBool ShowOperatorsName
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.ShowOperatorsNameOnInvoice.Value;
			}
		}

		public ZBool ShowOperatorsSignatore => AccountingConfigurationRegistry.Instance.ShowOperatorsSignature.Value;

		public ZString CreatedByUserInitials
		{
			get
			{
				return InvoicingBase.Creator != null ? InvoicingBase.Creator.GS_Code : ZString.Empty;
			}
		}

		public ZString CreatedByUserName
		{
			get
			{
				return InvoicingBase.Creator != null ? InvoicingBase.Creator.GS_FullName : ZString.Empty;
			}
		}

		public ZString CreatedByUserPhone
		{
			get
			{
				ZString result = "";
				if (InvoicingBase.Creator != null && AccountingConfigurationRegistry.Instance.ShowOperatorsWorkPhoneNumberOnInvoice.Value)
				{
					result = Res.GetString("0b5cda28-a8be-4755-b930-78d97276edce", "Phone: {0}", InvoicingBase.Creator.GS_WorkPhone_Formatted);
				}
				return result;
			}
		}

		public Image SignatureForInvoiceDocuments
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.ShowOperatorsNameOnInvoice.Value && AccountingConfigurationRegistry.Instance.ShowOperatorsSignature.Value
					&& InvoicingBase.Creator != null)
				{
					var result = InvoicingBase.Creator.SignatureImage;
					if (result != null)
					{
						var usageDetailsCollector = Factory?.ServiceContainer.GetService<DocumentUsageDetailsCollector>();
						usageDetailsCollector?.GetIsUserSignatureUsed(true);
					}
					return result;
				}

				return null;
			}
		}

		public ZBool IsCompanyRegisteredForGST
		{
			get { return CurrentCompany.IsGSTRegistered; }
		}

		public override ZBool IsTaxed => ShowTaxOnDocs && InvoicingBase.IsTaxed;

		public ZString InvoiceAuthorisationCreateUserIDNumberLabel => (NoResString)"Cashier TIN";

		public ZString InvoiceAuthorisationCreateUserIDNumber => InvoicingBase.Creator.Certificates.GetFirstCertificateNumber(CertificateTypePairList.Codes.TFN);

		public ZString InvoiceAuthorisationRecordTimeLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.InvoiceAuthorisationRecordTimeLabel);

		public ZString InvoiceAuthorisationRecordTime => TransactionHeaderAuthorisationRecord?.AHF_DateTime.ToDateTime().ToString("dd MMM yyyy hh:mm:ss") ?? ZString.Empty;

		public ZString InvoiceAuthorisationRecordNumberLabel => (NoResString)"SDC Invoice No";

		public ZString InvoiceAuthorisationRecordNumber => TransactionHeaderAuthorisationRecord?.AHF_Number ?? ZString.Empty;

		public ZString InvoiceAuthorisationRecordCounterLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.InvoiceAuthorisationRecordCounterLabel);

		public ZString InvoiceAuthorisationRecordCounter => TransactionHeaderAuthorisationRecord?.AHF_Counter ?? ZString.Empty;

		public ZString InvoiceAuthorisationBuyerIDNumberLabel => (NoResString)"Buyer TIN";

		public ZString InvoiceAuthorisationBuyerIDNumber => GetOrgTaxRegistrationNumberByType(Organisation.OrgHeader, OrgCusCode.CodeTypes.TaxFileCode, GlbCompany.CurrentCompany.IsInTaxCoreSupportedCountry() ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : ZString.Empty);

		public ZString InvoiceAuthorisationRecordIDNumberLabel => "TIN";

		public ZString InvoiceAuthorisationRecordIDNumber => TransactionHeaderAuthorisationRecord?.AHF_IDNumber ?? ZString.Empty;

		public ZString InvoiceAuthorisationRecordVerificationURL => TransactionHeaderAuthorisationRecord?.AHF_VerificationUrl ?? ZString.Empty;

		public ZString InvoiceAuthorisationRecordAuthorisationData => ObjectFactory.Get<ICountryComplianceFactory>()
			.GetTransactionAuthorisationRecordProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?
			.GetDecodedAuthorationData(TransactionHeaderAuthorisationRecord?.AHF_AuthorisationData ?? ZBlob.Empty) ?? ZString.Empty;

		public ZString InvoiceAuthorisationRecordPlaceOfIssue => TransactionHeaderAuthorisationRecord?.AHF_PlaceOfIssue ?? ZString.Empty;

		public ZString InvoiceAuthorisationRecordIssuerAuthorizationData => Convert.ToBase64String(TransactionHeaderAuthorisationRecord?.AHF_IssuerAuthorizationData ?? ZBlob.Empty) ?? ZString.Empty;

		public ZString InvoiceAuthorisationRecordDebtorNumber => (AccountingCountryFactory as IDebtorNumberProvider)?.GetDebtorName(TransactionHeaderAuthorisationRecord) ?? ZString.Empty;

		public ZString OriginalTransactionReferenceNumberLabel => (NoResString)"Ref No.";

		public ZString OriginalTransactionReferenceNumber => AdditionalTransactionHeaderAuthorisationRecordInfo.OriginalTransactionReferenceNumber;

		public ZString InvoiceAuthorisationRecordBusinessNameLabel => (NoResString)"Company";

		public ZString InvoiceAuthorisationRecordBusinessName => AdditionalTransactionHeaderAuthorisationRecordInfo.BusinessName;

		public ZString InvoiceAuthorisationRecordLocationNameLabel => (NoResString)"Store";

		public ZString InvoiceAuthorisationRecordLocationName => AdditionalTransactionHeaderAuthorisationRecordInfo.LocationName;

		public ZString InvoiceAuthorisationRecordAddressLabel => (NoResString)"Address";

		public ZString InvoiceAuthorisationRecordAddress => AdditionalTransactionHeaderAuthorisationRecordInfo.Address;

		public ZString InvoiceAuthorisationRecordDistrictLabel => (NoResString)"District";

		public ZString InvoiceAuthorisationRecordDistrict => AdditionalTransactionHeaderAuthorisationRecordInfo.District;

		public ZString InvoiceAuthorisationRecordIssuerCertificateIdentifier => TransactionHeaderAuthorisationRecord?.AHF_IssuerCertificateIdentifier ?? ZString.Empty;

		public ZString EInvoicePaymentMethodLabel => (NoResString)"Payment Method";

		public ZString EInvoicePaymentMethod => AdditionalTransactionHeaderAuthorisationRecordInfo.EInvoicePaymentMethod;

		public bool IsTaxCoreEInvoicing => InvoicingBase.IsTaxCoreInvoice && InvoicingBase.IsApprovedByGovt;

		public ZString TaxCoreEInvoiceIdentifier => IsCreditNote ? Res.GetString("9ec78963-a139-4e1b-b338-c1a75c1e2292", "NORMAL REFUND") :
			Res.GetString("5c1afb89-61d1-462e-bf78-387b4432a2f7", "NORMAL SALE");

		public ZString InvoiceAuthorisationRecordAuthorisationDataLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.InvoiceAuthorisationRecordAuthorisationDataLabel);

		public ZString InvoiceAuthorisationRecordDebtorNumberLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.InvoiceAuthorisationRecordDebtorNumberLabel);

		public ZString InvoiceAuthorisationRecordIssuerAuthorizationDataLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.InvoiceAuthorisationRecordIssuerAuthorizationDataLabel);

		public ZString InvoiceAuthorisationRecordIssuerCertificateIdentifierLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.InvoiceAuthorisationRecordIssuerCertificateIdentifierLabel);

		public ZString InvoiceAuthorisationRecordPlaceOfIssueLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.InvoiceAuthorisationRecordPlaceOfIssueLabel);

		public ZString GovernmentCreditTermsLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.GovernmentCreditTermsLabel);

		public ZString ComplianceSubtypeLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.ComplianceSubtypeLabel);

		public ZString EInvoicingGovernmentAllocatedNumberLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.EInvoicingGovernmentAllocatedNumberLabel);

		public ZBool IsTransactionEligibleForEInvoicing => TransactionHeader.GetMostRecentEInvoicingTransactionPivot() != null;

		public ZString EInvoicingGovernmentAllocatedID => TransactionHeader.AH_GovernmentAllocatedID;

		public ZString EmptyEInvoicingGovernmentAllocatedIDMessage => Res.GetString("9ce9d860-5e7a-401e-9ebc-029a6e55e920", "not yet available");

		public ZString TaxRegimeInformationLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.TaxRegimeInformationLabel);

		public ZString GovernmentAgreedPaymentMethodLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.GovernmentAgreedPaymentMethodLabel);

		public ZString InvoiceAuthorizationRecordVerificationURLLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.InvoiceAuthorizationRecordVerificationURLLabel);

		public ZString DebtorTaxRegimeLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.DebtorTaxRegimeLabel);

		public ZString SubjectToTaxLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.SubjectToTaxLabel);

		public ZString MeasurementUnitLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.MeasurementUnitLabel);

		public ZString GovernmentReportingCodeLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.GovernmentReportingCodeLabel);

		public ZString TaxBaseAmountLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.TaxBaseAmountLabel);

		public ZString TaxLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.TaxLabel);

		public ZString TaxAmountLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.TaxAmountLabel);

		public ZString IVATaxLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.IVATaxLabel);

		public ZString RetentionTaxLabel => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.RetentionTaxLabel);

		public ZString RecipientConsumptionTaxRegimeHeading => AccountingDependencyFactory.GetCountrySpecificLabelTranslator().GetTranslation(LabelsEnum.RecipientConsumptionTaxRegimeHeading, Country.GetConsumptionTaxDescription(InvoicingBase.Company.GC_RN_NKCountryCode));

		public ZString RecipientConsumptionTaxRegimeDescription
		{
			get
			{
				var countryCode = GetInvoiceCountry();
				var taxRegime = GetAccountingCountryComplianceFeature<IRecipientConsumptionTaxRegime>(countryCode);
				if (taxRegime != null)
				{
					var orgCusCode = InvoicingBase.Header?.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(countryCode, taxRegime.GetOrgCusCodes())?.OK_CodeType ?? string.Empty;

					if (!orgCusCode.IsEmpty)
					{
						return new OrgCodeLists().CustomsCodes_List(countryCode).GetDescriptionFromCode(orgCusCode);
					}
				}
				return ZString.Empty;
			}
		}

		IAccountingDependencyFactory AccountingDependencyFactory => accountingDependencyFactory ?? (accountingDependencyFactory = ObjectFactory.Get<IAccountingDependencyFactory>());
		IAccountingDependencyFactory accountingDependencyFactory;

		AccTransactionHeaderAuthorisationRecord TransactionHeaderAuthorisationRecord => transactionHeaderAuthorisationRecord ??
																						(transactionHeaderAuthorisationRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(Factory, InvoicingBase.PK, InvoicingBase.Company.Country.RN_Code));
		AccTransactionHeaderAuthorisationRecord transactionHeaderAuthorisationRecord;

		AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper AdditionalTransactionHeaderAuthorisationRecordInfo => additionalTransactionHeaderAuthorisationRecordInfo ??
			(additionalTransactionHeaderAuthorisationRecordInfo = (TransactionHeaderAuthorisationRecord as ISupportAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper)?.AdditionaInfo ?? new AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper());
		AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper additionalTransactionHeaderAuthorisationRecordInfo;

		protected IAccountingCountryFactory AccountingCountryFactory => accountingCountryFactory ??
			(accountingCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GetInvoiceCountry()));
		IAccountingCountryFactory accountingCountryFactory;

		protected FeatureInterface GetAccountingCountryComplianceFeature<FeatureInterface>(string countryCode) where FeatureInterface : class => ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>().GetFeatureInterface<FeatureInterface>(countryCode);

		protected ZString GetInvoiceCountry() => InvoicingBase.Company.GC_RN_NKCountryCode;

		public ZDateTimeOffset EInvoicingAuthorisationDateTime => InvoicingBase.EInvoicingAuthorisationDateTime;

		bool ShowTaxOnDocs => Organisation == null || !Organisation.MiscServ.ARDontShowTaxOnDocs;

		public ZBool IsTotalOSTaxAmountZero
		{
			get
			{
				return TotalOSTaxAmount == 0;
			}
		}

		public ZBool IsTotalOSTaxAmountRawZero
		{
			get
			{
				return TotalOSTaxAmountRaw == 0;
			}
		}

		protected ZBool HasQSTTax
		{
			get
			{
				ZBool result = ZBool.False;

				if (Organisation == null || !Organisation.MiscServ.ARDontShowTaxOnDocs)
				{
					foreach (DocARInvoiceLine invoiceLine in Lines)
					{
						if (invoiceLine.TaxRate != null &&
								(invoiceLine.TaxRate.ExtraType == AccTaxRate.ExtraTypes.QuebecQST || invoiceLine.TaxRate.ExtraType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase))
						{
							result = ZBool.True;
							break;
						}
					}
				}

				return result;
			}
		}

		public virtual ZBool IsTrailingPageRequired
		{
			get { return ZBool.False; }
		}

		public ZBool InvoiceLinesWithOSValue
		{
			get
			{
				ZBool result = ZBool.False;
				foreach (DocARInvoiceLine line in Lines)
				{
					if (line.Charge != null)
					{
						if (line.Charge.OSSellCurrency != null &&
								line.Charge.OSSellCurrency.Code != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							result = ZBool.True;
							break;
						}
					}
				}

				return result;
			}
		}

		public ZBool ShowCreditBalanceMessage
		{
			get
			{
				ZBool result;
				if (DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					result = !IsCancelled && (IsCreditNote || (TransactionHeader.AH_TransactionType == TransactionTypes.AdjustmentNote && (TransactionHeader.AH_InvoiceAmount < 0)));
				}
				else
				{
					result = IsCreditNote;
				}

				return result;
			}
		}

		public ZBool HasSERLineOnly
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malaysia)
				{
					ZBool result = true;
					foreach (DocARInvoiceLine invoiceLine in Lines)
					{
						if (invoiceLine.TaxRate != null && invoiceLine.TaxRate.ExtraType != AccTaxRate.ExtraTypes.ServiceTax)
						{
							result = false;
							break;
						}
					}

					return result;
				}
				else
				{
					return false;
				}
			}
		}

		public ZBool HasAtLeastOneSERLine
		{
			get
			{
				ZBool result = false;
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malaysia)
				{
					foreach (DocARInvoiceLine invoiceLine in Lines)
					{
						if (invoiceLine.TaxRate != null && invoiceLine.TaxRate.ExtraType == AccTaxRate.ExtraTypes.ServiceTax)
						{
							result = true;
							break;
						}
					}
				}

				return result;
			}
		}

		public ZBool HasIGICLineOnly
		{
			get
			{
				return !Lines.Cast<DocARInvoiceLine>().Any(x => x.TaxRate != null && x.TaxRate.ExtraType != AccTaxRate.ExtraTypes.RegionalTax);
			}
		}

		public ZBool HasGSTANDQSTLine
		{
			get
			{
				ZBool result = false;
				foreach (DocARInvoiceLine invoiceLine in Lines)
				{
					if (invoiceLine.TaxRate != null
							&& (invoiceLine.TaxRate.IsRatedTax())
							&& (invoiceLine.TaxRate.ExtraType == AccTaxRate.ExtraTypes.QuebecQST || invoiceLine.TaxRate.ExtraType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase)
					)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		public ZBool HasVATANDNHILLine
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Any(x => x.TaxRate != null && x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.Ghana &&
																											 x.TaxRate.Type == AccTaxRate.Types.Rated && x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase);
			}
		}

		public ZBool HasGSTANDEDULine
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Any(
						x => x.TaxRate != null
								 && x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India
								 && (x.TaxRate.IsRatedTax())
								 && x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax);
			}
		}

		public ZBool HasVATANDIGICLine
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Any(x => x.TaxRate != null && x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.Spain && x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.RegionalTax);
			}
		}

		public ZBool HasRETLine
		{
			get
			{
				ZBool result = false;
				foreach (DocARInvoiceLine invoiceLine in Lines)
				{
					if (invoiceLine.TaxRate != null && invoiceLine.TaxRate.Type == AccTaxRate.Types.Rated &&
							(invoiceLine.TaxRate.ExtraType == AccTaxRate.ExtraTypes.VATRetention || invoiceLine.TaxRate.ExtraType == AccTaxRate.ExtraTypes.VATRetentionFraction))
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		public ZBool HasSPVLine
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Any(x => x.TaxRate != null && x.TaxRate.AccTaxRate.IsVATRemittedByCustomer);
			}
		}

		public ZBool HasHSTLine
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Any(x => x.TaxRate != null && x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.Canada &&
																											 (x.TaxRate.Code.StartsWith("HST") || x.TaxRate.Code.StartsWith("CAPHST")));
			}
		}

		public ZBool HasGSTANDQCTLine
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Any(
						x => x.TaxRate != null
								 && x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India
								 && (x.TaxRate.IsRatedTax())
								 && x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase);
			}
		}

		public ZBool HasIntegratedGSTLine
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Any(
						x => x.TaxRate != null
								 && x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India
								 && x.TaxRate.Type == AccTaxRate.Types.IntegratedGST);
			}
		}

		public ZBool HasStateGSTLine
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Any(
						x => x.TaxRate != null
								 && x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India
								 && (x.TaxRate.IsRatedTax())
								 && x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.StateGST);
			}
		}

		public ZBool HasIndiaServiceTax
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Any(
						x => x.TaxRate != null
								 && x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India
								 && (x.TaxRate.Type == AccTaxRate.Types.ServiceTax
										 || x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.ServiceTax));
			}
		}

		public ZBool HasZeroIGSTAmount
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Any(
						x => x.TaxRate != null
								 && x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India
								 && (x.TaxRate.Type == AccTaxRate.Types.ExcludedFromTheTaxBase
										 || ((x.TaxRate.Type == AccTaxRate.Types.NotReportable || x.TaxRate.Type == AccTaxRate.Types.Exempt || x.TaxRate.Type == AccTaxRate.Types.ReverseRated) && x.TaxRate.ExtraType.IsEmpty))
				);
			}
		}

		public ZBool HasZeroRatedStateGST
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Any(
						x => x.TaxRate != null
								 && x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India
								 && ((x.TaxRate.Type == AccTaxRate.Types.NotReportable || x.TaxRate.Type == AccTaxRate.Types.Exempt || x.TaxRate.Type == AccTaxRate.Types.ReverseRated) && x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.StateGST)
				);
			}
		}

		public ZBool DisplayRemittancePaymentDetails
		{
			get
			{
				return !TransactionHeader.AH_IsCancelled && (TransactionHeader.AH_TransactionType == TransactionTypes.Invoice ||
																										 (TransactionHeader.AH_TransactionType == TransactionTypes.AdjustmentNote && (TransactionHeader.AH_InvoiceAmount > 0)));
			}
		}

		public ZBool DisplayReversalInfo
		{
			get
			{
				var result = false;

				if (!IsAmendingTransaction || AmendmentReasonDescription.IsEmpty)
				{
					if (IsCancelled)
					{
						if (!TransactionHeader.AH_TransactionBelongsToGroup.IsEmpty)
						{
							result = !TransactionHeader.AH_ReceiptType.IsEmpty;
						}
						else if (TransactionHeader is InvoicingBase transactionBase)
						{
							result = !transactionBase.CorrespondingReversedTransaction?.AH_ReceiptType.IsEmpty ?? false;
						}
					}
					else
					{
						result = !OriginalReferenceReason.IsEmpty;
					}
				}

				return result;
			}
		}

		ZBool HasApprovalRequestAndNotPosted
		{
			get { return InvoicingBase.HasApprovalRequestAndNotPosted; }
		}

		ZBool IsCreatedAsUnapprovedAPInvoiceToPreviewRequest
		{
			get { return InvoicingBase.HasContext(BusinessContext.UnapprovedAPInvoiceCreatedForRequestPreview); }
		}

		#endregion

		#region ZString Fields

		public ZString OsInvoiceAmountInWords
		{
			get
			{
				ZString result = ZString.Empty;

				if (Utilities.Round(OSTotal, 0) > MaximumInvoiceAmountToConvert)
				{
					throw new NotSupportedException(String.Format("The number {0} is over 999,999,999,999. This number is not supported by the current version of the number to words converter.", OSTotal));
				}
				else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.VietNam
								 && AccountOrg.Country.Code == Core.Constants.CountryCodes.VietNam)
				{
					result = NumberToString_VI_VN.VietnameseConvertNumberToWords((long)OSTotal);
				}
				else
				{
					result = NumberToString_EN.ConvertNumberToWords((long)OSTotal);
				}

				return result;
			}
		}

		const long MaximumInvoiceAmountToConvert = 999999999999;

		public ZString BatchLineDescription
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsInvoiceWithLoadList)
				{
					result = Res.GetString("a619d7f4-665a-46b9-a1e1-ec5180d476b3", "AGENTS REF: {0}", LoadList.AgentsReference) + " ";
				}
				else if (IsInvoiceWithCartage)
				{
					result = Res.GetString("2be47085-28e6-4e71-82eb-4c7c3dff0eec", "ORDER REF: {0}", Cartage.OrderReferenceNumber) + " ";
				}
				else if (IsInvoiceWithShipment && Shipment.HouseBill != Shipment.ShipmentNumber)
				{
					result = Res.GetString("d7b13075-5c2b-47e1-9b8a-3142a5671c72", "HBL: {0}", Shipment.HouseBill) + " ";
				}
				else if (IsInvoiceWithDeclaration)
				{
					if (!Declaration.OwnerRef.IsEmpty)
					{
						result = Res.GetString("d7a6baac-1702-4a85-9242-05cad451152b", "OWNER REF: {0}", Declaration.OwnerRef) + " ";
					}

					if (!Declaration.HouseBill.IsEmpty)
					{
						result += Res.GetString("107eae99-bf1b-4052-a817-b774808ab118", "HBL: {0}", Declaration.HouseBill) + " ";
					}
				}

				if (JobHeader != null)
				{
					result += Res.GetString("eac725d4-8ae0-464e-8d18-17ba2370b3cf", "JOB: {0}", JobHeader.JobNumber);
				}

				return result;
			}
		}

		public ZString LongCreditTerms
		{
			get
			{
				ZString result = ZString.Empty;

				CodeDescriptionPairList termList = new ARInvoiceTermsList();

				if (!IsCreditNote)
				{
					if (InvoiceTerm != "")
					{
						if (InvoiceTerm != "COD")
						{
							result = InvoiceTermDays.ToString() + " " + Res.GetString("b4d29521-4868-48fa-a58b-53d436354064", "days") + " ";
						}

						result += termList.GetDescriptionFromCode(InvoiceTerm).Trim();
					}
					else
					{
						if (termList.GetDescriptionFromCode("COD") != null)
						{
							result += termList.GetDescriptionFromCode("COD").Trim();
						}
					}
				}

				return result;
			}
		}

		public virtual MultilingualString CreditTerms => TransactionHeaderHelper.GetCreditTerms(TransactionType, TransactionHeader.Company.GC_RN_NKCountryCode, InvoiceTerm, InvoiceTermDays.ToString(), TransactionHeader.AH_InvoiceDate, TransactionHeader.AH_DueDate, TransactionHeader.IsExport(), (BaseJobDeclaration)Declaration?.WrappedObject, (CommonCartage)Cartage?.WrappedObject);

		public ZString OSTaxDisplayHeading
		{
			get
			{
				ZString result = "";

				if (GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.Malaysia)
				{
					result = GetTaxDisplayHeadingForMalaysia();
				}
				else if (HasVATANDNHILLine)
				{
					result = GhanaComplianceInfo.VATAndExtraTax;
				}
				else if (HasGSTANDQCTLine || HasGSTANDEDULine || HasIntegratedGSTLine || HasStateGSTLine || HasZeroIGSTAmount || HasZeroRatedStateGST)
				{
					result = GetSpecialGSTTaxDisplayHeading();
				}
				else if (HasGSTANDQSTLine && !HasHSTLine)
				{
					result = GetTranslatedCodeWithFallback() + "/" + GetQCTExtraTaxCodeFromCountryCode();
				}
				else if (!HasGSTANDQSTLine && HasHSTLine)
				{
					result = GSTAndHST;
				}
				else if (HasGSTANDQSTLine && HasHSTLine)
				{
					result = Res.GetString("e37f6465-4cd2-4e78-bbb1-074aefaee095", "GST/HST/QST");
				}
				else if (HasREGLineForSpain)
				{
					result = Res.GetString("8ce7cdec-5efd-4ee6-8417-38e75b51df7a", "IGIC");
				}
				else if (HasIndiaServiceTax)
				{
					result = Res.GetString("46bf5570-f31e-410d-8a76-7f1e47eecacc", "SER");
				}
				else if (IsTaxed)
				{
					result = TranslatedTaxCode;
				}
				return result;
			}
		}

		public static ZString GetTranslatedCodeWithFallback()
		{
			ZString translatedCode = TranslatedTaxCode;
			if (string.IsNullOrEmpty(translatedCode))
			{
				translatedCode = Res.GetString("cddf9aeb-de74-4fe6-9838-89994ad421d4", "GST");
			}
			return translatedCode;
		}

		ZString GetSpecialGSTTaxDisplayHeading()
		{
			var result = ZString.Empty;

			if (HasGSTANDQCTLine || HasGSTANDEDULine)
			{
				result = Res.GetString("46bf5570-f31e-410d-8a76-7f1e47eecacc", "SER");

				if (HasGSTANDQCTLine)
				{
					result = result + "/" + Res.GetString("59bdee71-0887-4468-86b9-519c0ad494d6", "CESS");
				}
				if (HasGSTANDEDULine)
				{
					result = result + "/" + Res.GetString("9df8f3e1-3702-404a-9715-3cd3fa2c6f6b", "EDU");
				}
			}
			if (HasIntegratedGSTLine || HasZeroIGSTAmount)
			{
				result = result + "/" + Res.GetString("0999cd3f-e685-4a01-b007-2ad58a52569d", "IGST");
			}
			if (HasStateGSTLine || HasZeroRatedStateGST)
			{
				result = result + "/" + Res.GetString("d6abc195-49c5-4920-9e47-c70b4828bdda", "CGST/SGST");
			}

			result = result.TrimStart('/');

			return result;
		}

		public static ZString TranslatedTaxCode
		{
			get
			{
				return GetTranslatedTaxCodeFromCountryCode(GlbCompany.CurrentCompany.Country.Code);
			}
		}

		public ZString InvoiceSubTotalDisplayHeading => IsTaxCoreEInvoicing ? Res.GetString("73b7a2b0-2ece-4c3e-a755-91f89a947e4c", "TOTAL (ex TAX)") :
			Res.GetString("2cf0a6c5-e90b-4206-abc6-4615c74fce0f", "SUBTOTAL");

		public ZString OSPrimaryTaxDisplayHeading
		{
			get
			{
				ZString result = "";

				if (GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.Malaysia)
				{
					result = GetTaxDisplayHeadingForMalaysia();
				}
				else if (IsTaxCoreEInvoicing)
				{
					result = Res.GetString("df584ec9-1e29-41d1-90d4-089fbb594df0", "TOTAL TAX");
				}
				else if (HasHSTLine)
				{
					result = GSTAndHST;
				}
				else if (HasGSTANDEDULine || HasIndiaServiceTax)
				{
					result = Res.GetString("11efe34d-4995-43bf-9f1b-a9ec238589d2", "SER");
				}
				else if (IsTaxed)
				{
					result = TranslatedTaxCode;
				}
				return result;
			}
		}

		public ZString OSTaxExtraRateQCTDisPlay
		{
			get
			{
				if (HasGSTANDQCTLine)
				{
					return Res.GetString("6aa7b9c5-fe8f-47ce-8048-ff9ac6c75915", "CESS");
				}
				if (HasVATANDNHILLine)
				{
					return GhanaComplianceInfo.ExtraTax;
				}
				else
				{
					var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.Country.Code) as ICountryComplianceInfo;
					if (complianceInfo != null && complianceInfo.HasExtraTaxInfo().HasValue && complianceInfo.HasExtraTaxInfo().Value)
					{
						return complianceInfo.GetExtraTaxDescription(AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase);
					}
					else
					{
						return Res.GetString("3afd6354-21eb-4b61-aafb-27f200a9b3c7", "QST");
					}
				}
			}
		}

		public ZString OSTaxExtraRateSBCDisPlay
		{
			get { return Res.GetString("99d60dae-d9b2-4d48-b160-e499a989d84b", "SBC"); }
		}

		public ZString OSTaxExtraRateKKCDisPlay
		{
			get { return Res.GetString("2839fcda-a692-4f09-aa7f-1a4737171aeb", "KKC"); }
		}

		#region Invoice Tax Date

		public ZDateTime InvoiceTaxDate => AccountingConfigurationRegistry.Instance.PrintTaxDateInARInvoiceDocument.Value == AccountingConstants.PrintTaxDateInARInvoiceDocumentOption.PrintTaxDateInHeaderLatest.Code
			? InvoiceTaxDateCacheProvider.GetLatestInvoiceTaxDate(InvoicingBase.Lines.Cast<InvoicingLineBase>(), InvoiceDate)
			: InvoicingBase.InvoiceTaxDate;

		InvoiceTaxDateCacheProvider InvoiceTaxDateCacheProvider => invoiceTaxDateCacheProvider ??= new InvoiceTaxDateCacheProvider();

		InvoiceTaxDateCacheProvider invoiceTaxDateCacheProvider;

		#endregion

		public ZString InvoiceTaxDateHeading => Ledger == LedgerTypes.AccountsReceivable ?
			Res.GetString("af2a6282-571e-4ad1-bbc7-21cf8e721c8c", "Date of Supply") :
			Res.GetString("5cbd4856-d42a-4826-b903-6e4e64ee97ff", "Tax Date");

		public ZString OSSPVExtraTaxLabel
		{
			get
			{
				switch (CurrentCompany.Organisation?.Country?.Code)
				{
					case Core.Constants.CountryCodes.CostaRica:
						return Res.GetString("36A3832D-9539-4F21-B989-08E1F40ADC6C", "EXONERATION");
					case Core.Constants.CountryCodes.Italy:
						return Res.GetString("20D186ED-4E6A-40F4-8E90-1FB62A7DCE21", "SPLIT PAYMENT");
				}
				return ZString.Empty;
			}
		}

		public ZString OSSPVExtraTaxCodeLabel
		{
			get
			{
				switch (CurrentCompany.Organisation?.Country?.Code)
				{
					case Core.Constants.CountryCodes.CostaRica:
						return Res.GetString("51F790BD-01FB-4D41-B662-429F9084156D", "Exon.");
					case Core.Constants.CountryCodes.Italy:
						return Res.GetString("3C8A672E-60CE-4020-BF36-E7B862F6D434", "SPV");
				}
				return ZString.Empty;
			}
		}

		protected static string GSTAndHST
		{
			get { return Res.GetString("89e6f883-57a5-4d0b-96c1-1b1baae490cc", "GST/HST"); }
		}

		public ZString InvoiceFooterMessage
		{
			get { return Res.GetString("8a0d361b-8117-47a8-a3e2-471102433618", "Please return a copy of this {0} with your payment if paying by cheque", DocumentTitle.ToLower()); }
		}

		public ZString InvoiceTypeReferenceNumberHeading
		{
			get
			{
				ZString result = InvoiceTypeReferenceNumberHeadingNoColon;
				return result.IsEmpty ? "" : result.ToUpper() + ":";
			}
		}

		public ZString InvoiceTypeReferenceNumberHeadingNoColon
		{
			get
			{
				ZString result = ZString.Empty;
				if (InvoiceType == InvoiceTypeConsol)
				{
					result = Res.GetString("49df1cf3-6494-4621-9721-c1a1e3a6414e", "Consol");
				}
				else if (InvoiceType == InvoiceTypeLoadList)
				{
					result = Res.GetString("5c0d4db6-bc85-435c-b405-a4afc56f772e", "Load List");
				}
				else if (InvoiceType == InvoiceTypeShipment || InvoiceType == InvoiceTypeShipmentReceivals || InvoiceType == InvoiceTypeAgencyShipment)
				{
					result = Res.GetString("0e2ada67-fbdf-4c8c-8a39-09b90597daba", "Shipment");
				}
				else if (InvoiceType == InvoiceTypeCustoms)
				{
					result = Res.GetString("1dde548c-1c00-42fb-afdd-807953817038", "Declaration");
				}
				else if (InvoiceType == InvoiceTypeReconDeclaration)
				{
					result = Res.GetString("8d7ec105-4d90-4efb-8ba5-77076afc8ae8", "Reconciliation");
				}
				else if (InvoiceType == InvoiceTypeImporterSecurityFiling)
				{
					result = Res.GetString("a838ce46-1027-4e40-94a1-33bf318aae95", "ISF Job");
				}
				else if (InvoiceType == InvoiceTypeTransport)
				{
					result = Res.GetString("f60a9022-4d99-4c6d-8cbd-9f3f7f2acfce", "Transport");
				}
				else if (InvoiceType == InvoiceTypeWarehousePeriodic)
				{
					result = Res.GetString("b7aa43cc-8110-4f7f-b103-307885de009d", "Invoice");
				}
				else if (InvoiceType == InvoiceTypeWarehouseWhsInwards || InvoiceType == InvoiceTypeWarehouseWhsOrder)
				{
					result = Res.GetString("44a31c00-025a-4537-a0bf-25dfecff31b9", "Docket");
				}
				else if (InvoiceType == InvoiceTypeWarehouseWhsStocktake)
				{
					result = Res.GetString("4edf6114-cb4c-4cad-b9f7-a8999c9450b6", "Stocktake");
				}
				else if (InvoiceType == InvoiceTypeWarehouseAdHocServiceJob)
				{
					result = Res.GetString("fc36119f-b04d-44c8-8efe-ed5f374ec891", "Customer Ref");
				}
				else if (InvoiceType == InvoiceTypeNctsHeader)
				{
					result = Res.GetString("{86BF5E15-C272-4370-80A1-4AD766971E2F}", "NCTS Movement");
				}
				return result;
			}
		}

		public ZString InvoiceTypeReferenceNumber
		{
			get
			{
				ZString result = ZString.Empty;

				if (InvoiceType == InvoiceTypeConsol && Consol != null)
				{
					result = Consol.ConsolNumber;
				}
				else if (InvoiceType == InvoiceTypeLoadList && LoadList != null)
				{
					result = LoadList.ConsolNumber;
				}
				else if ((InvoiceType == InvoiceTypeShipment || InvoiceType == InvoiceTypeShipmentReceivals) && Shipment != null)
				{
					result = Shipment.ShipmentNumber;
				}
				else if ((InvoiceType == InvoiceTypeAgencyShipment) && AgencyShipment != null)
				{
					result = AgencyShipment.ShipmentNumber;
				}
				else if ((InvoiceType == InvoiceTypeCustoms || InvoiceType == InvoiceTypeReconDeclaration) && Declaration != null)
				{
					result = Declaration.DeclarationReference;
				}
				else if (InvoiceType == InvoiceTypeImporterSecurityFiling && ISF != null)
				{
					result = ISF.BF_JobReference;
				}
				else if (InvoiceType == InvoiceTypeContainerRegistration && ContainerRegistration != null)
				{
					result = ContainerRegistration.ContainerJobID;
				}
				else if (InvoiceType == InvoiceTypeTransport && Cartage != null)
				{
					result = Cartage.ConsignmentID;
				}
				else if (InvoiceType == InvoiceTypeWarehousePeriodic && WarehouseInvoice != null)
				{
					result = WarehouseInvoice.InvoiceNumber;
				}
				else if ((InvoiceType == InvoiceTypeWarehouseWhsInwards || InvoiceType == InvoiceTypeWarehouseWhsOrder) && WarehouseDocket != null)
				{
					result = WarehouseDocket.DocketID;
				}
				else if ((InvoiceType == InvoiceTypeWarehouseWhsStocktake) && WarehouseStocktake != null)
				{
					result = WarehouseStocktake.StocktakeNumber;
				}
				else if (InvoiceType == InvoiceTypeWarehouseAdHocServiceJob && WarehouseAdHocServiceJob != null)
				{
					result = WarehouseAdHocServiceJob.CustomerReferenceNumber;
				}
				if (InvoiceType == InvoiceTypeNctsHeader)
				{
					result = NctsHeader.MOVEMENTREFERENCENUMBER;
				}

				return result;
			}
		}

		#region InvoiceType

		public ZString InvoiceType
		{
			get { return InvoiceTypeCore; }
		}

		protected virtual ZString InvoiceTypeCore
		{
			get
			{
				ZString result = InvoiceTypeMisc;
				if (!ConsolidatedInvoiceRef.IsEmpty)
				{
					if (TransactionHeader.AH_JH.IsEmpty)
					{
						if (Consol != null)
						{
							result = InvoiceTypeConsol;
						}
					}
					else
					{
						var job = TransactionHeader.Job;
						if (job != null)
						{
							result = GetInvoiceTypeBasedOnJob(job);
						}
					}
				}
				else if (InvoiceTypeCalculationProvider.IsDeferredInvoiceType(TransactionHeader.AH_TransactionCategory))
				{
					result = "PeriodicInvoice";
				}
				else if (IsAmendingTransactionForPeriodicInvoiceWithSingleJob && !JobRelatedToAmendingTransactionCreatedForPeriodicInvoice.IsNull)
				{
					result = GetInvoiceTypeBasedOnJob(JobRelatedToAmendingTransactionCreatedForPeriodicInvoice);
				}

				return result;
			}
		}

		ZString GetInvoiceTypeBasedOnJob(JobHeader job)
		{
			ZString result = InvoiceTypeMisc;

			if (job.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix)
			{
				result = GetShipmentType();
			}
			else if (job.JH_ParentTableCode == JobConsolSchema.Constants.Prefix)
			{
				result = GetLoadListType();
			}
			else if (job.JH_ParentTableCode == JobDeclarationSchema.Constants.Prefix)
			{
				if (Declaration?.IsReconciliation ?? false)
				{
					result = InvoiceTypeReconDeclaration;
				}
				else
				{
					result = InvoiceTypeCustoms;
				}
			}
			else if (job.JH_ParentTableCode == JobCartageSchema.Constants.Prefix)
			{
				result = InvoiceTypeTransport;
			}
			else if (job.JH_ParentTableCode == JobContainerSchema.Constants.Prefix)
			{
				result = InvoiceTypeContainerRegistration;
			}
			else if (job.JH_ParentTableCode == JobStorageSchema.Constants.Prefix)
			{
				result = InvoiceTypeWarehousePeriodic;
			}
			else if (job.JH_ParentTableCode == WhsDocketSchema.Constants.Prefix)
			{
				result = GetWarehouseInvoiceType();
			}
			else if (job.JH_ParentTableCode == WhsStocktakeSchema.Constants.Prefix)
			{
				result = InvoiceTypeWarehouseWhsStocktake;
			}
			else if (job.JH_ParentTableCode == CusISFHeaderSchema.Constants.Prefix)
			{
				result = InvoiceTypeImporterSecurityFiling;
			}
			else if (job.JH_ParentTableCode == WhsAdHocServiceJobSchema.Constants.Prefix)
			{
				result = InvoiceTypeWarehouseAdHocServiceJob;
			}
			else if (job.JH_ParentTableCode == CusInBondHeaderSchema.Constants.Prefix)
			{
				result = GetCusInBondHeaderType();
			}

			return result;
		}

		ZString GetLoadListType()
		{
			ZString result = InvoiceTypeMisc;
			if (LoadList != null && LoadList.IsCFS)
			{
				result = InvoiceTypeLoadList;
			}
			return result;
		}

		ZString GetWarehouseInvoiceType()
		{
			ZString result = InvoiceTypeMisc;

			if (WarehouseDocketType == "INW")
			{
				result = InvoiceTypeWarehouseWhsInwards;
			}
			else if (WarehouseDocketType == "ORD")
			{
				result = InvoiceTypeWarehouseWhsOrder;
			}
			return result;
		}

		ZString GetShipmentType()
		{
			ZString result = InvoiceTypeMisc;

			if (Shipment != null)
			{
				if (Shipment.IsCFSRegistered && !Shipment.IsForwardRegistered)
				{
					result = InvoiceTypeShipmentReceivals;
				}
				else
				{
					result = InvoiceTypeShipment;
				}
			}
			else if (AgencyShipment != null)
			{
				result = InvoiceTypeAgencyShipment;
			}

			return result;
		}

		ZString GetCusInBondHeaderType()
		{
			ZString result = InvoiceTypeMisc;

			if (NctsHeader != null)
			{
				result = InvoiceTypeNctsHeader;
			}

			return result;
		}

		#endregion

		public ZString Copy
		{
			get
			{
				ZString result = "";

				if (InvoicePrinted && AccountingConfigurationRegistry.Instance.ShouldInvoiceShowCopyWhenPrintedSubsequentTimes.Value)
				{
					result = Res.GetString("27c0897f-1d14-49e9-86d1-2df1523b8141", "Copy");
				}
				else if (!WrapperInfo.Name.IsEmpty)
				{
					result = WrapperInfo.Name;
				}

#if DEBUG
				if (Globals.IsTest && DocCopyList_ForTestOnly != null)
				{
					DocCopyList_ForTestOnly.Add(result);
				}
#endif

				return result;
			}
		}

		public ZString CostConfirmationHeadingText
		{
			get
			{
				if (Ledger == LedgerTypes.UnapprovedPayableTransactions || HasApprovalRequestAndNotPosted || IsCreatedAsUnapprovedAPInvoiceToPreviewRequest)
				{
					return AccountingConfigurationRegistry.Instance.UnapprovedInvoiceCostConfirmationHeadingText.Value;
				}
				else
				{
					return AccountingConfigurationRegistry.Instance.CostConfirmationHeadingText.Value;
				}
			}
		}

		public MultilingualString CostConfirmationDocumentTitle
		{
			get
			{
				if (Ledger == LedgerTypes.UnapprovedPayableTransactions || HasApprovalRequestAndNotPosted || IsCreatedAsUnapprovedAPInvoiceToPreviewRequest)
				{
					return AccountingConfigurationRegistry.Instance.UnapprovedInvoiceCostConfirmationDocumentTitle.Value;
				}
				else
				{
					return AccountingConfigurationRegistry.Instance.CostConfirmationDocumentTitle.Value;
				}
			}
		}

		public virtual ZString DocumentTitle
		{
			get
			{
				ZString title = ZString.Empty;

				if (IsInvoice)
				{
					title = GetTitleForInvoice();
				}
				else if (IsCreditNote)
				{
					title = GetTitleForCreditNote();
				}
				else if (IsAdjustmentNote)
				{
					title = GetTitleForAdjustmentNote();
				}

				return title;
			}
		}

		ZString GetTitleForInvoice()
		{
			ZString result;

			if (IsTaxed && InvoicingBase.IsTaxReportable)
			{
				result = TaxInvoiceTitle;
			}
			else
			{
				result = NonTaxInvoiceTitle;
			}

			return result;
		}

		ZString GetTitleForCreditNote()
		{
			ZString result;

			if (IsTaxed && InvoicingBase.IsTaxReportable)
			{
				result = TaxCreditNoteTitle;
			}
			else
			{
				result = NonTaxCreditNoteTitle;
			}

			return result;
		}

		protected abstract ZString TaxInvoiceTitle { get; }
		protected abstract ZString NonTaxInvoiceTitle { get; }
		protected abstract ZString TaxCreditNoteTitle { get; }
		protected abstract ZString NonTaxCreditNoteTitle { get; }
		protected abstract ZString TaxAdjustmentNoteTitle { get; }
		protected abstract ZString NonTaxAdjustmentNoteTitle { get; }

		protected bool IsProForma
		{
			get { return !InvoicingBase.IsInDatabase; }
		}

		ZString GetTitleForAdjustmentNote()
		{
			ZString result;

			if (IsTaxed && InvoicingBase.IsTaxReportable)
			{
				result = TaxAdjustmentNoteTitle;
			}
			else
			{
				result = NonTaxAdjustmentNoteTitle;
			}

			return result;
		}

		public ZString JobChargesExchangeRateAndCurrency
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				List<ZString> currencyList = new List<ZString>();

				foreach (DocARInvoiceLine current in Lines)
				{
					DocJobInvoicingJobCharge chargeWrapper = current.Charge;

					if (chargeWrapper != null &&
							chargeWrapper.OSSellCurrency != null &&
							!currencyList.Contains(chargeWrapper.OSSellCurrency.Code)
					)
					{
						builder.Append(chargeWrapper.OSSellCurrency.Code + " " + chargeWrapper.OSSellExRate.ToString(4));
						currencyList.Add(chargeWrapper.OSSellCurrency.Code);
					}
				}

				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString CompanyTaxRegistrationNumber
		{
			get
			{
				return CurrentCompany.BusinessRegNo;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZString CompanyTaxRegistrationExtraNumber
		{
			get
			{
				ZString result = ZString.Empty;

				if (Branch != null && Branch.Organisation != null && Branch.Organisation.Country != null)
				{
					switch (Branch.Organisation.Country.Code)
					{
						case Core.Constants.CountryCodes.Russia:
							result = Branch.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.RussiaCodeTypes.KPP, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Russia));
							break;
						case Core.Constants.CountryCodes.Morocco:
							result = Branch.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.MoroccoCodeTypes.ICE, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Morocco));
							break;
						default:
							break;
					}
				}

				if (string.IsNullOrEmpty(result) && CurrentCompany.Organisation != null && CurrentCompany.Organisation.Country != null)
				{
					switch (CurrentCompany.Organisation.Country.Code)
					{
						case Core.Constants.CountryCodes.Greece:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.GreeceCodeTypes.DOY, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Greece));
							break;
						case Core.Constants.CountryCodes.SriLanka:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.SriLankaCodeTypes.SVATBusinessRegistrationNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.SriLanka));
							break;
						case Core.Constants.CountryCodes.Azerbaijan:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.AzerbaijanCodeTypes.TIN, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Azerbaijan));
							break;
						case Core.Constants.CountryCodes.Kenya:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.KenyaCodeTypes.PIN, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Kenya));
							break;
						case Core.Constants.CountryCodes.Mauritius:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.CodeTypes.BusinessRegistrationNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Mauritius));
							break;
						case Core.Constants.CountryCodes.Tanzania:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.TanzaniaCodeTypes.TIN, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Tanzania));
							break;
						case Core.Constants.CountryCodes.Turkey:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(TurkeyOrgCusCodeInfo.OrgCusCodes.VDM, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Turkey));
							break;
						case Core.Constants.CountryCodes.Madagascar:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.MadagascarCodeTypes.NIS, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Madagascar));
							break;
						case Core.Constants.CountryCodes.Russia:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.RussiaCodeTypes.KPP, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Russia));
							break;
						case Core.Constants.CountryCodes.Morocco:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.MoroccoCodeTypes.ICE, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Morocco));
							break;
						case Core.Constants.CountryCodes.Lithuania:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.LithuaniaCodeTypes.IMK, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Lithuania));
							break;
						case Core.Constants.CountryCodes.Curacao:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.CuracaoCodeTypes.CCR, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Curacao));
							break;
						case Core.Constants.CountryCodes.Togo:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.TogoCodeTypes.NIC, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Togo));
							break;
						case Core.Constants.CountryCodes.Benin:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.BeninCodeTypes.NRC, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Benin));
							break;
						case Core.Constants.CountryCodes.Kosovo:
							result = CurrentCompany.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.KosovoCodeTypes.NFK, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Kosovo));
							break;
						default:
							break;
					}
				}
				return result;
			}
		}

		public ZString CompanyTaxRegistrationPANNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (Organisation != null && Organisation.Country != null && Organisation.Country.Code == Core.Constants.CountryCodes.India)
				{
					result = CurrentCompany.Organisation.PAN;
				}
				return result;
			}
		}

		public ZString CompanyTaxRegistrationQSTNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (GlbCompany.CurrentCompany.Country.RN_Code == Core.Constants.CountryCodes.Canada && HasQSTTax)
				{
					OrgCusCode qstRegistrationCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CACodeTypes.QuebecSalesTaxID, GlbCompany.CurrentCompany.Country);
					if (qstRegistrationCode != null && !qstRegistrationCode.OK_CustomsRegNo.IsEmpty)
					{
						result = qstRegistrationCode.OK_CustomsRegNo;
					}
				}
				return result;
			}
		}

		ZString TaxCode
		{
			get
			{
				var result = ZString.Empty;
				if (CurrentCompany != null)
				{
					string value;
					if (TaxCodeDictionary.TryGetValue(CurrentCompany.Country.Code.ToString(), out value))
					{
						result = value;
					}
					else
					{
						result = GetTranslatedTaxCodeFromCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					}
				}
				return result;
			}
		}

		public ZString TaxCodeSuffix
		{
			get
			{
				var result = ZString.Empty;
				switch (CurrentCompany.Country.Code)
				{
					case Core.Constants.CountryCodes.Singapore:
						result = " " + Res.GetString("4c00952e-f7e7-41c4-8db3-c18357b67f23", "Reg No:") + " ";
						break;
					case Core.Constants.CountryCodes.Fiji:
					case Core.Constants.CountryCodes.Maldives:
					case Core.Constants.CountryCodes.Tonga:
					case Core.Constants.CountryCodes.Nigeria:
						result = Res.GetString("9b2dc1bb-a6e8-44e9-a549-3cbc71934c7b", "TIN:") + " ";
						break;
					case Core.Constants.CountryCodes.Vanuatu:
					case Core.Constants.CountryCodes.Germany:
					case Core.Constants.CountryCodes.Australia:
					case Core.Constants.CountryCodes.Peru:
					case Core.Constants.CountryCodes.Guatemala:
					case Core.Constants.CountryCodes.Colombia:
					case Core.Constants.CountryCodes.Bolivia:
					case Core.Constants.CountryCodes.Venezuela:
					case Core.Constants.CountryCodes.Ecuador:
					case Core.Constants.CountryCodes.Paraguay:
					case Core.Constants.CountryCodes.Nicaragua:
					case Core.Constants.CountryCodes.Panama:
					case Core.Constants.CountryCodes.Uruguay:
					case Core.Constants.CountryCodes.ElSalvador:
					case Core.Constants.CountryCodes.PuertoRico:
					case Core.Constants.CountryCodes.FrenchPolynesia:
					case Core.Constants.CountryCodes.Honduras:
					case Core.Constants.CountryCodes.Mali:
					case Core.Constants.CountryCodes.EquatorialGuinea:
					case Core.Constants.CountryCodes.Chad:
					case Core.Constants.CountryCodes.Algeria:
					case Core.Constants.CountryCodes.Niger:
					case Core.Constants.CountryCodes.VietNam:
					case Core.Constants.CountryCodes.Senegal:
					case Core.Constants.CountryCodes.CoteDivoire:
					case Core.Constants.CountryCodes.Cameroon:
					case Core.Constants.CountryCodes.Mozambique:
					case Core.Constants.CountryCodes.DominicanRepublic:
					case Core.Constants.CountryCodes.Yemen:
					case Core.Constants.CountryCodes.Malawi:
					case Core.Constants.CountryCodes.Turkey:
					case Core.Constants.CountryCodes.CostaRica:
					case Core.Constants.CountryCodes.Ghana:
					case Core.Constants.CountryCodes.Belarus:
					case Core.Constants.CountryCodes.SierraLeone:
					case Core.Constants.CountryCodes.Cambodia:
					case Core.Constants.CountryCodes.Malta:
					case Core.Constants.CountryCodes.Madagascar:
					case Core.Constants.CountryCodes.Kiribati:
					case Core.Constants.CountryCodes.Nepal:
					case Core.Constants.CountryCodes.Barbados:
					case Core.Constants.CountryCodes.Curacao:
					case Core.Constants.CountryCodes.Togo:
					case Core.Constants.CountryCodes.Benin:
					case Core.Constants.CountryCodes.Rwanda:
					case Core.Constants.CountryCodes.Kosovo:
					case Core.Constants.CountryCodes.Brazil:
						result = ZString.Empty;
						break;
					default:
						result = " #: ";
						break;
				}
				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505: Avoid unmaintainable code")]
		public ZString TaxId
		{
			get
			{
				var taxIdFromJobDocAddressForPT = GetTaxIdFromJobDocAddressForPT();
				if (!taxIdFromJobDocAddressForPT.IsEmpty)
				{
					return taxIdFromJobDocAddressForPT;
				}

				var resultStringBuilder = new ZStringBuilder();

				if (AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.Value && IsTaxed)
				{
					var taxIDMacroDataProvider = (AccountingCountryFactory as IInstanceProvider<ITaxIDMacroDataProvider>)?.Get();
					if (taxIDMacroDataProvider != null)
					{
						var taxIDMacroData = taxIDMacroDataProvider.GetTaxIDMacroData();
						if (taxIDMacroData != null)
						{
							var regNo = GetTaxBranchOrgProxyRegistrationNumberWithFallback(taxIDMacroData.OrgTaxRegistrationCode, InvoicingBase.Company.GC_RN_NKCountryCode);
							if (!regNo.IsEmpty)
							{
								resultStringBuilder.Append(taxIDMacroData.OrgTaxRegistrationPrefix);
								resultStringBuilder.Append(regNo);
							}
							if (!taxIDMacroData.ExtraOrgTaxRegistrationCode.IsEmpty && !taxIDMacroData.ExtraOrgTaxRegistrationPrefix.IsEmpty)
							{
								var extraRegNo = GetTaxBranchOrgProxyRegistrationNumberWithFallback(taxIDMacroData.ExtraOrgTaxRegistrationCode, InvoicingBase.Company.GC_RN_NKCountryCode);
								if (!extraRegNo.IsEmpty)
								{
									resultStringBuilder.Append(" ");
									resultStringBuilder.Append(taxIDMacroData.ExtraOrgTaxRegistrationPrefix);
									resultStringBuilder.Append(extraRegNo);
								}
							}
						}

						return resultStringBuilder.ToString();
					}

					#region Obsolete Code - Please do not add new countries in this section instead use above AccountingCountryFactory approach

					var registrationNumberPrefix = ZString.Empty;
					var businessRegNo = CurrentCompany.BusinessRegNo;

					if (TransactionHeader.Company != null &&
								TransactionHeader.Company.Country != null
								&& (TransactionHeader.Company.Country.IsPartOfEuropeanUnion || TransactionHeader.Company.Country.RN_Code == Core.Constants.CountryCodes.UnitedKingdom)
								&& !CurrentCompany.BusinessRegNo.StartsWith(TransactionHeader.Company.GC_RN_NKCountryCode)
								&& (TransactionHeader.Company.Country.RN_Code != Core.Constants.CountryCodes.Spain
									|| (TransactionHeader.Branch != null && !DoesTransactionHaveREGLineAndOrgRecordedIGIC(TransactionHeader.Branch.OrgProxy))))
					{
						registrationNumberPrefix = RefCountry.GetPrefixForTaxRegistrationCode(TransactionHeader.Company.GC_RN_NKCountryCode);
					}

					switch (CurrentCompany.Country.Code)
					{
						case Core.Constants.CountryCodes.Andorra:
							var andorraRegNo = GetTaxBranchOrgProxyRegistrationNumberWithFallback(AndorraOrgCusCodeInfo.OrgCusCodes.IGI, Core.Constants.CountryCodes.Andorra);
							if (!andorraRegNo.IsEmpty)
							{
								AppendTaxInfo(registrationNumberPrefix, andorraRegNo, AndorraOrgCusCodeInfo.OrgCusCodes.NRT);
							}
							break;
						case Core.Constants.CountryCodes.India:
							AppendIndiaTaxInfo();
							break;

						case Core.Constants.CountryCodes.Chad:
							var chadRegNo = GetOrgProxyCustomsRegNo(OrgCusCode.ChadCodeTypes.NIF, Core.Constants.CountryCodes.Chad);
							if (!chadRegNo.IsEmpty)
							{
								AppendTaxInfo(registrationNumberPrefix, chadRegNo);
							}
							break;
						case Core.Constants.CountryCodes.PalestinianTerritory:
							var palestineRegNo = GetOrgProxyCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.PalestinianTerritory);
							if (!palestineRegNo.IsEmpty)
							{
								AppendTaxInfo(registrationNumberPrefix, palestineRegNo);
							}
							break;
						case Core.Constants.CountryCodes.SaintMartin:
							var saintMartinRegNo = GetOrgProxyCustomsRegNo(SaintMartinOrgCusCodeInfo.OrgCusCodes.TGC, Core.Constants.CountryCodes.SaintMartin);
							if (!saintMartinRegNo.IsEmpty)
							{
								AppendTaxInfo(registrationNumberPrefix, saintMartinRegNo);
							}
							break;
						case Core.Constants.CountryCodes.Burundi:
							var burundiRegNo = GetOrgProxyCustomsRegNo(OrgCusCode.CodeTypes.TVACode, Core.Constants.CountryCodes.Burundi);
							if (!burundiRegNo.IsEmpty)
							{
								AppendTaxInfo(registrationNumberPrefix, burundiRegNo);
							}
							break;
						case Core.Constants.CountryCodes.Vanuatu:
							var vanuatuRegNo = GetOrgProxyCustomsRegNo(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.Vanuatu);
							if (!vanuatuRegNo.IsEmpty)
							{
								AppendTaxInfo(registrationNumberPrefix, vanuatuRegNo, (NoResString)"TIN #: "); // country specific code
							}
							break;
						case Core.Constants.CountryCodes.Djibouti:
							var djiboutiRegNo = GetOrgProxyCustomsRegNo(DjiboutiOrgCusCodeInfo.OrgCusCodes.NIF, Core.Constants.CountryCodes.Djibouti);
							if (!djiboutiRegNo.IsEmpty)
							{
								AppendTaxInfo(registrationNumberPrefix, djiboutiRegNo, DjiboutiOrgCusCodeInfo.OrgCusCodes.NIF);
							}
							break;
						case Core.Constants.CountryCodes.Albania:
							var albaniaRegNo = GetOrgProxyCustomsRegNo(AlbaniaOrgCusCodeInfo.OrgCusCodes.NIT, Core.Constants.CountryCodes.Albania);
							if (!albaniaRegNo.IsEmpty)
							{
								AppendTaxInfo(registrationNumberPrefix, albaniaRegNo, AlbaniaOrgCusCodeInfo.OrgCusCodes.NIPT);
							}
							break;
						case Core.Constants.CountryCodes.Mauritania:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Mauritania, registrationNumberPrefix, OrgCusCode.CodeTypes.TVACode, MauritaniaOrgCusCodeInfo.OrgCusCodes.NIF, (NoResString)"NIF #: "); // country specific code
							break;
						case Core.Constants.CountryCodes.Congo:
							var congoRegNo = GetOrgProxyCustomsRegNo(CongoOrgCusCodeInfo.OrgCusCodes.NIU, Core.Constants.CountryCodes.Congo);
							if (!congoRegNo.IsEmpty)
							{
								AppendTaxInfo(registrationNumberPrefix, congoRegNo, CongoOrgCusCodeInfo.OrgCusCodes.NIU);
							}
							break;
						case Core.Constants.CountryCodes.BosniaAndHerzegovina:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.BosniaAndHerzegovina, registrationNumberPrefix, OrgCusCode.BosniaAndHerzegovinaCodeTypes.PDV, OrgCusCode.BosniaAndHerzegovinaCodeTypes.IDB, Res.GetString("e42f487a-e378-45cf-9971-714cc961bca4", "ID #: "));
							break;

						case Core.Constants.CountryCodes.Uzbekistan:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Uzbekistan, registrationNumberPrefix, UzbekistanOrgCusCodeInfo.OrgCusCodes.QQS, UzbekistanOrgCusCodeInfo.OrgCusCodes.STR, (NoResString)"STIR #: "); // country specific code
							break;

						case Core.Constants.CountryCodes.Montenegro:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Montenegro, registrationNumberPrefix, MontenegroOrgCusCodeInfo.OrgCusCodes.PDV, MontenegroOrgCusCodeInfo.OrgCusCodes.PIB, (NoResString)"PIB #: ");
							break;

						case Core.Constants.CountryCodes.Bahamas:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Bahamas, registrationNumberPrefix, OrgCusCode.CodeTypes.VATCode, BahamasOrgCusCodeInfo.OrgCusCodes.TIN, (NoResString)"TIN #: ");
							break;

						case Core.Constants.CountryCodes.Angola:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Angola, registrationNumberPrefix, OrgCusCode.CodeTypes.IVA, OrgCusCode.AngolaCodeTypes.NumeroDeIdentificacioFiscal, Res.GetString("20fd6485-9e64-445a-9736-6e61b4f5ef5a", "NIF #: "));
							break;

						case Core.Constants.CountryCodes.Lesotho:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Lesotho, registrationNumberPrefix, OrgCusCode.CodeTypes.VATCode, LesothoOrgCusCodeInfo.OrgCusCodes.TIN, (NoResString)"TIN #: ");
							break;

						case Core.Constants.CountryCodes.Gabon:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Gabon, registrationNumberPrefix, GabonOrgCusCodeInfo.OrgCusCodes.NIF, GabonOrgCusCodeInfo.OrgCusCodes.RCM, (NoResString)"RCCM #: ");
							break;

						case Core.Constants.CountryCodes.Cyprus:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Cyprus, string.Empty, CyprusOrgCusCodeInfo.OrgCusCodes.TIC, OrgCusCode.CodeTypes.VATCode, (NoResString)"VAT #: ");
							break;

						case Core.Constants.CountryCodes.Guyana:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Guyana, registrationNumberPrefix, GuyanaOrgCusCodeInfo.OrgCusCodes.TIN, OrgCusCode.CodeTypes.VATCode, (NoResString)"VAT #: ");
							break;

						case Core.Constants.CountryCodes.Guinea:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Guinea, registrationNumberPrefix, OrgCusCode.CodeTypes.TVACode, GuineaOrgCusCodeInfo.OrgCusCodes.NIF, (NoResString)"NIF #: ");
							break;

						case Core.Constants.CountryCodes.ElSalvador:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.ElSalvador, registrationNumberPrefix, ElSalvadorOrgCusCodeInfo.OrgCusCodes.NRC, ElSalvadorOrgCusCodeInfo.OrgCusCodes.NIT, Res.GetString("89d63a19-1137-449d-99e3-dd8d4e62b03a", "NIT #: "));
							break;

						case Core.Constants.CountryCodes.Pakistan:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Pakistan, registrationNumberPrefix, OrgCusCode.CodeTypes.VATCode, PakistanOrgCusCodeInfo.OrgCusCodes.NTN, Res.GetString("ebaad0d0-1241-4891-a2da-165c7d40e77a", "NTN #: "));
							break;

						case Core.Constants.CountryCodes.Moldova:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Moldova, registrationNumberPrefix, OrgCusCode.CodeTypes.TVACode, MoldovaOrgCusCodeInfo.OrgCusCodes.NCF, (NoResString)"CF #: ");
							break;

						case Core.Constants.CountryCodes.Morocco:
							AppendTaxInfo(registrationNumberPrefix, CurrentCompany.BusinessRegNo);
							break;
						case Core.Constants.CountryCodes.UnitedArabEmirates:
						case Core.Constants.CountryCodes.Sudan:
						case Core.Constants.CountryCodes.Bahrain:
						case Core.Constants.CountryCodes.Kuwait:
						case Core.Constants.CountryCodes.Oman:
						case Core.Constants.CountryCodes.Qatar:
						case Core.Constants.CountryCodes.SaudiArabia:
							resultStringBuilder.Append(!string.IsNullOrWhiteSpace(BranchTaxIDNumber) ? FormattableString.Invariant($"{BranchTaxIDHeading}: {BranchTaxIDNumber}") : string.Empty);
							break;
						case Core.Constants.CountryCodes.Malaysia:
							var taxRegistrationType = GetTaxRegistrationTypeForMalaysia();
							var result = GetOrgTaxRegistrationNumberByType(GlbCompany.CurrentCompany.OrgProxy, taxRegistrationType);
							AppendTaxInfo(registrationNumberPrefix, result);
							break;
						case Core.Constants.CountryCodes.Ecuador:
							AppendTaxInfo(registrationNumberPrefix, businessRegNo);
							resultStringBuilder.Append(!string.IsNullOrWhiteSpace(BranchTaxIDNumber) ? FormattableString.Invariant($", {BranchTaxIDHeading}: {BranchTaxIDNumber}") : string.Empty);
							resultStringBuilder.Append(!string.IsNullOrWhiteSpace(BranchBusRegNumber) ? FormattableString.Invariant($", {BranchBusRegHeading}: {BranchBusRegNumber}") : string.Empty);
							break;
						case Core.Constants.CountryCodes.Canada:
							AppendTaxInfo(registrationNumberPrefix, businessRegNo);
							var companyTaxRegistrationQSTNumber = CompanyTaxRegistrationQSTNumber;
							if (!companyTaxRegistrationQSTNumber.IsEmpty)
							{
								resultStringBuilder.Append("  ");
								resultStringBuilder.Append(OrgCusCode.CACodeTypes.QuebecSalesTaxID);
								resultStringBuilder.Append(TaxCodeSuffix);
								resultStringBuilder.Append(companyTaxRegistrationQSTNumber);
							}
							break;
						case Core.Constants.CountryCodes.Spain:
							if (TransactionHeader.Branch != null && DoesTransactionHaveREGLineAndOrgRecordedIGIC(TransactionHeader.Branch.OrgProxy))
							{
								var orgCusCodes = TransactionHeader.Branch.OrgProxy.CustomsCodes.Cast<OrgCusCode>().Where(x => x.OK_CodeType == OrgCusCode.SpainCodeTypes.IGC);
								businessRegNo = orgCusCodes.Any() ? (string)orgCusCodes.First().OK_CustomsRegNo : string.Empty;
							}
							AppendTaxInfo(registrationNumberPrefix, businessRegNo);
							break;
						case Core.Constants.CountryCodes.Brazil:
							AppendDoubleTaxInfo(Core.Constants.CountryCodes.Brazil, registrationNumberPrefix, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalTaxPayerRegistration, Res.GetString("3887e130-fe83-4622-b0d3-bc1618c6aa97", "IM: "));
							break;
						default:
							AppendTaxInfo(registrationNumberPrefix, businessRegNo);
							break;
					}

					if (CurrentCompany.Organisation != null && CurrentCompany.Organisation.Country != null)
					{
						var companyTaxRegistrationExtraNumber = CompanyTaxRegistrationExtraNumber;
						if (!companyTaxRegistrationExtraNumber.IsEmpty)
						{
							AddExtraTaxRegistrationNumbers(companyTaxRegistrationExtraNumber);
						}
					}

					#endregion
				}
				return resultStringBuilder.ToString().TrimStart(',').TrimStart();

				void AddExtraTaxRegistrationNumbers(ZString companyTaxRegistrationExtraNumber)
				{
					switch (CurrentCompany.Organisation?.Country?.Code)
					{
						case Core.Constants.CountryCodes.Greece:
							resultStringBuilder.Append(" ");
							resultStringBuilder.Append(Res.GetString("7887bf1d-f6f9-4da2-80f3-2c6acf9c3fcc", "DOY: {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.Turkey:
							resultStringBuilder.Append(" ");
							resultStringBuilder.Append(Res.GetString("5d752a41-a2f8-4520-ba8f-02c9701f5f2a", "TAX OFFICE: {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.SriLanka:
							resultStringBuilder.Append("  ");
							resultStringBuilder.Append(Res.GetString("a15bd161-7699-4b5e-a5de-ac0f64f79e9f", "SVAT#: {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.Azerbaijan:
						case Core.Constants.CountryCodes.Tanzania:
							resultStringBuilder.Append("  ");
							resultStringBuilder.Append(Res.GetString("bc74f33e-b77b-430f-9086-ac9e9d67df7e", "TIN#: {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.Kenya:
							resultStringBuilder.Append("  ");
							resultStringBuilder.Append(Res.GetString("db27f7dd-ca10-4c9a-940c-799761883ee3", "PIN#: {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.Mauritius:
							resultStringBuilder.Append("  ");
							resultStringBuilder.Append(Res.GetString("b1d8227d-674d-4605-b985-c84b14056c5b", "BRN#: {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.Madagascar:
							resultStringBuilder.Append("  ");
							resultStringBuilder.Append(Res.GetString("369136fa-c184-4d03-a8d5-b754480e964b", "N° Statistique: {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.Russia:
							resultStringBuilder.Append("  ");
							resultStringBuilder.Append(Res.GetString("3ce74fba-0e22-4604-ad8c-6ba3c2887041", "KPP #: {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.Morocco:
							resultStringBuilder.Append("  ");
							resultStringBuilder.Append(Res.GetString("D24BBA4F-0DE3-44E4-B539-70318EE1B608", "ICE #: {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.Lithuania:
							resultStringBuilder.Append("  ");
							resultStringBuilder.Append(Res.GetString("29bb1325-bcbd-4f53-a904-4260c9137774", "ĮM.KODA # {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.Curacao:
							resultStringBuilder.Append("  ");
							resultStringBuilder.Append(Res.GetString("6e160bfe-2683-4463-adc5-ee20facf4285", "CCRN #: {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.Togo:
							resultStringBuilder.Append("  ");
							resultStringBuilder.Append(Res.GetString("f9f737b4-a4ad-43eb-8482-479ae5ed4306", "NIC #: {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.Benin:
							resultStringBuilder.Append("  ");
							resultStringBuilder.Append(Res.GetString("923c0f14-95a6-44cf-b537-40ef13830e29", "NRC #: {0}", companyTaxRegistrationExtraNumber));
							break;
						case Core.Constants.CountryCodes.Kosovo:
							resultStringBuilder.Append("  ");
							resultStringBuilder.Append(Res.GetString("869dc433-f9c0-436c-a0de-4f34b34385f3", "NFK #: {0}", companyTaxRegistrationExtraNumber));
							break;
						default:
							break;
					}
				}

				void AppendTaxInfo(string registrationNumberPrefix, string businessRegNo, string taxCode = null)
				{
					resultStringBuilder.Append(taxCode ?? TaxCode);
					resultStringBuilder.Append(TaxCodeSuffix);
					resultStringBuilder.Append(registrationNumberPrefix);
					resultStringBuilder.Append(businessRegNo);
				}

				ZString GetOrgProxyCustomsRegNo(string codeType, string countryCode)
				{
					ZString result;

					var branchOrgProxy = TransactionHeader.Branch.OrgProxy;
					if (branchOrgProxy != null)
					{
						result = GetCustomsRegNo(branchOrgProxy, codeType, countryCode);
					}
					else
					{
						result = GetCustomsRegNo(TransactionHeader.Company.OrgProxy, codeType, countryCode);
					}

					return result;
				}

				ZString GetTaxBranchOrgProxyRegistrationNumberWithFallback(string codeType, string countryCode)
				{
					ZString result;
					var taxBranchOrgProxy = TransactionHeader.TaxBranch?.OrgProxy;

					if (taxBranchOrgProxy != null)
					{
						result = GetCustomsRegNo(taxBranchOrgProxy, codeType, countryCode);
					}
					else
					{
						result = GetOrgProxyCustomsRegNo(codeType, countryCode);
					}

					return result;
				}

				void AppendIndiaTaxInfo()
				{
					if (HasIntegratedGSTLine || HasStateGSTLine || HasZeroIGSTAmount || HasZeroRatedStateGST)
					{
						resultStringBuilder.Append(!string.IsNullOrWhiteSpace(BranchTaxIDNumber) ? string.Format(CultureInfo.InvariantCulture, "{0}: {1}", BranchTaxIDHeading, BranchTaxIDNumber) : string.Empty);
						resultStringBuilder.Append(!string.IsNullOrWhiteSpace(BranchBusRegNumber) ? string.Format(CultureInfo.InvariantCulture, ", {0}: {1}", BranchBusRegHeading, BranchBusRegNumber) : string.Empty);
						resultStringBuilder.Append(", " + BranchFullName);
					}
					else
					{
						resultStringBuilder.Append(Res.GetString("f50f9d80-2f5e-41ce-957b-2cddf4a93408", "Service Tax Registration No: {0}", CurrentCompany.BusinessRegNo));
						var companyTaxRegistrationPANNumber = CompanyTaxRegistrationPANNumber;
						if (!string.IsNullOrWhiteSpace(companyTaxRegistrationPANNumber))
						{
							resultStringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "  {0}: {1}", IndiaOrgCusCodeInfo.OrgCusCodes.PAN, companyTaxRegistrationPANNumber));
						}
					}
				}

				void AppendDoubleTaxInfo(string countryCode, string registrationNumberPrefix, string codeType1, string codeType2, string codeType2Label)
				{
					var codeType1Value = GetOrgProxyCustomsRegNo(codeType1, countryCode);
					var codeType2Value = GetOrgProxyCustomsRegNo(codeType2, countryCode);

					if (!codeType1Value.IsEmpty)
					{
						AppendTaxInfo(registrationNumberPrefix, codeType1Value);
						resultStringBuilder.Append(" ");
					}
					if (!codeType2Value.IsEmpty)
					{
						resultStringBuilder.Append(codeType2Label);
						resultStringBuilder.Append(registrationNumberPrefix);
						resultStringBuilder.Append(codeType2Value);
					}
				}
			}
		}

		ZString GetTaxIdFromJobDocAddressForPT()
		{
			if (AccountingUtils.ShouldPortugalStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting(TransactionHeader.AH_Ledger, TransactionHeader.AH_TransactionType))
			{
				if (TransactionHeader is InvoicingBase invoicingBase)
				{
					var docAddress = invoicingBase.DocAddresses.FindByDocAddressType(DocAddressType.BranchOrCompanyProxyARAdress);
					return docAddress?.E2_GovRegNum ?? ZString.Empty;
				}
			}

			return ZString.Empty;
		}

		public Dictionary<string, string> TaxCodeDictionary
		{
			get
			{
				if (taxCodeDictionary == null)
				{
					taxCodeDictionary = new Dictionary<string, string>();
					AddTaxCodeToDictionary(taxCodeDictionary);
				}
				return taxCodeDictionary;
			}
		}

		Dictionary<string, string> taxCodeDictionary;

		void AddTaxCodeToDictionary(Dictionary<string, string> dictionary)
		{
			if (dictionary != null)
			{
				dictionary.Add(Core.Constants.CountryCodes.India, ZString.Empty);
				dictionary.Add(Core.Constants.CountryCodes.Fiji, ZString.Empty);
				dictionary.Add(Core.Constants.CountryCodes.Maldives, ZString.Empty);
				dictionary.Add(Core.Constants.CountryCodes.Tonga, ZString.Empty);
				dictionary.Add(Core.Constants.CountryCodes.Nigeria, ZString.Empty);
				dictionary.Add(Core.Constants.CountryCodes.Pakistan, PakistanComplianceInfo.RecipientTaxIdPrefix);
				dictionary.Add(Core.Constants.CountryCodes.Bangladesh, BangladeshComplianceInfo.RecipientTaxIdPrefix);
				dictionary.Add(Core.Constants.CountryCodes.PapuaNewGuinea, PapuaNewGuineaComplianceInfo.RecipientTaxIdPrefix);
				dictionary.Add(Core.Constants.CountryCodes.Gabon, GabonComplianceInfo.RecipientTaxIdPrefix);
				dictionary.Add(Core.Constants.CountryCodes.Cyprus, CyprusComplianceInfo.RecipientTaxIdPrefix);
				dictionary.Add(Core.Constants.CountryCodes.Guyana, GuyanaComplianceInfo.RecipientTaxIdPrefix);
				dictionary.Add(Core.Constants.CountryCodes.Germany, Res.GetString("15acd49d-a182-44b1-b2b0-3a53cbef0a98", "VAT ID No:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Australia, Res.GetString("b7f817bf-e081-4931-a7c8-156e10b91ffe", "ABN:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Peru, Res.GetString("20086765-3997-4df6-a6b5-78269a8c96c4", "R.U.C. #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Guatemala, Res.GetString("5eaacd39-e382-465c-bebe-38a8dc63fe5d", "NIT #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Colombia, Res.GetString("5eaacd39-e382-465c-bebe-38a8dc63fe5d", "NIT #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Bolivia, Res.GetString("5eaacd39-e382-465c-bebe-38a8dc63fe5d", "NIT #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Venezuela, Res.GetString("097d6de7-5322-4532-9225-7d7ce7e81e76", "RIF #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Ecuador, Res.GetString("1735ec1a-7bdf-41e0-8f17-94d6f517c9f8", "RUC #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Paraguay, Res.GetString("1735ec1a-7bdf-41e0-8f17-94d6f517c9f8", "RUC #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Nicaragua, Res.GetString("1735ec1a-7bdf-41e0-8f17-94d6f517c9f8", "RUC #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Panama, Res.GetString("1735ec1a-7bdf-41e0-8f17-94d6f517c9f8", "RUC #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Uruguay, Res.GetString("74d7cbac-278c-499a-9378-52c1977b6dc9", "RUT #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.ElSalvador, Res.GetString("33162933-9E90-4839-BB21-2A336708D244", "NRC #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.PuertoRico, Res.GetString("c8edf708-576e-4408-b737-8fbc95f7b48b", "NRC #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.FrenchPolynesia, Res.GetString("d031c1dd-8923-4eb0-8e05-c6e33f95aad4", "TAHITI #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Honduras, Res.GetString("8d5779b0-4e0e-4350-a351-052536e9676d", "RTN #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Mali, Res.GetString("f670e58a-368c-4b53-8128-51cb5cf506d4", "NIF #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.EquatorialGuinea, Res.GetString("f670e58a-368c-4b53-8128-51cb5cf506d4", "NIF #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Chad, Res.GetString("f670e58a-368c-4b53-8128-51cb5cf506d4", "NIF #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Algeria, Res.GetString("f670e58a-368c-4b53-8128-51cb5cf506d4", "NIF #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Niger, Res.GetString("f670e58a-368c-4b53-8128-51cb5cf506d4", "NIF #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.VietNam, Res.GetString("88737248-7918-413e-bb19-35f335f38f3f", "MST #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Senegal, Res.GetString("0d757745-8af8-4ea4-a957-2b4f854c83f8", "NINEA #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.CoteDivoire, Res.GetString("861f2d24-de81-4323-8f68-536beff1d26d", "CC #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Cameroon, Res.GetString("d9934d81-7540-4d09-85f7-3dfb0e31cedc", "NIU #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Mozambique, Res.GetString("a89f3b66-e125-4326-bb58-3251e625fbc0", "NUIT #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.DominicanRepublic, Res.GetString("ba269ac7-3584-45b3-89ec-2aea10554da6", "RNC #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Yemen, Res.GetString("98c934ab-3dde-44b1-ba6e-14167c17eef2", "GST #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Malawi, Res.GetString("06b7712d-6c14-4482-82b9-8dc26644685c", "TPIN #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Turkey, Res.GetString("30a2de99-c04f-4a61-8fb3-0a65392058b1", "TIN:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.CostaRica, Res.GetString("8376120d-78f7-4247-bb33-fa1bfcaaff94", "CÉDULA JURÍDICA #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Ghana, Res.GetString("d77773b8-9a04-4404-8372-ce9efaaaa953", "TIN #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Belarus, Res.GetString("530f067f-83ea-4947-b9bf-f0baefb0fe92", "TIN #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.SierraLeone, Res.GetString("619b8946-ae44-4c7a-940c-1df46428c09c", "TIN #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Cambodia, Res.GetString("f813817b-8894-4192-ad78-eb9c540ff5c9", "VATTIN #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Malta, Res.GetString("877cd48b-d9a5-4523-b905-5e8ff1f135f4", "VAT #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Madagascar, Res.GetString("7b0cbd78-dc63-455f-b589-6ed569cfb5f8", "NIF #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Kiribati, Res.GetString("dc9d52e5-db89-408e-a511-e13a826795aa", "TIN #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Nepal, Res.GetString("47C135E9-12E3-49EC-AD4F-3BDC4F4A5AA6", "VAT #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Curacao, Res.GetString("06C96302-DFA8-41FE-B803-4AA175A7AC9D", "CRIB #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Barbados, Res.GetString("5BB54E21-6E57-4B50-BAF4-1968027B66B6", "TIN #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Togo, Res.GetString("D89E9F73-753E-4E16-814E-06DAC4BCD6FB", "NIF #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Benin, Res.GetString("26982A2B-5264-44FD-9141-A527F2AFCC14", "IFU #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Rwanda, Res.GetString("D27BEB63-2650-4665-B7F5-C7632376694B", "TIN #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Kosovo, Res.GetString("4236EA2C-58B7-4B1A-B9E9-B39DC6C9D20B", "TVSH #:") + " ");
				dictionary.Add(Core.Constants.CountryCodes.Malaysia, Res.GetString("9DF5A51D-F02C-4464-9628-FB5C535F14F0", "REGISTRATION"));
				dictionary.Add(Core.Constants.CountryCodes.BosniaAndHerzegovina, Res.GetString("4d406a0c-2301-4d43-8eba-cf016bae2c88", "PDV"));
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Spain && TransactionHeader.Branch != null && DoesTransactionHaveREGLineAndOrgRecordedIGIC(TransactionHeader.Branch.OrgProxy))
				{
					dictionary.Add(Core.Constants.CountryCodes.Spain, Res.GetString("c5de35f8-4a31-4a27-8db6-e14c8e461372", "NIF"));
				}
				dictionary.Add(Core.Constants.CountryCodes.NewCaledonia, Res.GetString("3e4dc093-138b-45eb-86fb-6d559e8141d8", "TGC"));
				dictionary.Add(Core.Constants.CountryCodes.Brazil, Res.GetString("3801d2dc-dc62-4144-b8ca-92aa3a33c056", "CNPJ:") + " ");
			}
		}

		public ZString OrgProxyTaxId
		{
			get
			{
				ZString result = ZString.Empty;
				OrgHeader orgProxy = TransactionHeader.Branch.OrgProxy;

				if (orgProxy != null)
				{
					result = orgProxy.RawTaxRegistrationNumber;
				}

				if (result.IsEmpty)
				{
					orgProxy = TransactionHeader.Company.OrgProxy;

					if (orgProxy != null)
					{
						result = orgProxy.RawTaxRegistrationNumber;
					}
				}
				return result;
			}
		}

		public virtual ZString Message
		{
			get
			{
				if (IsInvoice && IsDisbursement)
				{
					return DisbursementInvoiceMessage;
				}
				else if (IsInvoice)
				{
					return InvoiceMessage;
				}
				else if (IsCreditNote)
				{
					return CreditNoteMessage;
				}
				else if (IsAdjustmentNote)
				{
					return AdjustmentNoteMessage;
				}
				else
				{
					return "";
				}
			}
		}

		public ZString JobInvoiceNumber
		{
			get { return InvoicingBase.InvoiceNumberPrefixed; }
		}

		public ZString PaymentReference
		{
			get
			{
				if (AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.Value && !ConsolidatedInvoiceRef.IsEmpty)
				{
					return TransactionNumberPrefixed;
				}
				return ZString.Empty;
			}
		}

		public ZString ShipmentWeight
		{
			get
			{
				ZString result = ZString.Empty;

				if (Shipment != null)
				{
					if (AccountOrg != null && AccountOrg.IsForwarder)
					{
						result = FormatNumber(Shipment.DocumentedWeight);
					}
					else
					{
						result = Shipment.Weight;
					}
				}

				return result;
			}
		}

		public ZString ShipmentVolume
		{
			get
			{
				ZString result = ZString.Empty;

				if (Shipment != null)
				{
					if (AccountOrg != null && AccountOrg.IsForwarder)
					{
						result = FormatNumber(Shipment.DocumentedVolume);
					}
					else
					{
						result = Shipment.Volume;
					}
				}

				return result;
			}
		}

		public ZString ShipmentCharageable
		{
			get
			{
				ZString result = ZString.Empty;

				if (Shipment != null)
				{
					if (AccountOrg != null && AccountOrg.IsForwarder)
					{
						result = FormatNumber(Shipment.DocumentedChargeable);
					}
					else
					{
						result = Shipment.Chargeable;
					}
				}

				return result;
			}
		}

		public virtual ZString VesselVoyageFlight
		{
			get
			{
				ZString vessel = ZString.Empty;
				ZString voyageFlight = ZString.Empty;

				switch (InvoiceType)
				{
					case InvoiceTypeConsol:
						if (Consol != null)
						{
							vessel = Consol.VesselName;
							voyageFlight = Consol.VoyageNumber;
						}
						break;

					case InvoiceTypeLoadList:
						if (LoadList != null)
						{
							vessel = LoadList.VesselName;
							voyageFlight = LoadList.VoyageNumber;
						}
						break;

					case InvoiceTypeAgencyShipment:
						if (AgencyShipment != null && AgencyShipment.Sailing != null)
						{
							if (AgencyShipment.Sailing.Vessel != null)
							{
								vessel = AgencyShipment.Sailing.Vessel.Code;
							}

							voyageFlight = AgencyShipment.Sailing.VoyageNo;
						}
						break;

					case InvoiceTypeShipment:
					case InvoiceTypeShipmentReceivals:
						if (Shipment != null && Shipment.Sailing != null)
						{
							if (Shipment.Sailing.Vessel != null)
							{
								vessel = Shipment.Sailing.Vessel.Code;
							}

							voyageFlight = Shipment.Sailing.VoyageNo;
						}
						break;

					case InvoiceTypeCustoms:
						if (Declaration != null)
						{
							if (Declaration.Vessel != null)
							{
								vessel = Declaration.Vessel.Code;
							}

							voyageFlight = Declaration.VoyageFlightNo;
						}
						break;

					case InvoiceTypeTransport:
						if (Cartage != null)
						{
							if (Cartage.Vessel != null)
							{
								vessel = Cartage.Vessel.Code;
							}

							voyageFlight = Cartage.Voyage;
						}
						break;

					case InvoiceTypeContainerRegistration:
						if (ContainerRegistration != null)
						{
							vessel = ContainerRegistration.Vessel;
							voyageFlight = ContainerRegistration.Voyage;
						}
						break;
				}

				ZString delimiter = (!vessel.IsEmpty && !voyageFlight.IsEmpty) ? " / " : "";
				return vessel + delimiter + voyageFlight;
			}
		}

		public ZString HouseBillNumber
		{
			get
			{
				ZString result = ZString.Empty;

				switch (InvoiceType)
				{
					case InvoiceTypeConsol:
						break;

					case InvoiceTypeLoadList:
						break;

					case InvoiceTypeShipment:
					case InvoiceTypeShipmentReceivals:
						if (Shipment != null)
						{
							result = Shipment.HouseBill;
						}

						break;

					case InvoiceTypeCustoms:
						if (Declaration != null)
						{
							result = Declaration.HouseBill;
						}

						break;

					case InvoiceTypeTransport:
						if (Cartage != null)
						{
							result = Cartage.WaybillNumber;
						}

						break;

					case InvoiceTypeContainerRegistration:
						if (ContainerRegistration != null && ContainerRegistration.Shipment != null)
						{
							result = ContainerRegistration.Shipment.HouseBill;
						}

						break;
				}

				return result;
			}
		}

		public ZString ETD
		{
			get
			{
				ZString result = ZString.Empty;

				switch (InvoiceType)
				{
					case InvoiceTypeConsol:
						if (Consol != null)
						{
							result = Consol.ETD.ToString("dd/MM/yyyy");
						}

						break;

					case InvoiceTypeLoadList:
						if (LoadList != null)
						{
							result = LoadList.ETD.ToString("dd/MM/yyyy");
						}

						break;

					case InvoiceTypeShipment:
					case InvoiceTypeShipmentReceivals:
						if (Shipment != null)
						{
							result = Shipment.ETD.ToString("dd/MM/yyyy");
						}

						break;

					case InvoiceTypeCustoms:
						if (Declaration != null)
						{
							result = Declaration.ExportDate.ToString("dd/MM/yyyy");
						}

						break;

					case InvoiceTypeTransport:
						break;

					case InvoiceTypeContainerRegistration:
						if (ContainerRegistration != null && ContainerRegistration.Sailing != null)
						{
							result = ContainerRegistration.Sailing.ETD.ToString("dd/MM/yyyy");
						}

						break;
				}

				return result;
			}
		}

		public ZString PortOfLoading
		{
			get
			{
				ZString result = ZString.Empty;

				switch (InvoiceType)
				{
					case InvoiceTypeConsol:
						if (Consol != null && Consol.PortOfLoading != null)
						{
							result = Consol.PortOfLoading.PortName;
						}

						break;

					case InvoiceTypeLoadList:
						if (LoadList != null && LoadList.PortOfLoading != null)
						{
							result = LoadList.PortOfLoading.PortName;
						}

						break;

					case InvoiceTypeAgencyShipment:
						if (AgencyShipment != null && AgencyShipment.PortOfLoading != null)
						{
							result = AgencyShipment.PortOfLoading.PortName;
						}

						break;

					case InvoiceTypeShipment:
					case InvoiceTypeShipmentReceivals:
						if (Shipment != null && Shipment.Consol != null && Shipment.Consol.PortOfLoading != null)
						{
							result = Shipment.Consol.PortOfLoading.PortName;
						}

						break;

					case InvoiceTypeCustoms:
						if (Declaration != null && Declaration.Shipment != null && Declaration.Shipment.Consol != null && Declaration.Shipment.Consol.PortOfLoading != null)
						{
							result = Declaration.Shipment.Consol.PortOfLoading.PortName;
						}

						break;

					case InvoiceTypeTransport:
						break;

					case InvoiceTypeContainerRegistration:
						if (ContainerRegistration != null && ContainerRegistration.Consol != null && ContainerRegistration.Consol.PortOfLoading != null)
						{
							result = ContainerRegistration.Consol.PortOfLoading.PortName;
						}

						break;
				}

				return result;
			}
		}

		public ZString PortOfDischarge
		{
			get
			{
				ZString result = ZString.Empty;

				switch (InvoiceType)
				{
					case InvoiceTypeConsol:
						if (Consol != null && Consol.PortOfDischarge != null)
						{
							result = Consol.PortOfDischarge.PortName;
						}

						break;

					case InvoiceTypeLoadList:
						if (LoadList != null && LoadList.PortOfDischarge != null)
						{
							result = LoadList.PortOfDischarge.PortName;
						}

						break;

					case InvoiceTypeAgencyShipment:
						if (AgencyShipment != null && AgencyShipment.PortOfDischarge != null)
						{
							result = AgencyShipment.PortOfDischarge.PortName;
						}

						break;

					case InvoiceTypeShipment:
					case InvoiceTypeShipmentReceivals:
						if (Shipment != null && Shipment.Consol != null && Shipment.Consol.PortOfDischarge != null)
						{
							result = Shipment.Consol.PortOfDischarge.PortName;
						}

						break;

					case InvoiceTypeCustoms:
						if (Declaration != null && Declaration.Shipment != null && Declaration.Shipment.Consol != null && Declaration.Shipment.Consol.PortOfDischarge != null)
						{
							result = Declaration.Shipment.Consol.PortOfDischarge.PortName;
						}

						break;

					case InvoiceTypeTransport:
						break;

					case InvoiceTypeContainerRegistration:
						if (ContainerRegistration != null && ContainerRegistration.Consol != null && ContainerRegistration.Consol.PortOfDischarge != null)
						{
							result = ContainerRegistration.Consol.PortOfDischarge.PortName;
						}

						break;
				}

				return result;
			}
		}

		public ZString Destination
		{
			get
			{
				ZString result = ZString.Empty;

				switch (InvoiceType)
				{
					case InvoiceTypeConsol:
						break;

					case InvoiceTypeLoadList:
						break;

					case InvoiceTypeShipment:
					case InvoiceTypeShipmentReceivals:
						if (Shipment != null && Shipment.DestinationLoco != null)
						{
							result = Shipment.DestinationLoco.PortName;
						}

						break;

					case InvoiceTypeCustoms:
						if (Declaration != null && Declaration.Shipment != null && Declaration.Shipment.DestinationLoco != null)
						{
							result = Declaration.Shipment.DestinationLoco.PortName;
						}

						break;

					case InvoiceTypeTransport:
						break;

					case InvoiceTypeContainerRegistration:
						if (ContainerRegistration != null && ContainerRegistration.Shipment != null && ContainerRegistration.Shipment.DestinationLoco != null)
						{
							result = ContainerRegistration.Shipment.DestinationLoco.PortName;
						}

						break;
				}

				return result;
			}
		}

		public ZString Description
		{
			get
			{
				ZString result = ZString.Empty;

				if (LinesForInvoice != null)
				{
					for (int i = 0; i < LinesForInvoice.Count && i < DescriptionHeight; i++)
					{
						string temp = LinesForInvoice[i].LineDescription.Split('\n')[0];

						if (temp.Length > DescriptionWidth)
						{
							temp = temp.Substring(0, DescriptionWidth);
						}
						result += temp + "\n";
					}
				}

				return result.TrimEnd();
			}
		}

		public ZString Amount
		{
			get
			{
				ZString result = ZString.Empty;

				if (LinesForInvoice != null)
				{
					for (int i = 0; i < LinesForInvoice.Count && i < DescriptionHeight; i++)
					{
						result += GetLineOSExTaxAmount(LinesForInvoice[i]).ToString("N") + "\n";
					}
				}
				return result.TrimEnd();
			}
		}

		protected virtual ZDecimal GetLineOSExTaxAmount(IDocARInvoiceLine line)
		{
			return line.OSExTaxAmount;
		}

		public ZString ConsolNumber
		{
			get
			{
				ZString result = ZString.Empty;

				switch (InvoiceType)
				{
					case InvoiceTypeConsol:
						if (Consol != null)
						{
							result = Consol.ConsolNumber;
						}

						break;

					case InvoiceTypeLoadList:
						if (LoadList != null)
						{
							result = LoadList.ConsolNumber;
						}

						break;

					case InvoiceTypeShipment:
					case InvoiceTypeShipmentReceivals:
						if (Shipment != null && Shipment.Consol != null)
						{
							result = Shipment.Consol.ConsolNumber;
						}

						break;

					case InvoiceTypeCustoms:
						if (Declaration != null && Declaration.Shipment != null && Declaration.Shipment.Consol != null)
						{
							result = Declaration.Shipment.Consol.ConsolNumber;
						}

						break;

					case InvoiceTypeTransport:
						break;

					case InvoiceTypeContainerRegistration:
						if (ContainerRegistration != null && ContainerRegistration.Consol != null)
						{
							result = ContainerRegistration.Consol.ConsolNumber;
						}

						break;
				}

				return result;
			}
		}

		public ZString ShipmentNumber
		{
			get
			{
				ZString result = ZString.Empty;

				switch (InvoiceType)
				{
					case InvoiceTypeConsol:
						break;

					case InvoiceTypeLoadList:
						break;

					case InvoiceTypeShipment:
					case InvoiceTypeShipmentReceivals:
						if (Shipment != null)
						{
							result = Shipment.ShipmentNumber;
						}

						break;

					case InvoiceTypeCustoms:
						if (Declaration != null && Declaration.Shipment != null)
						{
							result = Declaration.Shipment.ShipmentNumber;
						}

						break;

					case InvoiceTypeTransport:
						break;

					case InvoiceTypeContainerRegistration:
						if (ContainerRegistration != null && ContainerRegistration.Shipment != null)
						{
							result = ContainerRegistration.Shipment.ShipmentNumber;
						}

						break;
				}

				return result;
			}
		}

		public ZString HXDNumber
		{
			get
			{
				ZString result = ZString.Empty;

				switch (InvoiceType)
				{
					case InvoiceTypeConsol:
						break;

					case InvoiceTypeLoadList:
						break;

					case InvoiceTypeShipment:
					case InvoiceTypeShipmentReceivals:
						if (Shipment != null)
						{
							result = Shipment.HXDNumber;
						}

						break;

					case InvoiceTypeCustoms:
						if (Declaration != null && Declaration.Shipment != null)
						{
							result = Declaration.Shipment.HXDNumber;
						}

						break;

					case InvoiceTypeTransport:
						break;

					case InvoiceTypeContainerRegistration:
						if (ContainerRegistration != null && ContainerRegistration.Shipment != null)
						{
							result = ContainerRegistration.Shipment.HXDNumber;
						}

						break;
				}

				return result;
			}
		}

		public ZString CreditBalanceMessage
		{
			get
			{
				ZString result = ZString.Empty;

				if (TransactionHeader.AH_TransactionType == TransactionTypes.CreditNote)
				{
					result = Res.GetString("dd46af76-3876-4ae9-98e3-b2d6975c7f0d", "THIS IS A CREDIT NOTE, NO PAYMENT REQUIRED");
				}
				else if (TransactionHeader.AH_TransactionType == TransactionTypes.AdjustmentNote && (TransactionHeader.AH_InvoiceAmount < 0))
				{
					result = Res.GetString("8427b50f-65d2-4739-9d9f-30385279347c", "THIS IS AN ADJUSTMENT NOTE, NO PAYMENT REQUIRED");
				}

				return result;
			}
		}

		public DocOrganisation USImporterOfRecord
		{
			get { return Declaration != null ? Declaration.ImporterOfRecord : null; }
		}

		public ZString EntryNumber
		{
			get { return Declaration != null ? Declaration.CustomsEntryNumber : ZString.Empty; }
		}

		public ZString EntryPortName
		{
			get { return Declaration != null ? Declaration.EntryPortName : ZString.Empty; }
		}

		public ZString EntryPortCode
		{
			get { return Declaration != null ? Declaration.EntryPortCode : ZString.Empty; }
		}

		public ZString PaymentType
		{
			get { return Declaration != null ? Declaration.PaymentType : ZString.Empty; }
		}

		public ZString PaymentTypeDescription
		{
			get { return Declaration != null ? Declaration.PaymentTypeDescription : ZString.Empty; }
		}

		public ZString InvoiceReversalData
		{
			get
			{
				InvoicingBase transactionBase = Factory.Load<InvoicingBase>(TransactionHeader.PK);
				ZString retVal = ZString.Empty;

				InvoicingBase correspondingTransactionBase = transactionBase.CorrespondingReversedTransaction;
				if (correspondingTransactionBase != null)
				{
					DocARInvoice correspondingTransaction = DocARInvoice.New(correspondingTransactionBase, Factory);

					bool rootCancelled = TransactionHeader.AH_TransactionBelongsToGroup.IsEmpty;

					if (rootCancelled)
					{
						retVal = Res.GetString("B2074C20-30C5-4529-B3BC-6CF71B3DD119", "{0} {1} WAS CANCELLED BY {2} {3} {4}",
								DocumentTitle, JobInvoiceNumber, correspondingTransaction.InvoiceDate.ToShortDateString(), correspondingTransaction.DocumentTitle, correspondingTransaction.JobInvoiceNumber);
					}
					else
					{
						retVal = Res.GetString("8E232666-FBA1-4136-8850-1DEBFDF6C42E", "{0} {1} CANCELS {2} {3} {4}",
								DocumentTitle, JobInvoiceNumber, correspondingTransaction.InvoiceDate.ToShortDateString(), correspondingTransaction.DocumentTitle, correspondingTransaction.JobInvoiceNumber);
					}
				}
				return retVal;
			}
		}

		public ZString ReversalOrAmendingReason
		{
			get
			{
				ZString reason = ZString.Empty;
				if (TransactionHeader != null && !TransactionHeader.AH_TransactionBelongsToGroup.IsEmpty)
				{
					if (!IsAmending(TransactionHeader))
					{
						reason = AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.Value.GetDescriptionFromCode(TransactionHeader.AH_ReceiptType);
					}
					else
					{
						IAmending amending = TransactionHeader as IAmending;
						if (amending != null && amending.IsAmendingTransaction)
						{
							reason = AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.Value.GetDescriptionFromCode(amending.AmendingReasonCode);
						}
					}
				}
				return reason;
			}
		}

		public ZString ReversalReason
		{
			get
			{
				return Res.GetString("6b8cbfd9-cfa3-4569-8d2e-2a4577c69aaa", "Reason for Reversal: {0}", GetReversalReasonDescription(TransactionHeader));
			}
		}

		ZString GetReversalReasonDescription(TransactionHeader header)
		{
			ZString reason = ZString.Empty;
			if (!header.AH_TransactionBelongsToGroup.IsEmpty && !IsAmending(header))
			{
				reason = AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.Value.GetDescriptionFromCode(header.AH_ReceiptType);
				if (reason.IsEmpty)
				{
					reason = AccountingMasterFilesRegistry.Instance.CreditNoteReasonCodesList.Value.GetDescriptionFromCode(header.AH_ReceiptType);
				}
			}
			else
			{
				InvoicingBase transactionBase = Factory.Load<InvoicingBase>(header.PK);
				InvoicingBase correspondingTransactionBase = transactionBase.CorrespondingReversedTransaction;
				if (correspondingTransactionBase != null)
				{
					reason = AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.Value.GetDescriptionFromCode(correspondingTransactionBase.AH_ReceiptType);
					if (reason.IsEmpty)
					{
						reason = AccountingMasterFilesRegistry.Instance.CreditNoteReasonCodesList.Value.GetDescriptionFromCode(correspondingTransactionBase.AH_ReceiptType);
					}
				}
			}

			if (reason.IsEmpty)
			{
				reason = OriginalReferenceReason;
			}

			return reason;
		}

		#endregion

		#region Amendment

		public ZString OriginalReferenceReason
		{
			get
			{
				var countrySpecificOriginalInvoiceReference = ObjectFactory.Get<ICountryComplianceFactory>()?.GetIOriginalInvoiceReference(TransactionHeader.Company.GC_RN_NKCountryCode);
				if (countrySpecificOriginalInvoiceReference != null)
				{
					return countrySpecificOriginalInvoiceReference.GetDocOriginalReferenceReason(InvoicingBase.ReasonDescription, TransactionHeader.AH_OriginalReferenceStartDate, TransactionHeader.AH_OriginalReferenceEndDate);
				}
				else
				{
					return InvoicingBase.ReasonDescription;
				}
			}
		}

		public ZBool IsAmendingTransaction
		{
			get
			{
				return IsAmending(TransactionHeader);
			}
		}

		ZBool IsAmending(TransactionHeader header)
		{
			var amending = header as IAmending;
			return amending != null && amending.IsAmendingTransaction;
		}

		public ZString AmendmentData
		{
			get
			{
				var retVal = ZString.Empty;

				IAmending amending = TransactionHeader as IAmending;
				if (amending != null && amending.IsAmendingTransaction)
				{
					if (amending.OriginalTransaction != null)
					{
						var originalTransactionBase = Factory.Load<InvoicingBase>(amending.OriginalTransaction.PK);
						var originalTransaction = DocARInvoice.New(originalTransactionBase, Factory);
						retVal = Res.GetString("F7285F9F-2454-493D-A2E1-A611D0ECC42C", "{0} {1} IS AN AMENDMENT TO {2} {3} {4}",
								DocumentTitle, JobInvoiceNumber, originalTransaction.InvoiceDate.ToShortDateString(), originalTransaction.DocumentTitle, originalTransaction.JobInvoiceNumber);
					}
					else
					{
						retVal = Res.GetString("6366C199-1649-4A6C-94C3-A20EF1899AB6", "{0} {1} IS AN AMENDMENT TO TRANSACTION {2} {3}",
		DocumentTitle, JobInvoiceNumber, TransactionHeader.AH_OriginalInvoiceDate.ToShortDateString(), TransactionHeader.AH_OriginalTransactionNum);
					}
				}

				return retVal;
			}
		}

		public ZString AmendmentReasonCode
		{
			get
			{
				var result = ZString.Empty;
				var amending = TransactionHeader as IAmending;
				if (amending != null && amending.IsAmendingTransaction)
				{
					result = amending.AmendingReasonCode;
				}
				return result;
			}
		}

		public ZString AmendmentReasonDescription
		{
			get
			{
				var result = ZString.Empty;
				if (!AmendmentReasonCode.IsEmpty)
				{
					result = AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.Value.GetDescriptionFromCode(AmendmentReasonCode);
				}
				return result;
			}
		}

		public ZString AmendmentReason
		{
			get
			{
				var result = ZString.Empty;
				if (!AmendmentReasonDescription.IsEmpty)
				{
					result = string.Format(Res.GetString("1bde2b21-e99d-4155-a9d7-2e7320fa1c84", "Reason for Amendment:") + " {0}", AmendmentReasonDescription);
				}
				return result;
			}
		}

		ZString GetAmendmentReasonDescription(TransactionHeader header)
		{
			ZString reason = ZString.Empty;

			IAmending amending = header as IAmending;
			if (amending != null && amending.IsAmendingTransaction)
			{
				reason = AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.Value.GetDescriptionFromCode(amending.AmendingReasonCode);
			}
			return reason;
		}

		ZBool IsAmendingTransactionForPeriodicInvoiceWithSingleJob
		{
			get
			{
				if (!isAmendingTransactionForPeriodicInvoiceWithSingleJob.HasValue)
				{
					SetPeriodicInvoiceAndAmendingTransactionRelatedFields();
				}
				return isAmendingTransactionForPeriodicInvoiceWithSingleJob.Value;
			}
		}
		ZBool? isAmendingTransactionForPeriodicInvoiceWithSingleJob;

		protected ZBool IsAmendingTransactionForPeriodicInvoiceWithMultipleJobs
		{
			get
			{
				if (!isAmendingTransactionForPeriodicInvoiceWithMultipleJobs.HasValue)
				{
					SetPeriodicInvoiceAndAmendingTransactionRelatedFields();
				}
				return isAmendingTransactionForPeriodicInvoiceWithMultipleJobs.Value;
			}
		}
		ZBool? isAmendingTransactionForPeriodicInvoiceWithMultipleJobs;

		JobHeader JobRelatedToAmendingTransactionCreatedForPeriodicInvoice
		{
			get
			{
				if (jobRelatedToAmendingTransactionCreatedForPeriodicInvoice == null)
				{
					SetPeriodicInvoiceAndAmendingTransactionRelatedFields();
				}
				return jobRelatedToAmendingTransactionCreatedForPeriodicInvoice;
			}
		}
		JobHeader jobRelatedToAmendingTransactionCreatedForPeriodicInvoice;

		void SetPeriodicInvoiceAndAmendingTransactionRelatedFields()
		{
			if (!periodicInvoiceAndAmendingTransactionRelatedFieldsSet)
			{
				var (isAmendingTransactionForPeriodicInvoice, isSingleJob, job) = TransactionHeader.CheckIsAmendingTransactionForPeriodicInvoice();
				isAmendingTransactionForPeriodicInvoiceWithSingleJob = isAmendingTransactionForPeriodicInvoice && isSingleJob;
				isAmendingTransactionForPeriodicInvoiceWithMultipleJobs = isAmendingTransactionForPeriodicInvoice && !isSingleJob;
				jobRelatedToAmendingTransactionCreatedForPeriodicInvoice = (isAmendingTransactionForPeriodicInvoice && isSingleJob) ? job : null;
				periodicInvoiceAndAmendingTransactionRelatedFieldsSet = true;
			}
		}
		bool periodicInvoiceAndAmendingTransactionRelatedFieldsSet;

		#endregion

		#region Transaction Verb

		public ZString TransactionVerb
		{
			get
			{
				var result = ZString.Empty;
				if (TransactionHeader != null && !TransactionHeader.AH_TransactionBelongsToGroup.IsEmpty)
				{
					if (TransactionHeader.IsAmendingTransaction)
					{
						result = Res.GetString("9e7052f0-09e5-4804-9287-f83623f9bdd2", "amends");
					}
					else if (TransactionHeader.IsReversalTransaction)
					{
						result = Res.GetString("b6e15fb2-da07-4bfa-9c55-f1e7758e6ef5", "reverses");
					}
				}
				return result;
			}
		}

		#endregion

		#region Related invoice

		InvoicingBase RelatedInvoice
		{
			get
			{
				return !TransactionHeader.AH_TransactionBelongsToGroup.IsEmpty ? Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, TransactionHeader.AH_TransactionBelongsToGroup)) : null;
			}
		}

		InvoicingBase[] RelatedToInvoices
		{
			get
			{
				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, InvoicingBase.PK);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.Equal, InvoicingBase.AH_GC);
				return Factory.Load<InvoicingBase>(filter);
			}
		}

		public ZString RelatedTransactionJobInvoiceNumber
		{
			get
			{
				return RelatedInvoice != null ? RelatedInvoice.InvoiceNumberPrefixed : ZString.Empty;
			}
		}

		public ZString RelatedTransactionComplianceSubType
		{
			get
			{
				return RelatedInvoice != null ? RelatedInvoice.AH_ComplianceSubType : ZString.Empty;
			}
		}

		public ZString RelatedTransactionGovernmentComplianceNumber
		{
			get
			{
				return RelatedInvoice != null ? RelatedInvoice.AH_TransactionReference : ZString.Empty;
			}
		}

		public ZString ReasonForAmendmentOrReversal
		{
			get
			{
				ZString result = ZString.Empty;
				if (TransactionHeader != null && !TransactionHeader.AH_TransactionBelongsToGroup.IsEmpty)
				{
					result = GetAmendmentReasonDescription(TransactionHeader);
					if (result.IsEmpty)
					{
						result = GetReversalReasonDescription(TransactionHeader);
					}
				}
				return result;
			}
		}

		#endregion

		#region ZDateTime Fields

		public new ZDateTime DueDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (!IsCreditNote)
				{
					result = TransactionHeader.AH_DueDate;
				}

				return result;
			}
		}

		#endregion

		#region ZDecimal

		public ZDecimal OSTotalExcludeSPVAmount
		{
			get { return OSTotal - TotalOSSPVAmount; }
		}

		public new ZDecimal OSTotal
		{
			get { return OSTotalCore * AmountMultiplierForARCreditNote; }
		}

		public ZString OSTotalRawFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(OSExTaxAmount + TotalOSTaxAmountRaw, Currency);
			}
		}
		public ZString OSTotalFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(OSTotal, Currency);
			}
		}

		public ZString OSTotalFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(OSTotal * RevertSignForARCreditNote, Currency);
			}
		}

		protected virtual ZDecimal OSTotalCore
		{
			get
			{
				var roundingScale = TransactionHeader.TransactionCurrency != null ? TransactionHeader.TransactionCurrency.Decimals : GlbCompany.CurrentCompany.LocalCurrency.Decimals;
				return ZArchitecture.Core.Utilities.Round(TransactionHeader.AH_OSTotalAmount, roundingScale);
			}
		}

		public ZDecimal LocalTotal
		{
			get { return LocalTotalCore * AmountMultiplierForARCreditNote; }
		}

		public ZString LocalTotalFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(LocalTotal, GlbCompany.CurrentCompany.LocalCurrency);
			}
		}

		public ZString LocalTotalFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(LocalTotal * RevertSignForARCreditNote, GlbCompany.CurrentCompany.LocalCurrency);
			}
		}

		protected virtual ZDecimal LocalTotalCore
		{
			get
			{
				return ZArchitecture.Core.Utilities.Round(TransactionHeader.AH_LocalTotalAmount, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			}
		}

		public ZDecimal TotalOSTaxAmount
		{
			get { return TotalOSTaxAmountCore; }
		}

		public ZString TotalOSTaxAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSTaxAmount, Currency);
			}
		}

		public ZDecimal TotalOSTaxAmountRaw
		{
			get { return TotalOSTaxAmountRawCore; }
		}

		public ZString OSDocumentTitleForITAutofattura
		{
			get
			{
				var result = TransactionHeader.AH_TransactionType == TransactionTypes.Invoice ||
							 TransactionHeader.AH_TransactionType == TransactionTypes.AdjustmentNote ?
						Res.GetString("1E4D1C1A-62E6-4AE3-891E-90B40989A1D8", "Autofattura") :
						Res.GetString("B16FC650-2F48-4847-9E3A-69636762CECE", "Autoaccredito");
				return result;
			}
		}

		public ZString TotalOSTaxAmountRawFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSTaxAmountRaw, Currency);
			}
		}

		public ZString TotalOSTaxAmountFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSTaxAmount * RevertSignForARCreditNote, Currency);
			}
		}

		public ZString TotalOSIGICAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSIGICAmount, Currency);
			}
		}

		public ZString TotalOSIGICAmountFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSIGICAmount * RevertSignForARCreditNote, Currency);
			}
		}

		public ZString TotalOSSERAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSSERAmount, Currency);
			}
		}

		public ZString TotalOSSERAmountFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSSERAmount * RevertSignForARCreditNote, Currency);
			}
		}

		public ZString TotalOSQSTAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSQSTAmount, Currency);
			}
		}

		public ZString TotalOSSBCAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSSBCAmount, Currency);
			}
		}

		public ZString TotalOSKKCAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSKKCAmount, Currency);
			}
		}

		public ZString TotalOSQSTAmountFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSQSTAmount * RevertSignForARCreditNote, Currency);
			}
		}

		public ZString TotalOSEDUAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSEDUAmount, Currency);
			}
		}

		public ZString TotalOSEDUAmountFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSEDUAmount * RevertSignForARCreditNote, Currency);
			}
		}

		public ZString TotalOSEDUPrimaryAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSEDUPrimaryAmount, Currency);
			}
		}

		public ZString TotalOSEDUPrimaryAmountFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSEDUPrimaryAmount * RevertSignForARCreditNote, Currency);
			}
		}

		public ZString TotalOSEDUSecondaryAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSEDUSecondaryAmount, Currency);
			}
		}

		public ZString TotalOSEDUSecondaryAmountFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSEDUSecondaryAmount * RevertSignForARCreditNote, Currency);
			}
		}

		public ZString TotalOSRETAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSRETAmount, Currency);
			}
		}

		public ZString TotalOSRETAmountFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSRETAmount * RevertSignForARCreditNote, Currency);
			}
		}

		public ZString TotalOSVATExcludeSPVAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(-TotalOSSPVAmount, Currency);
			}
		}

		public ZString TotalOSVATExcludeSPVAmountFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(-TotalOSSPVAmount * RevertSignForARCreditNote, Currency);
			}
		}

		public ZString TotalOSSPVAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSSPVAmount, Currency);
			}
		}

		public ZString TotalOSSPVAmountFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSSPVAmount * RevertSignForARCreditNote, Currency);
			}
		}

		public ZString TotalLocalSPVAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(-TotalLocalSPVAmount, GlbCompany.CurrentCompany.LocalCurrency);
			}
		}

		public ZString OSTotalExcludeSPVAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(OSTotalExcludeSPVAmount, Currency);
			}
		}

		public ZString OSTotalExcludeSPVAmountFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(OSTotalExcludeSPVAmount * RevertSignForARCreditNote, Currency);
			}
		}

		protected virtual ZDecimal TotalOSTaxAmountCore
		{
			get
			{
				ZDecimal result = 0M;
				ZBool hasGSTANDQSTLine = HasGSTANDQSTLine;
				ZBool hasGSTANDEDULine = HasGSTANDEDULine;
				ZBool hasRETLine = HasRETLine;
				ZBool hasVATANDIGICLine = HasVATANDIGICLine;
				foreach (DocARInvoiceLine line in Lines)
				{
					if (!line.OSTaxDisplay.IsEmpty && line.OSTaxDisplay != Res.GetString("a990c1ed-496f-4375-87c6-150e8e725927", "N/A"))
					{
						if (line.HasIndiaIntegratedOrStateGST)
						{
							continue;
						}
						else if (hasVATANDIGICLine)
						{
							result += Convert.ToDecimal(line.OSTaxAmount);
						}
						else
						{
							result += Convert.ToDecimal(hasGSTANDQSTLine || hasGSTANDEDULine || hasRETLine ? line.OSGSTAmount : line.OSTaxAmount);
						}
					}
				}
				return result;
			}
		}

		protected virtual ZDecimal TotalOSTaxAmountRawCore
		{
			get
			{
				ZDecimal result = 0M;
				foreach (DocARInvoiceLine line in Lines)
				{
					if (!line.OSTaxDisplay.IsEmpty && line.OSTaxDisplay != Res.GetString("a990c1ed-496f-4375-87c6-150e8e725927", "N/A"))
					{
						result += Convert.ToDecimal(line.OSTaxAmount_Raw);
					}
				}
				return result;
			}
		}

		public ZDecimal TotalOSIGICAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoiceLine line in Lines)
				{
					result += Convert.ToDecimal(line.OSIGICAmount);
				}
				return result;
			}
		}

		public ZDecimal TotalOSSERAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoiceLine line in Lines)
				{
					result += Convert.ToDecimal(line.OSSERAmount);
				}
				return result;
			}
		}

		public ZDecimal TotalOSQSTAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoiceLine line in Lines)
				{
					result += Convert.ToDecimal(line.OSQSTAmount);
				}
				return result;
			}
		}

		public ZDecimal TotalOSSBCAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoiceLine line in Lines)
				{
					result += Convert.ToDecimal(line.OSSBCAmount);
				}
				var roundingScale = TransactionHeader.TransactionCurrency != null ? TransactionHeader.TransactionCurrency.Decimals : GlbCompany.CurrentCompany.LocalCurrency.Decimals;
				return Utilities.Round(result, roundingScale);
			}
		}

		public ZDecimal TotalOSKKCAmount
		{
			get
			{
				ZDecimal result = 0m;
				if (TotalOSQSTAmount != ZDecimal.Zero)
				{
					result = TotalOSQSTAmount - TotalOSSBCAmount;
				}
				return result;
			}
		}

		public ZDecimal TotalOSEDUAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoiceLine line in Lines)
				{
					result += Convert.ToDecimal(line.OSEDUAmount);
				}
				return result;
			}
		}

		public ZDecimal TotalOSEDUPrimaryAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoiceLine line in Lines)
				{
					result += Convert.ToDecimal(line.OSEDUPrimaryAmount);
				}
				return result;
			}
		}

		public ZDecimal TotalOSEDUSecondaryAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoiceLine line in Lines)
				{
					result += Convert.ToDecimal(line.OSEDUSecondaryAmount);
				}
				return result;
			}
		}

		public ZDecimal TotalOSRETAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoiceLine line in Lines)
				{
					result += Convert.ToDecimal(line.OSRETAmount);
				}
				return result;
			}
		}

		public ZDecimal TotalOSSPVAmount
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Sum(x => x.OSSPVAmount);
			}
		}

		public ZDecimal TotalLocalSPVAmount
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Sum(x => x.LocalSPVAmount);
			}
		}

		public ZDecimal TotalOSIntegratedGSTAmount
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Sum(x => x.OSIntegratedGSTAmount);
			}
		}

		public ZDecimal TotalOSCentreGSTAmount
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Sum(x => x.OSCentreGSTAmount);
			}
		}

		public ZDecimal TotalOSStateGSTAmount
		{
			get
			{
				return Lines.Cast<DocARInvoiceLine>().Sum(x => x.OSStateGSTAmount);
			}
		}

		public ZString TotalOSIntegratedGSTAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSIntegratedGSTAmount, Currency);
			}
		}

		public ZString TotalOSCentreGSTAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSCentreGSTAmount, Currency);
			}
		}

		public ZString TotalOSStateGSTAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSStateGSTAmount, Currency);
			}
		}

		#region InvoiceSubTotal

		public ZDecimal InvoiceSubTotal
		{
			get
			{
				return InvoiceSubTotalCore;
			}
		}

		public ZString InvoiceSubTotalFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(InvoiceSubTotal, Currency);
			}
		}

		public ZString InvoiceSubTotalFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(InvoiceSubTotal * RevertSignForARCreditNote, Currency);
			}
		}

		protected virtual ZDecimal InvoiceSubTotalCore
		{
			get
			{
				ZDecimal result = 0M;

				foreach (DocARInvoiceLine line in Lines)
				{
					result += (Organisation == null ? ZBool.False : Organisation.MiscServ.ARDontShowTaxOnDocs) ? line.OSAmount : line.OSExTaxAmount;
				}

				return result;
			}
		}

		#endregion

		#region InvoiceLocalSubTotal

		public ZDecimal InvoiceLocalSubTotal
		{
			get
			{
				return InvoiceLocalSubTotalCore;
			}
		}

		public ZString InvoiceLocalSubTotalFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(InvoiceLocalSubTotal, GlbCompany.CurrentCompany.LocalCurrency);
			}
		}

		public ZString InvoiceLocalSubTotalFormattedWithSign
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(InvoiceLocalSubTotal * RevertSignForARCreditNote, GlbCompany.CurrentCompany.LocalCurrency);
			}
		}

		protected virtual ZDecimal InvoiceLocalSubTotalCore
		{
			get
			{
				ZDecimal result = 0M;

				foreach (DocARInvoiceLine line in Lines)
				{
					result += (Organisation == null ? ZBool.False : Organisation.MiscServ.ARDontShowTaxOnDocs) ? line.LocalAmount : line.LocalExTaxAmount;
				}

				return result;
			}
		}

		#endregion

		public ZString OSOutstandingAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(this.TransactionHeader.OSOutstandingAmountMatching * AmountMultiplierForARCreditNote, Currency);
			}
		}

		public ZDecimal ARAdvancePaymentReceivedAmount
		{
			get
			{
				ZDecimal totalAdvancePaymentReceivedAmount = 0;
				foreach (DocARInvoiceLine line in Lines)
				{
					var advancePaymentLine = line.Charge?.JobCharge?.ARCashAdvanceRequestLine; //should I rename ArCashAdvanceRequestLine?
					if (advancePaymentLine != null)
					{
						totalAdvancePaymentReceivedAmount += advancePaymentLine.CAL_OSPaidAmount;
					}
				}
				return totalAdvancePaymentReceivedAmount;
			}
		}

		public ZString ARAdvancePaymentReceivedAmountFormatted => FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(ARAdvancePaymentReceivedAmount, Currency);

		#endregion

		#region Outstanding Amount for Multiple Installments

		public ZString OSOutstandingAmountForMLIFormatted
		{
			get
			{
				if (MultipleInstallments.IsNullOrEmpty())
				{
					return OSOutstandingAmountFormatted;
				}

				ZDecimal amount = ZDecimal.Zero;

				foreach (var installment in MultipleInstallments.Cast<DocTransactionHeader>())
				{
					amount += installment.OSOutstandingAmount;
				}
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(amount, Currency);
			}
		}

		#endregion

		#region China Class A Related Field

		#region Business Reg. No.

		ZString fBusinessRegNo;
		public ZString BusinessRegNo
		{
			get
			{
				if (fBusinessRegNo.IsEmpty)
				{
					fBusinessRegNo = GetOrgRegistrationCode(OrgCusCode.CodeTypes.GovBusinessCode);
				}
				return fBusinessRegNo;
			}
		}

		ZString GetOrgRegistrationCode(string cusCode)
		{
			ZString regNo = "";
			if (GlbBranch.CurrentBranch.OrgProxy != null)
			{
				OrgCusCode businessRegNo = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(
						cusCode, GlbCompany.CurrentCompany.Country);
				if (businessRegNo != null)
				{
					regNo = businessRegNo.OK_CustomsRegNo;
				}
			}
			return regNo;
		}

		#endregion

		#region Tax Reg. No.

		ZString fTaxRegNo;
		public ZString TaxRegNo
		{
			get
			{
				if (fTaxRegNo.IsEmpty)
				{
					fTaxRegNo = GetOrgRegistrationCode(OrgCusCode.CodeTypes.TaxFileCode);
				}
				return fTaxRegNo;
			}
		}

		#endregion

		#region Business Seal

		ZString fBusinessSeal;
		public ZString BusinessSeal
		{
			get
			{
				if (fBusinessSeal.IsEmpty)
				{
					if (GlbBranch.CurrentBranch.OrgProxy != null)
					{
						OrgAddressList addresses = GlbBranch.CurrentBranch.OrgProxy.Addresses.AddressesOfType(OrgConstants.AddressType.Receivables);
						foreach (OrgAddress currentAddress in addresses)
						{
							if (currentAddress.AddressCapability.GetIsMainAddress(OrgConstants.AddressType.Receivables))
							{
								fBusinessSeal = currentAddress.OA_CompanyNameOverride;
								break;
							}
						}
					}
				}
				return fBusinessSeal;
			}
		}

		#endregion

		#endregion

		#region Terms & Conditions

		public ZBool PrintBackPage
		{
			get { return TermsAndConditions != null && !ImageRegistryDataType.IsNullDataRepresentation(TermsAndConditions); }
		}

		public Image TermsAndConditions
		{
			get { return (IncludeTradingTerms) ? GetTermsAndConditions() : null; }
		}

		Image GetTermsAndConditions()
		{
			if (termsAndConditions?.IsDisposed() ?? false)
			{
				termsAndConditions = null;
			}

			return termsAndConditions ?? (termsAndConditions = AccountingConfigurationRegistry.Instance.InvoiceTradingTerms.Value);
		}

		Image termsAndConditions;

		#endregion

		#region Payment Remittance

		public ZString InvoiceRemittanceType
		{
			get
			{
				return InvoicingBase.AH_InvoicePaymentReferenceCode;
			}
		}

		public ZString InvoiceRemittanceReference
		{
			get
			{
				return InvoicingBase.InvoiceRemittanceReference;
			}
		}

		public ZString InvoiceNumber
		{
			get
			{
				return ((IInvoiceRemittance)InvoicingBase).InvoiceNumber;
			}
		}

		public ZString InvoiceTotalInLocalCurrency
		{
			get
			{
				return ((IInvoiceRemittance)InvoicingBase).InvoiceTotalInLocalCurrency;
			}
		}

		public ZString InvoiceTotalInInvoiceCurrency
		{
			get
			{
				return ((IInvoiceRemittance)InvoicingBase).InvoiceTotalInInvoiceCurrency;
			}
		}

		public ZString InvoiceRemittanceMessage
		{
			get
			{
				return ((IInvoiceRemittance)InvoicingBase).Message;
			}
		}

		public ZString InvoiceRemittanceBillerCode
		{
			get
			{
				return ((IInvoiceRemittance)InvoicingBase).BillerCode;
			}
		}

		public ZString InvoiceRemittanceBillerAccountNumber
		{
			get
			{
				return ((IInvoiceRemittance)InvoicingBase).BillerAccountNumber;
			}
		}

		public ZString DebtorOrganizationCode
		{
			get
			{
				return ((IInvoiceRemittance)InvoicingBase).DebtorOrganizationCode;
			}
		}

		public ZString DebtorClientNumber
		{
			get
			{
				return ((IInvoiceRemittance)InvoicingBase).DebtorClientNumber;
			}
		}

		public ZString InvoiceTransactionReference
		{
			get
			{
				return ((IInvoiceRemittance)InvoicingBase).InvoiceTransactionReference;
			}
		}

		#endregion

		#region Suspended Tax

		public ZBool HasSuspendedTax
		{
			get
			{
				return (from DocARInvoiceLine line in Lines
						where line.TaxRate != null && line.TaxRate.AccTaxRate != null &&
									line.TaxRate.AccTaxRate.AT_Type == AccTaxRate.Types.Suspended
						select line).Any();
			}
		}

		ZString SuspendedTaxTitleCore(ZBool useZeroAmountTaxDescriptionOverride)
		{
			ZString result = ZString.Empty;

			var taxRates = GetSuspendedTaxRates();

			if (taxRates.Any())
			{
				result = (useZeroAmountTaxDescriptionOverride ? (string)AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.Suspended) : Res.GetString("63858a3c-7811-4e4d-9047-1e39f38d530a", "Suspended"))
								 + " " + TranslatedTaxCode + (taxRates.Length == 1 ? " @" + taxRates[0].ToStringTrimZeros() + "%" : "");
			}
			return result.ToUpper();
		}

		public ZString SuspendedTaxTitle
		{
			get
			{
				return SuspendedTaxTitleCore(false);
			}
		}

		public ZString SuspendedTaxTitleWithDescriptionOverride
		{
			get
			{
				return SuspendedTaxTitleCore(true);
			}
		}

		public ZDecimal SuspendedTaxAmount
		{
			get
			{
				return (from DocARInvoiceLine line in Lines
						where line.TaxRate != null && line.TaxRate.AccTaxRate != null &&
									line.TaxRate.AccTaxRate.AT_Type == AccTaxRate.Types.Suspended
						group line by line.TaxRateAmount_Raw into taxRateGroup
						select new
						{
							suspendedTaxAmount = (taxRateGroup.Sum(x => (x.OSExTaxAmount) * taxRateGroup.Key) / 100m)
						}).Sum(x => x.suspendedTaxAmount);
			}
		}

		ZString TotalValueIncludingSuspendedTaxTitleCore(ZBool useZeroAmountTaxDescriptionOverride)
		{
			ZString result = ZString.Empty;

			var taxRates = GetSuspendedTaxRates();

			if (taxRates.Any())
			{
				result = Res.GetString("a6629dc9-5744-498b-abb9-eb1231e5c617", "Total Value of Supply Including {0} {1}",
						(useZeroAmountTaxDescriptionOverride ? (string)AccountingHelperClass.ZeroAmountTaxTypesDescriptionValue(AccTaxRate.Types.Suspended) : Res.GetString("727d02b0-d176-41b3-aa59-a9af4d80f5af", "Suspended")),
						TranslatedTaxCode);
			}
			return result.ToUpper();
		}

		public ZString TotalValueIncludingSuspendedTaxTitle
		{
			get
			{
				return TotalValueIncludingSuspendedTaxTitleCore(false);
			}
		}

		public ZString TotalValueIncludingSuspendedTaxTitleWithDescriptionOverride
		{
			get
			{
				return TotalValueIncludingSuspendedTaxTitleCore(true);
			}
		}

		public ZDecimal TotalValueIncludingSuspendedTax
		{
			get
			{
				return TotalOSAmount + SuspendedTaxAmount;
			}
		}

		ZDecimal[] GetSuspendedTaxRates()
		{
			return (from DocARInvoiceLine line in Lines
					where line.TaxRate != null && line.TaxRate.AccTaxRate != null &&
								line.TaxRate.AccTaxRate.AT_Type == AccTaxRate.Types.Suspended
					select line.TaxRateAmount_Raw).Distinct().Take(2).ToArray();
		}

		#endregion

		#endregion

		#region Roll Up

		internal static MultilingualString GetDescriptionByStyleAndGroup(ZString styleId, ZString groupId) => AccountingConfigurationRegistry.Instance.InvoiceRollupAndGroupDescriptionRegistryItem.GetDescription(styleId, groupId);

		internal ZBool IsExportDepartment
		{
			get { return Department != null && Department.Export; }
		}

		internal ZBool IsImportDepartment
		{
			get { return Department != null && Department.Import; }
		}

		#endregion

		#region CopyInformation

		public ZBool IncludeTradingTerms
		{
			get { return WrapperInfo.IncludeTradingTerms; }
		}

		protected ARInvoiceDocWrapperCopyInfo WrapperInfo
		{
			get
			{
				if (fWrapperInfo == null)
				{
					if (CopyWrapperInfo != null)
					{
						fWrapperInfo = CopyWrapperInfo;
					}
					else
					{
						fWrapperInfo = new ARInvoiceDocWrapperCopyInfo(OriginalWrapperInfo);
					}
				}
				return fWrapperInfo;
			}
		}

		ARInvoiceDocWrapperCopyInfo fWrapperInfo;

		InvoiceCopy OriginalWrapperInfo
		{
			get
			{
				if (fOriginalWrapperInfo == null)
				{
					InvoiceCopyCollection collection = AccountingConfigurationRegistry.Instance.InvoiceCopies.Value;
					foreach (InvoiceCopy entry in collection)
					{
						if (entry.IsOriginal)
						{
							fOriginalWrapperInfo = entry;
						}
					}
				}
				return fOriginalWrapperInfo;
			}
		}

		InvoiceCopy fOriginalWrapperInfo;

		ARInvoiceDocWrapperCopyInfo CopyWrapperInfo
		{
			get { return (ARInvoiceDocWrapperCopyInfo)((IBODocDataProvider)this).AdditionalCopyInfo; }
		}

		#endregion

		#region Claims

		public DocAccQueryClaimCollection AccClaimsForInvoice
		{
			get
			{
				if (fAccClaimsForInvoice == null)
				{
					fAccClaimsForInvoice = new DocAccQueryClaimCollection(Factory);

					ZQuery sQLFilter = new ZQuery(AccQueryClaimSchema.AY_AH, InvoicingBase.PK);
					ARAccQueryClaimCollection claimsCollection = new ARAccQueryClaimCollection(Factory, sQLFilter);
					claimsCollection.Load();

					foreach (ARAccQueryClaim claim in claimsCollection)
					{
						DocAccQueryClaim docClaim = DocAccQueryClaim.New(claim, Factory);
						fAccClaimsForInvoice.Add(docClaim);
					}
				}

				return fAccClaimsForInvoice;
			}
		}

		DocAccQueryClaimCollection fAccClaimsForInvoice;

		public ZString ClaimInfoAsString
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				foreach (DocAccQueryClaim claim in AccClaimsForInvoice)
				{
					ZString claimInfo = "(" + claim.Reference + ") "
															+ claim.ShortDescription + " "
															+ claim.Details;
					builder.Append(claimInfo);
					builder.Append(" , ");
				}

				return builder.ToString().Trim().TrimEnd(',').Trim();
			}
		}

		#endregion

		#region RecipientNameAddress

		public ZString RecipientNameAddress
		{
			get
			{
				var result = ZString.Empty;
				var proxy = TransactionHeader.Branch.OrgProxy ?? TransactionHeader.Company.OrgProxy;
				if (proxy != null)
				{
					var addresses = proxy.Addresses.OfType<OrgAddress>().Where(x => x.OA_IsActive);
					foreach (OrgAddress orgAddress in addresses)
					{
						if (orgAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Receivables))
						{
							result = GetFormattedAddress(orgAddress);
							break;
						}
					}
					if (result == ZString.Empty)
					{
						foreach (OrgAddress orgAddress in addresses)
						{
							if (orgAddress.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Office))
							{
								result = GetFormattedAddress(orgAddress);
								break;
							}
						}
					}
					result = proxy.OH_FullName + System.Environment.NewLine + result;
				}
				return result;
			}
		}

		ZString GetFormattedAddress(OrgAddress address)
		{
			ZString result = ZString.Empty;
			if (address != null)
			{
				if (address.OA_Address1 != ZString.Empty)
				{
					result = address.OA_Address1 + System.Environment.NewLine;
				}

				if (address.OA_Address2 != ZString.Empty)
				{
					result = result + address.OA_Address2 + System.Environment.NewLine;
				}

				if (address.OA_City != ZString.Empty)
				{
					result = result + address.OA_City + " ";
				}

				if (address.OA_State != ZString.Empty)
				{
					result = result + address.OA_State + " ";
				}

				result = result + address.OA_PostCode;
			}
			return result;
		}

		#endregion

		#region Group By Rate and Message

		IOrderedEnumerable<Tuple<string, ZDecimal, List<DocARInvoiceLine>>> LinesGroupedByRateAndMessageCore(ref List<Tuple<string, ZDecimal, List<DocARInvoiceLine>>> result, ZBool useZeroAmountTaxTypesDescriptionFromRegistry)
		{
			if (result == null)
			{
				result = new List<Tuple<string, ZDecimal, List<DocARInvoiceLine>>>();
				BuildTaxMessagesToAsterisksMapping();

				string message;

				var mainGroups = Lines.Where(y => ((DocARInvoiceLine)y).TaxRate != null)
						.GroupBy(x => new
						{
							AL_A9_VATClass = (TaxMessagesToAsterisksMapping.ContainsKey(((DocARInvoiceLine)x).Line.AL_A9_VATClass) ? ((DocARInvoiceLine)x).Line.AL_A9_VATClass : ZGuid.Empty)
								,
							((DocARInvoiceLine)x).Line.TaxRate.AT_Type,
							((DocARInvoiceLine)x).Line.TaxRate.AT_ExtraTaxRateType
						});

				foreach (var mainGroup in mainGroups)
				{
					if (TaxMessagesToAsterisksMapping.ContainsKey(mainGroup.Key.AL_A9_VATClass))
					{
						var subGroups = mainGroup.GroupBy(x => new { ((DocARInvoiceLine)x).TaxRateAmount_Raw, ((DocARInvoiceLine)x).Line.AL_TaxExtraRateCalc });

						foreach (var subGroup in subGroups)
						{
							List<DocARInvoiceLine> lines = new List<DocARInvoiceLine>();
							foreach (DocARInvoiceLine docARInvoiceLine in subGroup)
							{
								lines.Add(docARInvoiceLine);
							}

							var mapping = TaxMessagesToAsterisksMapping[mainGroup.Key.AL_A9_VATClass];
							if (IsLocalInvoice())
							{
								message = string.Format("{0} {1}", mapping.Item1, !string.IsNullOrEmpty(mapping.Item3) ? mapping.Item3 : mapping.Item2);
							}
							else
							{
								message = string.Format("{0} {1}", mapping.Item1, mapping.Item2);
							}

							result.Add(new Tuple<string, ZDecimal, List<DocARInvoiceLine>>(message, lines.Sum(x => x.OSExTaxAmount), lines));
						}
					}
					else
					{
						if (!(DocTaxRate.IsRatedTax(mainGroup.Key.AT_Type) || mainGroup.Key.AT_Type == AccTaxRate.Types.CapitalRated))
						{
							var lines = new List<DocARInvoiceLine>();

							foreach (DocARInvoiceLine docARInvoiceLine in mainGroup)
							{
								lines.Add(docARInvoiceLine);
							}

							if (lines.Count > 0)
							{
								if (useZeroAmountTaxTypesDescriptionFromRegistry)
								{
									message = lines.FirstOrDefault().GetTaxAmountDisplayWithRegistryRule();
								}
								else
								{
									message = lines.FirstOrDefault().GetTaxAmountDisplay();
								}
								result.Add(new Tuple<string, ZDecimal, List<DocARInvoiceLine>>(message, lines.Sum(x => x.OSExTaxAmount), lines));
							}
						}
						else
						{
							var subGroups = mainGroup.GroupBy(x => new { ((DocARInvoiceLine)x).TaxRateAmount_Raw, ((DocARInvoiceLine)x).Line.AL_TaxExtraRateCalc });

							foreach (var subGroup in subGroups)
							{
								var lines = new List<DocARInvoiceLine>();

								foreach (DocARInvoiceLine docARInvoiceLine in subGroup)
								{
									lines.Add(docARInvoiceLine);
								}

								if (lines.Count > 0)
								{
									if (subGroup.Key.TaxRateAmount_Raw == 0M)
									{
										if (useZeroAmountTaxTypesDescriptionFromRegistry)
										{
											message = lines.FirstOrDefault().GetTaxAmountDisplayWithRegistryRule();
										}
										else
										{
											message = lines.FirstOrDefault().GetTaxAmountDisplay();
										}
									}
									else
									{
										message = string.Format("{0}{1}", mainGroup.Key.AT_Type, subGroup.Key);
									}

									result.Add(new Tuple<string, ZDecimal, List<DocARInvoiceLine>>(message, lines.Sum(x => x.OSExTaxAmount), lines));
								}
							}
						}
					}
				}
			}

			return result.OrderByDescending(x => x.Item2);
		}

		IOrderedEnumerable<Tuple<string, ZDecimal, List<DocARInvoiceLine>>> LinesGroupedByRateAndMessage
		{
			get
			{
				return LinesGroupedByRateAndMessageCore(ref linesGroupedByRateAndMessage, false);
			}
		}
		List<Tuple<string, ZDecimal, List<DocARInvoiceLine>>> linesGroupedByRateAndMessage;

		IOrderedEnumerable<Tuple<string, ZDecimal, List<DocARInvoiceLine>>> LinesGroupedByRateAndMessageWithZeroAmountTaxDescriptionFromRegistry
		{
			get
			{
				return LinesGroupedByRateAndMessageCore(ref linesGroupedByRateAndMessageWithZeroAmountTaxDescriptionFromRegistry, true);
			}
		}
		List<Tuple<string, ZDecimal, List<DocARInvoiceLine>>> linesGroupedByRateAndMessageWithZeroAmountTaxDescriptionFromRegistry;

#if DEBUG

		internal void ResetLinesGroupedByRateAndMessageForTestOnly()
		{
			linesGroupedByRateAndMessage = null;
			linesGroupedByRateAndMessageWithZeroAmountTaxDescriptionFromRegistry = null;
		}

		public static IDisposable RecordDocCopies_ForTestOnly(List<string> targetList) => new DisposableAction(() => DocCopyList_ForTestOnly = targetList, () => DocCopyList_ForTestOnly = null);

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static List<string> DocCopyList_ForTestOnly;

#endif

		public ZString TotalExTaxByRateAndMessageColumn
		{
			get
			{
				StringBuilder message = new StringBuilder();
				foreach (var group in LinesGroupedByRateAndMessage)
				{
					message.AppendLine(FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(group.Item2, Currency));
					if (group.Item3.Any(x => x.OSExtraTaxAmount != 0))
					{
						message.AppendLine();
					}
				}

				return message.ToString();
			}
		}

		public ZString TaxRateByRateAndMessageColumnRaw => TaxRateByRateAndMessageColumnRawCore(line => line.TaxRateDisplay);

		public ZString TaxRateByRateAndMessageColumnRawAlwaysShowPercentage => TaxRateByRateAndMessageColumnRawCore(line => line.TaxRateDisplayAlwaysShowPercentage);

		ZString TaxRateByRateAndMessageColumnRawCore(Func<DocARInvoiceLine, string> getTaxRateDisplay)
		{
			StringBuilder message = new StringBuilder();
			foreach (var group in LinesGroupedByRateAndMessage)
			{
				message.AppendLine(getTaxRateDisplay(group.Item3.First()));
				if (group.Item3.Sum(x => x.OSExtraTaxAmount) != 0)
				{
					message.AppendLine(group.Item3.First().OSTaxDisplayExtraRate);
				}
			}
			return message.ToString();
		}

		public ZString TaxRateByRateAndMessageColumn => TaxRateByRateAndMessageColumnCore(line => line.TaxRateDisplay);

		public ZString TaxRateByRateAndMessageColumnAlwaysShowPercentage => TaxRateByRateAndMessageColumnCore(line => line.TaxRateDisplayAlwaysShowPercentage);

		ZString TaxRateByRateAndMessageColumnCore(Func<DocARInvoiceLine, string> getTaxRateDisplay)
		{
			StringBuilder message = new StringBuilder();
			var displayTaxRateInAllLinesOfTaxSummary = AccountingConfigurationRegistry.Instance.DisplayTaxRateInAllLinesOfTaxSummary.Value;
			foreach (var group in LinesGroupedByRateAndMessage)
			{
				var taxSum = group.Item3.Sum(x => (x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.ServiceTax && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malaysia)
					? x.OSSERAmount
					: x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.RegionalTax || x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.VATRemittedByCustomer
						? x.OSGSTAmount
						: x.OSTaxAmount);
				if (taxSum != 0 || displayTaxRateInAllLinesOfTaxSummary)
				{
					message.AppendLine(getTaxRateDisplay(group.Item3.First()));
					if (group.Item3.Sum(x => x.OSExtraTaxAmount) != 0)
					{
						message.AppendLine(group.Item3.First().OSTaxDisplayExtraRate);
					}
				}
				else
				{
					message.AppendLine();
				}
			}

			return message.ToString();
		}

		public ZString TotalTaxByRateAndMessageColumnRaw
		{
			get
			{
				StringBuilder message = new StringBuilder();
				foreach (var group in LinesGroupedByRateAndMessage)
				{
					ZDecimal totalTax = 0M;
					ZDecimal extraTax = 0M;
					foreach (DocARInvoiceLine line in group.Item3)
					{
						if (line.OSExtraTaxAmount != 0)
						{
							totalTax += line.OSGSTAmount;
							extraTax += line.OSExtraTaxAmount;
						}
						else
						{
							if (line.TaxRate != null)
							{
								totalTax += line.OSTaxAmount_Raw;
							}
						}
					}

					message.AppendLine(FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(totalTax, Currency));
					if (group.Item3.Any(x => x.OSExtraTaxAmount != 0))
					{
						message.AppendLine(FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(extraTax, Currency));
					}
				}

				return message.ToString();
			}
		}

		public ZString TotalTaxByRateAndMessageColumn
		{
			get
			{
				StringBuilder message = new StringBuilder();
				foreach (var group in LinesGroupedByRateAndMessage)
				{
					ZDecimal totalTax = 0M;
					ZDecimal extraTax = 0M;
					foreach (DocARInvoiceLine line in group.Item3)
					{
						if (line.OSExtraTaxAmount != 0)
						{
							totalTax += line.OSGSTAmount;
							extraTax += line.OSExtraTaxAmount;
						}
						else
						{
							if (line.TaxRate != null)
							{
								if (line.TaxRate.ExtraType == AccTaxRate.ExtraTypes.ServiceTax
										&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malaysia)
								{
									totalTax += line.OSSERAmount;
								}
								else if (line.TaxRate.ExtraType == AccTaxRate.ExtraTypes.RegionalTax)
								{
									totalTax += line.OSGSTAmount;
								}
								else
								{
									totalTax += line.OSTaxAmount;
								}
							}
						}
					}

					message.AppendLine(FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(totalTax, Currency));
					if (group.Item3.Any(x => x.OSExtraTaxAmount != 0))
					{
						message.AppendLine(FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(extraTax, Currency));
					}
				}

				return message.ToString();
			}
		}

		public ZString TaxMessagesByRateAndMessageColumn
		{
			get
			{
				StringBuilder message = new StringBuilder();
				foreach (var group in LinesGroupedByRateAndMessage)
				{
					if (group.Item1.StartsWith("RAT", StringComparison.OrdinalIgnoreCase) || group.Item1.StartsWith("CAP", StringComparison.OrdinalIgnoreCase))
					{
						message.AppendLine();
					}
					else
					{
						var msg = group.Item1.Replace(System.Environment.NewLine, " ");
						if (group.Item1.StartsWith("*"))
						{
							var strAsteriskOnly = msg.Substring(0, msg.IndexOf(' ') + 1).Trim();
							var strAfterAsterisk = msg.Substring(msg.IndexOf(' ') + 1).Trim();
							message.Append(strAsteriskOnly + " ");
							msg = strAfterAsterisk;
						}
						message.AppendLine(msg.Length > truncationLength ? msg.Substring(0, truncationLength) : msg);
					}
				}
				return message.ToString();
			}
		}

		public ZString TaxGroupCodeByRateAndMessageColumn
		{
			get
			{
				var message = new StringBuilder();
				LinesGroupedByRateAndMessage.ForEach(x => message.AppendLine(x.Item3.First().TaxGroupCode));
				return message.ToString();
			}
		}

		public ZString TaxGroupNameByRateAndMessageColumn
		{
			get
			{
				if (GlbCompany.CurrentCompany.IsInTaxCoreSupportedCountry())
				{
					var taxGroupCodeList = CountryComplianceFactory.GetITaxMessageGroupProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?.GetTaxMessageGroup();
					var message = new StringBuilder();
					LinesGroupedByRateAndMessage.ForEach(x => message.AppendLine(taxGroupCodeList.GetDescriptionFromCode(x.Item3.First().TaxGroupCode)));
					return message.ToString();
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString TaxMessagesByRateAndMessageColumnWithNumbers
		{
			get
			{
				StringBuilder message = new StringBuilder();
				foreach (var group in LinesGroupedByRateAndMessageWithZeroAmountTaxDescriptionFromRegistry)
				{
					String msg = group.Item1.Replace(System.Environment.NewLine, " ");
					if (msg.StartsWith("RAT", StringComparison.OrdinalIgnoreCase) || group.Item1.StartsWith("CAP", StringComparison.OrdinalIgnoreCase))
					{
						msg = "";
					}
					else if (msg.StartsWith("*"))
					{
						var strAsteriskOnly = msg.Substring(0, msg.IndexOf(' ') + 1).Trim();
						var strAfterAsterisk = msg.Substring(msg.IndexOf(' ') + 1).Trim();
						if (!string.IsNullOrEmpty(strAsteriskOnly))
						{
							string number = strAsteriskOnly.Length.ToString() + ". ";
							message.Append(number);
						}
						msg = strAfterAsterisk;
					}
					message.AppendLine(msg.Length > truncationLength ? msg.Substring(0, truncationLength) : msg);
					if (group.Item3.Any(x => x.OSExtraTaxAmount != 0))
					{
						message.AppendLine();
					}
				}

				return message.ToString();
			}
		}

		const int truncationLength = 65;

		#endregion

		#region TaxTranasctions

		protected override DocTaxTransactionCollection GetTaxTransactionsCore() => TaxTransactions;

		public DocTaxTransactionCollection TaxTransactions
		{
			get
			{
				if (taxTransactions == null)
				{
					taxTransactions = DocTaxTransactionCollection.New(Factory);
					var accTaxTransactions = ObjectFactory.Get<ITaxProcessor>().GetTaxTransactions(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(InvoicingBase));

					foreach (var taxTransaction in accTaxTransactions)
					{
						var docTaxTransactionToAdd = Factory.GetCachedValue(taxTransaction.PK.ToStringKey(), () => DocTaxTransaction.New(taxTransaction, Factory));
						taxTransactions.Add(docTaxTransactionToAdd);
					}
				}

				return taxTransactions;
			}
		}

		DocTaxTransactionCollection taxTransactions;

		#endregion

		#region TaxTranasctionLinePivots

		public DocTaxTransactionLinePivotCollection TaxTransactionLinePivots
		{
			get
			{
				if (taxTransactionLinePivots == null)
				{
					taxTransactionLinePivots = DocTaxTransactionLinePivotCollection.New(Factory);
					var accTaxTransactions = TaxTransactions.OfType<DocTaxTransaction>().Select(x => x.TaxTransaction).ToArray();
					var taxTransactionPivots = ObjectFactory.Get<ITaxProcessor>().LoadTaxRecordPivots(accTaxTransactions);

					foreach (var pivot in taxTransactionPivots.OrderBy(x => x.TaxTransaction.ATT_TaxSystemCode).ThenBy(x => x.TransactionLine.AL_Sequence)) // Ordering to make it consistent to apply ShowNumberOfRows
					{
						var docPivotToAdd = Factory.GetCachedValue(pivot.PK.ToStringKey(), () => DocTaxTransactionLinePivot.New(pivot, Factory));
						taxTransactionLinePivots.Add(docPivotToAdd);
					}
				}

				return taxTransactionLinePivots;
			}
		}

		DocTaxTransactionLinePivotCollection taxTransactionLinePivots;

		#endregion

		public ZString TransactionOrganizationPeruDNI
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Peru)
				{
					var header = InvoicingBase.Header;

					if (header != null)
					{
						var dni = header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.PeruCodeTypes.DNI, Core.Constants.CountryCodes.Peru);

						if (dni != null)
						{
							return dni.OK_CustomsRegNo;
						}
					}
				}
				return ZString.Empty;
			}
		}

		#region Cost/Sell Payment Bases

		public DocJobPaymentBasisCollection CostPaymentBases
		{
			get
			{
				if (costPaymentBases == null)
				{
					costPaymentBases = new DocJobPaymentBasisCollection(Factory);
					costPaymentBases.AddRange(LinesForInvoice
						.Cast<IDocARInvoiceLine>()
						.Where(x => x.Charge != null)
						.SelectMany(x => x.Charge.CostPaymentBases));
				}
				return costPaymentBases;
			}
		}
		DocJobPaymentBasisCollection costPaymentBases;

		public DocJobPaymentBasisCollection SellPaymentBases
		{
			get
			{
				if (sellPaymentBases == null)
				{
					sellPaymentBases = new DocJobPaymentBasisCollection(Factory);
					sellPaymentBases.AddRange(LinesForInvoice
						.Cast<IDocARInvoiceLine>()
						.Where(x => x.Charge != null)
						.SelectMany(x => x.Charge.SellPaymentBases));
				}

				return sellPaymentBases;
			}
		}
		DocJobPaymentBasisCollection sellPaymentBases;

		#endregion

		#region Implementation

		public override string ToString()
		{
			return TransactionNumber;
		}

		protected bool IsInvoiceWithShipment
		{
			get { return InvoiceType == InvoiceTypeShipment && Shipment != null; }
		}

		protected bool IsInvoiceWithConsol
		{
			get { return InvoiceType == InvoiceTypeConsol && Consol != null; }
		}

		protected bool IsInvoiceWithDeclaration
		{
			get { return InvoiceType == InvoiceTypeCustoms && Declaration != null; }
		}

		protected bool IsInvoiceWithLoadList
		{
			get { return InvoiceType == InvoiceTypeLoadList && LoadList != null; }
		}

		protected bool IsInvoiceWithCartage
		{
			get { return InvoiceType == InvoiceTypeTransport && Cartage != null; }
		}

		protected bool IsInvoice
		{
			get { return TransactionType == ZArchitecture.Core.TransactionTypes.Invoice; }
		}

		protected bool IsAdjustmentNote
		{
			get { return TransactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote; }
		}

		bool IsARCreditNote
		{
			get { return IsCreditNote && Ledger == LedgerTypes.AccountsReceivable; }
		}

		int RevertSignForARCreditNote
		{
			get
			{
				int result = AmountMultiplierForARCreditNote;  // cancel the effect of registry ShowARCreditNoteAmountsWithOppositeSign first
				return IsARCreditNote ? result * -1 : result;
			}
		}

		protected abstract string InvoiceMessage { get; }
		protected abstract string CreditNoteMessage { get; }
		protected abstract string AdjustmentNoteMessage { get; }

		string DisbursementInvoiceMessage
		{
			get { return IsProForma ? AccountingConfigurationRegistry.Instance.ProFormaDisbursementMessage.Value : AccountingConfigurationRegistry.Instance.DisbursementMessage.Value; }
		}

		public InvoicingBase InvoicingBase
		{
			get { return (InvoicingBase)TransactionHeader; }
		}

		public bool IsRollUpEntireConsol => GroupOrSubtotal == OrgConstants.GroupOrSubTotalCharges.Code.RollUpEntireConsol;

		string GetTaxDisplayHeadingForMalaysia()
		{
			var registrationType = GetTaxRegistrationTypeForMalaysia();
			var result = registrationType == OrgCusCode.CodeTypes.GSTCode ?
							Res.GetString("2F5677BF-E6E4-44DA-A5CB-A9847A5776D6", "GST") :
							Res.GetString("9FAEE1F2-35C3-4483-BEF2-FF7D62A7573A", "SERVICE TAX");
			return result;
		}

		#endregion

		protected override ZString GetRecipientNameAddressCore()
		{
			return RecipientNameAddress;
		}

		protected override ZString GetCostConfirmationDocumentTitleCore()
		{
			return CostConfirmationDocumentTitle;
		}

		protected override ZString GetCostConfirmationHeadingTextCore()
		{
			return CostConfirmationHeadingText;
		}

		protected override ZString GetPostedByCore()
		{
			return PostedBy;
		}

		protected override ZString GetOSTaxDisplayHeadingCore()
		{
			return OSTaxDisplayHeading;
		}

		protected override ZBool GetIsTaxedCore()
		{
			return IsTaxed;
		}

		protected override ZBool GetIsTotalOSTaxAmountZeroCore()
		{
			return IsTotalOSTaxAmountZero;
		}

		protected override ZBool GetIsTotalOSTaxAmountRawZeroCore()
		{
			return IsTotalOSTaxAmountRawZero;
		}

		protected override ZString GetInvoiceSubTotalFormattedCore()
		{
			return InvoiceSubTotalFormatted;
		}

		protected override ZString GetTotalOSTaxAmountFormattedCore()
		{
			return TotalOSTaxAmountFormatted;
		}

		protected override ZString GetOSDocumentTitleForITAutofattura()
		{
			return OSDocumentTitleForITAutofattura;
		}

		protected override ZString GetTotalOSTaxAmountRawFormattedCore()
		{
			return TotalOSTaxAmountRawFormatted;
		}

		protected override ZString GetTotalOSISICAmountFormattedCore()
		{
			return TotalOSIGICAmountFormatted;
		}

		protected override ZString GetTotalOSQSTAmountFormattedCore()
		{
			return TotalOSQSTAmountFormatted;
		}

		protected override ZString GetTotalOSSBCAmountFormattedCore()
		{
			return TotalOSSBCAmountFormatted;
		}

		protected override ZString GetTotalOSKKCAmountFormattedCore()
		{
			return TotalOSKKCAmountFormatted;
		}

		protected override ZString GetTotalOSEDUPrimaryAmountFormattedCore()
		{
			return TotalOSEDUPrimaryAmountFormatted;
		}

		protected override ZString GetTotalOSEDUSecondaryAmountFormattedCore()
		{
			return TotalOSEDUSecondaryAmountFormatted;
		}

		protected override ZString GetTotalOSRETAmountFormattedCore()
		{
			return TotalOSRETAmountFormatted;
		}

		protected override ZString GetTotalOSSPVAmountFormattedCore()
		{
			return TotalOSSPVAmountFormatted;
		}

		protected override ZString GetOSTotalFormattedCore()
		{
			return OSTotalFormatted;
		}

		protected override ZString GetOSTotalRawFormattedCore()
		{
			return OSTotalRawFormatted;
		}

		protected override ZBool GetHasSERLineOnlyCore()
		{
			return HasSERLineOnly;
		}

		protected override ZBool GetHasAtLeastOneSERLineCore()
		{
			return HasAtLeastOneSERLine;
		}

		protected override ZBool GetHasVATANDIGICLineCore()
		{
			return HasVATANDIGICLine;
		}

		protected override ZBool GetHasIGICLineOnlyCore()
		{
			return HasIGICLineOnly;
		}

		protected override ZBool GetHasGSTANDQSTLineCore()
		{
			return HasGSTANDQSTLine;
		}

		protected override ZBool GetHasGSTANDQCTLineCore()
		{
			return HasGSTANDQCTLine;
		}

		protected override ZBool GetHasGSTANDEDULineCore()
		{
			return HasGSTANDEDULine;
		}

		protected override ZBool GetHasRETLineCore()
		{
			return HasRETLine;
		}

		protected override ZBool GetHasIntegratedGSTLineCore()
		{
			return HasIntegratedGSTLine;
		}

		protected override ZBool GetHasStateGSTLineCore()
		{
			return HasStateGSTLine;
		}

		protected override ZBool GetHasZeroIGSTAmountCore()
		{
			return HasZeroIGSTAmount;
		}

		protected override ZString GetTotalOSIntegratedGSTAmountFormattedCore()
		{
			return TotalOSIntegratedGSTAmountFormatted;
		}

		protected override ZString GetTotalOSCentreGSTAmountFormattedCore()
		{
			return TotalOSCentreGSTAmountFormatted;
		}

		protected override ZString GetTotalOSStateGSTAmountFormattedCore()
		{
			return TotalOSStateGSTAmountFormatted;
		}

		protected override ZBool GetShowLocalGSTAmountCore()
		{
			return ShowLocalGSTAmount;
		}

		protected override ZString GetOSPrimaryTaxDisplayHeadingCore()
		{
			return OSPrimaryTaxDisplayHeading;
		}

		protected override ZString GetInvoiceSubTotalDisplayHeadingCore()
		{
			return InvoiceSubTotalDisplayHeading;
		}

		protected override ZString GetOSTaxExtraRateQCTDisPlayCore()
		{
			return OSTaxExtraRateQCTDisPlay;
		}

		protected override ZString GetOSSPVExtraTaxCodeLabelCore()
		{
			return OSSPVExtraTaxCodeLabel;
		}

		protected override ZString GetOSTaxExtraRateSBCDisPlayCore()
		{
			return OSTaxExtraRateSBCDisPlay;
		}

		protected override ZString GetOSTaxExtraRateKKCDisPlayCore()
		{
			return OSTaxExtraRateKKCDisPlay;
		}

		protected override ZString GetOSSPVExtraTaxLabelCore()
		{
			return OSSPVExtraTaxLabel;
		}

		protected override ZDateTime GetInvoiceTaxDateCore()
		{
			return InvoiceTaxDate;
		}

		protected override ZBool GetShowInvoiceTaxDateCore()
		{
			return ShowInvoiceTaxDate;
		}

		protected override ZString GetInvoiceTaxDateHeadingCore()
		{
			return InvoiceTaxDateHeading;
		}

		protected override DocGenericTransactionLineCollection GetLinesForInvoiceCore()
		{
			var result = new DocGenericTransactionLineCollection(InvoicingBase.Factory);

			foreach (IDocARInvoiceLine line in LinesForInvoice)
			{
				if (line is DocARInvoiceLine)
				{
					result.Add(DocGenericTransactionLine.New((DocARInvoiceLine)line, Factory));
				}
				else if (line is DocARInvoiceLineForRollUp)
				{
					result.Add(DocGenericTransactionLine.New((DocARInvoiceLineForRollUp)line, Factory));
				}
			}

			return result;
		}

		OperationsJobHelper OpJobHelper
		{
			get
			{
				if (jobHelper == null)
				{
					jobHelper = new OperationsJobHelper(JobHeader, Shipment, null, Factory);
				}
				return jobHelper;
			}
		}
		OperationsJobHelper jobHelper;

		protected override BaseInvoiceDocLineRollUpper DocLineRollUpper => new CommonInvoiceDocLineRollUpper(this);
	}
}
