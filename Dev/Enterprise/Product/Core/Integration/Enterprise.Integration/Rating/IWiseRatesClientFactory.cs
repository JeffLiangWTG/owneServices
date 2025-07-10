using System.Diagnostics.CodeAnalysis;
using System.Threading;
using WiseRates.Api.Client;

namespace Enterprise.Integration.Rating
{
	public interface IWiseRatesClientFactory
	{
		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "I better know what I need in each particular case, don't order me what to do")]
		(IWiseRatesClient client, string failureMessage) TryCreate(string correlationID, int secondsBeforeTokenExpiry = 30, CancellationToken cancellationToken = default, ILogger logger = null);
	}
}