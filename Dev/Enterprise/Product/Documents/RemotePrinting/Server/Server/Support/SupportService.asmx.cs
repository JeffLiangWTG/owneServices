using System;
using System.Globalization;
using System.Web.Script.Services;
using System.Web.Services;
using Enterprise.RemotePrinting.Server.Model;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Server.Support
{
	/// <summary>
	/// Summary description for SupportService
	/// </summary>
	[WebService(Namespace = "http://www.cargowise.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	[ScriptService]
	public class SupportService : WebService
	{
		[WebMethod]
		[ScriptMethod]
		public SignalRClientInfo[] GetSignalRClients()
		{
			return RemoteHub.Controller.GetRegisteredClients();
		}

		[WebMethod]
		[ScriptMethod]
		public void RequestClientLogs(string clientId, string emailAddress, string fromDateString, string toDateString, int logTypes)
		{
			if (RemoteHub.Controller.HasRegisteredClient(clientId) &&
				!string.IsNullOrEmpty(emailAddress) &&
				logTypes != 0 &&
				DateTime.TryParseExact(fromDateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate) &&
				DateTime.TryParseExact(toDateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
			{
				var requestDetails = new ClientLogRequestDetails
				{
					EmailAddress = emailAddress,
					FromDate = fromDate,
					ToDate = toDate,
					LogTypes = (LogTypes)logTypes,
				};

				RemoteHub.Controller.RequestClientLogs(clientId, requestDetails);
			}
		}

		public const string DateFormat = "yyyy-MM-dd";
	}
}
