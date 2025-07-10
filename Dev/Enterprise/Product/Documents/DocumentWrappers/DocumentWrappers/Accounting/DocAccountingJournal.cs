using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ReportingBookLoader;
using Enterprise.DocumentWrappers.Accounting.TaxFramework;

namespace Enterprise.DocumentWrappers
{
	public class DocAccountingJournal : DocBaseWrapperWithJobHeader, IGenericTransactionHeaderPlugIn
	{
		DocAccountingJournal(IAccountingJournal accJournal, BusinessObjectFactory factoryToWrap)
			: base(accJournal, factoryToWrap)
		{
		}

		public static DocAccountingJournal New(IAccountingJournal accJournal, BusinessObjectFactory factoryToWrap)
		{
			return new DocAccountingJournal(accJournal, factoryToWrap);
		}

		#region IGenericTransactionHeaderPlugIn members

		GenericTransactionHeaderSupporter IGenericTransactionHeaderPlugIn.HeaderSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocAccountingJournalSupporter(this)); }
		}
		GenericTransactionHeaderSupporter fGenericTransactionSupporter;

		class DocAccountingJournalSupporter : GenericTransactionHeaderSupporter
		{
			public DocAccountingJournalSupporter(DocAccountingJournal parent)
			{
				this.Parent = parent;
			}
			protected readonly DocAccountingJournal Parent;

			protected internal override DocAccountingJournalCollection GetRelatedJournals()
			{
				var result = new DocAccountingJournalCollection(Parent.Factory);
				var journalPKs = Parent.AccountingJournal.RelatedGLJournals.Select(x => x.PK);
				DataTable generalLedgerTransactionData = null;
				if (Parent.AccountingJournal.ReportingBook != null && Parent.AccountingJournal.RelatedGLJournals.Any())
				{
					var startPostDate = Parent.AccountingJournal.RelatedGLJournals.Min(x => x.AH_PostDate);
					var endPostDate = Parent.AccountingJournal.RelatedGLJournals.Max(x => x.AH_PostDate);
					generalLedgerTransactionData = AccountingJournalReportingBookLoader.LoadReportingBook(Parent.AccountingJournal.ReportingBook.PK.ToGuid(), startPostDate, endPostDate, journalPKs);
				}
				var accountingJournals = (new AccountingJournalDataLoader(Parent.AccountingJournal.ReportingBook?.PK, generalLedgerTransactionData)).LoadTransactionsWithHeader(journalPKs);
				foreach (var journal in accountingJournals.OrderBy(x => x.TransactionNumber))
				{
					result.Add(New(journal, Parent.Factory));
				}
				return result;
			}

			protected internal override ZString GetLedger()
			{
				return Parent.Ledger;
			}

			protected internal override ZString GetTransactionNumber()
			{
				return Parent.TransactionNumber;
			}

			protected internal override ZString GetTransactionType()
			{
				return Parent.TransactionType;
			}

			protected internal override ZString GetTransactionReference() => Parent.TransactionReference;

			protected internal override ZString GetConsolidatedInvoiceRef() => Parent.ConsolidatedInvoiceRef;

			protected internal override ZString GetComplianceSubType() => Parent.ComplianceSubType;

			protected internal override ZString GetAJOptionField1Caption()
			{
				return Parent.AJOptionField1Caption;
			}

			protected internal override ZString GetAJOptionField1Value()
			{
				return Parent.AJOptionField1Value;
			}

			protected internal override ZString GetAJOptionField2Caption()
			{
				return Parent.AJOptionField2Caption;
			}

			protected internal override ZString GetAJOptionField2Value()
			{
				return Parent.AJOptionField2Value;
			}

			protected internal override ZString GetAJOptionField3Caption()
			{
				return Parent.AJOptionField3Caption;
			}

			protected internal override ZString GetAJOptionField3Value()
			{
				return Parent.AJOptionField3Value;
			}

			protected internal override ZString GetDesc()
			{
				return Parent.Desc;
			}

			protected internal override ZString GetReportingBookCode()
			{
				return Parent.ReportingBookCode;
			}

			protected internal override ZString GetReportingBookDescription()
			{
				return Parent.ReportingBookDescription;
			}

			protected internal override ZString GetChartCode()
			{
				return Parent.ChartCode;
			}

			protected internal override ZString GetChartDescription()
			{
				return Parent.ChartDescription;
			}

			protected internal override ZBool GetDisplayParentAccount()
			{
				return Parent.DisplayParentAccount;
			}

			protected internal override ZString GetCurrencyCode()
			{
				return Parent.CurrencyCode;
			}

			protected internal override ZString GetCurrentCompanyCurrencyCode()
			{
				return Parent.CurrentCompany == null || Parent.CurrentCompany.Currency == null ? ZString.Empty : Parent.CurrentCompany.Currency.Code;
			}

			protected internal override ZInt GetCurrentCompanyCurrencyDecimalPlaces()
			{
				return Parent.CurrentCompanyCurrencyDecimalPlaces;
			}

			protected internal override ZDateTime GetCreatedDate()
			{
				return Parent.CreatedDate;
			}

			protected internal override ZString GetCreatingUser()
			{
				return Parent.CreatingUser;
			}

			protected internal override DocGenericTransactionLineCollection GetAJLines()
			{
				return Parent.LinesWithNonZeroAmount;
			}

			protected internal override DocAccountingJournalTaxDetailCollection GetTaxDetails()
			{
				return Parent.TaxDetails;
			}

			protected internal override ZDecimal TotalDebitAmount => Parent.TotalDebitAmount;

			protected internal override ZDecimal TotalCreditAmount => Parent.TotalCreditAmount;

			protected internal override ZBool HasForeignCurrencyLines => Parent.HasForeignCurrencyLines;

			protected internal override ZString GetRequestedBy()
			{
				return Parent.LastPostedRequest_RequesterFullName;
			}

			protected internal override ZDateTime GetRequestedTime()
			{
				return Parent.LastPostedRequest_RequestedTime;
			}

			protected internal override ZString GetApprovedBy()
			{
				return Parent.LastPostedRequest_ApproverFullName;
			}

			protected internal override ZDateTime GetApprovedTime()
			{
				return Parent.LastPostedRequest_ApprovedTime;
			}

			protected internal override ZString GetOriginalRequester()
			{
				return Parent.OriginalRequest_RequesterFullName;
			}

			protected internal override ZString GetOriginalApprover()
			{
				return Parent.OriginalPostedRequest_ApproverFullName;
			}

			protected internal override ZString GetJournalEntriesNumber()
			{
				return Parent.JournalEntriesNumber;
			}
		}
		#endregion

		public ZString TransactionType
		{
			get { return AccountingJournal.TransactionType; }
		}

		public ZString Ledger
		{
			get { return AccountingJournal.Ledger; }
		}

		public ZString TransactionNumber
		{
			get { return AccountingJournal.TransactionNumber; }
		}

		public ZString TransactionReference => AccountingJournal.TransactionReference;

		public ZString ConsolidatedInvoiceRef => AccountingJournal.ConsolidatedInvoiceRef;

		public ZString ComplianceSubType => AccountingJournal.ComplianceSubType;

		public ZString PostToPeriod
		{
			get { return AccountingJournal.PostToPeriod; }
		}

		public ZBool IsMissingPeriodForReportingBook
		{
			get { return AccountingJournal.IsMissingPeriodForReportingBook; }
		}

		public IAccountingJournal AccountingJournal
		{
			get { return (IAccountingJournal)WrappedObject; }
		}

		public ZString ReportingBookCode
		{
			get { return AccountingJournal.ReportingBookCode; }
		}

		public ZString ReportingBookDescription
		{
			get { return AccountingJournal.ReportingBookDescription; }
		}

		public ZString ChartCode
		{
			get { return AccountingJournal.ChartCode; }
		}

		public ZString ChartDescription
		{
			get { return AccountingJournal.ChartDescription; }
		}

		public ZBool DisplayParentAccount
		{
			get { return AccountingJournal.DisplayParentAccount; }
		}

		public ZString AJOptionField1Caption
		{
			get
			{
				return (AccountingJournal.ApplicableOptionalFields != null && AccountingJournal.ApplicableOptionalFields.Count > 0) ? AccountingJournal.ApplicableOptionalFields.First().Key : ZString.Empty;
			}
		}

		public ZString AJOptionField1Value
		{
			get
			{
				return (AccountingJournal.ApplicableOptionalFields != null && AccountingJournal.ApplicableOptionalFields.Count > 0) ? AccountingJournal.ApplicableOptionalFields.First().Value : ZString.Empty;
			}
		}

		public ZString AJOptionField2Caption
		{
			get
			{
				return (AccountingJournal.ApplicableOptionalFields != null && AccountingJournal.ApplicableOptionalFields.Count > 1) ? AccountingJournal.ApplicableOptionalFields.Skip(1).First().Key : ZString.Empty;
			}
		}

		public ZString AJOptionField2Value
		{
			get
			{
				return (AccountingJournal.ApplicableOptionalFields != null && AccountingJournal.ApplicableOptionalFields.Count > 1) ? AccountingJournal.ApplicableOptionalFields.Skip(1).First().Value : ZString.Empty;
			}
		}

		public ZString AJOptionField3Caption
		{
			get
			{
				return (AccountingJournal.ApplicableOptionalFields != null && AccountingJournal.ApplicableOptionalFields.Count > 2) ? AccountingJournal.ApplicableOptionalFields.Skip(2).First().Key : ZString.Empty;
			}
		}

		public ZString AJOptionField3Value
		{
			get
			{
				return (AccountingJournal.ApplicableOptionalFields != null && AccountingJournal.ApplicableOptionalFields.Count > 2) ? AccountingJournal.ApplicableOptionalFields.Skip(2).First().Value : ZString.Empty;
			}
		}

		public ZString Desc
		{
			get
			{
				return AccountingJournal.TransactionDescription;
			}
		}

		public ZString CurrencyCode
		{
			get
			{
				return !string.IsNullOrEmpty(AccountingJournal.Currency) ? AccountingJournal.Currency : ZString.Empty;
			}
		}

		public ZInt CurrentCompanyCurrencyDecimalPlaces => AccountingJournal.LocalCurrencyDecimals;

		public ZDateTime CreatedDate
		{
			get
			{
				return AccountingJournal.CreatedDate;
			}
		}

		public ZString CreatingUser
		{
			get
			{
				return AccountingJournal.CreatedBy;
			}
		}

		public ZString LastPostedRequest_RequesterFullName
		{
			get
			{
				return AccountingJournal.LastPostedRequest_RequesterFullName;
			}
		}

		public ZDateTime LastPostedRequest_RequestedTime
		{
			get
			{
				return AccountingJournal.LastPostedRequest_RequestedTime;
			}
		}

		public ZString LastPostedRequest_ApproverFullName
		{
			get
			{
				return AccountingJournal.LastPostedRequest_ApproverFullName;
			}
		}

		public ZDateTime LastPostedRequest_ApprovedTime
		{
			get
			{
				return AccountingJournal.LastPostedRequest_ApprovedTime;
			}
		}

		public ZString OriginalRequest_RequesterFullName
		{
			get
			{
				return AccountingJournal.OriginalRequest_RequesterFullName;
			}
		}

		public ZString OriginalPostedRequest_ApproverFullName
		{
			get
			{
				return AccountingJournal.OriginalPostedRequest_ApproverFullName;
			}
		}

		public ZString JournalEntriesNumber
		{
			get
			{
				return AccountingJournal.JournalEntriesNumber;
			}
		}

		public override DocJobHeader JobHeader
		{
			get
			{
				if (jobHeader == null && AccountingJournal.Job != null)
				{
					jobHeader = DocJobHeader.New(AccountingJournal.Job, Factory);
				}

				return jobHeader;
			}
		}
		DocJobHeader jobHeader;

		public DocGenericTransactionLineCollection LinesWithNonZeroAmount
		{
			get
			{
				if (linesWithNonZeroAmount == null)
				{
					linesWithNonZeroAmount = new DocGenericTransactionLineCollection(AccountingJournal.Factory);
					var filteredLines = GetOrderedLines().Where(x => x.Item2.DebitAmountDecimal != 0 || x.Item2.CreditAmountDecimal != 0).Select(x => x.Item1).ToArray();
					linesWithNonZeroAmount.AddRange(filteredLines);
				}

				return linesWithNonZeroAmount;
			}
		}
		DocGenericTransactionLineCollection linesWithNonZeroAmount;

		DocAccountingJournalTaxDetailCollection TaxDetails
		{
			get
			{
				if (taxDetails == null)
				{
					var factory = AccountingJournal.Factory;
					taxDetails = new DocAccountingJournalTaxDetailCollection(factory);

					if (AccountingJournal.TaxDetails != null)
					{
						foreach (var item in AccountingJournal.TaxDetails)
						{
							if (item.Amount != 0M)
							{
								var taxDetailsWrapper = DocAccountingJournalTaxDetail.New(item, factory);
								taxDetails.Add(taxDetailsWrapper);
							}
						}
					}
				}

				return taxDetails;
			}
		}
		DocAccountingJournalTaxDetailCollection taxDetails;

		public ZBool HasForeignCurrencyLines
		{
			get
			{
				if (!hasForeignCurrencyLines.HasValue)
				{
					hasForeignCurrencyLines = Lines.Any(x => x.Item1.Currency.Code != AccountingJournal.Currency);
				}

				return hasForeignCurrencyLines.Value;
			}
		}
		ZBool? hasForeignCurrencyLines;

		public ZDecimal TotalDebitAmount
		{
			get
			{
				if (!totalDebitAmount.HasValue)
				{
					ZDecimal result = 0M;

					foreach (Tuple<DocGenericTransactionLine, DocAccountingJournalLine> line in Lines)
					{
						result += line.Item1.DebitAmountDecimal;
					}

					totalDebitAmount = result;
				}

				return totalDebitAmount.Value;
			}
		}
		ZDecimal? totalDebitAmount;

		public ZDecimal TotalCreditAmount
		{
			get
			{
				if (!totalCreditAmount.HasValue)
				{
					ZDecimal result = 0M;
					foreach (Tuple<DocGenericTransactionLine, DocAccountingJournalLine> line in Lines)
					{
						result += line.Item1.CreditAmountDecimal;
					}

					totalCreditAmount = result;
				}

				return totalCreditAmount.Value;
			}
		}
		ZDecimal? totalCreditAmount;

		List<Tuple<DocGenericTransactionLine, DocAccountingJournalLine>> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new List<Tuple<DocGenericTransactionLine, DocAccountingJournalLine>>();
					if (AccountingJournal.Lines != null)
					{
						foreach (var line in AccountingJournal.Lines)
						{
							if (!AccountingJournal.GroupByLinesWhilePrinting || lines.FirstOrDefault(x => x.Item2.AddAccountingJournalLine(line)) == null)
							{
								var docAJLine = DocAccountingJournalLine.New(line, Factory);
								lines.Add(Tuple.Create(DocGenericTransactionLine.New(docAJLine, Factory), docAJLine));
							}
						}
					}
				}
				return lines;
			}
		}
		List<Tuple<DocGenericTransactionLine, DocAccountingJournalLine>> lines;

		List<Tuple<DocGenericTransactionLine, DocAccountingJournalLine>> GetOrderedLines() =>
				Lines.OrderBy(x => x.Item2.ChargeCode?.Code)
								.ThenBy(x => x.Item2.RevRecognitionType)
								.ThenBy(x => x.Item2.TaxBasis)
								.ThenBy(x => x.Item2.PostDate)
								.ThenBy(x => x.Item2.PostPeriod)
								.ThenBy(x => x.Item2.GLAccount?.AccountNumber)
								.ThenBy(x => x.Item2.Branch?.BranchName)
								.ThenBy(x => x.Item2.Department?.Code)
								.ThenByDescending(x => x.Item2.DebitCreditSign).ToList();
	}
}
