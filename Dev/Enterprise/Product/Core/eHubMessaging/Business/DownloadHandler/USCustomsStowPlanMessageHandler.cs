using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.USCustomsStowPlan)]
	class USCustomsStowPlanMessageHandler : USCustomsMessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.StowPlan;

			return interchange;
		}
	}
}
