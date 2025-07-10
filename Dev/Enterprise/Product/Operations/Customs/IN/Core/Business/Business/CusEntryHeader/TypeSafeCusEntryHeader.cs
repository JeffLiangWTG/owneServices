using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public partial class CusEntryHeader : Customs.Business.CusEntryHeader
{
	public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;

	public new CusEntryHeader Clone() => (CusEntryHeader)base.Clone();

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "The underlying Architecture returns arrays for collections")]
	public new JobComInvoiceHeader[] InvoiceHeaders => (JobComInvoiceHeader[])base.InvoiceHeaders;

	public new Customs.Business.ICusEntryLineCollection<CusEntryLine> MergedLines => (Customs.Business.ICusEntryLineCollection<CusEntryLine>)base.MergedLines;

	[ChildEditable(true)]
	public new Customs.Business.IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (Customs.Business.IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

	public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

	[ChildEditable(true)]
	public new Customs.Business.ICusEntryHeaderChargesCollection<CusEntryHeaderCharges> Charges => (Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>)base.Charges;

	public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

	protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);

	protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(this);

	protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

	protected override Customs.Business.ICusEntryHeaderChargesCollection<Customs.Business.CusEntryHeaderCharges> CreateNewCusEntryHeaderChargesCollection() => new Customs.Business.CusEntryHeaderChargesCollection<CusEntryHeaderCharges>(this);

	protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => new CusEntryHeaderValidation(this);
}
