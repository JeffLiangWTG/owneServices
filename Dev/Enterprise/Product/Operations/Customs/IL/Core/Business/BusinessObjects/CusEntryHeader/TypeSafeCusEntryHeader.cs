using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public partial class CusEntryHeader : Customs.Business.CusEntryHeader
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

		public new CusEntryHeader Clone() => (CusEntryHeader)base.Clone();

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new JobComInvoiceHeader[] InvoiceHeaders() => (JobComInvoiceHeader[])base.InvoiceHeaders;

		public new ICusEntryLineCollection<CusEntryLine> MergedLines => (ICusEntryLineCollection<CusEntryLine>)base.MergedLines;

		[ChildEditable(true)]
		public new AllCusEntryLineCollection AllEntryLines => (AllCusEntryLineCollection)base.AllEntryLines;

		public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

		[ChildEditable(true)]
		public new ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> Charges => (CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.Charges;

		public new IConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges> ConfirmedCharges => (ConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.ConfirmedCharges;

		public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		protected override IConfirmedCusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewConfirmedCusEntryHeaderChargesCollection() => new ConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		protected override ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new CusEntryLineCollection<CusEntryLine>(this);

		protected override IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new AllCusEntryLineCollection(this);

		protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

		protected override ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

		protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => new CusEntryHeaderValidation(this);
	}
}
