using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class APReversalDateHelper
	{
		public static void DefaultReversalDates(InvoicingBase reversedInvoice)
		{
			if (reversedInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				IJobInvoicingPlugIn consumer = GetConsumer(reversedInvoice);
				if (consumer != null)
				{
					PostDateConfigurationHelper postDateConfigurationHelper = new PostDateConfigurationHelper(consumer, true);
					ZDateTime? originalPostDate = reversedInvoice.OriginalTransaction != null ? reversedInvoice.OriginalTransaction.AH_PostDate : null;
					ZDateTime postDate = postDateConfigurationHelper.GetPostDate(ZDateTime.Today, reversedInvoice.AH_InvoiceDate, originalPostDate);
					reversedInvoice.AH_PostDate = postDate;
				}

				if (AccountingConfigurationRegistry.Instance.AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate.Value && reversedInvoice.OriginalTransaction != null)
				{
					reversedInvoice.AH_InvoiceDate = reversedInvoice.OriginalTransaction.AH_InvoiceDate;
				}
			}
		}

		static IJobInvoicingPlugIn GetConsumer(InvoicingBase reversedInvoice)
		{
			IJobInvoicingPlugIn consumer = null;

			consumer = reversedInvoice.SingleConsolConsumer;

			if (consumer == null)
			{
				ZGuid pk = reversedInvoice.SingleJobInvoicePK;

				if (pk.IsValid)
				{
					Job job = reversedInvoice.Factory.Load<Job>(pk);

					if (job != null)
					{
						job.InitializeParentFromGenericJobWithoutSettingDefaults();
						consumer = job.PlugInData;
					}
				}
			}

			return consumer;
		}
	}
}