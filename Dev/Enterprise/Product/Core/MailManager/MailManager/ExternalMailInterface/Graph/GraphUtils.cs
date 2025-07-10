using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Enterprise.Environment;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Enterprise.MailManager.ExternalMailInterface
{
	static class GraphUtils
	{
		#region TaskExtensions

		internal static T GetResultByAwaiter<T>(this Task<T> task, bool continueOnCapturedContext = false) => task == null ? default : task.ConfigureAwait(continueOnCapturedContext).GetAwaiter().GetResult();

		internal static void GetResultByAwaiter(this Task task, bool continueOnCapturedContext = false)
		{
			if (task != null)
			{
				task.ConfigureAwait(continueOnCapturedContext).GetAwaiter().GetResult();
			}
		}

		#endregion

		#region Graph API

		internal static GraphServiceClient GetGraphServiceClient(this AuthenticationResult authResult, Action<string> logAction = null)
		{
			if (logAction != null)
			{
				var authProvider = authResult.GetAuthenticationProvider();
				var handlers = GraphClientFactory.CreateDefaultHandlers(authProvider);
				handlers.Add(new GraphRequestsLoggingHandler(logAction));

				var httpClient = GraphClientFactory.Create(handlers);
				return new GraphServiceClient(httpClient);
			}
			else
			{
				return new GraphServiceClient(authResult.GetAuthenticationProvider());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		internal static IAuthenticationProvider GetAuthenticationProvider(this AuthenticationResult authResult)
		{
			return new DelegateAuthenticationProvider(r =>
			{
				r.Headers.Authorization = new AuthenticationHeaderValue("bearer", authResult.AccessToken);
				r.Version = new Version("1.1");
				return Task.CompletedTask;
			});
		}

		internal static bool UseUserToken(byte[] userToken)
		{
			return userToken is { Length: > 0 };
		}

		internal static IUserRequestBuilder GetUserRequestBuilder(GraphServiceClient graphClient, Ms365OAuth2Configuration configuration, string mailboxEmailAddress = null)
		{
			if (configuration.IsUserToken)
			{
				return graphClient.Me;
			}

			if (string.IsNullOrEmpty(mailboxEmailAddress))
			{
				mailboxEmailAddress = Env.Registry.MailboxEmailAddress;
			}
			return graphClient.Users[mailboxEmailAddress];
		}

		internal static string GetValue(this IMessageSingleValueExtendedPropertiesCollectionPage singleValueExtendedProperties, string propertyId)
		{
			return singleValueExtendedProperties?.FirstOrDefault(p => p.Id.Equals(propertyId, StringComparison.OrdinalIgnoreCase))?.Value;
		}

		#endregion
	}
}
