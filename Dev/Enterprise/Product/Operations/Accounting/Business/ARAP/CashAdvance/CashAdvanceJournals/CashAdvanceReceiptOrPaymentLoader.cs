using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public class CashAdvanceReceiptOrPaymentLoader
	{
		public CashAdvanceReceiptOrPaymentLoader(CashAdvanceRequestHeader cashAdvanceRequest)
		{
			CashAdvanceRequest = cashAdvanceRequest;
		}
		CashAdvanceRequestHeader CashAdvanceRequest { get; }

		public List<CashAdvanceReceiptOrPaymentDetails> Get()
		{
			var factory = CashAdvanceRequest.Factory;
			var sqlText = FormattableString.Invariant($@"SELECT AH_PK, AH_TransactionNum, AH_ChequeOrReference
			FROM dbo.AccTransactionHeader AH
				INNER JOIN dbo.AccTransactionMatchLink AP ON AP.AP_AH = AH.AH_PK
				INNER JOIN 
				(
					SELECT AP_MatchGroupNum, AH_GC
					FROM dbo.AccTransactionHeader
						INNER JOIN dbo.AccTransactionMatchLink ON AP_AH = AH_PK
					WHERE AH_CAH_CashAdvanceRequestHeader = @cahPK 
						AND AH_TransactionCategory = @category 
						AND AH_TransactionType = 'JNL'
				) MatchGroup ON MatchGroup.AP_MatchGroupNum = AP.AP_MatchGroupNum AND MatchGroup.AH_GC = AH.AH_GC
			WHERE AH.AH_TransactionType = @transactionType and AH.AH_Ledger = @ledger");

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add(ZSqlParameter.New((NoResString)"@cahPK", CashAdvanceRequest.PK, AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader));
			sqlParams.Add(ZSqlParameter.New((NoResString)"@category", CashAdvanceReceiptOrPaymentJournalCreator.GetMatchingJournalTransactionCategory(CashAdvanceRequest.CAH_Ledger), AccTransactionHeaderSchema.AH_TransactionCategory));
			sqlParams.Add(ZSqlParameter.New("@transactionType", CashAdvanceReceiptOrPaymentJournalCreator.GetCashAdvanceReceiptOrPaymentTransactionType(CashAdvanceRequest.CAH_Ledger), AccTransactionHeaderSchema.AH_TransactionType));
			sqlParams.Add(ZSqlParameter.New((NoResString)"@ledger", CashAdvanceRequest.CAH_Ledger, AccTransactionHeaderSchema.AH_Ledger));

			var dBizos = new DynamicBusinessObjectCollection(factory);
			dBizos.Load(sqlText, sqlParams.ToArray());
			var receipts = new List<CashAdvanceReceiptOrPaymentDetails>();
			foreach (DynamicBusinessObject bizo in dBizos)
			{
				receipts.Add(new CashAdvanceReceiptOrPaymentDetails(transactionPK: (ZGuid)bizo["AH_PK"], transactionNumber: (ZString)bizo["AH_TransactionNum"], chequeOrReferenceNumber: (ZString)bizo["AH_ChequeOrReference"]));
			}
			return receipts;
		}
	}

	public struct CashAdvanceReceiptOrPaymentDetails
	{
		public CashAdvanceReceiptOrPaymentDetails(ZGuid transactionPK, ZString transactionNumber, ZString chequeOrReferenceNumber)
		{
			TransactionPK = transactionPK;
			TransactionNumber = transactionNumber;
			ChequeOrReferenceNumber = chequeOrReferenceNumber;
		}

		internal ZGuid TransactionPK { get; }
		internal ZString TransactionNumber { get; }
		internal ZString ChequeOrReferenceNumber { get; }
		public override string ToString()
		{
			var textBuilder = new ZStringBuilder(FormattableString.Invariant($"{TransactionNumber}"));
			if (!ChequeOrReferenceNumber.IsEmpty)
			{
				textBuilder.Append(FormattableString.Invariant($"/{ChequeOrReferenceNumber}"));
			}
			return textBuilder.ToString();
		}
	}
}
