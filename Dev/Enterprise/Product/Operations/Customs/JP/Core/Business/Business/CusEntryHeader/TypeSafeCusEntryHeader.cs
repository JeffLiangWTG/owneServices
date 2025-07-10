using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Business
{
	public partial class CusEntryHeader
	{
		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

		public new CusEntryHeader Clone() => (CusEntryHeader)base.Clone();

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceHeader[] InvoiceHeaders() => (JobComInvoiceHeader[])base.InvoiceHeaders;

		public new Customs.Business.ICusEntryLineCollection<CusEntryLine> MergedLines => (Customs.Business.ICusEntryLineCollection<CusEntryLine>)base.MergedLines;

		[ChildEditable(true)]
		public new Customs.Business.IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (Customs.Business.IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		[ChildEditable(true)]
		public new Customs.Business.ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> Charges => (Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.Charges;

		[ChildEditable(true)]
		public new Common.EDIMessageCollection Messages => (Common.EDIMessageCollection)base.Messages;

		public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

		protected override Enterprise.Messaging.Business.EDIMessageCollection GetNewMessageCollection() => new Common.EDIMessageCollection(this);

		protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);

		protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(this);

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

		protected override Customs.Business.ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => new CusEntryHeaderValidation(this);
	}
}
