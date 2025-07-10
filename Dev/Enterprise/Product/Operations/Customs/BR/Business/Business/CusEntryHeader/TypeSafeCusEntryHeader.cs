using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	partial class CusEntryHeader
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

		public new CusEntryHeader Clone() => (CusEntryHeader)base.Clone();

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Underlying Registry Architecture currently requires array")]
		public new JobComInvoiceHeader[] InvoiceHeaders => (JobComInvoiceHeader[])base.InvoiceHeaders;

		public new Customs.Business.ICusEntryLineCollection<CusEntryLine> MergedLines => (Customs.Business.CusEntryLineCollection<CusEntryLine>)base.MergedLines;

		[ChildEditable(true)]
		public new Customs.Business.IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (Customs.Business.IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		[ChildEditable(true)]
		public new Customs.Business.ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> Charges => (Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.Charges;

		public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		#endregion

		#region Implementation

		protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection()
		{
			return new Customs.Business.CusEntryLineCollection<CusEntryLine>(this, new string[] { CustomsPostedStatusList.Codes.Active, CustomsPostedStatusList.Codes.Accepted, CustomsPostedStatusList.Codes.UpdatePending });
		}

		protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(this);

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

		protected override Customs.Business.ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => new CusEntryHeaderValidation(this);

		#endregion
	}
}
