using System.Net;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class AccreditationLinkCreator
	{
		public static string GetUrl(OrgContactWebUser webUser)
		{
			string result = "";

			if (webUser.IsLoggedIn)
			{
				SecureQueryString queryString = new SecureQueryString();
				ZGuid orgContactPK = (webUser.IsSuperUser) ? ZGuid.Missing : webUser.LoggedInUserPK;
				queryString.Add(OrgHeaderSchema.Constants.OH_Code, webUser.LoggedInOrganisation.OH_Code);
				queryString.Add(OrgContactSchema.Constants.Prefix, orgContactPK.ToString());

				string baseUrl = WebDataRegistry.Instance.WebCertificationUrl.Value;
				string encodedQueryString = WebUtility.UrlEncode(queryString.ToString());
				result = string.Format("{0}/default.aspx?{1}={2}", baseUrl.Trim().TrimEnd('/'), SecureQueryString.QueryStringKey, encodedQueryString);
			}

			return result;
		}
	}
}
