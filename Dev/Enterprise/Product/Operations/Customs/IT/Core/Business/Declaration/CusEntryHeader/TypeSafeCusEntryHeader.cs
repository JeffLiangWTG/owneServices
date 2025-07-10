using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class CusEntryHeader
{
	public new Customs.Business.ICusEntryLineCollection<CusEntryLine> MergedLines => (Customs.Business.ICusEntryLineCollection<CusEntryLine>)base.MergedLines;

	[ChildEditable(true)]
	public new Customs.Business.IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (Customs.Business.AllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

	[ChildEditable(true)]
	public new ITEDIMessageCollection Messages => (ITEDIMessageCollection)base.Messages;

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

	public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

	public new AddInfoCusEntryHeaderLookups AddInfoLookups => (AddInfoCusEntryHeaderLookups)base.AddInfoLookups;

	public new Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo> EntryPayInfos => (Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo>)base.EntryPayInfos;

	protected override EU.Business.Declaration.AddInfoCusEntryHeader GetNewAddInfoCusEntryHeader() => new AddInfoCusEntryHeader(CH_AddInfoInfo);

	protected override Customs.Business.ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new Customs.Business.CusEntryLineCollection<CusEntryLine>(this);

	protected override Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new Customs.Business.AllCusEntryLineCollection<CusEntryLine>(this);

	protected override Enterprise.Messaging.Business.EDIMessageCollection GetNewMessageCollection() => new ITEDIMessageCollection(this);

	protected override Customs.Business.BusinessObjects.Interfaces.ICusEntryPayInfoCollection<Customs.Business.CusEntryPayInfo> CreateNewEntryPayInfosCollection() => new Customs.Business.CusEntryPayInfoCollection<CusEntryPayInfo>(this);

	protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

	protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => new CusEntryHeaderValidation(this);

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new JobComInvoiceHeader RandomHeader => (JobComInvoiceHeader)base.RandomHeader;
}
