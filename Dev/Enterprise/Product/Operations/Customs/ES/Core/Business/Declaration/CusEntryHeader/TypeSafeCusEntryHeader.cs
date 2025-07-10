using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public partial class CusEntryHeader
	{
		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

		public new AddInfoCusEntryHeaderLookups AddInfoLookups => (AddInfoCusEntryHeaderLookups)base.AddInfoLookups;

		public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		protected override EU.Business.Declaration.AddInfoCusEntryHeader GetNewAddInfoCusEntryHeader() => new AddInfoCusEntryHeader(CH_AddInfoInfo);

		[ChildEditable(true)]
		public new Customs.Business.IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (Customs.Business.IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

		protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(this);

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => new CusEntryHeaderValidation(this);

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);
	}
}
