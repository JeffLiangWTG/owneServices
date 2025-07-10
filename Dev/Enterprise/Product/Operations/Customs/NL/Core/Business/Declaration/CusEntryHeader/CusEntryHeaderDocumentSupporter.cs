using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusEntryHeaderDocumentSupporter : EU.Business.Declaration.CusEntryHeaderDocumentSupporter
{
	public CusEntryHeaderDocumentSupporter(CusEntryHeader header) : base(header) { }

	public const string UTBDocument = ".UTBDocument";

	public const string ReleaseDocument = ".ReleaseDocument";

	protected new CusEntryHeader EntryHeader => (CusEntryHeader)BusinessObject;

	public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun) => false;

	protected override List<DataContextValue> GetSupportedBODataSources() => new()
	{
		new(UTBDocument),
		new(ReleaseDocument)
	};

	protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
	{
		var result = Array.Empty<IBODocDataProvider>();
		switch (dataContextValue.FullDataContext)
		{
			case UTBDocument:
				result = new IBODocDataProvider[] { BODocDataProvider.Get(new UTBDocumentWrapper(EntryHeader)) };
				break;
			case ReleaseDocument:
				result = new IBODocDataProvider[] { BODocDataProvider.Get(new ReleaseDocumentWrapper(EntryHeader)) };
				break;
		}

		return result;
	}
}
