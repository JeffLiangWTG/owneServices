using CargoWise.Types;

namespace Enterprise.Dash.Integration.Services
{
	public interface IDashEDocsService
	{
		DashEDocsDetails GetDashEDocsDetails(ZGuid docMainId, ZGuid docId, ZString docToken);
	}
}
