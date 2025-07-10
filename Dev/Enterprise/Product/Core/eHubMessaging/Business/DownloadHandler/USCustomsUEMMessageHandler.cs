using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.USCustomsExportManifest)]
	class USCustomsUEMMessageHandler : USCustomsMessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.USExportManifest;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.USCustomsExportManifest;

			return interchange;
		}
	}
}
