using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business
{
	public abstract class AccountingJournalLineCreatorForARAP : AccountingJournalLineCreator
	{
		protected AccountingJournalLineCreatorForARAP(string ledgerType, ReadOnlyBusinessObjectFactory factory)
			: base(factory)
		{
			this.ledgerType = ledgerType;
		}
		readonly string ledgerType;

		protected AccGLHeader ControlAccount
		{
			get
			{
				AccGLHeader controlAccount = null;

				if (ledgerType.Equals(LedgerTypes.AccountsReceivable))
				{
					controlAccount = GLControlAccounts.Instance.ARControlAccount;
				}
				else if (ledgerType.Equals(LedgerTypes.AccountsPayable))
				{
					controlAccount = GLControlAccounts.Instance.APControlAccount;
				}

				return controlAccount;
			}
		}
		protected string ARControlAccountRegistryName
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.ARControlAccount.Caption;
			}
		}
		protected string APControlAccountRegistryName
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.APControlAccount.Caption;
			}
		}

		protected ZDecimal CalculateOSAmount(ZDecimal localAmount)
		{
			return Env.CurrentCompany.ExchangeRate.LocalToForeign(localAmount, ExchangeRate, Currency);
		}

		protected abstract ZDecimal ExchangeRate { get; }
		protected abstract ZString Currency { get; }
	}

	public class INVCRDADJAccountingJournalLineCreator : AccountingJournalLineCreatorForARAP
	{
		public INVCRDADJAccountingJournalLineCreator(string transactionType, string ledger, TransactionLine line, ReadOnlyBusinessObjectFactory factory)
			: base(ledger, factory)
		{
			Argument.NotNull(ledger, nameof(ledger));
			Argument.NotNull(transactionType, nameof(transactionType));
			Argument.NotNull(line, nameof(line));
			this.transactionType = transactionType;
			this.ledger = ledger;
			this.transactionLine = line;
		}
		protected readonly string transactionType;
		protected readonly string ledger;
		protected readonly TransactionLine transactionLine;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			var result = new List<AccountingJournalLine>();

			if (transactionLine.ChargeCode != null && transactionLine.ChargeCode.AC_ChargeType == ChargeType.Comment)
			{
				if (transactionLine.AL_LineAmount != 0)
				{
					throw new InvalidAccountingJournalOperationException(AccountingConstants.GetAmountShouldBeZeroForCMTLineErrorMessage(transactionLine.TransactionHeader.AH_TransactionNum, transactionLine.TransactionHeader.AH_TransactionType));
				}
				else
				{
					return result;
				}
			}

			if (transactionLine.GLHeader == null)
			{
				throw new InvalidAccountingJournalOperationException(AccountingConstants.GetGLAccountShouldNotBeEmptyErrorMessage(transactionLine.TransactionHeader.AH_TransactionNum, transactionLine.TransactionHeader.AH_TransactionType));
			}

			//POSTING LINE VALUE
			if (ControlAccount == null)
			{
				throw new MissingGLHeaderException(ledger.Equals(LedgerTypes.AccountsReceivable) ? ARControlAccountRegistryName : APControlAccountRegistryName);
			}

			var controlAccountLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(controlAccountLine, transactionLine, 0, ControlAccount, transactionLine.AL_LineAmount);
			result.Add(new AccountingJournalLine(controlAccountLine));

			if (ControlAccountSuspense == null)
			{
				throw new MissingGLHeaderException(transactionLine.AL_LineType.Equals(TransactionLineTypes.Revenue) ?
																			Res.GetString("c9af96fc-afc7-474e-813b-2e1986d57c92", "Revenue Suspense Control Account")
																				: Res.GetString("21463e89-d72f-4256-a6fc-42524a21ec82", "Cost Suspense Control Account"));
			}

			var suspenseAccountLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(suspenseAccountLine, transactionLine, 1, ControlAccountSuspense, transactionLine.AL_LineAmount * (-1));
			result.Add(new AccountingJournalLine(suspenseAccountLine));

			var isVATRecoverable = transactionLine.AL_LineType.Equals(TransactionLineTypes.Cost) && transactionLine.AL_InputGSTVATRecoverable != 1 && transactionLine.AL_LocalTaxAmount_NotRecoverable != 0;
			var isCashBasisVAT = transactionLine.AL_GSTVATBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code);

			//POSTING GST VALUE
			if (transactionLine.AL_GSTVAT != 0)
			{
				//COMMON
				var gstAmount = isVATRecoverable && !isCashBasisVAT ? new ZDecimal(Utilities.Round(transactionLine.AL_GSTVAT * transactionLine.AL_InputGSTVATRecoverable, transactionLine.Company.LocalCurrency.Decimals)) : transactionLine.AL_GSTVAT;

				var recVATLineDr = Factory.New<AccTransactionLines>();
				PopulateLineProperties(recVATLineDr, transactionLine, 2, ControlAccount, gstAmount);
				result.Add(new AccountingJournalLine(recVATLineDr));

				if (GSTAccountForNotRecognizedLine == null)
				{
					throw new MissingGLHeaderException(transactionLine.AL_LineType.Equals(TransactionLineTypes.Revenue) ?
															(!isCashBasisVAT ? Res.GetString("5700a9cd-ff38-4d09-872e-4554ef1d4354", "Reportable Tax Output Control Account") : Res.GetString("9e0a9134-b3e9-411d-a670-2ba2f75dd1f0", "Pending Tax Output Control Account"))
																: (!isCashBasisVAT ? Res.GetString("47a393f9-c02a-48ad-bdb0-48c649939485", "Reportable Tax Input Control Account") : Res.GetString("9fd936c6-9ca1-4af3-893f-70e12c913eb2", "Pending Tax Input Control Account")));
				}

				var recVATLineCr = Factory.New<AccTransactionLines>();
				PopulateLineProperties(recVATLineCr, transactionLine, 3, GSTAccountForNotRecognizedLine, gstAmount * (-1));
				result.Add(new AccountingJournalLine(recVATLineCr));

				if (isVATRecoverable && !isCashBasisVAT)
				{
					//Non Recoverable VAT
					var nrecVATLineDr = Factory.New<AccTransactionLines>();

					var notRecoverableAmount = transactionLine.AL_GSTVAT - new ZDecimal(Utilities.Round(transactionLine.AL_GSTVAT * transactionLine.AL_InputGSTVATRecoverable, transactionLine.Company.LocalCurrency.Decimals));
					PopulateLineProperties(nrecVATLineDr, transactionLine, 4, ControlAccount, notRecoverableAmount);
					result.Add(new AccountingJournalLine(nrecVATLineDr));

					var nrecVATLineCr = Factory.New<AccTransactionLines>();
					PopulateLineProperties(nrecVATLineCr, transactionLine, 5, transactionLine.GLHeader, notRecoverableAmount * (-1));
					result.Add(new AccountingJournalLine(nrecVATLineCr));
				}
			}

			//CHANGING GL HEADER AFTER RECOGNITION
			if (!transactionLine.AL_ReverseDate.IsEmpty)
			{
				var controlAccountLineRecognizedDr = Factory.New<AccTransactionLines>();
				PopulateLineProperties(controlAccountLineRecognizedDr, transactionLine, 5, ControlAccountSuspense, transactionLine.AL_LineAmount);
				controlAccountLineRecognizedDr.AL_PostDate = transactionLine.AL_ReverseDate;
				result.Add(new AccountingJournalLine(controlAccountLineRecognizedDr));

				var controlAccountLineRecognizedCr = Factory.New<AccTransactionLines>();
				PopulateLineProperties(controlAccountLineRecognizedCr, transactionLine, 7, transactionLine.GLHeader, transactionLine.AL_LineAmount * (-1));
				controlAccountLineRecognizedCr.MultiSubAccountTypeCode = SubAccountHelper.GetMultiSubAccountTypeCode(transactionLine as ISupportMultiSubAccounts, controlAccountLineRecognizedCr.Factory);
				controlAccountLineRecognizedCr.AL_PostDate = transactionLine.AL_ReverseDate;
				result.Add(new AccountingJournalLine(controlAccountLineRecognizedCr));
			}

			// CASH BASIS VAT - Moving GST from Pending to Posting
			if (transactionLine.AL_GSTVAT != 0 && transactionLine.AL_GSTVATBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code))
			{
				if (GSTAccountForRecognizedLine == null)
				{
					throw new MissingGLHeaderException(transactionLine.AL_LineType.Equals(TransactionLineTypes.Revenue) ?
															Res.GetString("5700a9cd-ff38-4d09-872e-4554ef1d4354", "Reportable Tax Output Control Account")
																: Res.GetString("47a393f9-c02a-48ad-bdb0-48c649939485", "Reportable Tax Input Control Account"));
				}

				var query = new ZQuery(AccCashBasisVATSchema.YC_AL_TransactionLine, transactionLine.PK);
				query.OrderBy = AccCashBasisVATSchema.YC_PostDate.Name;
				var cashBasisVATLines = Factory.Load<AccCashBasisVAT>(query);
				CreateJournalLineForCashBasisVATLines(cashBasisVATLines, isVATRecoverable, result);
			}

			return result;
		}

		void CreateJournalLineForCashBasisVATLines(AccCashBasisVAT[] cashBasisVATLines, bool isVATRecoverable, List<AccountingJournalLine> result)
		{
			if (cashBasisVATLines != null)
			{
				ZShort count = 0;
				foreach (AccCashBasisVAT cashVAT in cashBasisVATLines)
				{
					var cashVATAmount = isVATRecoverable ? Utilities.Round(cashVAT.YC_TaxAmount * transactionLine.AL_InputGSTVATRecoverable, transactionLine.AL_Calc_LocalRXDecimals) : (decimal)cashVAT.YC_TaxAmount;

					var vatRecoverableDr = Factory.New<AccTransactionLines>();
					PopulateLineProperties(vatRecoverableDr, transactionLine, 8 + count, GSTAccountForNotRecognizedLine, cashVATAmount);
					vatRecoverableDr.AL_PostDate = cashVAT.YC_PostDate;
					result.Add(new AccountingJournalLine(vatRecoverableDr));

					var vatRecoverableCr = Factory.New<AccTransactionLines>();
					PopulateLineProperties(vatRecoverableCr, transactionLine, 9 + count, GSTAccountForRecognizedLine, cashVATAmount * (-1));
					vatRecoverableCr.AL_PostDate = cashVAT.YC_PostDate;
					result.Add(new AccountingJournalLine(vatRecoverableCr));

					if (isVATRecoverable)
					{
						var vatNotRecoverable = cashVAT.YC_TaxAmount - cashVATAmount;
						var vatNotRecoverableDr = Factory.New<AccTransactionLines>();
						PopulateLineProperties(vatNotRecoverableDr, transactionLine, 10 + count, GSTAccountForNotRecognizedLine, vatNotRecoverable);
						vatNotRecoverableDr.AL_PostDate = cashVAT.YC_PostDate;
						result.Add(new AccountingJournalLine(vatNotRecoverableDr));

						var vatNotRecoverableCr = Factory.New<AccTransactionLines>();
						PopulateLineProperties(vatNotRecoverableCr, transactionLine, 11 + count, transactionLine.GLHeader, vatNotRecoverable * (-1));
						vatNotRecoverableCr.AL_PostDate = cashVAT.YC_PostDate;
						result.Add(new AccountingJournalLine(vatNotRecoverableCr));
					}

					count++;
				}
			}
		}

		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			line.CopyPersistentValuesFrom(sourceObject);
			line.AL_AT = ZGuid.Empty;
			line.AL_GSTVAT = 0M;
		}
		protected override bool CanAccountingJournalLineBeCreated()
		{
			return (ledger == LedgerTypes.AccountsPayable || ledger == LedgerTypes.AccountsReceivable)
					&& (transactionType == TransactionTypes.Invoice || transactionType == TransactionTypes.CreditNote || transactionType == TransactionTypes.AdjustmentNote)
					&& (transactionLine.AL_LineType == TransactionLineTypes.Revenue || transactionLine.AL_LineType == TransactionLineTypes.Cost);
		}
		protected override ZDecimal ExchangeRate { get { return transactionLine.AL_ExchangeRate; } }
		protected override ZString Currency { get { return transactionLine.AL_RX_NKTransactionCurrency; } }

		void PopulateLineProperties(AccTransactionLines line, AccTransactionLines sourceLine, ZShort sequence, AccGLHeader glHeader, ZDecimal amount)
		{
			line.CopyPersistentValuesFrom(sourceLine);
			line.AL_AT = ZGuid.Empty;
			line.AL_GSTVAT = 0M;
			line.AL_AG = glHeader.PK;
			line.AL_Desc = glHeader.AG_DescriptionMultilingual;
			line.AL_LineAmount = amount;
			var multiplier = amount < 0 ? -1 : 1;
			line.AL_OSAmount = sourceLine.AL_GSTVAT.IsEmpty ? (ZDecimal)(sourceLine.AL_OSAmount * multiplier) : CalculateOSAmount(amount);
			line.AL_Sequence = line.AL_Sequence + sequence;
		}
		AccGLHeader ControlAccountSuspense
		{
			get
			{
				AccGLHeader controlAccount = null;

				if (transactionLine.AL_LineType.Equals(TransactionLineTypes.Revenue))
				{
					controlAccount = GLControlAccounts.Instance.ARSuspenseControlAccount;
				}
				else if (transactionLine.AL_LineType.Equals(TransactionLineTypes.Cost))
				{
					controlAccount = GLControlAccounts.Instance.APSuspenseControlAccount;
				}

				return controlAccount;
			}
		}
		AccGLHeader GSTAccountForNotRecognizedLine
		{
			get
			{
				AccGLHeader gstAccountForNotRecognizedLine = null;

				if (transactionLine.AL_LineType.Equals(TransactionLineTypes.Revenue) && transactionLine.AL_GSTVATBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code))
				{
					gstAccountForNotRecognizedLine = GLControlAccounts.Instance.GSTOutputControlAccount;
				}
				else if (transactionLine.AL_LineType.Equals(TransactionLineTypes.Revenue) && transactionLine.AL_GSTVATBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code))
				{
					gstAccountForNotRecognizedLine = GLControlAccounts.Instance.PendingGSTOutputControlAccount;
				}
				else if (transactionLine.AL_LineType.Equals(TransactionLineTypes.Cost) && transactionLine.AL_GSTVATBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code))
				{
					gstAccountForNotRecognizedLine = GLControlAccounts.Instance.GSTInputControlAccount;
				}
				else if (transactionLine.AL_LineType.Equals(TransactionLineTypes.Cost) && transactionLine.AL_GSTVATBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code))
				{
					gstAccountForNotRecognizedLine = GLControlAccounts.Instance.PendingGSTInputControlAccount;
				}

				return gstAccountForNotRecognizedLine;
			}
		}
		AccGLHeader GSTAccountForRecognizedLine
		{
			get
			{
				AccGLHeader gstAccountForRecognizedLine = null;

				if (transactionLine.AL_LineType.Equals(TransactionLineTypes.Revenue) && transactionLine.AL_GSTVATBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code))
				{
					gstAccountForRecognizedLine = GLControlAccounts.Instance.GSTOutputControlAccount;
				}
				else if (transactionLine.AL_LineType.Equals(TransactionLineTypes.Cost) && transactionLine.AL_GSTVATBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code))
				{
					gstAccountForRecognizedLine = GLControlAccounts.Instance.GSTInputControlAccount;
				}

				return gstAccountForRecognizedLine;
			}
		}
	}

	public class PAYRECAccountingJournalLineCreator : AccountingJournalLineCreatorForARAP
	{
		public PAYRECAccountingJournalLineCreator(TransactionHeader header, ReadOnlyBusinessObjectFactory factory)
			: base(header.AH_Ledger, factory)
		{
			Argument.NotNull(header, "TransactionHeader");
			this.transactionHeader = header;
		}
		protected readonly TransactionHeader transactionHeader;

		protected override bool CanAccountingJournalLineBeCreated()
		{
			return (transactionHeader.AH_Ledger == LedgerTypes.AccountsPayable || transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
					&& (transactionHeader.AH_TransactionType == TransactionTypes.Payment || transactionHeader.AH_TransactionType == TransactionTypes.Receipt);
		}
		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			var result = new List<AccountingJournalLine>();

			var drLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(drLine, transactionHeader);
			drLine.AL_AG = transactionHeader.BankAccount.GLHeader.PK;
			drLine.AL_Desc = transactionHeader.BankAccount.GLHeader.AG_DescriptionMultilingual;
			drLine.AL_LineAmount = transactionHeader.AH_InvoiceAmount * (-1);
			drLine.AL_OSAmount = transactionHeader.AH_OSTotal * (-1);
			result.Add(new AccountingJournalLine(drLine));

			if (ControlAccount == null)
			{
				throw new MissingGLHeaderException(transactionHeader.AH_Ledger.Equals(LedgerTypes.AccountsReceivable) ? ARControlAccountRegistryName : APControlAccountRegistryName);
			}

			var crLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(crLine, transactionHeader);
			crLine.AL_AG = ControlAccount.PK;
			crLine.AL_Desc = ControlAccount.AG_DescriptionMultilingual;
			crLine.AL_LineAmount = transactionHeader.AH_InvoiceAmount;
			crLine.AL_OSAmount = transactionHeader.AH_OSTotal;
			result.Add(new AccountingJournalLine(crLine));

			return result;
		}
		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			var transaction = sourceObject as TransactionHeader;
			if (transaction != null)
			{
				line.AL_LineType = transaction.AH_TransactionType;
				line.AL_AT = ZGuid.Empty;
				line.AL_A9_VATClass = ZGuid.Empty;
				line.AL_GSTVAT = 0M;
				line.AL_ExchangeRate = transaction.GetHighPrecisionExchangeRate();
				line.AL_RX_NKTransactionCurrency = transaction.AH_RX_NKTransactionCurrency;
				line.AL_PostDate = transaction.AH_PostDate;
				line.AL_PostPeriod = transaction.AH_PostPeriod;
				line.AL_PostToGL = transaction.AH_PostToGL;
				line.AL_JH = transaction.AH_JH;
				line.AL_AC = ZGuid.Empty;
				line.AL_GC = transaction.AH_GC;
				line.AL_GE = transaction.AH_GE;
				line.AL_GB = transaction.AH_GB;
				line.AL_OH = transaction.AH_OH;
			}
		}
		protected override ZDecimal ExchangeRate { get { return transactionHeader.AH_ExchangeRate; } }
		protected override ZString Currency { get { return transactionHeader.AH_RX_NKTransactionCurrency; } }
	}

	public class EXXOVPDSCAccountingJournalLineCreator : AccountingJournalLineCreatorForARAP
	{
		public EXXOVPDSCAccountingJournalLineCreator(TransactionHeader header, ReadOnlyBusinessObjectFactory factory)
			: base(header.AH_Ledger, factory)
		{
			Argument.NotNull(header, "TransactionHeader");
			this.transactionHeader = header;
		}
		readonly TransactionHeader transactionHeader;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			var result = new List<AccountingJournalLine>();

			if (ControlAccount == null)
			{
				throw new MissingGLHeaderException(transactionHeader.AH_Ledger.Equals(LedgerTypes.AccountsReceivable) ? ARControlAccountRegistryName : APControlAccountRegistryName);
			}

			var drLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(drLine, transactionHeader);
			drLine.AL_AG = ControlAccount.PK;
			drLine.AL_Desc = ControlAccount.AG_DescriptionMultilingual;
			drLine.AL_LineAmount = transactionHeader.AH_InvoiceAmount;
			drLine.AL_OSAmount = transactionHeader.AH_OSTotal;
			result.Add(new AccountingJournalLine(drLine));

			var crLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(crLine, transactionHeader);
			crLine.AL_AG = transactionHeader.GLHeader.PK;
			crLine.AL_Desc = transactionHeader.GLHeader.AG_DescriptionMultilingual;
			crLine.AL_LineAmount = transactionHeader.AH_InvoiceAmount * (-1);
			crLine.AL_OSAmount = transactionHeader.AH_OSTotal * (-1);
			result.Add(new AccountingJournalLine(crLine));

			return result;
		}
		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			var transaction = sourceObject as TransactionHeader;
			if (transaction != null)
			{
				line.AL_LineType = transaction.AH_TransactionType;
				line.AL_AT = ZGuid.Empty;
				line.AL_A9_VATClass = ZGuid.Empty;
				line.AL_GSTVAT = 0M;
				line.AL_ExchangeRate = transaction.GetHighPrecisionExchangeRate();
				line.AL_RX_NKTransactionCurrency = transaction.AH_RX_NKTransactionCurrency;
				line.AL_PostDate = transaction.AH_PostDate;
				line.AL_PostPeriod = transaction.AH_PostPeriod;
				line.AL_PostToGL = transaction.AH_PostToGL;
				line.AL_JH = transaction.AH_JH;
				line.AL_AC = ZGuid.Empty;
				line.AL_GC = transaction.AH_GC;
				line.AL_GE = transaction.AH_GE;
				line.AL_GB = transaction.AH_GB;
				line.AL_OH = transaction.AH_OH;
			}
		}
		protected override bool CanAccountingJournalLineBeCreated()
		{
			return (transactionHeader.AH_Ledger == LedgerTypes.AccountsPayable || transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
					&& (transactionHeader.AH_TransactionType == TransactionTypes.ExchangeDifference || transactionHeader.AH_TransactionType == TransactionTypes.Overpayment || transactionHeader.AH_TransactionType == TransactionTypes.Discount);
		}
		protected override ZDecimal ExchangeRate { get { return transactionHeader.AH_ExchangeRate; } }
		protected override ZString Currency { get { return transactionHeader.AH_RX_NKTransactionCurrency; } }
	}

	public class CTRAccountingJournalLineCreator : AccountingJournalLineCreatorForARAP
	{
		public CTRAccountingJournalLineCreator(TransactionHeader header, ReadOnlyBusinessObjectFactory factory)
			: base(header.AH_Ledger, factory)
		{
			this.transactionHeader = header;
		}
		protected readonly TransactionHeader transactionHeader;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			if (GLControlAccounts.Instance.APControlAccount == null)
			{
				throw new MissingGLHeaderException(ARControlAccountRegistryName);
			}

			if (GLControlAccounts.Instance.ARControlAccount == null)
			{
				throw new MissingGLHeaderException(APControlAccountRegistryName);
			}

			var result = new List<AccountingJournalLine>();

			var drLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(drLine, transactionHeader);
			drLine.AL_AG = GLControlAccounts.Instance.APControlAccount.PK;
			drLine.AL_Desc = GLControlAccounts.Instance.APControlAccount.AG_DescriptionMultilingual;

			var crLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(crLine, transactionHeader);
			crLine.AL_AG = GLControlAccounts.Instance.ARControlAccount.PK;
			crLine.AL_Desc = GLControlAccounts.Instance.ARControlAccount.AG_DescriptionMultilingual;

			if (transactionHeader.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				drLine.AL_LineAmount = transactionHeader.AH_InvoiceAmount;
				drLine.AL_OSAmount = transactionHeader.AH_OSTotal;

				crLine.AL_LineAmount = transactionHeader.AH_InvoiceAmount * (-1);
				crLine.AL_OSAmount = transactionHeader.AH_OSTotal * (-1);
			}
			else if (transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				drLine.AL_LineAmount = transactionHeader.AH_InvoiceAmount * (-1);
				drLine.AL_OSAmount = transactionHeader.AH_OSTotal * (-1);

				crLine.AL_LineAmount = transactionHeader.AH_InvoiceAmount;
				crLine.AL_OSAmount = transactionHeader.AH_OSTotal;
			}
			result.Add(new AccountingJournalLine(drLine));
			result.Add(new AccountingJournalLine(crLine));
			return result;
		}
		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			var transaction = sourceObject as TransactionHeader;
			if (transaction != null)
			{
				line.AL_LineType = transaction.AH_TransactionType;
				line.AL_AT = ZGuid.Empty;
				line.AL_A9_VATClass = ZGuid.Empty;
				line.AL_GSTVAT = 0M;
				line.AL_ExchangeRate = transaction.GetHighPrecisionExchangeRate();
				line.AL_RX_NKTransactionCurrency = transaction.AH_RX_NKTransactionCurrency;
				line.AL_PostDate = transaction.AH_PostDate;
				line.AL_PostPeriod = transaction.AH_PostPeriod;
				line.AL_PostToGL = transaction.AH_PostToGL;
				line.AL_JH = transaction.AH_JH;
				line.AL_AC = ZGuid.Empty;
				line.AL_GC = transaction.AH_GC;
				line.AL_GE = transaction.AH_GE;
				line.AL_GB = transaction.AH_GB;
				line.AL_OH = transaction.AH_OH;
			}
		}
		protected override bool CanAccountingJournalLineBeCreated()
		{
			return (transactionHeader.AH_Ledger == LedgerTypes.AccountsPayable || transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
					&& transactionHeader.AH_TransactionType == TransactionTypes.Contra;
		}
		protected override ZDecimal ExchangeRate { get { return transactionHeader.AH_ExchangeRate; } }
		protected override ZString Currency { get { return transactionHeader.AH_RX_NKTransactionCurrency; } }
	}

	public class JNLAccountingJournalLineCreator : AccountingJournalLineCreatorForARAP
	{
		public JNLAccountingJournalLineCreator(TransactionHeader header, ReadOnlyBusinessObjectFactory factory)
			: base(header.AH_Ledger, factory)
		{
			Argument.NotNull(header, "TransactionHeader");
			this.transactionHeader = header;
		}
		readonly TransactionHeader transactionHeader;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			var result = new List<AccountingJournalLine>();

			if (ControlAccount == null)
			{
				throw new MissingGLHeaderException(transactionHeader.AH_Ledger.Equals(LedgerTypes.AccountsReceivable) ? ARControlAccountRegistryName : APControlAccountRegistryName);
			}

			var drLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(drLine, transactionHeader);
			drLine.AL_AG = ControlAccount.PK;
			drLine.AL_Desc = ControlAccount.AG_DescriptionMultilingual;
			drLine.AL_LineAmount = transactionHeader.AH_InvoiceAmount;
			drLine.AL_OSAmount = transactionHeader.AH_OSTotal;
			result.Add(new AccountingJournalLine(drLine));

			var crLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(crLine, transactionHeader);
			crLine.AL_AG = transactionHeader.GLHeader.PK;
			crLine.MultiSubAccountTypeCode = SubAccountHelper.GetMultiSubAccountTypeCode(transactionHeader as ISupportMultiSubAccounts, crLine.Factory);
			crLine.AL_Desc = transactionHeader.GLHeader.AG_DescriptionMultilingual;
			crLine.AL_LineAmount = transactionHeader.AH_InvoiceAmount * (-1);
			crLine.AL_OSAmount = transactionHeader.AH_OSTotal * (-1);
			result.Add(new AccountingJournalLine(crLine));

			return result;
		}
		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			var transaction = sourceObject as TransactionHeader;
			if (transaction != null)
			{
				line.AL_LineType = transaction.AH_TransactionType;
				line.AL_AT = ZGuid.Empty;
				line.AL_A9_VATClass = ZGuid.Empty;
				line.AL_GSTVAT = 0M;
				line.AL_ExchangeRate = transaction.GetHighPrecisionExchangeRate();
				line.AL_RX_NKTransactionCurrency = transaction.AH_RX_NKTransactionCurrency;
				line.AL_PostDate = transaction.AH_PostDate;
				line.AL_PostPeriod = transaction.AH_PostPeriod;
				line.AL_PostToGL = transaction.AH_PostToGL;
				line.AL_JH = transaction.AH_JH;
				line.AL_AC = ZGuid.Empty;
				line.AL_GC = transaction.AH_GC;
				line.AL_GE = transaction.AH_GE;
				line.AL_GB = transaction.AH_GB;
				line.AL_OH = transaction.AH_OH;
			}
		}
		protected override bool CanAccountingJournalLineBeCreated()
		{
			return (transactionHeader.AH_Ledger == LedgerTypes.AccountsPayable || transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
					&& (transactionHeader.AH_TransactionType == TransactionTypes.Journal);
		}
		protected override ZDecimal ExchangeRate { get { return transactionHeader.AH_ExchangeRate; } }
		protected override ZString Currency { get { return transactionHeader.AH_RX_NKTransactionCurrency; } }
	}

	public class TRFAccountingJournalLineCreator : AccountingJournalLineCreatorForARAP
	{
		public TRFAccountingJournalLineCreator(TransactionHeader transaction, ReadOnlyBusinessObjectFactory factory)
			: base(transaction.AH_Ledger, factory)
		{
			Argument.NotNull(transaction, "transaction");
			this.transaction = transaction;
		}
		protected readonly TransactionHeader transaction;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			bool isFromTransaction = (transaction.AH_TransactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRow)
										|| (transaction.AH_TransactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRowWhenReversing);

			var result = new List<AccountingJournalLine>();
			var oppositeTransactionQuery = new ZQuery(AccTransactionHeaderSchema.AH_GC, transaction.AH_GC);
			oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, transaction.AH_Ledger);
			oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transaction.AH_TransactionType);
			oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.AH_TransactionNum);
			oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transaction.PK);
			var oppositeTransaction = Factory.LoadTop1<TransactionHeader>(oppositeTransactionQuery);

			if (oppositeTransaction != null)
			{
				var fromTranaction = isFromTransaction ? transaction : oppositeTransaction;
				var fromLine = Factory.New<AccTransactionLines>();
				PopulateLineProperties(fromLine, fromTranaction);
				fromLine.AL_LineAmount = fromTranaction.AH_InvoiceAmount;
				fromLine.AL_OSAmount = fromTranaction.AH_OSTotal;
				result.Add(new AccountingJournalLine(fromLine));

				var toTranaction = !isFromTransaction ? transaction : oppositeTransaction;
				var toLine = Factory.New<AccTransactionLines>();
				PopulateLineProperties(toLine, toTranaction);
				toLine.AL_LineAmount = toTranaction.AH_InvoiceAmount;
				toLine.AL_OSAmount = toTranaction.AH_OSTotal;
				result.Add(new AccountingJournalLine(toLine));
			}
			return result;
		}
		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			var srctransaction = sourceObject as TransactionHeader;
			if (srctransaction != null)
			{
				line.AL_LineType = srctransaction.AH_TransactionType;
				line.AL_AG = ControlAccount.PK;
				line.AL_Desc = srctransaction.AH_Desc;
				line.AL_AT = ZGuid.Empty;
				line.AL_A9_VATClass = ZGuid.Empty;
				line.AL_GSTVAT = 0M;
				line.AL_ExchangeRate = srctransaction.GetHighPrecisionExchangeRate();
				line.AL_RX_NKTransactionCurrency = srctransaction.AH_RX_NKTransactionCurrency;
				line.AL_PostDate = srctransaction.AH_PostDate;
				line.AL_PostPeriod = srctransaction.AH_PostPeriod;
				line.AL_PostToGL = srctransaction.AH_PostToGL;
				line.AL_JH = srctransaction.AH_JH;
				line.AL_AC = ZGuid.Empty;
				line.AL_GC = srctransaction.AH_GC;
				line.AL_GE = srctransaction.AH_GE;
				line.AL_GB = srctransaction.AH_GB;
				line.AL_OH = srctransaction.AH_OH;
			}
		}
		protected override bool CanAccountingJournalLineBeCreated()
		{
			return transaction.AH_TransactionType == TransactionTypes.Transfer && (transaction.AH_Ledger == LedgerTypes.AccountsPayable || transaction.AH_Ledger == LedgerTypes.AccountsReceivable);
		}
		protected override ZDecimal ExchangeRate { get { return transaction.AH_ExchangeRate; } }
		protected override ZString Currency { get { return transaction.AH_RX_NKTransactionCurrency; } }
	}
}
