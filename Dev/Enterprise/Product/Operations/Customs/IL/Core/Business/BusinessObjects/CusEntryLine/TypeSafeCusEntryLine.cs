namespace Enterprise.Customs.IL.Business
{
	public partial class CusEntryLine : Customs.Business.CusEntryLine
	{
		public new CusEntryLine Clone() => (CusEntryLine)base.Clone();

		public new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> Fees => (CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)base.Fees;

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		public new JobComInvoiceLine RandomLine => base.RandomLine as JobComInvoiceLine;

		public new CusEntryLineLookups Lookups => (CusEntryLineLookups)base.Lookups;

		public new CusEntryLineValidation Validation => (CusEntryLineValidation)base.Validation;

		protected override System.Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

		protected override Customs.Business.ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection() => new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(this, Factory);

		protected override Customs.Business.CusEntryLineLookups GetNewLookups() => new CusEntryLineLookups(this);

		protected override Customs.Business.CusEntryLineValidation GetNewValidation() => new CusEntryLineValidation(this);
	}
}
