using CargoWise.ComponentModel;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder
{
	public class EHubMessageDirectorForUSDIS
	{
		readonly INotifications notifier;
		readonly EDIInterchange interchange;

		public EHubMessageDirectorForUSDIS(EDIInterchange interchange, INotifications notifier)
		{
			this.notifier = notifier;
			this.interchange = interchange;
		}

		public EHubMessageBuilder CreateBuilder()
		{
			if (interchange.EI_ApplicationCode == ApplicationCodeList.Codes.USCustomsDIS)
			{
				if (interchange.EI_InterchangeType == EDIInterchangeTypeList.Codes.USDISSubmission)
				{
					return new EHubMessageBuilderForUSDSubmission(interchange, notifier);
				}
				else
				{
					return new EHubMessageBuilderForUSDIS(interchange, notifier);
				}
			}

			return null;
		}
	}
}

