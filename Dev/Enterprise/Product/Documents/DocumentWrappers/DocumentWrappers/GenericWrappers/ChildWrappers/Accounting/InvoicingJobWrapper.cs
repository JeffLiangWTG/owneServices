using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("JobNum"), WrapperTypeName("Invoicing Job")]
	public class InvoicingJobWrapper : GenericWrapper
	{
		public InvoicingJobWrapper(Job job, BusinessObjectFactory factory)
			: base(job, factory)
		{
			legacyWrapper = DocJobInvoicingJob.New(job, Factory);
		}

		public InvoicingJobWrapper(Job job, OrgHeader debtor, BusinessObjectFactory factory)
			: this(job, factory)
		{
			this.debtorBO = debtor;
		}

		readonly DocJobInvoicingJob legacyWrapper;
		readonly OrgHeader debtorBO;

		Job Job
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (Job)WrappedObject; }
		}

		#region Implementation

		public ZString JobNum
		{
			get { return legacyWrapper.JobNum; }
		}

		public DocJobInvoicingJobChargeCollection Charges
		{
			get { return charges ?? (charges = GetCharges()); }
		}
		DocJobInvoicingJobChargeCollection charges;

		public DocJobPaymentBasisCollection CostPaymentBases
		{
			get
			{
				if (costPaymentBases == null)
				{
					var docs = Charges
						.Cast<DocJobInvoicingJobCharge>()
						.SelectMany(c => c.CostPaymentBases);

					costPaymentBases = new DocJobPaymentBasisCollection(Factory);
					costPaymentBases.AddRange(docs);
				}

				return costPaymentBases;
			}
		}
		DocJobPaymentBasisCollection costPaymentBases;

		public DocJobPaymentBasisCollection SellPaymentBases
		{
			get
			{
				if (sellPaymentBases == null)
				{
					var docs = Charges
						.Cast<DocJobInvoicingJobCharge>()
						.SelectMany(c => c.SellPaymentBases);

					sellPaymentBases = new DocJobPaymentBasisCollection(Factory);
					sellPaymentBases.AddRange(docs);
				}

				return sellPaymentBases;
			}
		}
		DocJobPaymentBasisCollection sellPaymentBases;

		DocJobInvoicingJobChargeCollection GetCharges()
		{
			DocJobInvoicingJobChargeCollection result = null;

			if (debtorBO == null)
			{
				result = legacyWrapper.Charges;
			}
			else
			{
				result = new DocJobInvoicingJobChargeCollection(Factory);

				foreach (Charge charge in Job.Charges.Cast<Charge>().Where(ch => ch.JR_OH_SellAccount == debtorBO.PK))
				{
					result.Add(DocJobInvoicingJobCharge.New(charge, Factory));
				}
			}

			return result;
		}

		public OrganisationWrapper Debtor
		{
			get { return debtor ?? (debtor = debtorBO != null ? new OrganisationWrapper(OrganisationUsageType.Debtor, debtorBO, ContactType.LocalClient, Factory) : null); }
		}
		OrganisationWrapper debtor;

		#endregion
	}
}
