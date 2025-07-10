using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.Business;

public class CusEntryHeaderDocumentSupporter : Customs.Business.CusEntryHeaderDocumentSupporter
{
	public CusEntryHeaderDocumentSupporter(CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

	protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		=> new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapperWithoutException(supportedWrappers[dataContext], EntryHeader) };

	readonly Dictionary<DataContext, ZString> supportedWrappers = new Dictionary<DataContext, ZString>()
		{
			{ DataContext.CusEntryHeader, $"{typeof(DocBaseCusEntryHeader).FullName}, {typeof(DocBaseCusEntryHeader).Assembly.GetName().Name}" },
		};

	public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun) => false;

	public override string GetFilterValue(DocumentFilters filterName) => filterName == DocumentFilters.CTY ? CountryCodes.Switzerland : base.GetFilterValue(filterName);
}

