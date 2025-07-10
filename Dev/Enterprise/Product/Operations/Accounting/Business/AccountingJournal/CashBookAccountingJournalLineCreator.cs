using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class DRCDPYAccountingJournalLineCreator : AccountingJournalLineCreator
	{
		public DRCDPYAccountingJournalLineCreator(AccBankAccount bankAccount, TransactionLine line, ReadOnlyBusinessObjectFactory factory, ZGuid headerBranchPK, ZGuid headerDepartmentPK)
			: base(factory)
		{
			Argument.NotNull(bankAccount, "bankAccount");
			Argument.NotNull(line, "transactionLine");
			this.bankAccount = bankAccount;
			this.transactionLine = line;
			this.headerBranchPK = headerBranchPK;
			this.headerDepartmentPK = headerDepartmentPK;
		}
		protected readonly AccBankAccount bankAccount;
		protected readonly TransactionLine transactionLine;
		protected readonly ZGuid headerBranchPK;
		protected readonly ZGuid headerDepartmentPK;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			var result = new List<AccountingJournalLine>();

			var lineOSAmount = transactionLine.AL_GSTVAT.IsEmpty ? transactionLine.AL_OSAmount : CalculateOSAmount(transactionLine.AL_LineAmount);
			var glLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(glLine, transactionLine);
			glLine.AL_Desc = transactionLine.GLHeader.AG_DescriptionMultilingual;
			glLine.AL_LineAmount = transactionLine.AL_LineAmount * (-1);
			glLine.AL_OSAmount = lineOSAmount * (-1);
			glLine.MultiSubAccountTypeCode = SubAccountHelper.GetMultiSubAccountTypeCode(transactionLine as ISupportMultiSubAccounts, glLine.Factory);
			result.Add(new AccountingJournalLine(glLine));

			var bankAccountLine = Factory.New<AccTransactionLines>();
			PopulateLineProperties(bankAccountLine, transactionLine);
			bankAccountLine.AL_LineAmount = transactionLine.AL_LineAmount;
			bankAccountLine.AL_OSAmount = lineOSAmount;
			bankAccountLine.AL_AG = bankAccount.AB_AG;
			bankAccountLine.AL_Desc = bankAccount.GLHeader.AG_DescriptionMultilingual;
			bankAccountLine.AL_GB = headerBranchPK;
			bankAccountLine.AL_GE = headerDepartmentPK;
			result.Add(new AccountingJournalLine(bankAccountLine));

			if (transactionLine.AL_GSTVAT != 0)
			{
				var isVATRecoverable = transactionLine.AL_LineType.Equals(TransactionTypes.DirectPayment) && transactionLine.AL_InputGSTVATRecoverable != 1 && transactionLine.AL_LocalTaxAmount_NotRecoverable != 0;

				var vatGLAccount = (transactionLine.AL_LineType == TransactionTypes.DirectPayment) ? GLControlAccounts.Instance.GSTInputControlAccount : GLControlAccounts.Instance.GSTOutputControlAccount;
				if (vatGLAccount == null)
				{
					var registryLocation = transactionLine.AL_LineType == TransactionTypes.DirectPayment ? AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Caption : AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Caption;
					throw new MissingGLHeaderException(registryLocation);
				}

				var recoverableAmount = transactionLine.AL_GSTVAT;
				if (isVATRecoverable)
				{
					recoverableAmount = new ZDecimal(Utilities.Round(transactionLine.AL_GSTVAT * transactionLine.AL_InputGSTVATRecoverable, transactionLine.Company.LocalCurrency.Decimals));
					var notRecoverableAmount = transactionLine.AL_GSTVAT - recoverableAmount;

					var notRecVatLine = Factory.New<AccTransactionLines>();
					PopulateLineProperties(notRecVatLine, transactionLine);
					notRecVatLine.AL_LineAmount = notRecoverableAmount * (-1);
					notRecVatLine.AL_OSAmount = CalculateOSAmount(notRecoverableAmount) * (-1);
					notRecVatLine.AL_AG = transactionLine.GLHeader.PK;
					notRecVatLine.AL_Desc = transactionLine.GLHeader.AG_DescriptionMultilingual;
					result.Add(new AccountingJournalLine(notRecVatLine));
				}

				var vatLine = Factory.New<AccTransactionLines>();
				PopulateLineProperties(vatLine, transactionLine);
				vatLine.AL_LineAmount = recoverableAmount * (-1);
				vatLine.AL_OSAmount = CalculateOSAmount(recoverableAmount) * (-1);
				vatLine.AL_AG = vatGLAccount.PK;
				vatLine.AL_Desc = vatGLAccount.AG_DescriptionMultilingual;
				result.Add(new AccountingJournalLine(vatLine));

				var bankVATLine = Factory.New<AccTransactionLines>();
				bankVATLine.CopyPersistentValuesFrom(transactionLine);
				bankVATLine.AL_LineAmount = transactionLine.AL_GSTVAT;
				bankVATLine.AL_OSAmount = CalculateOSAmount(transactionLine.AL_GSTVAT);
				bankVATLine.AL_AG = bankAccount.AB_AG;
				bankVATLine.AL_Desc = bankVATLine.GLHeader.AG_DescriptionMultilingual;
				bankVATLine.AL_GB = headerBranchPK;
				bankVATLine.AL_GE = headerDepartmentPK;
				result.Add(new AccountingJournalLine(bankVATLine));
			}

			return result;
		}
		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			line.CopyPersistentValuesFrom(sourceObject);
			line.AL_AT = ZGuid.Empty;
			line.AL_GSTVAT = 0M;
		}

		protected ZDecimal CalculateOSAmount(ZDecimal localAmount)
		{
			return Env.CurrentCompany.ExchangeRate.LocalToForeign(localAmount, transactionLine.AL_ExchangeRate, transactionLine.AL_RX_NKTransactionCurrency);
		}
		protected override bool CanAccountingJournalLineBeCreated()
		{
			return transactionLine.AL_LineType == TransactionTypes.DirectPayment || transactionLine.AL_LineType == TransactionTypes.DirectReceipt;
		}
	}

	public class CBTRFAccountingJournalLineCreator : AccountingJournalLineCreator
	{
		public CBTRFAccountingJournalLineCreator(TransactionHeader transaction, ReadOnlyBusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(transaction, "transaction");
			this.transaction = transaction;
		}
		protected readonly TransactionHeader transaction;

		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			bool isFromTransaction = (transaction.AH_TransactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRow) || (transaction.AH_TransactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRowWhenReversing);

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
			var srcTransaction = sourceObject as TransactionHeader;
			if (srcTransaction != null)
			{
				line.AL_LineType = srcTransaction.AH_TransactionType;
				line.AL_AG = srcTransaction.BankAccount.AB_AG;
				line.AL_Desc = srcTransaction.BankAccount.GLHeader.AG_DescriptionMultilingual;
				line.AL_AT = ZGuid.Empty;
				line.AL_A9_VATClass = ZGuid.Empty;
				line.AL_GSTVAT = 0M;
				line.AL_ExchangeRate = srcTransaction.GetHighPrecisionExchangeRate();
				line.AL_RX_NKTransactionCurrency = srcTransaction.AH_RX_NKTransactionCurrency;
				line.AL_PostDate = srcTransaction.AH_PostDate;
				line.AL_PostPeriod = srcTransaction.AH_PostPeriod;
				line.AL_PostToGL = srcTransaction.AH_PostToGL;
				line.AL_JH = srcTransaction.AH_JH;
				line.AL_AC = ZGuid.Empty;
				line.AL_GC = srcTransaction.AH_GC;
				line.AL_GE = srcTransaction.AH_GE;
				line.AL_GB = srcTransaction.AH_GB;
				line.AL_OH = srcTransaction.AH_OH;
			}
		}

		protected override bool CanAccountingJournalLineBeCreated()
		{
			return transaction.AH_TransactionType == TransactionTypes.Transfer && transaction.AH_Ledger == LedgerTypes.CashBook;
		}
	}

	public class CBEXXAccountingJournalLineCreator : AccountingJournalLineCreator
	{
		public CBEXXAccountingJournalLineCreator(TransactionHeader transaction, ReadOnlyBusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(transaction, "transaction");
			this.transaction = transaction;
		}

		protected readonly TransactionHeader transaction;
		protected override IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore()
		{
			var result = new List<AccountingJournalLine>();

			var line = Factory.New<AccTransactionLines>();
			PopulateLineProperties(line, transaction);
			line.AL_LineAmount = transaction.AH_InvoiceAmount * (-1);
			line.AL_OSAmount = transaction.AH_OSExTaxAmount != 0 ? transaction.AH_OSTotal * (-1) : 0M;
			result.Add(new AccountingJournalLine(line));

			var bankAccline = Factory.New<AccTransactionLines>();
			PopulateLineProperties(bankAccline, transaction);
			bankAccline.AL_AG = transaction.BankAccount.AB_AG;
			bankAccline.AL_Desc = transaction.BankAccount.GLHeader.AG_DescriptionMultilingual;
			bankAccline.AL_LineAmount = transaction.AH_InvoiceAmount;
			bankAccline.AL_OSAmount = transaction.AH_OSExTaxAmount != 0 ? transaction.AH_OSTotal : ZDecimal.Zero;
			result.Add(new AccountingJournalLine(bankAccline));

			return result;
		}
		protected override void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject)
		{
			var srcTransaction = sourceObject as TransactionHeader;
			if (srcTransaction != null)
			{
				line.AL_LineType = srcTransaction.AH_TransactionType;
				line.AL_AG = srcTransaction.AH_AG;
				line.AL_Desc = srcTransaction.GLHeader.AG_DescriptionMultilingual;
				line.AL_AT = ZGuid.Empty;
				line.AL_A9_VATClass = ZGuid.Empty;
				line.AL_GSTVAT = 0M;
				line.AL_ExchangeRate = srcTransaction.GetHighPrecisionExchangeRate();
				line.AL_RX_NKTransactionCurrency = srcTransaction.AH_RX_NKTransactionCurrency;
				line.AL_PostDate = srcTransaction.AH_PostDate;
				line.AL_PostPeriod = srcTransaction.AH_PostPeriod;
				line.AL_PostToGL = srcTransaction.AH_PostToGL;
				line.AL_JH = srcTransaction.AH_JH;
				line.AL_AC = ZGuid.Empty;
				line.AL_GC = srcTransaction.AH_GC;
				line.AL_GE = srcTransaction.AH_GE;
				line.AL_GB = srcTransaction.AH_GB;
				line.AL_OH = srcTransaction.AH_OH;
			}
		}

		protected override bool CanAccountingJournalLineBeCreated()
		{
			return transaction.AH_TransactionType == TransactionTypes.ExchangeDifference && transaction.AH_Ledger == LedgerTypes.CashBook;
		}
	}
}
