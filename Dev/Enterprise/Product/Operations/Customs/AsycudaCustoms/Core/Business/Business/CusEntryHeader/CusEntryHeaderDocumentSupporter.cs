using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusEntryHeaderDocumentSupporter : Customs.Business.CusEntryHeaderDocumentSupporter
	{
		public CusEntryHeaderDocumentSupporter(CusEntryHeader header)
			: base(header)
		{
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			return dataContextValue.FullDataContext == SummaryCusDecDocument
				? (ZString)Res.GetString("179C8F97-5764-4252-AF3A-D7D94076E6B6", "Entry Header {0} cannot be found.", EntryHeader.CH_BGMReference)
				: base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		protected new CusEntryHeader EntryHeader => (CusEntryHeader)BusinessObject;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(SummaryCusDecDocument));
			return result;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return [DocCusEntryHeader.New(EntryHeader, Factory)];
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			return dataContextValue.FullDataContext == SummaryCusDecDocument
				? new[] { BODocDataProvider.Get(new SummaryCusDecDocumentWrapper(EntryHeader)) }
				: base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}

		internal const string SummaryCusDecDocument = ".SummaryCusDecDocument";
	}
}
