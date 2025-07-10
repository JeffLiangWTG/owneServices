using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.ComponentModel;
using Enterprise.Integration;
using Enterprise.Registry.Business.eServices;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public static class OAuth2ConnectFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int MinCacheTimeSeconds = 10;

		public async static Task<(AuthToken token, bool isCached)> GetToken(IOAuth2Parameters oAuth2Parameters, INotifications notifier, CancellationToken cancellationToken)
		{
			object cachedValue = Cache.Get(oAuth2Parameters.CachingKey);
			if (cachedValue is AuthToken token)
			{
				return (token, true);
			}

			notifier.Add(new Notification(NotificationType.Information, eAdaptorNextLogs.NoTokenInCache()));

			AuthToken authToken;
			try
			{
				authToken = await OAuth2Connect.RequestAuthToken(oAuth2Parameters, cancellationToken).ConfigureAwait(false);
			}
			catch (OAuth2Exception ex)
			{
				notifier.AddError(eAdaptorNextLogs.ErrorOccurredWhenClaimingToken(ex.Message));
				throw;
			}

			var absoluteExpiration = DateTimeOffset.Now.AddSeconds(authToken.ExpiresIn - MinCacheTimeSeconds);
			notifier.Add(new Notification(NotificationType.Information, eAdaptorNextLogs.NewToken(absoluteExpiration.ToString())));

			if (authToken.ExpiresIn > MinCacheTimeSeconds)
			{
				var policy = new CacheItemPolicy();
				policy.AbsoluteExpiration = absoluteExpiration;
				Cache.Add(new CacheItem(oAuth2Parameters.CachingKey, authToken), policy);
			}

			return (authToken, false);
		}

		async static Task<AuthToken> GenerateToken(CancellationToken cancellationToken)
		{
			var config = eAdaptorRegistry.Instance.eAdaptorNextOutbound.Value;
			if (config == null || !config.IsOAuth2Enabled)
			{
				throw new ArgumentException("eAdaptorNextOutbound is not enabled.");
			}
			return await OAuth2Connect.RequestAuthToken(config, cancellationToken).ConfigureAwait(false);
		}

		public static void ClearTokens(IOAuth2Parameters oAuth2Parameters)
		{
			Cache.Remove(oAuth2Parameters.CachingKey);
		}

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static readonly object initLock = new object();

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static MemoryCache cache;

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static MemoryCache Cache
		{
			get
			{
				if (cache == null)
				{
					lock (initLock)
					{
						if (cache == null)
						{
							var cacheSettings = new NameValueCollection(1)
							{
								{ "CacheMemoryLimitMegabytes", "16" }
							};

							cache = new MemoryCache(nameof(AuthToken), cacheSettings);
						}
					}
				}
				return cache;
			}
		}
	}
}
