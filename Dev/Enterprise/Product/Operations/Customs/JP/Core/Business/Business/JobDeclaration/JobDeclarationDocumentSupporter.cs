using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.JP.Business
{
	public class JobDeclarationDocumentSupporter(JobDeclaration declaration) : Customs.Business.BaseJobDeclarationDocumentSupporter(declaration)
	{
		protected JobDeclaration Declaration => (JobDeclaration)BaseJobDeclaration;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(MessageDocumentSupporter.ExportPermitMessageDocument));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			return dataContextValue.FullDataContext switch
			{
				MessageDocumentSupporter.ExportPermitMessageDocument => ExportPermitMessages.Select(x => new MessageDocumentSupporter(x).GetExportPermitMessageDocumentWrapper()).ToArray(),
				_ => base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun),
			};
		}

		IEnumerable<EDIMessage> ExportPermitMessages => Factory.GetValue(ref exportPermitMessagesCached, () => Declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().SelectMany(x => x.Messages.Cast<EDIMessage>().Where(e => e.IsExportPermitMessage)));
		CachedProperty<IEnumerable<EDIMessage>> exportPermitMessagesCached;

		public override string GetFilterValue(DocumentFilters filterName)
		{
			return filterName switch
			{
				DocumentFilters.IsJPExportPermitMessageDocumentSupport => ExportPermitMessages.Any() ? Customs.Business.YesNoList.Codes.Yes : Customs.Business.YesNoList.Codes.No,
				_ => base.GetFilterValue(filterName),
			};
		}
	}
}
