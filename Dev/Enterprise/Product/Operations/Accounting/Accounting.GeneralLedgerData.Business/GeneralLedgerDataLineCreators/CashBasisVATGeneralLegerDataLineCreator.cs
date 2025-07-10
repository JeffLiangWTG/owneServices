using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class CashBasisVATGeneralLegerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var result = new List<DebitCreditEntryItem>();
			var taxAmount = (decimal)gLDDataSourceRow[AccCashBasisVATSchema.Constants.YC_TaxAmount];
			var transactionLine = GetTransactionLine((Guid)gLDDataSourceRow[AccCashBasisVATSchema.Constants.YC_AL_TransactionLine]);
			var lineType = transactionLine.AL_LineType;
			var gSTAccountForNotRecognizedLine = GetGSTAccountForNotRecognizedLine(lineType, transactionLine.AL_GSTVATBasis);
			var gSTAccountForRecognizedLine = GetGSTAccountForRecognizedLine(lineType, transactionLine.AL_GSTVATBasis) ?? throw new MissingGLHeaderException(lineType.Equals(TransactionLineTypes.Revenue) ?
				Res.GetString("92779a4c-aff8-49ba-8b19-3cef77e770a8", "Reportable Tax Output Control Account")
				: Res.GetString("1f680ba5-00d0-400e-b559-f68e6ccd9674", "Reportable Tax Input Control Account"));

			if (gSTAccountForNotRecognizedLine == null)
			{
				throw new MissingGLHeaderException(lineType.Equals(TransactionLineTypes.Revenue) ?
														Res.GetString("efc91f19-cdd4-4131-a90b-8e30c930d4ce", "Pending Tax Output Control Account")
														: Res.GetString("7ce76426-bb45-4029-a20a-ca6384e4debb", "Pending Tax Input Control Account"));
			}

			var postDate = (DateTime)gLDDataSourceRow[AccCashBasisVATSchema.Constants.YC_PostDate];
			var isVATRecoverable = lineType.Equals(TransactionLineTypes.Cost) && transactionLine.AL_InputGSTVATRecoverable != 1 && transactionLine.AL_LocalTaxAmount_NotRecoverable != 0;
			var cashVATAmount = isVATRecoverable ? Utilities.Round(taxAmount * transactionLine.AL_InputGSTVATRecoverable, transactionLine.AL_Calc_LocalRXDecimals) : taxAmount;
			var (pendingGSTOutputInputControlAccountType, gSTOutputInputControlAccountType) = GetGLDAccountTypes(lineType);

			var vatRecoverableDr = CreateAndPopulateDebitCreditLine(gSTAccountForNotRecognizedLine.PK, pendingGSTOutputInputControlAccountType, cashVATAmount, ZDecimal.Zero, postDate, AccountingConstants.GLDTypeCodes.RealizeCashBasisVAT);
			var vatRecoverableCr = CreateAndPopulateDebitCreditLine(gSTAccountForRecognizedLine.PK, gSTOutputInputControlAccountType, cashVATAmount * (-1), ZDecimal.Zero, postDate, AccountingConstants.GLDTypeCodes.RealizeCashBasisVAT);

			result.Add(vatRecoverableDr);
			result.Add(vatRecoverableCr);

			if (isVATRecoverable)
			{
				var localVatNotRecoverable = taxAmount - cashVATAmount;
				var pendingGSTRecoverableOutputInputControlAccountType = lineType == TransactionLineTypes.Revenue ? GLDAccountTypes.PendingGSTRecoverableOutputControlAccount : GLDAccountTypes.PendingGSTRecoverableInputControlAccount;
				var vatNotRecoverableDr = CreateAndPopulateDebitCreditLine(gSTAccountForNotRecognizedLine.PK, pendingGSTRecoverableOutputInputControlAccountType, localVatNotRecoverable, ZDecimal.Zero, postDate, AccountingConstants.GLDTypeCodes.RealizeCashBasisVAT);
				var vatNotRecoverableCr = CreateAndPopulateDebitCreditLine(transactionLine.GLHeader.PK, GLDAccountTypes.TransactionLineGLAccount, localVatNotRecoverable * (-1), ZDecimal.Zero, postDate, AccountingConstants.GLDTypeCodes.RealizeCashBasisVAT);

				result.Add(vatNotRecoverableDr);
				result.Add(vatNotRecoverableCr);
			}

			return result.ToArray();
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			var transactionLine = GetTransactionLine((Guid)gLDDataSourceRow[AccCashBasisVATSchema.Constants.YC_AL_TransactionLine]);
			var generalLedgerDataBasic = CreateGeneralLedgerDataBasicBasedOnLine(((INeedRow)transactionLine).Row);
			generalLedgerDataBasic.CashBasisVatPK = (Guid)gLDDataSourceRow[AccCashBasisVATSchema.Constants.PK];

			return generalLedgerDataBasic;
		}

		TransactionLine GetTransactionLine(Guid linePK)
		{
			if (fTransactionLine == null)
			{
				fTransactionLine = ReadOnlyFactory.Load<TransactionLine>(linePK);
			}
			else if (fTransactionLine.PK != linePK)
			{
				fTransactionLine = ReadOnlyFactory.Load<TransactionLine>(linePK);
			}

			return fTransactionLine;
		}
		TransactionLine fTransactionLine;

		AccGLHeader GetGSTAccountForRecognizedLine(ZString transactionLineType, ZString taxBasis)
		{
			AccGLHeader gstAccountForRecognizedLine = null;

			if (transactionLineType.Equals(TransactionLineTypes.Revenue) && taxBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code))
			{
				gstAccountForRecognizedLine = GLControlAccounts.Instance.GSTOutputControlAccount;
			}
			else if (transactionLineType.Equals(TransactionLineTypes.Cost) && taxBasis.Equals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code))
			{
				gstAccountForRecognizedLine = GLControlAccounts.Instance.GSTInputControlAccount;
			}

			return gstAccountForRecognizedLine;
		}

		(string pendingGSTOutputInputControlAccountType, string gSTOutputInputControlAccountType) GetGLDAccountTypes(ZString lineType)
		{
			var pendingGSTOutputInputControlAccountType = lineType == TransactionLineTypes.Revenue ? GLDAccountTypes.PendingGSTOutputControlAccount : GLDAccountTypes.PendingGSTInputControlAccount;
			var gSTOutputInputControlAccountType = lineType == TransactionLineTypes.Revenue ? GLDAccountTypes.GSTOutputControlAccount : GLDAccountTypes.GSTInputControlAccount;
			return (pendingGSTOutputInputControlAccountType, gSTOutputInputControlAccountType);
		}
	}
}
