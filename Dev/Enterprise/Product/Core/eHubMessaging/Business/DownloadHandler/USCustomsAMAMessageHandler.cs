using CargoWise.eHub.Common.Extensions;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.USCustomsAMA)]
	class USCustomsAMAMessageHandler : USCustomsMessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();

			Message.MessageStream.SeekBegin();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.USAMA;
			interchange.EI_InterchangeType = GetInterchangeType(interchange);

			return interchange;
		}

		string GetInterchangeType(EDIInterchange interchange)
		{
			var bodyText = interchange.GetEI_BodyTextReader(true).ReadToEnd();
			return bodyText.Substring(0, 3);
		}
	}
}
