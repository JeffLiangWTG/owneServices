namespace Enterprise.Accounting.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;

	public class APInvoiceCollectionForDocScanning : BaseAPInvoiceCollectionForDocScanning
	{
		public APInvoiceCollectionForDocScanning(BusinessObjectFactory factory, ZQuery filter, GlbCompany company)
			: base(factory, filter, company)
		{
		}

		public APInvoiceCollectionForDocScanning(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter, GlbCompany.CurrentCompany)
		{
		}

		public new APInvoice this[int index]
		{
			get { return (APInvoice)Elements[index]; }
		}

		public new APInvoice AddNew()
		{
			return (APInvoice)base.AddNew();
		}

		protected override ZString TransactionTypeForFilter
		{
			get { return TransactionTypes.Invoice; }
		}
	}
}

