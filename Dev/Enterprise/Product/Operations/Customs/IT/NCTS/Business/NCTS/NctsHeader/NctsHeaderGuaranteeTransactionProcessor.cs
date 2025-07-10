using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderGuaranteeTransactionProcessor
{
	public NctsHeaderGuaranteeTransactionProcessor(NctsHeader nctsHeader)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
	}

	readonly NctsHeader nctsHeader;

	public IReadOnlyCollection<BaseCusGuaranteeLineTransaction> AddNewConsumeTransaction() => AddNewTransaction(NctsDepartureCommentPrefix, applicationId: ZString.Empty, PermitTransactionStatusList.Codes.Pending, (transactionValue) => -transactionValue, ZDateTime.Now).ToList().AsReadOnly();

	public IReadOnlyCollection<BaseCusGuaranteeLineTransaction> AddNewWriteOffTransaction(ZString applicationId, ZDate transactionDate) => AddNewTransaction(NctsWriteCommentPrefix, applicationId, PermitTransactionStatusList.Codes.Confirmed, (transactionValue) => transactionValue, transactionDate).ToList().AsReadOnly();

	public void DeletePendingTransactions(ZString applicationId)
	{
		foreach (var transaction in GetPendingTransactions(applicationId))
		{
			transaction.Delete();
		}
	}

	public void ConfirmPendingTransactions(ZString applicationId)
	{
		if (nctsHeader.MovementReferenceNumber.IsEmpty)
		{
			return;
		}

		foreach (var transaction in GetPendingTransactions(applicationId))
		{
			transaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction.CPL_Comment = $"NCTS departure {nctsHeader.MovementReferenceNumber}";
		}
	}

	IEnumerable<BaseCusGuaranteeLineTransaction> AddNewTransaction(ZString commentPrefix, ZString applicationId, ZString status, Func<ZDecimal, ZDecimal> transactionValueConverter, ZDateTime transactionDate)
	{
		var guarantees = nctsHeader.IsPhase5 ? nctsHeader.MovementHeader.Guarantees : nctsHeader.Guarantees;

		foreach (var guarantee in guarantees)
		{
			var cusGuarantee = guarantee?.CusGuarantee;
			if (cusGuarantee == null)
			{
				continue;
			}

			var transaction = cusGuarantee.AddTransaction(
				reference: nctsHeader.JobNumber,
				comment: ZString.Format("{0} {1}", commentPrefix, nctsHeader.JobNumber),
				appId: applicationId,
				procedure: ZString.Empty,
				value: transactionValueConverter(Math.Abs(guarantee.PW_BondAmount)),
				quantity: 0m,
				status: status,
				transactionDate: transactionDate,
				checkBursting: false);

			if (transaction != null)
			{
				yield return (BaseCusGuaranteeLineTransaction)transaction;
			}
		}
	}

	IEnumerable<BaseCusPermitLineTransaction> GetPendingTransactions(ZString applicationId)
	{
		var query = new ZQuery(CusPermitLineTransactionSchema.CPL_Reference, nctsHeader.BH_JobReference);
		var transactions = nctsHeader.Factory.Load<BaseCusPermitLineTransaction>(query);
		return transactions.Where(x => x.CPL_AppId == applicationId && x.IsPending);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant prefix")]
	const string NctsDepartureCommentPrefix = "NCTS departure";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant prefix")]
	const string NctsWriteCommentPrefix = "NCTS write-off";
}
