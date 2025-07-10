using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public interface IProfitShareShipmentChargePoster
	{
		IReadOnlyCollection<TransactionHeader> PostCharge(Charge charge, Job job, IAdjustPostedInvoiceHelper adjustPostedInvoiceHelper);

		void UpdateJobStatus(Job job);
	}

	[WTG.StaticAnalysis.Annotation.Immutable]
	internal class ProfitShareShipmentChargePoster : IProfitShareShipmentChargePoster
	{
		public IReadOnlyCollection<TransactionHeader> PostCharge(Charge charge, Job job, IAdjustPostedInvoiceHelper adjustPostedInvoiceHelper)
		{
			Argument.NotNull(charge, nameof(charge));
			Argument.NotNull(job, nameof(job));
			Argument.NotNull(adjustPostedInvoiceHelper, nameof(adjustPostedInvoiceHelper));

			var result = Array.Empty<TransactionHeader>();
			if ((!AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value && charge.JR_OSCostAmt != 0m && charge.JR_LocalCostAmt != 0m) ||
				(AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value && charge.JR_OSSellAmt != 0m && charge.JR_LocalSellAmt != 0m))
			{
				job.JH_IsProfitSharePosted = true;

				var transactionsCreated = new TransactionCreatorHashtable();
				if (AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.Value)
				{
					var invoice = new ChargePoster(job.Factory).Post(charge);
					adjustPostedInvoiceHelper.AdjustPostedInvoice(invoice);
					transactionsCreated.AddARInvoice(invoice);
				}
				else
				{
					new ProfitShareAPInvoiceCreator(charge).CreateTransactions(transactionsCreated);
				}
				result = transactionsCreated.GetAllARTransactions().Concat(transactionsCreated.GetAllAPTransactions()).ToArray();
			}
			return result;
		}

		public void UpdateJobStatus(Job job)
		{
			Argument.NotNull(job, nameof(job));

			if (AccountingConfigurationRegistry.Instance.AutoCompleteJobOnProfitShareCalculation.Value)
			{
				job.JH_Status = JobHeaderStatus.Complete.Code;
			}
		}
	}
}
