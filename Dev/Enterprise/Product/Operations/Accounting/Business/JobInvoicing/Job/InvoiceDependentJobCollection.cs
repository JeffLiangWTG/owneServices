using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class InvoiceDependentJobCollection : NonPersistentBusinessObjectCollection<InvoiceDependentJob>
	{
		public InvoiceDependentJobCollection(InvoicingBase invoice, BusinessObjectFactory factory)
			: base(factory)
		{
			this.invoice = invoice;
			this.factory = factory;
		}

		readonly InvoicingBase invoice;
		readonly BusinessObjectFactory factory;

		public void RemoveAndPopulateJobsAndAmountsForAPInvoiceSummaryTab()
		{
			var allJobs = invoice.Lines.Cast<InvoicingLineBase>().Where(x => x.InvoicingJob != null).Select(x => x.InvoicingJob).Distinct()
				.ToDictionary(job => job.PK, job => job);

			var currentJobs = this.Cast<InvoiceDependentJob>().ToDictionary(x => x.Job.PK, x => x.PK);

			var toBeAdded = allJobs.Where(pair => !currentJobs.ContainsKey(pair.Key)).Select(pair => pair.Value);
			var toBeRemoved = currentJobs.Where(pair => !allJobs.ContainsKey(pair.Key)).Select(pair => pair.Value);

			foreach (var pk in toBeRemoved)
			{
				this.Remove(pk);
			}

			foreach (var job in toBeAdded)
			{
				this.Add(new InvoiceDependentJob(factory, invoice, job));
#if DEBUG
				InvoiceDependentJobCreatedCount_ForTestOnly++;
#endif
			}
		}

		public new InvoiceDependentJob this[int index]
		{
			get { return ((InvoiceDependentJob)Elements[index]); }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		protected new void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
		}

#if DEBUG
		public int InvoiceDependentJobCreatedCount_ForTestOnly;
#endif
	}
}