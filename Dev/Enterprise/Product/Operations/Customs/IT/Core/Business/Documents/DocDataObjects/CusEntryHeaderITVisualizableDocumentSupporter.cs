using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;

namespace Enterprise.Customs.IT.Business.Documents.DocDataObjects;

public class CusEntryHeaderITVisualizableDocumentSupporter : CusEntryHeaderEUVisualizableDocumentSupporter
{
	public CusEntryHeaderITVisualizableDocumentSupporter(CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	protected override string GetDocProviderKey() => Core.Constants.CountryCodes.Italy;
}
