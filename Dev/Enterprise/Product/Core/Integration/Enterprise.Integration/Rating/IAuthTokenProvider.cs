using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AuthenticationService.Client.Models;

namespace Enterprise.Integration.Rating
{
	public interface IAuthTokenProvider
	{
		string ClientID { get; }
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		string AuthServiceURL { get; }

		/// <summary>
		///		Generates an authentication token which will be valid for at least <paramref name="secondsBeforeTokenExpiry"/> seconds.
		/// </summary>
		/// <param name="secondsBeforeTokenExpiry">
		///		The time in seconds before the token will be expired. For slow operations the value should be substantial (for example 10 minutes).
		/// </param>
		/// <param name="useEnvironmentLoginInfoFromConstructor">
		///		Use the environment details (e.g. Env.CurrentCompany) from the time of construction, rather than when GetToken is called.
		/// </param>
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		(string Token, string ValidationMessage) GetToken(string correlationID, int secondsBeforeTokenExpiry = 30, Action<LoginInfo> overrideLoginInfo = null, CancellationToken cancellationToken = default, bool useEnvironmentLoginInfoFromConstructor = false);
	}
}