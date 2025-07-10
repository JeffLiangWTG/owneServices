using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public new JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.Invoice; }
		}

		protected new JobComInvoiceHeader Parent
		{
			get { return (JobComInvoiceHeader)base.Parent; }
		}

		public override OrgHeaderCollection Suppliers
		{
			get
			{
				var fSuppliersList = new ConsignorCollection(Factory);
				var importer = Parent.Importer_Effective;
				if (importer != null)
				{
					fSuppliersList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
						"Consignor - Related Consignee", "Property",
						delegate
						{ return importer.PK; }));
				}
				return fSuppliersList;
			}
		}
	}
}
