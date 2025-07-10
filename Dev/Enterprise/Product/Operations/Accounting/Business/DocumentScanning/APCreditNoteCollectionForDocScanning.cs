namespace Enterprise.Accounting.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;

	public class APCreditNoteCollectionForDocScanning : BaseAPInvoiceCollectionForDocScanning
	{
		public APCreditNoteCollectionForDocScanning(BusinessObjectFactory factory, ZQuery filter, GlbCompany company)
			: base(factory, filter, company)
		{
		}

		public APCreditNoteCollectionForDocScanning(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter, GlbCompany.CurrentCompany)
		{
		}

		public new APCreditNote this[int index]
		{
			get { return (APCreditNote)Elements[index]; }
		}

		public new APCreditNote AddNew()
		{
			return (APCreditNote)base.AddNew();
		}

		protected override ZString TransactionTypeForFilter
		{
			get { return TransactionTypes.CreditNote; }
		}
	}
}

