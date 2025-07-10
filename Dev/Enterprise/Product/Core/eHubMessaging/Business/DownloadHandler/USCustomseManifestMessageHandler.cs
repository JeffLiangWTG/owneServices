using System.IO;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.USCustomseManifest)]
	class USCustomseManifestMessageHandler : USCustomsMessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			using (var reader = new StreamReader(this.Message.MessageStream))
			{
				var interchange = base.CreateInterchange();
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.USeManifest;

				return interchange;
			}
		}
	}
}
