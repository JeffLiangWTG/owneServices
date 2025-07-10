using Enterprise.Customs.DE.Business.Documents;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Declaration;
public class CusEntryHeaderDocumentSupporter(CusEntryHeader entryHeader) : EU.Business.Declaration.CusEntryHeaderDocumentSupporter(entryHeader)
{
	protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun) =>
		dataContext switch
		{
			DataContext.SADH => DocumentSupporterHelper.GetSADHWrappers(EntryHeader),
			DataContext.CusEntryHeader => DocumentSupporterHelper.GetCusEntryHeaderWrappers(EntryHeader),
			_ => base.GetDocumentWrappersInternal(dataContext, commandBeingRun),
		};

	new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;
}
