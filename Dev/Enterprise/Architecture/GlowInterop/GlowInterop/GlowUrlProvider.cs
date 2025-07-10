using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Registry.Business;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public class GlowUrlProvider : IGlowUrlProvider
	{
		public GlowUrlProvider(INotifications notifications)
		{
			Notifications = Argument.NotNull(notifications, nameof(notifications));
		}

		INotifications Notifications { get; }

		public Uri TryGenerateUrl(string endpoint)
		{
			return TryGenerateUrl(endpoint, ResString.GetMultilingualString("6d92d45b-03e4-4250-847a-bca64f568813", "module"));
		}

		public Uri TryGenerateUrl(string endpoint, string jobDescription)
		{
			return TryGenerateUrl(endpoint, jobDescription, null);
		}

		public Uri TryGenerateUrl(string endpoint, string jobDescription, IEnumerable<(string Name, string Value)> additionalQueryStrings)
		{
			Argument.NotNullOrEmpty(endpoint, nameof(endpoint));
			Argument.NotNull(jobDescription, nameof(jobDescription));

			var baseURL = VerifyGlowRegistry(jobDescription);

			if (baseURL.IsNullOrEmpty())
			{
				return null;
			}

			return UrlBuilder.GenerateURL(new Uri(baseURL), endpoint, additionalQueryStrings: additionalQueryStrings);
		}

		public Uri TryGenerateUrlSpecificBranch(string endpoint, string jobDescription, IEnumerable<(string Name, string Value)> additionalQueryStrings, Guid branch)
		{
			Argument.NotNullOrEmpty(endpoint, nameof(endpoint));
			Argument.NotNull(jobDescription, nameof(jobDescription));

			var baseURL = VerifyGlowRegistry(jobDescription);

			if (baseURL.IsNullOrEmpty())
			{
				return null;
			}

			return UrlBuilder.GenerateURL(new Uri(baseURL), endpoint, additionalQueryStrings: additionalQueryStrings, branch: branch);
		}

		string VerifyGlowRegistry(string jobDescription)
		{
			var baseURL = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			if (string.IsNullOrWhiteSpace(baseURL))
			{
				var errorMessage = ResString.GetMultilingualString("12c6b55f-d60d-429a-a9e0-3bac17f0af63",
@"This {0} cannot be opened in a browser as GLOW has not been configured for this client.
Registry: {1}/{2}", jobDescription, GlowRegistry.Instance.GlowPortalsUri.Category, GlowRegistry.Instance.GlowPortalsUri.Caption);
				Notifications.AddError(errorMessage);
				return null;
			}

			return baseURL;
		}
	}
}
