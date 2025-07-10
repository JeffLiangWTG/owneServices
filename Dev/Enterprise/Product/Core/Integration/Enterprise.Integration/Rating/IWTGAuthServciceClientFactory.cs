using System.Diagnostics.CodeAnalysis;
using AuthenticationService.Client;

namespace Enterprise.Integration.Rating
{
	public interface IWTGAuthServciceClientFactory
	{
		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		IWTGAuthServiceClient Create(string serviceURL);
	}
}
