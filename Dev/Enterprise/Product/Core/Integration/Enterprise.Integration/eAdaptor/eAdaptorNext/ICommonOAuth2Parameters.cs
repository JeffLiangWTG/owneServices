
using System.Collections.Generic;

namespace Enterprise.Integration
{
	public interface ICommonOAuth2Parameters
	{
		string CachingKey { get; }
		string AuthorizationURL { get; }
		string FlowCode { get; }
		string ClientID { get; }
		IEnumerable<string> Scopes { get; }
	}
}
