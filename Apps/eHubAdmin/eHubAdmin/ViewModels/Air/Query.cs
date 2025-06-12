extern alias Sys;

namespace eServices.eHubAdmin.ViewModels.Air
{
	public class Query
	{
		public Query()
		{
		}

		public string Waybill { get; set; }

		public string TimeZoneIdIANA { get; set; } = "Australia/Sydney";
	}
}