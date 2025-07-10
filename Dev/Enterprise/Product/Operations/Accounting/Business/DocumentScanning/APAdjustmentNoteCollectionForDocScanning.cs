namespace Enterprise.Accounting.Business
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;

	public class APAdjustmentNoteCollectionForDocScanning : BaseAPInvoiceCollectionForDocScanning
	{
		public APAdjustmentNoteCollectionForDocScanning(BusinessObjectFactory factory, ZQuery filter, GlbCompany company)
			: base(factory, filter, company)
		{
		}

		public APAdjustmentNoteCollectionForDocScanning(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter, GlbCompany.CurrentCompany)
		{
		}

		public new APAdjustmentNote this[int index]
		{
			get { return (APAdjustmentNote)Elements[index]; }
		}

		public new APAdjustmentNote AddNew()
		{
			return (APAdjustmentNote)base.AddNew();
		}

		protected override ZString TransactionTypeForFilter
		{
			get { return TransactionTypes.AdjustmentNote; }
		}
	}
}

