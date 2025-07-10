using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public interface IElectronicInvoicingRequeueStrategy
	{
		RequeueDecision IsTransactionEnableToRequeue(TransactionHeader transaction);
		HashSet<ZString> GetReasonsToExcludeTransaction(HashSet<RequeueDecision> rejectionReasons);
	}

	public class ElectronicInvoicingRequeueStrategy : IElectronicInvoicingRequeueStrategy
	{
		public ElectronicInvoicingRequeueStrategy()
		{
			QueueReversedTransaction = ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(GlbCompany.CurrentCompany.Country.Code) as IComplianceInfoEInvoicingGUIActionQueueReversedTransaction;
		}

		readonly IComplianceInfoEInvoicingGUIActionQueueReversedTransaction QueueReversedTransaction;

		public HashSet<ZString> GetReasonsToExcludeTransaction(HashSet<RequeueDecision> rejectionReasons)
		{
			var list = new HashSet<ZString>();
			foreach (var rejectionReason in rejectionReasons)
			{
				switch (rejectionReason)
				{
					case RequeueDecision.RejectCancelled:
						list.Add(Res.GetString("EE6DB7FD-9CA2-410C-AF2A-CBB8CDA6B0E0", "Reversed transactions cannot be re-queued."));
						break;
				}
			}

			return list;
		}

		RequeueDecision IElectronicInvoicingRequeueStrategy.IsTransactionEnableToRequeue(TransactionHeader transaction)
		{
			Argument.NotNull(transaction, nameof(transaction));

			if ((QueueReversedTransaction?.RejectReQueueForReversedTransaction(transaction.AH_ComplianceSubType) ?? false) && (transaction.AH_IsCancelled))
			{
				return RequeueDecision.RejectCancelled;
			}

			return RequeueDecision.Requeue;
		}
	}

	public enum RequeueDecision
	{
		Requeue,
		RejectCancelled
	}
}
