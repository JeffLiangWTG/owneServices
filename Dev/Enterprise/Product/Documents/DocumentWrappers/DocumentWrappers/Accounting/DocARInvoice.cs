using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort.RollUpper;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using IQRCodeDataProvider = Enterprise.Accounting.Business.AccountingCountryFactory.IQRCodeDataProvider;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public partial class DocARInvoice : DocARInvoiceCommon, Enterprise.Integration.DocumentWrappers.IDocARInvoice, IShouldExcludeFromDocPackByDefault
	{
		#region Construction

		protected DocARInvoice(InvoicingBase invoice, BusinessObjectFactory factoryToWrap)
			: base(invoice, factoryToWrap)
		{
			this.invoice = invoice;
		}

		public static DocARInvoice New(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
		{
			DocARInvoice result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(invoicingBase, factoryToWrap);
			}
			else if (invoicingBase != null)
			{
				result = new DocARInvoice(invoicingBase, factoryToWrap);
			}

			return result;
		}

		public static DocARInvoice New(AccTransactionHeader transactionHeader, BusinessObjectFactory factoryToWrap)
		{
			DocARInvoice result = null;

			if (transactionHeader != null)
			{
				var invoice = factoryToWrap.Load<TransactionHeader>(transactionHeader.PK);
				result = DocARInvoice.New((InvoicingBase)invoice, factoryToWrap);
			}

			return result;
		}

		protected new delegate DocARInvoice NewDelegate(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap);
		protected new static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		public InvoicingBase Invoice
		{
			get { return invoice; }
		}
		readonly InvoicingBase invoice;

		#region LinesForInvoice

		protected override DocARInvoiceLineCollection LinesForInvoiceCore()
		{
			return IsPeriodicInvoice ? PeriodicInvoiceLinesForDisplay : base.LinesForInvoiceCore();
		}

		#endregion

		#region Org VAT GST Exporter Exemption Document Details

		internal List<JobRequiredDocument> EXVDocuments
		{
			get
			{
				if (fEXVDocuments == null && (TransactionType == TransactionTypes.Invoice || TransactionType == TransactionTypes.AdjustmentNote || TransactionType == TransactionTypes.CreditNote))
				{
					OrgHeader debtor = invoice.Header;
					if (debtor != null)
					{
						var currentCompanyCode = GlbCompany.CurrentCompany.GC_Code;
						foreach (JobRequiredDocument document in debtor.RequiredDocuments)
						{
							if (document.EQ_DocType == Core.Constants.RefDocTypes.VATExporterExemption && document.EQ_DocUsage == Enterprise.MasterFiles.Business.JobRequiredDocument.DocUsage.Debtor
								&& document.EQ_DocPeriod == Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic && document.EQ_RN_NKRelatedCountry == GlbCompany.CurrentCompany.Country.Code)
							{
								if (invoice.AH_PostDate >= document.EQ_DateReceived.ToZDateTime() && invoice.AH_PostDate <= document.EQ_ValidToDate)
								{
									fEXVDocuments = fEXVDocuments ?? new List<JobRequiredDocument>();

									if (document.Attributes[JobRequiredDocAttribTypeList.Codes.CompanyCode, currentCompanyCode] != null)
									{
										fEXVDocuments.Add(document);
									}
									else // fallback logic - if company specific record not found, then find the matching document without companyCode attribute (i.e. it's not company specific)
									{
										if (!document.Attributes.HasAttributeType(JobRequiredDocAttribTypeList.Codes.CompanyCode))
										{
											fEXVDocuments.Add(document);
										}
									}
								}
							}
						}
					}
				}

				return fEXVDocuments;
			}
		}

		List<JobRequiredDocument> fEXVDocuments;

		public ZBool HasEXVDocuments
		{
			get
			{
				return EXVDocuments != null && invoice.Lines.Cast<InvoicingLineBase>().Any(x => x.VATClass != null && x.VATClass.A9_IsTriggerExemptionMessage);
			}
		}

		public ZString EXVBuyerIssueDate(JobRequiredDocument document)
		{
			var result = ZString.Empty;
			if (document != null && document.Attributes.Count > 0)
			{
				var attrib = document.Attributes[JobRequiredDocAttribTypeList.Codes.BuyerIssueDate];
				if (attrib != null)
				{
					ZDateTime dateParsed;
					if (ZDateTime.TryParseISO8601Date(attrib.D0_AttribValue, out dateParsed))
					{
						result = dateParsed.ToShortDateString();
					}
				}
			}
			return result;
		}

		public ZString EXVValidFromReceivedDate(JobRequiredDocument document)
		{
			var result = ZString.Empty;
			if (document != null)
			{
				result = document.EQ_DateReceived.ToShortDateString();
				ZString documentReceivedDate = ZString.Empty;
				if (document.Attributes.Count > 0)
				{
					var attrib = document.Attributes[JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate];
					if (attrib != null)
					{
						documentReceivedDate = attrib.D0_AttribValue;
					}
				}
				if (!documentReceivedDate.IsEmpty)
				{
					ZDateTime dateParsed;
					if (ZDateTime.TryParseISO8601Date(documentReceivedDate, out dateParsed))
					{
						if (dateParsed > document.EQ_DateReceived.ToZDateTime())
						{
							result = dateParsed.ToShortDateString();
						}
					}
				}
			}
			return result;
		}

		public ZString EXVProtocolloNumber(JobRequiredDocument document)
		{
			var result = ZString.Empty;
			if (document != null && document.Attributes.Count > 0)
			{
				var attrib = document.Attributes[JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference];
				if (attrib != null)
				{
					result = attrib.D0_AttribValue;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "columnName")]
		public static class EXVColumnConstants
		{
			public const string Notes = "Notes";
			public const string DocNumbers = "DocNumbers";
			public const string ReceivedDates = "ReceivedDates";
			public const string BuyerIssueDates = "BuyerIssueDates";
			public const string ValidFromReceivedDates = "ValidFromReceivedDates";
			public const string ProtocolloNumbers = "ProtocolloNumbers";
		}

		public ZString EXVNotes => GetEXVColumnCore(EXVColumnConstants.Notes);
		public ZString EXVDocNumbers => GetEXVColumnCore(EXVColumnConstants.DocNumbers);
		public ZString EXVReceivedDates => GetEXVColumnCore(EXVColumnConstants.ReceivedDates);
		public ZString EXVBuyerIssueDates => GetEXVColumnCore(EXVColumnConstants.BuyerIssueDates);
		public ZString EXVValidFromReceivedDates => GetEXVColumnCore(EXVColumnConstants.ValidFromReceivedDates);
		public ZString EXVProtocolloNumbers => GetEXVColumnCore(EXVColumnConstants.ProtocolloNumbers);

		protected ZString GetEXVColumnCore(string columnName)
		{
			if (!HasEXVDocuments)
			{
				return ZString.Empty;
			}

			StringBuilder message = new StringBuilder();

			foreach (JobRequiredDocument document in EXVDocuments.OrderBy(x => x.EQ_DateReceived))
			{
				switch (columnName)
				{
					case EXVColumnConstants.Notes:
						message.AppendLine(document.EQ_DocumentNotes);
						break;
					case EXVColumnConstants.DocNumbers:
						message.AppendLine(document.EQ_DocNumber);
						break;
					case EXVColumnConstants.ReceivedDates:
						message.AppendLine(document.EQ_DateReceived.ToShortDateString());
						break;
					case EXVColumnConstants.BuyerIssueDates:
						message.AppendLine(EXVBuyerIssueDate(document));
						break;
					case EXVColumnConstants.ValidFromReceivedDates:
						message.AppendLine(EXVValidFromReceivedDate(document));
						break;
					case EXVColumnConstants.ProtocolloNumbers:
						message.AppendLine(EXVProtocolloNumber(document));
						break;
				}
			}
			return message.ToString();
		}

		#endregion

		#region Document Title & Messages

		protected override ZString TaxInvoiceTitle
		{
			get
			{
				var result = ZString.Empty;
				if (IsDisbursement)
				{
					if (IsProForma)
					{
						result = AccountingConfigurationRegistry.Instance.ProFormaTaxDisbursementInvoiceTitle.Value;
					}
					else if (InvoicingBase.IsReversalTransaction)
					{
						result = AccountingConfigurationRegistry.Instance.TaxDisbursementInvoiceReversalTitle.Value;
					}
					else if (InvoicingBase.IsAmendingTransaction)
					{
						result = AccountingConfigurationRegistry.Instance.TaxDisbursementInvoiceAmendmentTitle.Value;
						if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malaysia && hasAtLeastOneGSTLine)
						{
							result = Res.GetString("64ea1dbf-87a1-49a9-bcaa-1f279f1fa71d", "{0}", AccountingConfigurationRegistry.Instance.TaxDisbursementInvoiceAmendmentTitle.Value);
							if ((ZString)AccountingConfigurationRegistry.Instance.TaxDisbursementInvoiceAmendmentTitle.DefaultValue == result)
							{
								result += Res.GetString("26b4df8e-8f1a-4a38-8a7e-3f3687d62b69", " (DEBIT NOTE)");
							}
						}
					}
					else
					{
						result = AccountingConfigurationRegistry.Instance.TaxDisbursementInvoiceTitle.Value;
					}
				}
				else
				{
					if (IsProForma)
					{
						result = AccountingConfigurationRegistry.Instance.ProFormaTaxInvoiceTitle.Value;
					}
					else if (InvoicingBase.IsReversalTransaction)
					{
						result = AccountingConfigurationRegistry.Instance.TaxInvoiceReversalTitle.Value;
					}
					else if (InvoicingBase.IsAmendingTransaction)
					{
						result = AccountingConfigurationRegistry.Instance.TaxInvoiceAmendmentTitle.Value;
						if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Malaysia)
						{
							if (hasAtLeastOneGSTLine)
							{
								result = Res.GetString("c56b3fb8-4662-4696-9075-252ee2dfd5be", "{0}", AccountingConfigurationRegistry.Instance.TaxInvoiceAmendmentTitle.Value);
								if ((ZString)AccountingConfigurationRegistry.Instance.TaxInvoiceAmendmentTitle.DefaultValue == result)
								{
									result += Res.GetString("26b4df8e-8f1a-4a38-8a7e-3f3687d62b69", " (DEBIT NOTE)");
								}
							}
						}
					}
					else
					{
						result = AccountingConfigurationRegistry.Instance.TaxInvoiceTitle.Value;
					}
				}

				return result;
			}
		}

		bool hasAtLeastOneGSTLine
		{
			get
			{
				var query = new ZQuery(AccTaxRateSchema.AT_Code, "GST");
				query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Malaysia);
				var doesGSTExist = Factory.Load<AccTaxRate>(query).Length > 0;

				return doesGSTExist && InvoicingBase.Lines.Cast<InvoicingLineBase>().Any(x => x.TaxRate != null && x.TaxRate.AT_ExtraTaxRateType != AccTaxRate.ExtraTypes.ServiceTax);
			}
		}

		protected override ZString NonTaxInvoiceTitle
		{
			get
			{
				return IsDisbursement ? (IsProForma ? AccountingConfigurationRegistry.Instance.ProFormaNonTaxDisbursementInvoiceTitle.Value : AccountingConfigurationRegistry.Instance.NonTaxDisbursementInvoiceTitle.Value)
					  : (IsProForma ? AccountingConfigurationRegistry.Instance.ProFormaNonTaxInvoiceTitle.Value : AccountingConfigurationRegistry.Instance.NonTaxInvoiceTitle.Value);
			}
		}

		protected override ZString TaxCreditNoteTitle
		{
			get
			{
				var result = ZString.Empty;
				if (IsDisbursement)
				{
					if (IsProForma)
					{
						result = AccountingConfigurationRegistry.Instance.ProFormaTaxDisbursementCreditNoteTitle.Value;
					}
					else if (InvoicingBase.IsReversalTransaction)
					{
						result = AccountingConfigurationRegistry.Instance.TaxDisbursementCreditNoteReversalTitle.Value;
					}
					else if (InvoicingBase.IsAmendingTransaction)
					{
						result = AccountingConfigurationRegistry.Instance.TaxDisbursementCreditNoteAmendmentTitle.Value;
					}
					else
					{
						result = AccountingConfigurationRegistry.Instance.TaxDisbursementCreditNoteTitle.Value;
					}
				}
				else
				{
					if (IsProForma)
					{
						result = AccountingConfigurationRegistry.Instance.ProFormaTaxCreditNoteTitle.Value;
					}
					else if (InvoicingBase.IsReversalTransaction)
					{
						result = AccountingConfigurationRegistry.Instance.TaxCreditNoteReversalTitle.Value;
					}
					else if (InvoicingBase.IsAmendingTransaction)
					{
						result = AccountingConfigurationRegistry.Instance.TaxCreditNoteAmendmentTitle.Value;
					}
					else
					{
						result = AccountingConfigurationRegistry.Instance.TaxCreditNoteTitle.Value;
					}
				}
				return result;
			}
		}

		protected override ZString NonTaxCreditNoteTitle
		{
			get
			{
				var result = ZString.Empty;
				if (IsDisbursement)
				{
					result = IsProForma ? AccountingConfigurationRegistry.Instance.ProFormaNonTaxDisbursementCreditNoteTitle.Value : AccountingConfigurationRegistry.Instance.NonTaxDisbursementCreditNoteTitle.Value;
				}
				else
				{
					result = IsProForma ? AccountingConfigurationRegistry.Instance.ProFormaNonTaxCreditNoteTitle.Value : AccountingConfigurationRegistry.Instance.NonTaxCreditNoteTitle.Value;
				}
				return result;
			}
		}

		protected override ZString TaxAdjustmentNoteTitle
		{
			get
			{
				var result = ZString.Empty;
				if (IsProForma)
				{
					result = AccountingConfigurationRegistry.Instance.ProFormaTaxAdjustmentNoteTitle.Value;
				}
				else if (InvoicingBase.IsReversalTransaction)
				{
					result = AccountingConfigurationRegistry.Instance.TaxAdjustmentNoteReversalTitle.Value;
				}
				else
				{
					result = AccountingConfigurationRegistry.Instance.TaxAdjustmentNoteTitle.Value;
				}

				return result;
			}
		}

		protected override ZString NonTaxAdjustmentNoteTitle
		{
			get { return IsProForma ? AccountingConfigurationRegistry.Instance.ProFormaNonTaxAdjustmentNoteTitle.Value : AccountingConfigurationRegistry.Instance.NonTaxAdjustmentNoteTitle.Value; }
		}

		public override ZString Message
		{
			get
			{
				if (IsAdjustmentNote)
				{
					return AdjustmentNoteMessage;
				}
				else
				{
					return base.Message;
				}
			}
		}

		protected override string AdjustmentNoteMessage
		{
			get { return IsProForma ? AccountingConfigurationRegistry.Instance.ProFormaAdjustmentNoteMessage.Value : AccountingConfigurationRegistry.Instance.AdjustmentNoteMessage.Value; }
		}

		protected override string InvoiceMessage
		{
			get { return IsProForma ? AccountingConfigurationRegistry.Instance.ProFormaInvoiceMessage.Value : AccountingConfigurationRegistry.Instance.InvoiceMessage.Value; }
		}

		protected override string CreditNoteMessage
		{
			get { return IsProForma ? AccountingConfigurationRegistry.Instance.ProFormaCreditNoteMessage.Value : AccountingConfigurationRegistry.Instance.CreditNoteMessage.Value; }
		}

		public ZBool IsIcelandicInvoice
		{
			get { return (AccountOrg != null && AccountOrg.Country != null && AccountOrg.Country.Code == Core.Constants.CountryCodes.Iceland && CurrentCompany.Country.Code == Core.Constants.CountryCodes.Iceland); }
		}

		public override ZBool PrintStandard
		{
			get { return IsIcelandicInvoice ? ZBool.False : base.PrintStandard; }
		}

		internal Dictionary<ZString, ZDecimal> CrossExchangeRatesBasedOnJobChargeSellCurrency
		{
			get
			{
				if (crossExchangeRatesBasedOnJobChargeCurrency == null)
				{
					crossExchangeRatesBasedOnJobChargeCurrency = new Dictionary<ZString, ZDecimal>();

					var linesWithJobCharge = invoice.Lines.Cast<InvoicingLineBase>().Where(x => x.RelatedJobCharge != null && x.RelatedJobCharge is IReceivablesPostingCharge);
					foreach (var invoiceLinesGroupByCurrency in linesWithJobCharge.GroupBy(x => x.RelatedJobCharge.JR_RX_NKSellCurrency))
					{
						var total_JR_OSSellAmount = invoiceLinesGroupByCurrency.Sum(x => x.RelatedJobCharge.JR_OSSellAmt);
						var total_OSSellAmount = invoiceLinesGroupByCurrency.Sum(x => ((IReceivablesPostingCharge)x.RelatedJobCharge).OSSellAmount);
						var exchangeRate = ZDecimal.Zero;
						if (total_JR_OSSellAmount != 0 && total_OSSellAmount != 0)
						{
							var jobCharge = invoiceLinesGroupByCurrency.First().RelatedJobCharge;
							var sellExRateDecimals = CargoWise.ComponentModel.MetaData.GetDecimalPlaces(jobCharge, TypeDescriptor.GetProperties(jobCharge)[jobCharge.JR_OSSellExRateInfo.Name]);
							exchangeRate = new ZDecimal(Utilities.Round(GlbCompany.CurrentCompany.GC_IsReciprocal ? total_OSSellAmount / total_JR_OSSellAmount : total_JR_OSSellAmount / total_OSSellAmount, sellExRateDecimals));
						}
						crossExchangeRatesBasedOnJobChargeCurrency.Add(invoiceLinesGroupByCurrency.Key, exchangeRate);
					}
				}
				return crossExchangeRatesBasedOnJobChargeCurrency;
			}
		}

		Dictionary<ZString, ZDecimal> crossExchangeRatesBasedOnJobChargeCurrency;

		#endregion

		#region Periodic Invoices

		public ZBool UseDocBuilderForSupplementaryDetail
		{
			get { return DocumentsDataRegistry.Instance.UseNewDocBuilderSupplementaryDetail.Value && (IsPeriodicInvoice || IsAmendingTransactionForPeriodicInvoiceWithMultipleJobs); }
		}

		public ZString BatchInvoiceType
		{
			get
			{
				var result = InvoiceTypeLayoutList.Codes.CHG;

				if (invoice.Lines.Count > 0)
				{
					var invoiceTypes = invoice.Header.CompanyData.GetApplicableInvoiceTypes(ZString.Empty, ZString.Empty, ZString.Empty, true, JobTypeList);
					if (invoiceTypes != null && invoiceTypes.Count > 0)
					{
						result = invoiceTypes.Values.First().PI_Type;
					}
				}

				return result;
			}
		}

		public ZString DetentionDemurrageStatements =>
			AccountingConfigurationRegistry.Instance.InvoiceDetentionDemurrageStatements.GetValueWithoutFallback(invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);

		public ZBool HasLinesWithINVSecondaryLayout
		{
			get
			{
				return LinesForPeriodicInvoice != null && LinesForPeriodicInvoice.Cast<IDocARInvoiceLine>().Any(x => x.SecondaryLayoutWhenPrintedInPeriodicInvoice == InvoiceTypeLayoutList.Codes.INV);
			}
		}

		public ZBool HasLinesWithCHGSecondaryLayout
		{
			get
			{
				return LinesForPeriodicInvoice == null || LinesForPeriodicInvoice.Cast<IDocARInvoiceLine>().Any(x => x.SecondaryLayoutWhenPrintedInPeriodicInvoice == InvoiceTypeLayoutList.Codes.CHG);
			}
		}

		public ZBool HasLinesWithINVLayout
		{
			get
			{
				return LinesForPeriodicInvoice != null && LinesForPeriodicInvoice.Cast<IDocARInvoiceLine>().Any(x => x.LayoutWhenPrintedInPeriodicInvoice == InvoiceTypeLayoutList.Codes.INV);
			}
		}

		public ZBool HasLinesWithCHGLayout
		{
			get
			{
				return LinesForPeriodicInvoice == null || LinesForPeriodicInvoice.Cast<IDocARInvoiceLine>().Any(x => x.LayoutWhenPrintedInPeriodicInvoice == InvoiceTypeLayoutList.Codes.CHG);
			}
		}

		public ZBool HasLinesWithNONLayout
		{
			get
			{
				return LinesForPeriodicInvoice != null && LinesForPeriodicInvoice.Cast<IDocARInvoiceLine>().Any(x => x.LayoutWhenPrintedInPeriodicInvoice == InvoiceTypeLayoutList.Codes.NON);
			}
		}

		public ZBool HasRollUpLines
		{
			get
			{
				return LinesForInvoice?.Cast<IDocARInvoiceLine>().Any(x => x.IsRollUpLine) ?? false;
			}
		}

		public ZBool PrintQuantity =>
			AccountingConfigurationRegistry.Instance.PrintQuanityInInvoiceDocument.Value &&
			((!IsPeriodicInvoice && !HasRollUpLines) || (IsPeriodicInvoice && HasLinesWithNONLayout));

		public ZBool PrintTaxDetailPERRIISLX =>
			AccountingConfigurationRegistry.Instance.PrintTaxDetailPERRIISLX.Value;

		public ZBool PrintTaxDetailTRX =>
			AccountingConfigurationRegistry.Instance.PrintTaxDetailTRX.Value;

		public ZBool PrintTaxDetailSPR =>
			AccountingConfigurationRegistry.Instance.PrintTaxDetailSPR.Value;

		public ZBool PrintAccumulativeTotalAmountsInMultipageInvoices =>
			AccountingConfigurationRegistry.Instance.DisplayAccumulativeTotalAmountsInMultipageInvoices.Value;

		protected override DocARInvoiceLineCollection GetInvoiceLines()
		{
			DocARInvoiceLineCollection result = new DocARInvoiceLineCollection(Factory);

			foreach (InvoicingLineBase line in Invoice.Lines)
			{
				result.Add(DocBatchARInvoiceLineTransactionLine.New(line, Factory));
			}

			return result;
		}

		protected override DocARInvoiceLineCollection GetInvoiceLineByChargeCore()
		{
			DocARInvoiceLineCollection invoiceLineByCharge;
			var invoiceLines = new DocARInvoiceLineCollection(Factory);
			invoiceLines.AddRange(this.InvoiceLine.Cast<IDocARInvoiceLine>().Where(x => !IsPeriodicInvoice || x.LayoutWhenPrintedInPeriodicInvoice == InvoiceTypeLayoutList.Codes.CHG).ToList());

			if (this is DocARInvoice)
			{
				invoiceLines.ResetMultiplierTo1();
			}
			var grouper = new RollUpGrouperByChargeAndTax(DocLineRollUpper, this, Factory, invoiceLines);
			invoiceLineByCharge = grouper.RollUp();
			invoiceLineByCharge.Sort("LineDescription", ListSortDirection.Ascending);
			return invoiceLineByCharge;
		}

		protected override DocARInvoiceLineCollection GetInvoiceLineByJobCore()
		{
			DocARInvoiceLineCollection invoiceLineByJob = null;

			var invoiceLines = new DocARInvoiceLineCollection(Factory);
			invoiceLines.AddRange(this.InvoiceLine.Cast<IDocARInvoiceLine>().Where(x => !IsPeriodicInvoice || x.LayoutWhenPrintedInPeriodicInvoice == InvoiceTypeLayoutList.Codes.INV));

			if (this is DocARInvoice)
			{
				invoiceLines.ResetMultiplierTo1();
			}

			if (AccountingHelperClass.DescriptionInDocumentsForTaxAmountsRule == AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code)
			{
				var grouper = new RollUpGrouperByJob(DocLineRollUpper, this, Factory, invoiceLines);
				invoiceLineByJob = grouper.RollUp();
			}
			else
			{
				var grouper = new RollUpGrouperByJobAndTaxRate(DocLineRollUpper, this, Factory, invoiceLines);
				invoiceLineByJob = grouper.RollUp();
			}

			var removedLines = this.InvoiceLine.Cast<IDocARInvoiceLine>().Where(x => !IsPeriodicInvoice || x.LayoutWhenPrintedInPeriodicInvoice == InvoiceTypeLayoutList.Codes.INV).Except(invoiceLines.Cast<IDocARInvoiceLine>()).ToList();
			if (removedLines.Any())
			{
				this.InvoiceLine.RemoveRange(removedLines);
			}

			invoiceLineByJob.Sort("JobNumber", ListSortDirection.Ascending);

			return invoiceLineByJob;
		}

		protected override DocARInvoiceLineCollection GetInvoiceLineByNONCore()
		{
			var invoiceLineByNON = new DocARInvoiceLineCollection(Factory);
			invoiceLineByNON.AddRange(InvoiceLine.Cast<IDocARInvoiceLine>().Where(x => IsPeriodicInvoice && x.LayoutWhenPrintedInPeriodicInvoice == InvoiceTypeLayoutList.Codes.NON));

			if (invoiceLineByNON.Any() &&
				GroupOrSubtotal == OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical || GroupOrSubtotal == OrgConstants.GroupOrSubTotalCharges.Code.Sequence)
			{
				return GroupAndSortLines(invoiceLineByNON);
			}

			return invoiceLineByNON;
		}

		public DocARInvoiceLineCollection LinesForPeriodicInvoice
		{
			get
			{
				if (fLinesForPeriodicInvoice == null)
				{
					fLinesForPeriodicInvoice = new DocARInvoiceLineCollection(Factory);
					fLinesForPeriodicInvoice.AddRange(Lines);
				}
				fLinesForPeriodicInvoice.Sort("LineDescription", ListSortDirection.Ascending);
				return fLinesForPeriodicInvoice;
			}
		}
		DocARInvoiceLineCollection fLinesForPeriodicInvoice;

		public DocARInvoiceLineCollection PeriodicInvoiceLinesForDisplay
		{
			get
			{
				if (periodicInvoiceLinesForDisplay == null)
				{
					periodicInvoiceLinesForDisplay = new DocARInvoiceLineCollection(Factory);
					foreach (var (lines, layout) in new[] {
						(InvoiceLineByJob, InvoiceTypeLayoutList.Codes.INV),
						(InvoiceLineByCharge, InvoiceTypeLayoutList.Codes.CHG),
						(InvoiceLineByNON, InvoiceTypeLayoutList.Codes.NON) })
					{
						if (lines.Any())
						{
							var wrappedLines = lines
												.OfType<IDocARInvoiceLine>()
												.Select(l => new DocPeriodicInvoiceLineWrapper(l, layout))
												.ToArray();
							periodicInvoiceLinesForDisplay.AddRange(wrappedLines);
						}
					}
				}
				return periodicInvoiceLinesForDisplay;
			}
		}
		DocARInvoiceLineCollection periodicInvoiceLinesForDisplay;

		public DocARInvoiceLineCollection CFSInvoiceLine
		{
			get
			{
				if (fCFSInvoiceLine == null)
				{
					fCFSInvoiceLine = CreateSpecificModuleTypeLineCollection(InvoiceTypeModuleList.Codes.CFS);
				}
				fCFSInvoiceLine.Sort("SubInvoiceRef", ListSortDirection.Ascending);
				fCFSInvoiceLine.Sort("ChargeCode+Code", ListSortDirection.Ascending);
				return fCFSInvoiceLine;
			}
		}
		DocARInvoiceLineCollection fCFSInvoiceLine;

		public DocARInvoiceLineCollection CUSInvoiceLine
		{
			get
			{
				if (fCUSInvoiceLine == null)
				{
					fCUSInvoiceLine = CreateSpecificModuleTypeLineCollection(InvoiceTypeModuleList.Codes.CUS);
				}
				fCUSInvoiceLine.Sort("SubInvoiceRef", ListSortDirection.Ascending);
				fCUSInvoiceLine.Sort("ChargeCode+Code", ListSortDirection.Ascending);
				return fCUSInvoiceLine;
			}
		}
		DocARInvoiceLineCollection fCUSInvoiceLine;

		public DocARInvoiceLineCollection FWDInvoiceLine
		{
			get
			{
				if (fFWDInvoiceLine == null)
				{
					fFWDInvoiceLine = CreateSpecificModuleTypeLineCollection(InvoiceTypeModuleList.Codes.FWD);
				}
				fFWDInvoiceLine.Sort("SubInvoiceRef", ListSortDirection.Ascending);
				fFWDInvoiceLine.Sort("ChargeCode+Code", ListSortDirection.Ascending);
				return fFWDInvoiceLine;
			}
		}
		DocARInvoiceLineCollection fFWDInvoiceLine;

		public DocARInvoiceLineCollection MSCInvoiceLine
		{
			get
			{
				if (fMSCInvoiceLine == null)
				{
					fMSCInvoiceLine = CreateSpecificModuleTypeLineCollection(InvoiceTypeModuleList.Codes.MSC);
				}

				fMSCInvoiceLine.Sort("Sequence", ListSortDirection.Ascending);
				return fMSCInvoiceLine;
			}
		}
		DocARInvoiceLineCollection fMSCInvoiceLine;

		public DocARInvoiceLineCollection TCNInvoiceLine
		{
			get
			{
				if (fTCNInvoiceLine == null)
				{
					fTCNInvoiceLine = CreateSpecificModuleTypeLineCollection(InvoiceTypeModuleList.Codes.TCN);
				}
				fTCNInvoiceLine.Sort("SubInvoiceRef", ListSortDirection.Ascending);
				fTCNInvoiceLine.Sort("ChargeCode+Code", ListSortDirection.Ascending);
				return fTCNInvoiceLine;
			}
		}
		DocARInvoiceLineCollection fTCNInvoiceLine;

		public DocARInvoiceLineCollection TPTInvoiceLine
		{
			get
			{
				if (fTPTInvoiceLine == null)
				{
					fTPTInvoiceLine = CreateSpecificModuleTypeLineCollection(InvoiceTypeModuleList.Codes.TPT);
				}
				fTPTInvoiceLine.Sort("SubInvoiceRef", ListSortDirection.Ascending);
				fTPTInvoiceLine.Sort("ChargeCode+Code", ListSortDirection.Ascending);
				return fTPTInvoiceLine;
			}
		}
		DocARInvoiceLineCollection fTPTInvoiceLine;

		public DocARInvoiceLineCollection ISFInvoiceLine
		{
			get
			{
				if (fISFInvoiceLine == null)
				{
					fISFInvoiceLine = CreateSpecificModuleTypeLineCollection(InvoiceTypeModuleList.Codes.ISF);
				}
				fISFInvoiceLine.Sort("SubInvoiceRef", ListSortDirection.Ascending);
				fISFInvoiceLine.Sort("ChargeCode+Code", ListSortDirection.Ascending);
				return fISFInvoiceLine;
			}
		}
		DocARInvoiceLineCollection fISFInvoiceLine;

		DocARInvoiceLineCollection CreateSpecificModuleTypeLineCollection(ZString moduleType)
		{
			DocARInvoiceLineCollection invoiceLineCollection = new DocARInvoiceLineCollection(Factory);

			foreach (InvoicingLineBase line in Invoice.Lines)
			{
				if (line.Job != null && GetInvoiceModule(line.Job) == moduleType)
				{
					invoiceLineCollection.Add(DocBatchARInvoiceLineTransactionLine.New(line, Factory));
				}
				else if (line.Job == null && moduleType == InvoiceTypeModuleList.Codes.MSC)
				{
					invoiceLineCollection.Add(DocBatchARInvoiceLineTransactionLine.New(line, Factory));
				}
			}

			return invoiceLineCollection;
		}

		ZString[] JobTypeList
		{
			get
			{
				var jobTypes = new HashSet<ZString>();
				invoice.Lines.Cast<InvoicingLineBase>().ToList().ForEach((x) =>
				{
					if (x.InvoicingJob != null && x.InvoicingJob.JobType != null)
					{
						jobTypes.Add(x.InvoicingJob.JobType.Code);
					}
					else
					{
						jobTypes.Add("MSC");
					}
				});
				return jobTypes.ToArray();
			}
		}

		#endregion

		#region ZString Properties

		public ZString DeclarationEntryDetails
		{
			get
			{
				ARInvoice arInvoice = TransactionHeader as ARInvoice;

				return arInvoice != null ? arInvoice.DeclarationEntryDetails : ZString.Empty;
			}
		}

		public ZString InvoiceCopyMessage => WrapperInfo.Message;

		#endregion

		#region ZBool Properties

		public ZBool PrintOrderNumbersFromRelatedShipments
		{
			get
			{
				ZBool result = false;
				if (Shipment != null && Shipment.IsBuyersConsolMaster && Invoice.Header != null &&
					 (Invoice.Header.CompanyData.EffectiveBuyersConsolInvoicingStyle == Enterprise.Core.Constants.ConsolInvoicingStyles.Master
					 || Invoice.Header.CompanyData.EffectiveBuyersConsolInvoicingStyle == Enterprise.Core.Constants.ConsolInvoicingStyles.ApportionInvoiceMaster))
				{
					result = true;
				}

				return result;
			}
		}

		#endregion

		#region Digital Signature and Certification

		public ZString QRCodeData => (AccountingCountryFactory as IQRCodeDataProvider)?.GetTransactionQRCodeString(Invoice) ?? ZString.Empty;

		public ZString RemittanceDataCHSCOR => GetRemittanceData("SCOR", Invoice.ReceiptBankAccount?.IBAN);

		public ZString RemittanceDataCHQRR => GetRemittanceData("QRR", Invoice.ReceiptBankAccount?.AB_FullAccountNumber);

		string GetRemittanceData(string referenceType, string bankAccountInfo)
		{
			var company = Invoice.Company;
			var orgHeaderAddress = Invoice.Header?.MainAddress;

			return $@"SPC
0200
1
{bankAccountInfo}
K
{company.CompanyName}
{company.Address1}
{company.Postcode} {company.City}


{company.GC_RN_NKCountryCode}







{Invoice.AH_OSTotal.FormatDecimals("F2")}
{Invoice.AH_RX_NKTransactionCurrency}
K
{orgHeaderAddress?.CompanyName}
{orgHeaderAddress?.Address1}
{orgHeaderAddress?.Postcode} {orgHeaderAddress?.City}


{orgHeaderAddress?.OA_RN_NKCountryCode}
{referenceType}
{InvoiceRemittanceReference}
{Invoice.AH_Desc}
EPD";
		}

		public ZString FiscalSoftwareCertificateNumber
		{
			get
			{
				var result = ZString.Empty;
				if (TransactionHeader.Company.Country.Code == Core.Constants.CountryCodes.Portugal && !DigitalSignature.IsEmpty)
				{
					var softwareCertificateNumber = AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.Value;
					if (!softwareCertificateNumber.IsNullOrEmpty())
					{
						result = softwareCertificateNumber.EndsWith(fiscalSoftwareCertificateSuffix, StringComparison.CurrentCultureIgnoreCase) ?
								 softwareCertificateNumber
							   : softwareCertificateNumber + fiscalSoftwareCertificateSuffix;
					}
				}
				return result;
			}
		}
		internal const string fiscalSoftwareCertificateSuffix = "/AT";

		public ZString FiscalSoftwareCertificateMessage
		{
			get
			{
				var result = ZString.Empty;
				if (TransactionHeader.Company.Country.Code == Core.Constants.CountryCodes.Portugal)
				{
					result = AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumberMessage.Value;
				}
				return result;
			}
		}

		public ZString TransactionFiscalAuthorization
		{
			get
			{
				var result = ZString.Empty;
				if (TransactionHeader.Company.Country.Code == Core.Constants.CountryCodes.Portugal
					&& !DigitalSignature.IsEmpty
					&& !FiscalSoftwareCertificateNumber.IsEmpty)
				{
					result = DigitalSignature.SubstringSafe(0, 1) + DigitalSignature.SubstringSafe(10, 1) + DigitalSignature.SubstringSafe(20, 1) + DigitalSignature.SubstringSafe(30, 1);
				}
				return result;
			}
		}

		ZString DigitalSignature
		{
			get
			{
				if (!digitalSignature.HasValue)
				{
					digitalSignature = !TransactionHeader.AH_DigitalSignature_COMPRESSED.IsEmpty
						? Convert.ToBase64String(TransactionHeader.AH_DigitalSignature_COMPRESSED)
						: string.Empty;
				}
				return digitalSignature.Value;
			}
		}
		ZString? digitalSignature;

		#endregion

		#region Tax Regime Information for AR Transaction

		public ZString TaxRegimeInformation => ObjectFactory.Get<IGlobalEInvoicingObjectFactory>()?
			.GetCountryEInvoicingRegistryInformationProvider(TransactionHeader.Company.GC_RN_NKCountryCode)?
			.GetTaxRegimeInformation() ?? ZString.Empty;

		#endregion

		#region GovernmentAgreedPaymentMethod

		public ZString GovernmentAgreedPaymentMethod
		{
			get
			{
				var governmentAgreedPaymentMethod = ZString.Empty;

				var codeAndDescriptionForGovernmentAgreedPaymentMethod = (AccountingCountryFactory as IEquivalentAgreedPaymentMethodProvider)?
					.GetEquivalentAgreedPaymentMethodProvider()
					.GetEquivalentAgreedPaymentMethod(TransactionHeader.AH_AgreedPaymentMethodOverride, PaymentMethod != null ? PaymentMethod.Code : string.Empty);

				if (codeAndDescriptionForGovernmentAgreedPaymentMethod != null &&
					!string.IsNullOrEmpty(codeAndDescriptionForGovernmentAgreedPaymentMethod.Code))
				{
					governmentAgreedPaymentMethod = codeAndDescriptionForGovernmentAgreedPaymentMethod.CodeAndDescription;
				}

				return governmentAgreedPaymentMethod;
			}
		}

		#endregion

		public ZString DebtorTaxRegime
		{
			get
			{
				var countryCode = GetInvoiceCountry();
				var deptorTaxRegime = GetAccountingCountryComplianceFeature<IDebtorTaxRegime>(countryCode);
				if (deptorTaxRegime != null)
				{
					var orgCusCode = Invoice.Header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(deptorTaxRegime.GetOrgCusCode(), countryCode);

					if (orgCusCode != null)
					{
						var registrationNumber = orgCusCode.OK_CustomsRegNo;
						var description = deptorTaxRegime.GetTaxRegimeIdTypes().GetDescriptionFromCode(registrationNumber);

						if (!string.IsNullOrEmpty(description))
						{
							return $"{registrationNumber} - {description}";
						}
					}
				}
				return ZString.Empty;
			}
		}

		public ZString SourceReference => TransactionHeader.SourceReference;

		public ZString OriginalReferenceComplianceNumber
		{
			get
			{
				var result = ZString.Empty;
				var parentTransaction = ParentTransaction;

				if (parentTransaction != null)
				{
					if (!parentTransaction.TransactionHeader.SourceReference.IsEmpty)
					{
						var sourceReferenceOptionalPrefix = PortugalComplianceInfo.GetSourceReferencePrefix(parentTransaction.TransactionHeader.AH_ComplianceSubType) + " ";
						result = parentTransaction.TransactionHeader.SourceReference.StartsWith(sourceReferenceOptionalPrefix)
							? parentTransaction.TransactionHeader.SourceReference.SubstringSafe(sourceReferenceOptionalPrefix.Length)
							: parentTransaction.TransactionHeader.SourceReference;
					}
					else
					{
						result = parentTransaction.ComplianceNumber;
					}
				}

				return result;
			}
		}

		bool IShouldExcludeFromDocPackByDefault.IsExcluded => !DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.Value && Invoice.IsCancelled;

		public ZBool HasDetentionDemurrageChargeSubGroupForOSRA
			=> Invoice.Lines
				.OfType<InvoicingLineBase>()
				.Select(l => l?.ChargeCode?.AC_ChargeSubGroup ?? ZString.Empty)
				.Any(x => x == ChargeCodeSubGroupList.Storage
					   || x == ChargeCodeSubGroupList.CarrierStorage
					   || x == ChargeCodeSubGroupList.ContainerDetention
				);

		public ZString GovernmentCreditTerms
		{
			get
			{
				if (PaymentMethod != null && !string.IsNullOrEmpty(PaymentMethod.Code))
				{
					return PaymentMethod.CodeAndDescription;
				}

				return ZString.Empty;
			}
		}

		public ZDecimal GSTVATConversionExchangeRate
		{
			get
			{
				if (Env.CurrentCompany.Country.Currency.Code == Invoice.AH_RX_NKTransactionCurrency)
				{
					return 1;
				}

				var loginCompanyCountryCurrencyExchangeRate = ExchangeRateCalculator.GetOverrideExchangeRate(
					Env.CurrentCompany.Country.Currency.Code,
					Env.CurrentCompany.LocalCurrency.PK == Env.CurrentCompany.Country.Currency.PK,
					Invoice.AH_GC,
					ExchangeRateType.Sell,
					ExchangeRateValidLedgerEnum.AR,
					Invoice.AH_InvoiceDate,
					Invoice.AH_PostDate,
					Invoice.InvoiceTaxDate);

				return loginCompanyCountryCurrencyExchangeRate != 0
					? Utilities.Round(Invoice.AH_ExchangeRate / loginCompanyCountryCurrencyExchangeRate, 6)
					: 0;
			}
		}

		public ZBool EnableEInvoicingQRCode => AccountingElectronicMessagingRegistry.Instance.EnableEInvoicingQRCode.Value;

		CodeDescriptionPair PaymentMethod => paymentMethod ?? (paymentMethod = (AccountingCountryFactory as IInvoicePaymentMethodProvider)?
			.GetInvoicePaymentMethodProvider()
			.GetInvoicePaymentMethod(TransactionHeader.AH_InvoiceTerm, TransactionHeader.AH_InvoiceDate, TransactionHeader.AH_DueDate));
		CodeDescriptionPair paymentMethod;
	}
}
