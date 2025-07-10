using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class CBDRCDPYGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var header = ReadOnlyFactory.Load<TransactionHeader>((Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_AH]);
			var glHeaderofBankAccount = header?.BankAccount?.AB_AG ?? Guid.Empty;
			var glHeaderOfLine = (Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_AG];
			var headerBranchPK = header?.AH_GB.ToGuid() ?? Guid.Empty;
			var headerDepartmentPK = header?.AH_GE.ToGuid() ?? Guid.Empty;

			var lineList = new List<DebitCreditEntryItem>();
			var taxAmount = new ZDecimal(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_GSTVAT]);
			var osAmount = new ZDecimal(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_OSAmount]);
			var lineAmount = new ZDecimal(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_LineAmount]);
			var lineOSAmount = taxAmount.IsEmpty ? osAmount : CalculateOSAmount(lineAmount);
			var postDate = (DateTime)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_PostDate];
			var lineType = (string)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_LineType];
			var inputGSTVATRecoverable = new ZDecimal(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_InputGSTVATRecoverable]);
			var localTaxAmount_NotRecoverable = taxAmount - Utilities.Round(inputGSTVATRecoverable * taxAmount, LocalCurrencyDecimals);

			lineList.Add(CreateAndPopulateDebitCreditLine(glHeaderOfLine, GLDAccountTypes.TransactionLineGLAccount, lineAmount * (-1), lineOSAmount * (-1), postDate));
			var bankAccountLine = CreateAndPopulateDebitCreditLine(glHeaderofBankAccount, GLDAccountTypes.TransactionBankGLAccount, lineAmount, lineOSAmount, postDate);
			SetBranchAndDepartmentFromHeader(bankAccountLine, headerBranchPK, headerDepartmentPK);
			lineList.Add(bankAccountLine);

			if (!taxAmount.IsEmpty)
			{
				var isVATRecoverable = lineType.Equals(TransactionTypes.DirectPayment) && inputGSTVATRecoverable != 1 && localTaxAmount_NotRecoverable != 0;
				var vatGLAccount = (lineType == TransactionTypes.DirectPayment) ? GLControlAccounts.Instance.GSTInputControlAccount : GLControlAccounts.Instance.GSTOutputControlAccount;
				var vatGLAccountType = (lineType == TransactionTypes.DirectPayment) ? GLDAccountTypes.GSTInputControlAccount : GLDAccountTypes.GSTOutputControlAccount;
				if (vatGLAccount == null)
				{
					var registryLocation = lineType == TransactionTypes.DirectPayment ? AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Caption : AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Caption;
					throw new MissingGLHeaderException(registryLocation);
				}

				var recoverableAmount = taxAmount;

				if (isVATRecoverable)
				{
					recoverableAmount = new ZDecimal(Utilities.Round(taxAmount * inputGSTVATRecoverable, LocalCurrencyDecimals));
					var notRecoverableAmount = taxAmount - recoverableAmount;
					lineList.Add(CreateAndPopulateDebitCreditLine(glHeaderOfLine, GLDAccountTypes.GSTRecoverableTransactionLineGLAccount, notRecoverableAmount * (-1), CalculateOSAmount(notRecoverableAmount) * (-1), postDate));
				}

				lineList.Add(CreateAndPopulateDebitCreditLine(vatGLAccount.PK, vatGLAccountType, recoverableAmount * (-1), CalculateOSAmount(recoverableAmount) * (-1), postDate));
				var bankAccountLineForTax = CreateAndPopulateDebitCreditLine(glHeaderofBankAccount, GLDAccountTypes.TransactionBankGLAccountForGST, taxAmount, CalculateOSAmount(taxAmount), postDate);
				SetBranchAndDepartmentFromHeader(bankAccountLineForTax, headerBranchPK, headerDepartmentPK);
				lineList.Add(bankAccountLineForTax);
			}

			return lineList.ToArray();
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			return CreateGeneralLedgerDataBasicBasedOnLine(gLDDataSourceRow);
		}

		void SetBranchAndDepartmentFromHeader(DebitCreditEntryItem line, Guid headerBranchPK, Guid headerDepartmentPK)
		{
			line.BranchPK = headerBranchPK;
			line.DepartmentPK = headerDepartmentPK;
		}
	}
}
