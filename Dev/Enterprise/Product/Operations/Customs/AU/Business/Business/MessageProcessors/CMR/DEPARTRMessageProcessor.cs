using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DEPARTRMessageProcessor : CMRMessageResponseProcessor
	{
		public DEPARTRMessageProcessor(LoggingInformation logger)
			: base(logger, "DEP", "Export Departure Report (DEPARTR)")
		{
		}

		#region Implementation

		protected override bool DoAdditionalProcessing()
		{
			bool result = base.DoAdditionalProcessing();
			if (result)
			{
				ExportCustomsManifestHeader header = incomingMessage.EM_LinkedObject as ExportCustomsManifestHeader;
				if (header != null)
				{
					CMR3CharDocumentStatus docStatus = CMR3CharDocumentStatus.GetFromStatusText(statusType);
					header.ED_DepartureReportStatus = docStatus != null ? docStatus.Code : "";
				}
			}
			return result;
		}

		#endregion
	}
}
