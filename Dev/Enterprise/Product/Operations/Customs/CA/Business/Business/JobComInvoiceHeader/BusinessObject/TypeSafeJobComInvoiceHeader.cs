using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CA.Business
{
	public partial class JobComInvoiceHeader
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceHeader Clone()
		{
			return (JobComInvoiceHeader)base.Clone();
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		public new JobComInvoiceHeaderLookups Lookups
		{
			get { return (JobComInvoiceHeaderLookups)base.Lookups; }
		}

		public new JobComInvoiceHeaderValidation Validation
		{
			get { return (JobComInvoiceHeaderValidation)base.Validation; }
		}

		public new JobComInvoiceLineViewCollection JobComInvoiceLines
		{
			get { return (JobComInvoiceLineViewCollection)base.JobComInvoiceLines; }
		}

		[ChildEditable(true)]
		public new JobComInvChargeCollection<InvoiceCharge> Charges
		{
			get { return (JobComInvChargeCollection<InvoiceCharge>)base.Charges; }
		}

		[ChildEditable(true)]
		public new JobComInvApportionedChargeCollection<InvoiceApportionCharge> GroupCharges => (JobComInvApportionedChargeCollection<InvoiceApportionCharge>)base.GroupCharges;

		public new JobComInvoiceGroupHeader Master
		{
			get { return (JobComInvoiceGroupHeader)base.Master; }
		}

		public new JobComInvoiceGroupHeader GroupHeader
		{
			get { return (JobComInvoiceGroupHeader)base.GroupHeader; }
		}

		public new Bill Bill
		{
			get { return (Bill)base.Bill; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new JobComInvoiceHeaderLookups(this);
		}

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			Customs.Business.JobComInvoiceHeaderValidation result;
			switch (JZ_MessageType)
			{
				case JobMessageTypeList.Codes.Export:
					result = new ExportJobComInvoiceHeaderValidation(this);
					break;
				case JobMessageTypeList.Codes.Import:
				case JobMessageTypeList.Codes.LowValueShipments:
				case JobMessageTypeList.Codes.LVSForConsolidation:
				case JobMessageTypeList.Codes.ImportCopyforB2:
					result = new ImportJobComInvoiceHeaderValidation(this);
					break;
				case JobMessageTypeList.Codes.B2Adjustments:
				case JobMessageTypeList.Codes.XTypeEntry:
					result = new B2JobComInvoiceHeaderValidation(this);
					break;
				default:
					result = new JobComInvoiceHeaderValidation(this);
					break;
			}
			return result;
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			var collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			return new JobComInvoiceLineViewCollection(this, collection);
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			return JobDeclaration != null ? new JobComInvoiceLineViewCollection(this, JobDeclaration.InvoiceLines) : null;
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges()
		{
			return new JobComInvChargeCollection<InvoiceCharge>(this);
		}

		protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<InvoiceApportionCharge>(this);
		}

		#endregion

		#endregion
	}
}
