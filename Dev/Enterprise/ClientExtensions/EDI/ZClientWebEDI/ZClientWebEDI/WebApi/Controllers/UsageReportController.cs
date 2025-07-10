using System.Globalization;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[RoutePrefix("api/UsageReport")]
	public class UsageReportController : ControllerWithEnvironment
	{
		[Route("")]
		public string Get()
		{
			return "hello";
		}

		[HttpPost]
		[Route("{year}/{month}")]
		public OrgUsage Post(string year, string month, [FromBody] RequestContent requestContent)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var periodStart = GetReportPeriodStart(year, month);
				var userSession = UserSessionHelper.DecodeSessionString(requestContent?.UserSession);
				return !userSession.LoggedInOrganisationPK.IsEmpty ? new OrgUsage(new BusinessObjectFactory().Load<OrgHeader>(userSession.LoggedInOrganisationPK), periodStart, requestContent?.Product ?? "") : null;
			}
		}

		[HttpPost]
		[Route("GetStlCombinedUsageReport/{databasePK}/{period}")]
		public string GetStlCombinedUsageReport(string databasePK, int period, [FromBody] RequestContent requestContent)
		{
			var result = string.Empty;
			ZGuid dbPk;

			using (Db.DisposableActionForDbConnection())
			{
				if (!ZGuid.TryParse(databasePK, out dbPk))
				{
					result = "Invalid Database.";
				}
				else if (period < 197001 || period > 300001)
				{
					result = "Invalid Period.";
				}
				else
				{
					var userSession = UserSessionHelper.DecodeSessionString(requestContent?.UserSession);
					if (userSession.LoggedInOrganisationPK.IsEmpty || userSession.LoggedInUserPK.IsEmpty)
					{
						result = "Invalid User.";
					}
					else
					{
						result = StlCombinedUsageReportQueue.RequestReport(userSession.LoggedInOrganisationPK, userSession.LoggedInUserPK, dbPk, period, userSession.SupportStaffCode.ToUpper()) ? "OK" : "ERROR";
					}
				}

				return result;
			}
		}

		ZDateTime GetReportPeriodStart(string year, string month)
		{
			ZDateTime periodStart = ZDateTime.Empty;

			int selectedYear = 0;
			if (!string.IsNullOrEmpty(year))
			{
				selectedYear = int.Parse(year, CultureInfo.InvariantCulture);
			}

			int selectedMonth = 0;
			if (!string.IsNullOrEmpty(month))
			{
				selectedMonth = int.Parse(month, CultureInfo.InvariantCulture);
			}

			if (selectedYear > 0 && selectedMonth > 0)
			{
				periodStart = new ZDateTime(selectedYear, selectedMonth, 1);
			}

			return periodStart;
		}

		public class RequestContent
		{
			public string UserSession { get; set; }
			public string Product { get; set; }
		}
	}
}
