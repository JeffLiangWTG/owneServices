using System.Net;

namespace Enterprise.CustomerService.MyAccountLogin
{
	public interface ILoginService
	{
		string Url { get; set; }
		int Timeout { get; set; }
		IWebProxy Proxy { get; set; }

		string GetAutoLoginUrlWithReturnUrl(string staffSecuredQueryString, string url);
	}
}
