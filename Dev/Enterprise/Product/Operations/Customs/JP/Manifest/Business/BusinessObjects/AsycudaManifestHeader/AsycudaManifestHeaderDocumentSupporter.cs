using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaManifestHeaderDocumentSupporter(AsycudaManifestHeader header) : ASYCUDA.Business.AsycudaManifestHeaderDocumentSupporter(header)
	{
		protected AsycudaManifestHeader Header => (AsycudaManifestHeader)base.BusinessObject;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(MessageDocumentSupporter.MismatchInformationDocument));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			return dataContextValue.FullDataContext switch
			{
				MessageDocumentSupporter.MismatchInformationDocument => MismatchInformationMessages.Select(x => new MessageDocumentSupporter(x).GetMismatchInformationMessageDocumentWrapper()).ToArray(),
				_ => base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun),
			};
		}

		IEnumerable<EDIMessage> MismatchInformationMessages => Factory.GetValue(ref mismatchInformationMessagesCached, () => Header.Messages.Cast<EDIMessage>().Where(e => e.IsMismatchInformationMessage));
		CachedProperty<IEnumerable<EDIMessage>> mismatchInformationMessagesCached;

		public override string GetFilterValue(DocumentFilters filterName)
		{
			return filterName switch
			{
				DocumentFilters.IsJPDiscrepancyNoticeDocumentSupport => MismatchInformationMessages.Any() ? Customs.Business.YesNoList.Codes.Yes : Customs.Business.YesNoList.Codes.No,
				_ => base.GetFilterValue(filterName),
			};
		}
	}
}
