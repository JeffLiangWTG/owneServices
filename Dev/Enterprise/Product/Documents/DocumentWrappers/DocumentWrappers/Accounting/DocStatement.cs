using System;
using System.Drawing;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocStatement : DocBaseWrapper, IGenericTransactionHeaderPlugIn
	{
		#region Construction

		protected DocStatement(PrintStatement printStatement, BusinessObjectFactory factoryToWrap)
			: base(printStatement, factoryToWrap)
		{
		}

		public static DocStatement New(PrintStatement printStatement, BusinessObjectFactory factoryToWrap)
		{
			DocStatement result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(printStatement, factoryToWrap);
			}
			else if (printStatement != null && !(printStatement is PrintStatementForAccountMovement))
			{
				result = new DocStatement(printStatement, factoryToWrap);
			}
			else if (printStatement != null && printStatement is PrintStatementForAccountMovement)
			{
				result = new DocStatementForAccountMovement(printStatement as PrintStatementForAccountMovement, factoryToWrap);
			}

			return result;
		}

		protected delegate DocStatement NewDelegate(PrintStatement printStatement, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region IGenericTransactionPlugIn members

		GenericTransactionHeaderSupporter IGenericTransactionHeaderPlugIn.HeaderSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocStatementGenericTransactionSupporter(this)); }
		}
		DocStatementGenericTransactionSupporter fGenericTransactionSupporter;

		#endregion

		public class DocStatementGenericTransactionSupporter : GenericTransactionHeaderSupporter
		{
			public DocStatementGenericTransactionSupporter(DocStatement parent)
			{
				this.Parent = parent;
			}

			protected readonly DocStatement Parent;

			protected internal override ZString GetTransactionType()
			{
				return "PST";
			}

			protected internal override Image GetStatementLogo()
			{
				return Parent.StatementLogo;
			}

			protected internal override ZString GetOrganisationCode()
			{
				return Parent.Organisation == null ? ZString.Empty : Parent.Organisation.Code;
			}

			protected internal override ZString GetOrganisationName()
			{
				return Parent.Organisation == null ? ZString.Empty : Parent.Organisation.Name;
			}

			protected internal override ZString GetOrganisationARAgreedPaymentMethod()
			{
				return Parent.OrganisationARAgreedPaymentMethod;
			}

			protected internal override ZString GetOrganisationAPAgreedPaymentMethod()
			{
				return Parent.OrganisationAPAgreedPaymentMethod;
			}

			protected internal override ZString GetCurrencyCode()
			{
				return Parent.Currency == null ? ZString.Empty : Parent.Currency.Code;
			}

			protected internal override ZString GetSTDTerms()
			{
				return Parent.Organisation == null || Parent.Organisation.MiscServ == null ? ZString.Empty : Parent.Organisation.MiscServ.ShortenedCreditTerms;
			}

			protected internal override ZString GetDSBTerms()
			{
				return Parent.Organisation == null || Parent.Organisation.MiscServ == null ? ZString.Empty : Parent.Organisation.MiscServ.ShortenedDisbursementCreditTerms;
			}

			protected internal override ZGuid GetOrganisationARAddressOrgAddressPK()
			{
				return Parent.Organisation == null || Parent.Organisation.ARAddress == null || Parent.Organisation.ARAddress.OrgAddress == null ? ZGuid.Empty : Parent.Organisation.ARAddress.OrgAddress.PK;
			}

			protected internal override ZBool GetDisbursementInvoicesOnly()
			{
				return Parent.DisbursementInvoicesOnly;
			}

			protected internal override ZDateTime GetDisplayDate()
			{
				return Parent.DisplayDate;
			}

			protected internal override ZString GetDocumenTitle()
			{
				return Parent.DocumentTitle;
			}

			protected internal override ZString GetTaxId()
			{
				return Parent.TaxId;
			}

			protected internal override ZBool GetIsStrongHeading()
			{
				return Parent.IsStrongHeading;
			}

			protected internal override ZBool GetIsStatement()
			{
				return Parent.IsStatement;
			}

			protected internal override ZBool GetIsAccountMovementListing()
			{
				return Parent.IsAccountMovementListing;
			}

			protected internal override ZString GetAccountMovementListingGroupBy()
			{
				return Parent.AccountMovementListingGroupBy;
			}

			protected internal override ZDecimal GetAccountMovementOpeningBalance()
			{
				return Parent.OpeningBalance;
			}

			protected internal override ZDecimal GetAccountMovementClosingBalance()
			{
				return Parent.ClosingBalance;
			}

			protected internal override ZDateTime GetAccountMovementOpeningBalanceDate()
			{
				return Parent.OpeningBalanceDate;
			}

			protected internal override ZDateTime GetAccountMovementClosingBalanceDate()
			{
				return Parent.ClosingBalanceDate;
			}

			protected internal override ZString GetOpeningText()
			{
				return Parent.OpeningText;
			}

			protected internal override ZBool GetIssueBySettlementGroup()
			{
				return Parent.IssueBySettlementGroup;
			}

			protected internal override ZString GetCreditBalanceMessage()
			{
				return Parent.CreditBalanceMessage;
			}

			protected internal override ZBool GetHasCreditBalance()
			{
				return Parent.HasCreditBalance;
			}

			protected internal override ZString GetTotalCurrent()
			{
				return Parent.TotalCurrent;
			}

			protected internal override ZString GetTotalOverdue()
			{
				return Parent.TotalOverdue;
			}

			protected internal override ZBool GetDisplayNotYetOutstandingAmount()
			{
				return Parent.DisplayNotYetOutstandingAmount;
			}

			protected internal override ZString GetReceiptsNotMatched()
			{
				return Parent.ReceiptsNotMatched;
			}

			protected internal override ZString GetMessage()
			{
				return Parent.Message;
			}

			protected internal override ZString GetStatementBalanceFormatted()
			{
				return Parent.StatementBalanceFormatted;
			}

			protected internal override ZString GetPaymentRequestText()
			{
				return Parent.PaymentRequestText;
			}

			protected internal override ZString GetReceiptBankAccountBSB()
			{
				return Parent.ReceiptBankAccount == null ? ZString.Empty : Parent.ReceiptBankAccount.BSB;
			}

			protected internal override ZString GetReceiptBankAccountSWIFT()
			{
				return Parent.ReceiptBankAccount == null ? ZString.Empty : Parent.ReceiptBankAccount.SWIFT;
			}

			protected internal override ZString GetReceiptBankAccountAccountNum()
			{
				return Parent.ReceiptBankAccount == null ? ZString.Empty : Parent.ReceiptBankAccount.AccountNum;
			}

			protected internal override ZString GetReceiptBankAccountBankName()
			{
				return Parent.ReceiptBankAccount == null ? ZString.Empty : Parent.ReceiptBankAccount.BankName;
			}

			protected internal override ZString GetReceiptBankAccountBankAddress()
			{
				return Parent.ReceiptBankAccount == null ? ZString.Empty : Parent.ReceiptBankAccount.BankAddress;
			}

			protected internal override ZString GetReceiptBankAccountIBAN()
			{
				return Parent.ReceiptBankAccount == null ? ZString.Empty : Parent.ReceiptBankAccount.IBAN;
			}

			protected internal override ZString GetMailToAddressWithCountry()
			{
				return Parent.MailToAddressWithCountry;
			}

			protected internal override DocTransactionHeaderCollection GetTransactions()
			{
				return Parent.Transactions;
			}

			protected internal override DocGenericTransactionHeaderCollection GetGenericTransactions()
			{
				return Parent.GenericTransactions;
			}

			protected internal override ZString GetStatementTotalOverdueAmount()
			{
				return Parent.StatementTotalOverdueAmount;
			}

			protected internal override ZString GetStatementTotalCurrentAmount()
			{
				return Parent.TotalCurrentAmount;
			}

			protected internal override ZString GetStatementTotalDueAmount()
			{
				return Parent.StatementTotalDueAmount;
			}

			protected internal override ZString GetStatementTotalStatementAmount()
			{
				return Parent.StatementTotalStatementAmount;
			}

			protected internal override ZString GetTotal30DaysOverdue()
			{
				return Parent.Total30DaysOverdue;
			}

			protected internal override ZString GetTotal60DaysOverdue()
			{
				return Parent.Total60DaysOverdue;
			}

			protected internal override ZString GetTotal90DaysOverdue()
			{
				return Parent.Total90DaysOverdue;
			}

			protected internal override ZString GetTotal90PlusDaysOverdue()
			{
				return Parent.Total90PlusDaysOverdue;
			}

			protected internal override ZString GetTotalDueBetween0To29thDay()
			{
				return Parent.TotalDueBetween0To29thDay;
			}

			protected internal override ZString GetTotalDueBetween30To59thDay()
			{
				return Parent.TotalDueBetween30To59thDay;
			}

			protected internal override ZString GetTotalDueBetween60To89thDay()
			{
				return Parent.TotalDueBetween60To89thDay;
			}

			protected internal override ZString GetTotalDueBetween90To119thDay()
			{
				return Parent.TotalDueBetween90To119thDay;
			}

			protected internal override ZString GetTotalDueBetween120To149thDay()
			{
				return Parent.TotalDueBetween120To149thDay;
			}

			protected internal override ZString GetTotalDue150PlusDay()
			{
				return Parent.TotalDue150PlusDay;
			}

			protected internal override ZString GetTotal30DaysDue()
			{
				return Parent.Total30DaysDue;
			}

			protected internal override ZString GetTotal30PlusDaysOverDue()
			{
				return Parent.Total30PlusDaysOverDue;
			}
		}

		#region Logo

		public virtual Image StatementLogo
		{
			get
			{
				if (DocumentBrandingImage != null)
				{
					return DocumentBrandingImage;
				}
				else
				{
					if (Branch != null)
					{
						return Branch.GetInvoiceAndStatementLogo(GlbDepartment.CurrentDepartment.PK.ToGuid());
					}
				}

				return base.CompanyLogo;
			}
		}

		#endregion

		#region Properties

		public class Constants : Statement.Constants
		{
			public const string CalculatedMatchedAmount = "CalculatedMatchedAmount";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "value used inside switch..case.")]
			public const string CalculateDueByDueDate = "Due Date";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "value used inside switch..case.")]
			public const string CalculateDueByInvoiceDate = "Invoice Date";

			public static string ImmediatePaymentRequired
			{
				get { return Res.GetString("d6eeff81-56af-494b-b551-480ac548145c", "IMMEDIATE PAYMENT REQUIRED:") + " "; }
			}

			public static string ImmediatePaymentRequested
			{
				get { return Res.GetString("7da0a2d0-89da-4116-9428-f40509136d12", "IMMEDIATE PAYMENT REQUESTED:") + " "; }
			}
		}

		protected virtual ZDecimal CalculateTotalOverdue()
		{
			ZDecimal totalOverdue = 0M;
			foreach (DocTransactionHeader transaction in Transactions)
			{
				if (transaction.DueDate < Env.Time.CurrentLocalDate)
				{
					totalOverdue += transaction.Balance;
				}
			}
			return totalOverdue;
		}

		public ZString TotalOverdue
		{
			get
			{
				ZString result = ZString.Empty;
				if (TotalOverdueAmount != ZString.Empty)
				{
					result = Res.GetString("d361fb60-f5eb-4d7f-8091-7053a23b2bf5", "Overdue at statement date: {0} {1}", TotalOverdueAmount, Currency.Code);
				}

				return result;
			}
		}

		public ZBool DisplayNotYetOutstandingAmount
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.DisplayNotYetOutstandingAmountOnARStatementDocuments.Value;
			}
		}

		public ZString TotalCurrent
		{
			get
			{
				ZString result = ZString.Empty;
				if (TotalCurrentAmount != ZString.Empty)
				{
					result = Res.GetString("65262d50-6947-40ee-b713-d86e76a01a85", "Current at statement date: {0} {1}", TotalCurrentAmount, Currency.Code);
				}

				return result;
			}
		}

		public ZString ReceiptsNotMatched
		{
			get
			{
				ZString result = ZString.Empty;

				if (TotalOfReceiptsNotMatchedDecimal != 0M)
				{
					result = Res.GetString("65a0e348-dd2a-4436-89b4-468df107881a", "Total Unallocated Receipts:") + " " + TotalOfReceiptsNotMatchedAmount + " " + Currency.Code;
				}

				return result;
			}
		}

		public ZString TotalOfReceiptsNotMatchedAmount
		{
			get
			{
				ZString result = ZString.Empty;

				if (TotalOfReceiptsNotMatchedDecimal != 0M)
				{
					result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOfReceiptsNotMatchedDecimal, Currency);
				}

				return result;
			}
		}

		ZDecimal TotalOfReceiptsNotMatchedDecimal
		{
			get
			{
				ZDecimal result = 0m;

				foreach (DocTransactionHeader transaction in Transactions)
				{
					if (transaction.TransactionType == ZArchitecture.Core.TransactionTypes.Receipt)
					{
						result += transaction.Balance;
					}
				}

				return result;
			}
		}

		public virtual ZString TotalCurrentAmount
		{
			get
			{
				ZString result = ZString.Empty;
				if (DisplayNotYetOutstandingAmount && TotalCurrentDecimal != 0M)
				{
					result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalCurrentDecimal, Currency);
				}
				return result;
			}
		}

		public ZString TotalOverdueAmount
		{
			get
			{
				ZString result = ZString.Empty;
				if (TotalOverdueDecimal != 0M)
				{
					result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOverdueDecimal, Currency);
				}
				return result;
			}
		}
		public ZString StatementTotalOverdueAmount
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value)
				{
					result = Res.GetString("205125ce-a680-4188-b6cb-4f4f24427ad5", "{0} {1}", TotalOverdueAmount, Currency.Code);
				}
				return result;
			}
		}

		protected ZDecimal TotalCurrentDecimal
		{
			get
			{
				if (fTotalCurrentDecimal.IsEmpty)
				{
					fTotalCurrentDecimal = StatementBalance - TotalOverdueDecimal;
				}
				return fTotalCurrentDecimal;
			}
		}
		ZDecimal fTotalCurrentDecimal;

		ZDecimal TotalOverdueDecimal
		{
			get
			{
				if (fTotalOverdueDecimal.IsEmpty)
				{
					fTotalOverdueDecimal = CalculateTotalOverdue();
				}
				return fTotalOverdueDecimal;
			}
		}
		ZDecimal fTotalOverdueDecimal;

		public ZString TotalDueAmount
		{
			get
			{
				ZString result = ZString.Empty;
				ZDecimal totalDue = GetTotalDueAmnt();
				if (totalDue != 0M)
				{
					result = totalDue.ToString(2);
				}
				return result;
			}
		}

		public ZString StatementTotalDueAmount
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value)
				{
					ZDecimal totalDue = GetTotalDueAmnt();
					result = Res.GetString("4b19bc1d-f674-44a8-a084-c04009eafc84", "{0} {1}", FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(totalDue, Currency), Currency.Code);
				}
				return result;
			}
		}

		public ZString TotalStatementAmount
		{
			get
			{
				ZString result = ZString.Empty;
				ZDecimal totalDue = GetTotalStmntAmnt();
				if (totalDue != 0M)
				{
					result = totalDue.ToString(2);
				}

				return result;
			}
		}

		public ZString StatementTotalStatementAmount
		{
			get
			{
				ZString result = ZString.Empty;
				if (AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value)
				{
					ZDecimal totalDue = GetTotalStmntAmnt();
					result = Res.GetString("f222f787-b9c6-4a94-b308-8d4c4b1bf4f1", "{0} {1}", FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(totalDue, Currency), Currency.Code);
				}
				return result;
			}
		}

		public ZString Total30DaysOverdue
		{
			get
			{
				return GetDueAmountAsFormattedString(AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value, Constants.CalculateDueByDueDate, true, 1, 30);
			}
		}
		public ZString Total60DaysOverdue
		{
			get
			{
				return GetDueAmountAsFormattedString(AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value, Constants.CalculateDueByDueDate, true, 31, 60);
			}
		}
		public ZString Total90DaysOverdue
		{
			get
			{
				return GetDueAmountAsFormattedString(AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value, Constants.CalculateDueByDueDate, true, 61, 90);
			}
		}
		public ZString Total90PlusDaysOverdue
		{
			get
			{
				return GetDueAmountAsFormattedString(AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value, Constants.CalculateDueByDueDate, true, 91, 0);
			}
		}

		public ZString TotalDueBetween0To29thDay
		{
			get
			{
				return GetDueAmountAsFormattedString(AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value, Constants.CalculateDueByInvoiceDate, true, 0, 29);
			}
		}
		public ZString TotalDueBetween30To59thDay
		{
			get
			{
				return GetDueAmountAsFormattedString(AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value, Constants.CalculateDueByInvoiceDate, true, 30, 59);
			}
		}
		public ZString TotalDueBetween60To89thDay
		{
			get
			{
				return GetDueAmountAsFormattedString(AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value, Constants.CalculateDueByInvoiceDate, true, 60, 89);
			}
		}
		public ZString TotalDueBetween90To119thDay
		{
			get
			{
				return GetDueAmountAsFormattedString(AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value, Constants.CalculateDueByInvoiceDate, true, 90, 119);
			}
		}
		public ZString TotalDueBetween120To149thDay
		{
			get
			{
				return GetDueAmountAsFormattedString(AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value, Constants.CalculateDueByInvoiceDate, true, 120, 149);
			}
		}
		public ZString TotalDue150PlusDay
		{
			get
			{
				return GetDueAmountAsFormattedString(AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value, Constants.CalculateDueByInvoiceDate, true, 150, -1);
			}
		}

		public ZString Total30DaysDue
		{
			get
			{
				return GetDueAmountAsFormattedString(true, Constants.CalculateDueByDueDate, false, 0, 30);
			}
		}
		public ZString Total30PlusDaysOverDue
		{
			get
			{
				return GetDueAmountAsFormattedString(AccountingConfigurationRegistry.Instance.DisplayARStatementAgeingFields.Value, Constants.CalculateDueByDueDate, true, 31, -1);
			}
		}

		public ZString TaxId
		{
			get
			{
				ZString result = ZString.Empty;

				if (AccountingConfigurationRegistry.Instance.DisplayTaxRegistrationNumber.Value && GlbCompany.CurrentCompany != null)
				{
					ZString countryTaxCode = Res.GetString("6c7b6927-6a44-4598-ac95-349196b535e3", "TAX #:") + " ";
					if (GlbCompany.CurrentCompany.Country != null)
					{
						if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica)
						{
							countryTaxCode = Res.GetString("0bf428c7-b0f2-496b-b3b7-abdfc5b27be3", "VAT #:") + " ";
						}
						else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Singapore)
						{
							countryTaxCode = Res.GetString("7690a810-721d-4cc4-aebd-452997b4791f", "GST Reg No:") + " ";
						}
					}

					result = countryTaxCode + GlbCompany.CurrentCompany.GC_BusinessRegNo;
				}

				return result;
			}
		}

		public ZString DocumentTitle
		{
			get
			{
				return PrintStatement.GetDocumentName();
			}
		}

		public ZString OpeningText
		{
			get
			{
				ZString result = ZString.Empty;

				switch (PrintStatement.DocumentToPrint)
				{
					case Core.Constants.StatementCollectionLetterType.StatementOfAccount:
						result = ZString.Empty;
						break;

					case Core.Constants.StatementCollectionLetterType.FirstReminder:
						result = AccountingMasterFilesRegistry.Instance.CollectionAndDemandFirstReminderOpeningText.Value;
						break;

					case Core.Constants.StatementCollectionLetterType.SecondReminder:
						result = AccountingMasterFilesRegistry.Instance.CollectionAndDemandSecondReminderOpeningText.Value;
						break;

					case Core.Constants.StatementCollectionLetterType.CollectionLetter:
						result = AccountingMasterFilesRegistry.Instance.CollectionLetterOpeningText.Value;
						break;

					case Core.Constants.StatementCollectionLetterType.DemandLetter:
						result = AccountingMasterFilesRegistry.Instance.DemandLetterOpeningText.Value;
						break;
				}

				return result;
			}
		}

		public ZString PaymentRequestText
		{
			get
			{
				ZString result;

				if (PrintStatement.DocumentToPrint == Core.Constants.StatementCollectionLetterType.CollectionLetter ||
					PrintStatement.DocumentToPrint == Core.Constants.StatementCollectionLetterType.DemandLetter)
				{
					result = Constants.ImmediatePaymentRequired;
				}
				else
				{
					result = Constants.ImmediatePaymentRequested;
				}

				return result;
			}
		}

		public ZString Message
		{
			get { return AccountingConfigurationRegistry.Instance.StatementStandardMessage.Value; }
		}

		public ZString MailToAddressWithCountry
		{
			get
			{
				ZString result = ZString.Empty;
				if (Branch != null && Branch.MailToAddress != null)
				{
					result = Branch.MailToAddress.PostalAddress;
				}
				return result;
			}
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public virtual ZDecimal StatementBalance
		{
			get
			{
				ZDecimal total = 0M;

				if (Transactions != null)
				{
					foreach (DocTransactionHeader header in Transactions)
					{
						total += header.Balance;
					}
				}

				return total;
			}
		}

		public ZString StatementBalanceFormatted
		{
			get
			{
				return FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(StatementBalance, Currency);
			}
		}

		public ZDateTime DisplayDate
		{
			get { return PrintStatement.StatementDisplayDate; }
		}

		public ZDateTime CutOffDate
		{
			get
			{
				ZDateTime result = Env.Time.CurrentLocalDate;

				if (!PrintStatement.CutOffDate.IsEmpty)
				{
					result = PrintStatement.CutOffDate.AddDays(-1);
				}

				return result;
			}
		}

		public ZBool IsStatement
		{
			get { return (PrintStatement != null && PrintStatement.DocumentToPrint == Core.Constants.StatementCollectionLetterType.StatementOfAccount); }
		}

		public ZBool IsAccountMovementListing
		{
			get { return (IsStatement && PrintStatement is PrintStatementForAccountMovement); }
		}

		public virtual ZString AccountMovementListingGroupBy
		{
			get { return ZString.Empty; }
		}

		public virtual ZDateTime OpeningBalanceDate
		{
			get
			{
				return ZDateTime.Empty;
			}
		}

		public virtual ZDateTime ClosingBalanceDate
		{
			get
			{
				return ZDateTime.Empty;
			}
		}

		public virtual ZDecimal OpeningBalance
		{
			get
			{
				return ZDecimal.Zero;
			}
		}

		public virtual ZDecimal ClosingBalance
		{
			get
			{
				return ZDecimal.Zero;
			}
		}

		public ZBool DisbursementInvoicesOnly
		{
			get { return (PrintStatement != null && PrintStatement.DisbursementInvoicesOnly); }
		}

		public ZBool IsStrongHeading
		{
			get
			{
				ZBool result = ZBool.False;

				if (PrintStatement.DocumentToPrint == Core.Constants.StatementCollectionLetterType.CollectionLetter ||
					PrintStatement.DocumentToPrint == Core.Constants.StatementCollectionLetterType.DemandLetter)
				{
					result = ZBool.True;
				}

				return result;
			}
		}

		public ZBool IssueBySettlementGroup
		{
			get { return PrintStatement.IssueBySettlementGroup; }
		}

		public ZBool IssueByTransactionBranch
		{
			get { return PrintStatement.IssueByTransactionBranch; }
		}

		public ZBool IssueByTransactionDepartment
		{
			get { return PrintStatement.IssueByTransactionDepartment; }
		}

		public ZBool HasCreditBalance
		{
			get { return StatementBalance < 0; }
		}

		public ZString CreditBalanceMessage
		{
			get { return Res.GetString("6b4222b3-78e8-47c3-a1b2-0ae2352c9967", "THIS IS A CREDIT {0}, NO PAYMENT REQUIRED", DocumentTitle); }
		}

		protected PrintStatement PrintStatement
		{
			get { return (PrintStatement)WrappedObject; }
		}

		protected virtual ZString GetDueAmountAsFormattedString(bool isAllowed, string dueDateOption, bool isOverdue, int lowerAgeLimit, int upperAgeLimit)
		{
			ZString result = ZString.Empty;
			if (isAllowed)
			{
				result = Res.GetString("eaf19209-f2e5-46bc-b938-4259d9e4cf70", "{0} {1}", FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(GetDueAmount(Env.Time.CurrentLocalDate, dueDateOption, isOverdue, lowerAgeLimit, upperAgeLimit), Currency), Currency.Code);
			}
			return result;
		}

		protected ZDecimal GetDueAmount(DateTime baseDate, string dueDateOption, bool isOverdue, int lowerAgeLimit, int upperAgelimit = 0, ZDateTime? defaultDueDate = null)
		{
			ZDecimal totalDue = 0m;
			ZDateTime startDate = DateTime.MinValue;
			ZDateTime endDate = DateTime.MinValue;

			if (isOverdue)
			{
				endDate = baseDate.Date.AddDays((-1) * lowerAgeLimit);
				if (upperAgelimit > 0)
				{
					startDate = baseDate.Date.AddDays((-1) * upperAgelimit);
				}
			}
			else
			{
				startDate = baseDate.Date.AddDays(lowerAgeLimit);
				if (upperAgelimit > 0)
				{
					endDate = baseDate.Date.AddDays(upperAgelimit);
				}
			}

			ZDecimal totalOverdue = 0M;
			ZDate dueDate = (ZDate)Env.Time.CurrentLocalDate.Date;
			foreach (DocTransactionHeader transaction in TransactionsForDueBuckets)
			{
				switch (dueDateOption)
				{
					case Constants.CalculateDueByDueDate:
						dueDate = transaction.DueDate.IsValid ? transaction.DueDate.Date : (defaultDueDate.HasValue ? defaultDueDate.Value.Date : (ZDate)Env.Time.CurrentLocalDate.Date);
						break;
					case Constants.CalculateDueByInvoiceDate:
						dueDate = transaction.InvoiceDate.IsValid ? transaction.InvoiceDate.Date : (defaultDueDate.HasValue ? defaultDueDate.Value.Date : (ZDate)Env.Time.CurrentLocalDate.Date);
						break;
					default:
						break;
				}

				if ((!startDate.IsValid || dueDate >= startDate) && (!endDate.IsValid || dueDate <= endDate))
				{
					totalDue += transaction.Balance;
				}
			}
			return totalDue;
		}

		protected virtual ZDecimal GetTotalDueAmnt()
		{
			ZDecimal totalDue = 0M;
			foreach (DocTransactionHeader transaction in Transactions)
			{
				if (transaction.DueDate >= Env.Time.CurrentLocalDate)
				{
					totalDue += transaction.Balance;
				}
			}
			return totalDue;
		}

		protected virtual ZDecimal GetTotalStmntAmnt()
		{
			ZDecimal totalDue = 0M;
			foreach (DocTransactionHeader transaction in Transactions)
			{
				totalDue += transaction.Balance;
			}
			return totalDue;
		}

		#endregion

		#region Transaction Collection

		public virtual DocTransactionHeaderCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					TransactionHeaderCollection transactionHeaders = PrintStatement.Transactions;
					DocTransactionHeaderCollection transactionsList = new DocTransactionHeaderCollection(PrintStatement.Factory);
					fTransactions = new DocTransactionHeaderCollection(Factory);
					foreach (TransactionHeader currentTransaction in transactionHeaders)
					{
						if (currentTransaction.AH_TransactionType == TransactionTypes.InvoiceBatch)
						{
							InvoiceBatchHeader batchHeader = Factory.Load<InvoiceBatchHeader>(currentTransaction.PK);
							DocARBatchInvoice batchHeaderWrapper = DocARBatchInvoice.New(batchHeader, Factory, false);
							foreach (DocARBatchInvoiceLine line in batchHeaderWrapper.Invoices)
							{
								line.EndOfStatementPeriodForCalculatingMatchedAmount = PrintStatement.EndOfPeriod;
							}
							fTransactions.Add(batchHeaderWrapper);
						}
						else
						{
							DocTransactionHeader transactionWrapper = DocTransactionHeader.New(currentTransaction, Factory);
							transactionWrapper.EndOfStatementPeriodForCalculatingMatchedAmount = PrintStatement.EndOfPeriod;
							fTransactions.Add(transactionWrapper);
						}
					}
				}

				return fTransactions;
			}
		}

		protected DocTransactionHeaderCollection fTransactions;

		#endregion

		#region Transaction Collection for Due Buckets

		public virtual DocTransactionHeaderCollection TransactionsForDueBuckets
		{
			get
			{
				return Transactions;
			}
		}

		#endregion

		#region GenericTransaction Collection

		public virtual DocGenericTransactionHeaderCollection GenericTransactions
		{
			get
			{
				if (fGenericTransactions == null)
				{
					TransactionHeaderCollection transactionHeaders = PrintStatement.Transactions;
					DocGenericTransactionHeaderCollection transactionsList = new DocGenericTransactionHeaderCollection(PrintStatement.Factory);
					fGenericTransactions = new DocGenericTransactionHeaderCollection(Factory);
					foreach (TransactionHeader currentTransaction in transactionHeaders)
					{
						if (currentTransaction.AH_TransactionType == TransactionTypes.InvoiceBatch)
						{
							InvoiceBatchHeader batchHeader = Factory.Load<InvoiceBatchHeader>(currentTransaction.PK);
							DocGenericTransactionHeader batchHeaderWrapper = DocGenericTransactionHeader.New(batchHeader, Factory);

							foreach (DocARBatchInvoiceLine line in batchHeaderWrapper.Invoices)
							{
								line.EndOfStatementPeriodForCalculatingMatchedAmount = PrintStatement.EndOfPeriod;
							}
							SetOperationalJob(currentTransaction, batchHeaderWrapper);
							fGenericTransactions.Add(batchHeaderWrapper);
						}
						else
						{
							DocGenericTransactionHeader transactionWrapper = DocGenericTransactionHeader.New(currentTransaction, Factory);
							SetEndOfStatementPeriodForCalculatingMatchedAmount(PrintStatement.EndOfPeriod, transactionWrapper);
							SetOperationalJob(currentTransaction, transactionWrapper);
							fGenericTransactions.Add(transactionWrapper);
						}
					}
				}

				return fGenericTransactions;
			}
		}

		void SetOperationalJob(TransactionHeader currentTransaction, DocGenericTransactionHeader transactionWrapper)
		{
			if (currentTransaction.Job != null)
			{
				if (currentTransaction.Job.Parent == null)
				{
					currentTransaction.Job.InitializeParentFromGenericJobWithoutSettingDefaults();
				}
				transactionWrapper.OperationalJob = FreightWrapper.New(currentTransaction.Job.Parent as BusinessObject, Factory)[0];
			}
		}

		void SetEndOfStatementPeriodForCalculatingMatchedAmount(ZDateTime endOfPeriod, DocGenericTransactionHeader transactionWrapper)
		{
			if (transactionWrapper.HeaderPlugIn is DocTransactionHeader)
			{
				((DocTransactionHeader)transactionWrapper.HeaderPlugIn).EndOfStatementPeriodForCalculatingMatchedAmount = endOfPeriod;
			}
		}

		protected DocGenericTransactionHeaderCollection fGenericTransactions;

		#endregion

		#region Wrappers

		public DocBankAccount ReceiptBankAccount
		{
			get
			{
				ZString currencyNK = PrintStatement.CurrencyNK;
				AccBankAccount bankAccount = AccBankAccount.GetDefaultReceiptBankAccountForDebtor(OrganisationPK, currencyNK, PrintStatement.Branch, Factory);
				return DocBankAccount.New(bankAccount, Factory);
			}
		}

		public ZString AccountName
		{
			get { return Organisation != null ? Organisation.Name : ZString.Empty; }
		}

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(PrintStatement.Factory, PrintStatement.OrganisationPK); }
		}

		public ZGuid OrganisationPK
		{
			get { return PrintStatement.OrganisationPK; }
		}

		public ZString OrganisationARAgreedPaymentMethod
		{
			get
			{
				ZString result = ZString.Empty;
				if (Organisation != null && Organisation.CompanyData != null)
				{
					result = Organisation.CompanyData.OB_ARCreditAgreedPaymentMethod;
				}
				return result;
			}
		}

		public ZString OrganisationAPAgreedPaymentMethod
		{
			get
			{
				ZString result = ZString.Empty;
				if (Organisation != null && Organisation.CompanyData != null)
				{
					result = Organisation.CompanyData.OB_APCreditAgreedPaymentMethod;
				}
				return result;
			}
		}

		public DocCurrency Currency
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, PrintStatement.CurrencyNK);
				return currency != null ? DocCurrency.New(currency, Factory) : CurrentCompany.Currency;
			}
		}

		public DocBranch Branch
		{
			get
			{
				return DocBranch.New(PrintStatement.Branch, PrintStatement.Factory);
			}
		}

		#endregion
	}
}
