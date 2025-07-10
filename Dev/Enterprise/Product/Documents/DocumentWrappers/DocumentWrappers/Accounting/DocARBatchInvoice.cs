using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocARBatchInvoice : DocARBaseInvoice
	{
		#region Construction

		protected DocARBatchInvoice(InvoiceBatchHeader batchInvoice, BusinessObjectFactory factoryToWrap, bool createInvoiceLinesCollection)
			: base(batchInvoice, factoryToWrap)
		{
			if (createInvoiceLinesCollection)
			{
				CreateBatchInvoiceLineCollections();
			}
		}

		public static DocARBatchInvoice New(InvoiceBatchHeader batchInvoice, BusinessObjectFactory factoryToWrap)
		{
			return NewCore(batchInvoice, factoryToWrap, true);
		}

		public static DocARBatchInvoice New(InvoiceBatchHeader batchInvoice, BusinessObjectFactory factoryToWrap, bool createInvoiceLinesCollection)
		{
			return NewCore(batchInvoice, factoryToWrap, createInvoiceLinesCollection);
		}

		static DocARBatchInvoice NewCore(InvoiceBatchHeader batchInvoice, BusinessObjectFactory factoryToWrap, bool createInvoiceLinesCollection)
		{
			DocARBatchInvoice result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(batchInvoice, factoryToWrap);
			}
			else if (batchInvoice != null)
			{
				result = new DocARBatchInvoice(batchInvoice, factoryToWrap, createInvoiceLinesCollection);
			}
			return result;
		}

		protected new delegate DocARBatchInvoice NewDelegate(InvoiceBatchHeader batchInvoice, BusinessObjectFactory factoryToWrap);
		protected new static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region InvoiceWrapper Fields

		#region DocWrapper Collections

		#region Invoices

		public DocARBatchInvoiceLineCollection Invoices
		{
			get
			{
				if (fInvoices == null)
				{
					fInvoices = new DocARBatchInvoiceLineCollection(Factory);
					foreach (InvoicingBase invoice in BatchInvoice.Line)
					{
						fInvoices.Add(DocARBatchInvoiceLine.New(invoice, Factory));
					}
				}

				return fInvoices;
			}
		}

		DocARBatchInvoiceLineCollection fInvoices;

		protected override DocARBatchInvoiceLineCollection GetInvoicesCore()
		{
			return Invoices;
		}

		#endregion

		#region Invoice Lines

		protected override DocARInvoiceLineCollection GetInvoiceLines()
		{
			DocARInvoiceLineCollection result = new DocARInvoiceLineCollection(Factory);
			foreach (InvoicingBase invoice in BatchInvoice.Line)
			{
				foreach (InvoicingLineBase line in invoice.Lines)
				{
					result.Add(DocBatchARInvoiceLineTransactionLine.New(line, Factory));
				}
			}
			return result;
		}

		public ZBool HasGSTANDQSTLine
		{
			get
			{
				ZBool result = false;
				foreach (DocARInvoiceLine line in InvoiceLine)
				{
					if (line.TaxRate != null
						&& (line.TaxRate.IsRatedTax())
						&& (line.TaxRate.ExtraType == AccTaxRate.ExtraTypes.QuebecQST || line.TaxRate.ExtraType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase))
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		public ZBool HasGSTANDEDULine
		{
			get
			{
				return InvoiceLine.Cast<DocARInvoiceLine>().Any(
					x => x.TaxRate != null
					&& x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India
					&& (x.TaxRate.IsRatedTax())
					&& x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax);
			}
		}

		public ZBool HasGSTANDQCTLine
		{
			get
			{
				return InvoiceLine.Cast<DocARInvoiceLine>().Any(
					x => x.TaxRate != null
					&& x.TaxRate.AccTaxRate.AT_RN_NKCountry == Core.Constants.CountryCodes.India
					&& (x.TaxRate.IsRatedTax())
					&& x.TaxRate.ExtraType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase);
			}
		}

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

		public DocARInvoiceLineCollection MSCInvoiceLine
		{
			get
			{
				if (fMSCInvoiceLine == null)
				{
					fMSCInvoiceLine = CreateSpecificModuleTypeLineCollection(InvoiceTypeModuleList.Codes.MSC);
				}
				fMSCInvoiceLine.Sort("SubInvoiceRef", ListSortDirection.Ascending);
				fMSCInvoiceLine.Sort("ChargeCode+Code", ListSortDirection.Ascending);
				return fMSCInvoiceLine;
			}
		}

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

		DocARInvoiceLineCollection CreateSpecificModuleTypeLineCollection(ZString moduleType)
		{
			DocARInvoiceLineCollection invoiceLineCollection = new DocARInvoiceLineCollection(Factory);
			foreach (InvoicingBase invoice in BatchInvoice.Line)
			{
				if (BatchInvoice.GetInvoiceModuleCode(invoice) == moduleType)
				{
					foreach (InvoicingLineBase line in invoice.Lines)
					{
						invoiceLineCollection.Add(DocBatchARInvoiceLineTransactionLine.New(line, Factory));
					}
				}
			}
			return invoiceLineCollection;
		}

		DocARInvoiceLineCollection fInvoiceLine;
		DocARInvoiceLineCollection fCFSInvoiceLine;
		DocARInvoiceLineCollection fCUSInvoiceLine;
		DocARInvoiceLineCollection fFWDInvoiceLine;
		DocARInvoiceLineCollection fMSCInvoiceLine;
		DocARInvoiceLineCollection fTPTInvoiceLine;

#if DEBUG
		protected virtual
#endif
 void CreateBatchInvoiceLineCollections()
		{
			fInvoiceLine = new DocARInvoiceLineCollection(Factory);
			fCFSInvoiceLine = new DocARInvoiceLineCollection(Factory);
			fCUSInvoiceLine = new DocARInvoiceLineCollection(Factory);
			fFWDInvoiceLine = new DocARInvoiceLineCollection(Factory);
			fMSCInvoiceLine = new DocARInvoiceLineCollection(Factory);
			fTPTInvoiceLine = new DocARInvoiceLineCollection(Factory);

			foreach (InvoicingBase invoice in BatchInvoice.Line)
			{
				DocARInvoiceLineCollection invoiceLineCollection = null;
				switch (BatchInvoice.GetInvoiceModuleCode(invoice))
				{
					case InvoiceTypeModuleList.Codes.CFS:
						invoiceLineCollection = fCFSInvoiceLine;
						break;
					case InvoiceTypeModuleList.Codes.CUS:
						invoiceLineCollection = fCUSInvoiceLine;
						break;
					case InvoiceTypeModuleList.Codes.FWD:
						invoiceLineCollection = fFWDInvoiceLine;
						break;
					case InvoiceTypeModuleList.Codes.MSC:
						invoiceLineCollection = fMSCInvoiceLine;
						break;
					case InvoiceTypeModuleList.Codes.TPT:
						invoiceLineCollection = fTPTInvoiceLine;
						break;
				}
				foreach (InvoicingLineBase line in invoice.Lines)
				{
					invoiceLineCollection.Add(DocBatchARInvoiceLineTransactionLine.New(line, Factory));
					fInvoiceLine.Add(DocBatchARInvoiceLineTransactionLine.New(line, Factory));
				}
			}
		}

		#endregion

		#endregion

		#region DocWrappers

		public override DocBankAccount ReceiptBankAccount
		{
			get { return DocBankAccount.New(ReceiptBank, Factory); }
		}

		#endregion

		#region BizO InvoiceWrapper

		protected InvoiceBatchHeader BatchInvoice
		{
			get { return (InvoiceBatchHeader)WrappedObject; }
		}

		AccBankAccount ReceiptBank
		{
			get
			{
				ZGuid headerPK = BatchInvoice.Header != null ? BatchInvoice.Header.PK : ZGuid.Empty;
				ZString currencyNK = BatchInvoice.AH_RX_NKTransactionCurrency;

				return AccBankAccount.GetDefaultReceiptBankAccountForDebtor(headerPK, currencyNK, BatchInvoice.Branch, Factory);
			}
		}

		#endregion

		#endregion

		protected override DocARInvoiceLineCollection GetLinesToFormat()
		{
			return InvoiceLine;
		}

		#region ZString

		protected override ZString StatementDescriptionCore
		{
			get
			{
				ZString result = Res.GetString("460cf203-e916-4e48-ae6f-1980109d132c", "AR INVOICE - VARIOUS JOBS");
				return result;
			}
		}

		public ZString BatchInvoiceNumber
		{
			get
			{
				return BatchInvoice.TransactionNumberPrefixed;
			}
		}

		public ZString OrganisationCode
		{
			get { return (Organisation != null) ? Organisation.Code : ZString.Empty; }
		}

		public ZString OSTaxDisplayHeading
		{
			get
			{
				ZString result = "";

				if (HasGSTANDQCTLine && HasGSTANDEDULine)
				{
					result = "SER/CESS/EDU";
				}
				else if (HasGSTANDQCTLine && !HasGSTANDEDULine)
				{
					result = "SER/CESS";
				}
				else if (!HasGSTANDQCTLine && HasGSTANDEDULine)
				{
					result = "SER/EDU";
				}
				else if (HasGSTANDQSTLine)
				{
					result = "GST/QST";
				}
				else if (IsTaxed)
				{
					result = GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription;
				}
				return result;
			}
		}

		public ZString TaxId
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (DocARInvoice invoice in Invoices)
				{
					result = invoice.TaxId;

					if (!result.IsEmpty)
					{
						break;
					}
				}

				return result;
			}
		}

		public ZString DocumentTitle
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsTaxed)
				{
					result = AccountingConfigurationRegistry.Instance.TaxInvoiceBatchTitle.Value;
				}
				else
				{
					result = AccountingConfigurationRegistry.Instance.NonTaxInvoiceBatchTitle.Value;
				}
				return result;
			}
		}

		public ZString DetailsDocumentTitle
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsTaxed)
				{
					result = AccountingConfigurationRegistry.Instance.TaxInvoiceBatchDetailsTitle.Value;
				}
				else
				{
					result = AccountingConfigurationRegistry.Instance.NonTaxInvoiceBatchDetailsTitle.Value;
				}
				return result;
			}
		}

		public virtual MultilingualString CreditTerms
		{
			get
			{
				MultilingualString result = (NoResString)ZString.Empty;

				CodeDescriptionPairList termList = new InvoiceTermsListWithShortDescription();

				if (InvoiceTerm != "")
				{
					string invoiceTermDescription = termList.GetMultilingualDescriptionFromCode(InvoiceTerm);
					if (invoiceTermDescription != null)
					{
						result = (NoResString)string.Format(termList.GetMultilingualDescriptionFromCode(InvoiceTerm).ToString().Trim(), InvoiceTermDays.ToString());
					}
					else
					{
						result = (NoResString)(Res.GetString("f4da2241-1fa0-4627-8d93-e631052a13a8", "{0} days", InvoiceTermDays.ToString()) + " ");
					}
				}
				else
				{
					if (termList.GetMultilingualDescriptionFromCode(Core.Constants.InvoiceTerms.CashOnDelivery) != null)
					{
						result = (NoResString)(termList.GetMultilingualDescriptionFromCode(Core.Constants.InvoiceTerms.CashOnDelivery).ToString().Trim());
					}
				}
				return result;
			}
		}

		public ZString Message
		{
			get { return AccountingConfigurationRegistry.Instance.InvoiceMessage.Value; }
		}

		public ZString PaymentReference
		{
			get
			{
				return TransactionNumberPrefixed;
			}
		}

		public ZString BatchInvoiceType
		{
			get
			{
				ZString result = InvoiceTypeLayoutList.Codes.CHG;

				if (BatchInvoice.Header != null && BatchInvoice.Header.CompanyData != null)
				{
					var invoiceTypes = BatchInvoice.Header.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "ALL", true, BatchJobTypeList.ToArray());
					if (invoiceTypes != null && invoiceTypes.Count > 0)
					{
						result = invoiceTypes.Values.First().PI_Type;
					}
				}

				return result;
			}
		}

		public ZString BatchInvoiceSecondaryType
		{
			get
			{
				ZString result = InvoiceTypeLayoutList.Codes.CHG;

				if (BatchInvoice.Header != null && BatchInvoice.Header.CompanyData != null)
				{
					var invoiceTypes = BatchInvoice.Header.CompanyData.GetApplicableInvoiceTypes("ALL", "ALL", "ALL", true, BatchJobTypeList.ToArray());
					if (invoiceTypes.Count > 0)
					{
						result = invoiceTypes.Values.First().PI_SecondaryType;
					}
				}

				return result;
			}
		}

		public IEnumerable<ZString> BatchJobTypeList
		{
			get
			{
				var jobTypes = new HashSet<ZString>();
				BatchInvoice.Line.Cast<InvoicingBase>().ToList().ForEach((x) =>
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

		public ZString[] BatchInvoiceModuleList
		{
			get { return BatchInvoice.BatchInvoiceModuleList.ToArray(); }
		}

		#endregion

		#region ZBool

		public override ZBool IsTaxed
		{
			get
			{
				return Invoices.OfType<DocARInvoice>().Any(x => x.IsTaxed);
			}
		}

		public ZBool IsCompanyRegisteredForGST
		{
			get { return CurrentCompany.IsGSTRegistered; }
		}

		#endregion

		#region ZDecimal

		public ZDecimal InvoiceSubTotalTotal
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoice invoice in Invoices)
				{
					result += invoice.InvoiceSubTotal;
				}
				return result;
			}
		}

		public ZDecimal InvoiceOSTaxAmountTotal
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoice invoice in Invoices)
				{
					result += invoice.TotalOSTaxAmount;
				}
				return result;
			}
		}

		public ZDecimal InvoiceTotal
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoice invoice in Invoices)
				{
					result += invoice.OSTotal;
				}
				return result;
			}
		}

		public ZDecimal TotalOSEDUAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoice invoice in Invoices)
				{
					result += invoice.TotalOSEDUAmount;
				}
				return result;
			}
		}

		public ZString TotalOSEDUAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSEDUAmount, Currency);
			}
		}

		public ZDecimal TotalOSEDUPrimaryAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoice invoice in Invoices)
				{
					result += invoice.TotalOSEDUPrimaryAmount;
				}
				return result;
			}
		}

		public ZString TotalOSEDUPrimaryAmountFormatted
		{
			get { return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSEDUPrimaryAmount, Currency); }
		}

		public ZDecimal TotalOSEDUSecondaryAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (DocARInvoice invoice in Invoices)
				{
					result += invoice.TotalOSEDUSecondaryAmount;
				}
				return result;
			}
		}

		public ZString TotalOSEDUSecondaryAmountFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOSEDUSecondaryAmount, Currency);
			}
		}

		protected override ZDecimal BalanceCore
		{
			get
			{
				ZDecimal result = 0;

				foreach (DocTransactionHeader invoice in Invoices)
				{
					result += invoice.Balance;
				}

				return result;
			}
		}

		#endregion

		#region ZDateTime

		public ZDateTime PeriodEnding
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				foreach (DocARInvoice invoice in Invoices)
				{
					if (invoice.InvoiceDate > result)
					{
						result = InvoiceDate;
					}
				}

				if (result.IsEmpty)
				{
					result = ZDateTime.Now;
				}

				return result;
			}
		}

		#endregion
	}
}
