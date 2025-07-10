using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class INVCRDADJGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var result = new List<DebitCreditEntryItem>();

			var transactionHeaderInfo = GeneralLedgerDataRetriever.GetTransactionHeaderInfo(ReadOnlyFactory, (Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_AH]);
			var localAmount = (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_LineAmount];
			var chargeCode = ReadOnlyFactory.Load<AccChargeCode>(GetGuidValueFromColumnValue(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_AC]));

			if (chargeCode != null && chargeCode.AC_ChargeType == ChargeType.Comment)
			{
				if (localAmount != 0)
				{
					throw new InvalidAccountingJournalOperationException(AccountingConstants.GetAmountShouldBeZeroForCMTLineErrorMessage(transactionHeaderInfo.TransactionNum, transactionHeaderInfo.TransactionType));
				}
				else
				{
					return result.ToArray();
				}
			}

			var lineGLAccount = GetGuidValueFromColumnValue(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_AG]);
			if (lineGLAccount == Guid.Empty)
			{
				throw new InvalidAccountingJournalOperationException(AccountingConstants.GetGLAccountShouldNotBeEmptyErrorMessage(transactionHeaderInfo.TransactionNum, transactionHeaderInfo.TransactionType));
			}

			var ledger = transactionHeaderInfo?.Ledger ?? ZString.Empty;
			var transactionLineType = (string)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_LineType];
			var taxAmount = (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_GSTVAT];
			var isTaxAmountEmpty = taxAmount == decimal.Zero;
			var oSAmount = isTaxAmountEmpty ? (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_OSAmount] : decimal.Zero;
			var inputGSTVATRecoverable = (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_InputGSTVATRecoverable];
			var taxBasis = (string)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_GSTVATBasis];
			var lineType = (string)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_LineType];
			var recognizedDate = new ZDateTime(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_ReverseDate]);
			var postDate = new ZDateTime(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_PostDate]);
			var controlAccount = GetControlAccount(ledger);
			var controlAccountSuspense = GetControlAccountSuspense(lineType)
				?? throw new MissingGLHeaderException(lineType.Equals(TransactionLineTypes.Revenue)
					? Res.GetString("aa55143d-34cd-4b60-b098-d39dedcce90a", "Revenue Suspense Control Account")
					: Res.GetString("58c8b2a0-1dc0-464a-a7a3-1b43666746b9", "Cost Suspense Control Account"));

			var (aRAPControlAccountType, suspenseControlAccountType) = GetGLDAccountTypes(ledger, transactionLineType);

			if (gLDDataSourceRow.RowState == DataRowState.Added || gLDDataSourceRow.RowState == DataRowState.Unchanged)
			{
				if (controlAccount == null)
				{
					throw new MissingGLHeaderException(Res.GetString("7a45e309-053f-43bb-96be-81a2ca32c760", "AR/AP Control Account"));
				}

				AddPostDateDRCRLines();
				AddReverseDateDRCRLines();
			}
			else if (gLDDataSourceRow.RowState == DataRowState.Modified)
			{
				AddReverseDateDRCRLines();
			}

			return result.ToArray();

			void AddPostDateDRCRLines()
			{
				if (postDate.IsValid)
				{
					var controlAccountLine = CreateAndPopulateDebitCreditLine(controlAccount.PK, aRAPControlAccountType, localAmount, oSAmount, postDate);
					var suspenseAccountLine = CreateAndPopulateDebitCreditLine(controlAccountSuspense.PK, suspenseControlAccountType, localAmount * (-1), oSAmount * (-1), postDate);

					result.Add(controlAccountLine);
					result.Add(suspenseAccountLine);
					AddDRCRLinesForGST();
				}
			}

			void AddDRCRLinesForGST()
			{
				var gSTAccountForNotRecognizedLine = GetGSTAccountForNotRecognizedLine(lineType, taxBasis);
				if (taxAmount != 0)
				{
					var localTaxAmount_NotRecoverable = taxAmount - Utilities.Round(taxAmount * inputGSTVATRecoverable, LocalCurrencyDecimals);
					var isVATRecoverable = lineType.Equals(TransactionLineTypes.Cost) && inputGSTVATRecoverable != 1 && localTaxAmount_NotRecoverable != 0;
					var isCashBasisVAT = taxBasis.StartsWith(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code);
					var gSTLocalAmount = isVATRecoverable && !isCashBasisVAT ? new ZDecimal(Utilities.Round(taxAmount * inputGSTVATRecoverable, LocalCurrencyDecimals)) : (ZDecimal)taxAmount;

					if (gSTAccountForNotRecognizedLine == null)
					{
						throw new MissingGLHeaderException(lineType.Equals(TransactionLineTypes.Revenue) ?
																(!isCashBasisVAT ? Res.GetString("0fd4f972-2dbc-438a-8507-b423394ea20b", "Reportable Tax Output Control Account") : Res.GetString("efc91f19-cdd4-4131-a90b-8e30c930d4ce", "Pending Tax Output Control Account"))
																	: (!isCashBasisVAT ? Res.GetString("f159d160-12eb-4120-943e-5a83325d499f", "Reportable Tax Input Control Account") : Res.GetString("7ce76426-bb45-4029-a20a-ca6384e4debb", "Pending Tax Input Control Account")));
					}

					var (gSTControlAccountType, gSTOutputInputControlAccountType) = GetGLDAccountTypesForGST(ledger, transactionLineType);
					var recoverableVATLineDr = CreateAndPopulateDebitCreditLine(controlAccount.PK, gSTControlAccountType, gSTLocalAmount, ZDecimal.Zero, postDate);
					var recoverableVATLineCr = CreateAndPopulateDebitCreditLine(gSTAccountForNotRecognizedLine.PK, gSTOutputInputControlAccountType, gSTLocalAmount * (-1), ZDecimal.Zero, postDate);

					result.Add(recoverableVATLineDr);
					result.Add(recoverableVATLineCr);

					if (isVATRecoverable && !isCashBasisVAT)
					{
						var notRecoverableLocalAmount = taxAmount - new ZDecimal(Utilities.Round(taxAmount * inputGSTVATRecoverable, LocalCurrencyDecimals));
						var notRecoverableVATLineDr = CreateAndPopulateDebitCreditLine(controlAccount.PK, GLDAccountTypes.GSTRecoverableControlAccount, notRecoverableLocalAmount, ZDecimal.Zero, postDate);
						var notRecoverableVATLineCr = CreateAndPopulateDebitCreditLine(lineGLAccount, GLDAccountTypes.GSTRecoverableTransactionLineGLAccount, notRecoverableLocalAmount * (-1), ZDecimal.Zero, postDate);

						result.Add(notRecoverableVATLineDr);
						result.Add(notRecoverableVATLineCr);
					}
				}
			}

			void AddReverseDateDRCRLines()
			{
				if (recognizedDate.IsValid)
				{
					var controlAccountLineRecognizedDr = CreateAndPopulateDebitCreditLine(controlAccountSuspense.PK, suspenseControlAccountType, localAmount, oSAmount, recognizedDate, AccountingConstants.GLDTypeCodes.Recognition);
					var controlAccountLineRecognizedCr = CreateAndPopulateDebitCreditLine(lineGLAccount, GLDAccountTypes.TransactionLineGLAccount, localAmount * (-1), oSAmount * (-1), recognizedDate, AccountingConstants.GLDTypeCodes.Recognition);

					result.Add(controlAccountLineRecognizedDr);
					result.Add(controlAccountLineRecognizedCr);
				}
			}
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			return CreateGeneralLedgerDataBasicBasedOnLine(gLDDataSourceRow);
		}

		(string aRAPControlAccountType, string suspenseControlAccountType) GetGLDAccountTypes(ZString ledger, ZString transactionLineType)
		{
			var aRAPControlAccountType = ledger == LedgerTypes.AccountsPayable ? GLDAccountTypes.APControlAccount : GLDAccountTypes.ARControlAccount;
			var suspenseControlAccountType = transactionLineType == TransactionLineTypes.Revenue ? GLDAccountTypes.ARSuspenseControlAccount : GLDAccountTypes.APSuspenseControlAccount;
			return (aRAPControlAccountType, suspenseControlAccountType);
		}

		(string gSTControlAccountType, string gSTOutputInputControlAccountType) GetGLDAccountTypesForGST(ZString ledger, ZString transactionLineType)
		{
			var gSTControlAccountType = ledger == LedgerTypes.AccountsReceivable ? GLDAccountTypes.ARControlAccountForGST : GLDAccountTypes.APControlAccountForGST;
			var gSTOutputInputControlAccountType = transactionLineType == TransactionLineTypes.Revenue ? GLDAccountTypes.GSTOutputControlAccount : GLDAccountTypes.GSTInputControlAccount;
			return (gSTControlAccountType, gSTOutputInputControlAccountType);
		}
	}
}
