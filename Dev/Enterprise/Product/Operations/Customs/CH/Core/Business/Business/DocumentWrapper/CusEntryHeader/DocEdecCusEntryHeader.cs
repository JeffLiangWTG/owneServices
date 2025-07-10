using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class DocEdecCusEntryHeader : DocBaseCusEntryHeader
{
	public static new DocEdecCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap) => new DocEdecCusEntryHeader(cusEntryHeader, factoryToWrap);

	DocEdecCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap) : base(cusEntryHeader, factoryToWrap)
	{
	}

	public DocTraderDataWrapper Importer => importer ??= DocTraderDataWrapper.New(CusEntryHeader.Declaration?.ImporterDocumentaryAddress, Factory, AddressAttributeMaxLength);
	DocTraderDataWrapper importer;

	public override DocTraderDataWrapper Consignee => consignee ??= DocTraderDataWrapper.New(CusEntryHeader.Declaration?.IntermConsignee?.MainAddress, Factory, AddressAttributeMaxLength);
	DocTraderDataWrapper consignee;

	public DocEdecBusinessDataWrapper Business => business ??= DocEdecBusinessDataWrapper.New(CusEntryHeader, Factory);
	DocEdecBusinessDataWrapper business;

	public new DocEdecCusEntryLineCollection EntryLines => entryLines ??= CreateNewDocCusEntryLineCollection();
	DocEdecCusEntryLineCollection entryLines;

	DocEdecCusEntryLineCollection CreateNewDocCusEntryLineCollection()
	{
		var entryLines = new DocEdecCusEntryLineCollection(CusEntryHeader.MergedLines, Factory);
		entryLines.Cast<DocCusEntryLine>().ForEach(x => x.EntryHeader = this);
		entryLines.Sort(nameof(DocCusEntryLine.LineNumber), System.ComponentModel.ListSortDirection.Ascending);
		return entryLines;
	}

	protected override int AddressAttributeMaxLength => 20;
	protected override int PrevDocReferencesMaxLength => 226;
}
