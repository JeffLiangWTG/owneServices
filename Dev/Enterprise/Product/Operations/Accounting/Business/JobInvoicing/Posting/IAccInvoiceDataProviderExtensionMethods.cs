using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class IAccInvoiceDataProviderExtensionMethods
	{
		public const string Separator = "/";
		/// <summary>
		/// If there are any reversed ones, JobCharge.JR_APInvoiceNum should be [IAccDataProvider.InvoiceNum]/[NumberOfReversedAPInvoices]
		/// </summary>
		public static ZString GetAPInvoiceNumberToMatch(this IAccInvoiceDataProvider dataProvider, BusinessObjectFactory factory, ZGuid creditorPK)
		{
			ZString result = dataProvider.UniqueNumber;

			if (!result.IsEmpty && dataProvider.CustomsJob != null && creditorPK.IsValid)
			{
				Job job = new Job.Loader(dataProvider.CustomsJob.TopLevelObjectForJobToReference).Load();

				if (job != null)
				{
					APInvoice[] invoices = new APInvoice.Loader(factory).LoadIncludingReversed(result, creditorPK, job.PK);

					int numberOfReversedInvoice = 0;
					foreach (APInvoice invoice in invoices)
					{
						if (invoice.AH_IsCancelled)
						{
							numberOfReversedInvoice++;
						}
					}

					if (numberOfReversedInvoice > 0)
					{
						result += Separator + numberOfReversedInvoice;
					}
				}
			}

			return result;
		}
	}
}
