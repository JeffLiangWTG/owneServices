using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobInvoiceDescriptionReadOnly : JobConfigurationSelectorReadOnly
	{
		public JobInvoiceDescriptionReadOnly(JobInvoiceDescription parent)
			: base(parent)
		{
		}

		protected new JobInvoiceDescription Parent
		{
			get { return (JobInvoiceDescription)base.Parent; }
		}

		#region InvoiceDescription

		public bool InvoiceDescription_ReadOnly
		{
			get { return Parent.JobType.IsEmpty || Parent.JobTypeInfo.HasErrors(); }
		}

		#endregion
	}
}
