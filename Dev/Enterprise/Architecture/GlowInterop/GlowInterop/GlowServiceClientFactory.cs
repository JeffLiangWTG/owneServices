using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using CargoWise.Authentication.Glow.Client;
using CargoWise.Authentication.Primitives;
using CargoWise.Common;
using Enterprise.Environment;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public interface IGlowServiceClientFactory : IGlowUserDataManager, IDisposable
	{
		IGlowServiceClient Create(Uri baseUri);
	}

	class GlowServiceClientFactory : IGlowServiceClientFactory
	{
		public GlowServiceClientFactory()
			: this(null, null)
		{
		}

		public GlowServiceClientFactory(HttpClientHandler innerHandler, ConcurrentDictionary<string, (IUserSession, IAuthenticationController)> userSessionAndRelatedControllers)
		{
			this.innerHandler = innerHandler ?? new HttpClientHandler();
			clientFactory = new WTG.Foundation.Http.HttpClientFactory(() => this.innerHandler);
			this.userSessionAndRelatedControllers = userSessionAndRelatedControllers ?? new ConcurrentDictionary<string, (IUserSession UserSession, IAuthenticationController AuthenticationController)>();
			clientDomains = new ConcurrentHashSet<Uri>();
			tempUserCount = 0;
		}

		readonly HttpClientHandler innerHandler;
		readonly WTG.Foundation.Http.IHttpClientFactory clientFactory;

		readonly ConcurrentDictionary<string, (IUserSession UserSession, IAuthenticationController AuthenticationController)> userSessionAndRelatedControllers;
		readonly ConcurrentHashSet<Uri> clientDomains;
		int tempUserCount;

		public IGlowServiceClient Create(Uri baseUri)
		{
			clientDomains.TryAdd(baseUri);

			var key = $"{Env.CurrentUserPK}_{Env.CurrentBranchPK}_{Env.CurrentDepartmentPK}";
			var persistToFile = tempUserCount == 0;

			var (userSession, controller) = userSessionAndRelatedControllers.GetOrAdd(persistToFile ? "ActualUserKey" : key, id =>
			{
				var session = new UserSession(SessionType.General);
				var controller = new PersistentSingleSignOnAuthenticationController(
					session,
					new UserEnv { UserPK = Env.CurrentUserPK, Branch = Env.CurrentBranchPK, Department = Env.CurrentDepartmentPK },
					persistToFile);

				return (session, controller);
			});
			return new GlowServiceClient(baseUri, innerHandler, clientFactory, userSession, controller);
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "We expect ClearUserData to only be called during logout/app exit and no clients to be in use at that time")]
		public void ClearUserData()
		{
			var domainsToExpire = clientDomains.ToArray();
			foreach (var domain in domainsToExpire)
			{
				foreach (Cookie cookie in innerHandler.CookieContainer.GetCookies(domain))
				{
					cookie.Expired = true;
				}

				clientDomains.TryRemove(domain);
			}

			userSessionAndRelatedControllers.Clear();
		}

		public void Dispose()
		{
			innerHandler.Dispose();
			clientFactory.Dispose();
		}

		// Temporary user count is used to determine if we should persist the user session to file or not.
		// If there are no temporary users, we persist the session to file.
		// Temporary master should be considered as actual user and persist to file. We assume that SetTemporaryMasterUserContext is only called once before any other operations.
		public IDisposable IncreaseTempUserCount()
		{
			Interlocked.Increment(ref tempUserCount);
			return new DisposableAction(() =>
			{
				Interlocked.Decrement(ref tempUserCount);
			});
		}
	}
}
