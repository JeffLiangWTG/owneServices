using System;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.Application;
using CargoWise.Common.Cache;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business
{
	public class CommunicationPartyConfigCache : ICommunicationPartyConfigCache
	{
		static readonly int CACHE_SIZE_MAX_MB = 16;
		static readonly TimeSpan CACHE_EXPIRATION_TIME_MINUTES = TimeSpan.FromMinutes(5);

		public virtual DateTimeOffset GetExpirationDate()
		{
			return DateTimeOffset.Now.Add(CACHE_EXPIRATION_TIME_MINUTES);
		}

		readonly MemoryCache _inboundMemoryCache;
		readonly MemoryCache _outboundMemoryCache;

		public bool TryGetInboundCommunicationPartyConfigByClientId(ZString communicationAuthClientId, ZString communicationAuthEndpoint, string applicationCode, out IEDICommunicationPartyConfig communicationPartyConfig)
		{
			var query = GetInboundEDICommunicationPartyConfigSchemaQuery(applicationCode);

			var endpointQuery = new ZQuery();
			endpointQuery.AddToFilter(JoinCondition.Or, EDICommunicationAuthSchema.ECA_AuthorizationEndpoint, communicationAuthEndpoint.TrimEnd('/'));
			endpointQuery.AddToFilter(JoinCondition.Or, EDICommunicationAuthSchema.ECA_AuthorizationEndpoint, $"{communicationAuthEndpoint}/");

			var subquery = new ZDBOnlySubQuery(typeof(EDICommunicationAuth), EDICommunicationAuthSchema.PK);
			subquery.AddToFilter(EDICommunicationAuthSchema.ECA_ClientID, communicationAuthClientId);
			subquery.AddToFilter(endpointQuery);
			subquery.AddToFilter(EDICommunicationAuthSchema.ECA_AuthorizationMode, EDICommunicationAuthModesList.Codes.OAuthAuthentication);
			query.AddSubQuery(EDICommunicationPartyConfigSchema.ECC_ECA_Auth, subquery, JoinCondition.And);

			var cacheKey = $"{communicationAuthClientId}_{communicationAuthEndpoint}";

			communicationPartyConfig = GetCommunicationPartyConfig(cacheKey, _inboundMemoryCache, query);
			return communicationPartyConfig != null;
		}

		public bool TryGetInboundCommunicationPartyConfigByUsername(ZString username, string applicationCode, out IEDICommunicationPartyConfig communicationPartyConfig)
		{
			var query = GetInboundEDICommunicationPartyConfigSchemaQuery(applicationCode);

			var subquery = new ZDBOnlySubQuery(typeof(EDICommunicationAuth), EDICommunicationAuthSchema.PK);
			subquery.AddToFilter(EDICommunicationAuthSchema.ECA_Username, username);
			subquery.AddToFilter(EDICommunicationAuthSchema.ECA_AuthorizationMode, EDICommunicationAuthModesList.Codes.BasicAuthentication);
			query.AddSubQuery(EDICommunicationPartyConfigSchema.ECC_ECA_Auth, subquery, JoinCondition.And);

			var cacheKey = $"{username}";

			communicationPartyConfig = GetCommunicationPartyConfig(cacheKey, _inboundMemoryCache, query);
			return communicationPartyConfig != null;
		}

		public bool TryGetOutboundCommunicationPartyConfig(ZGuid communicationPartyConfigPk, out IEDICommunicationPartyConfig communicationPartyConfig)
		{
			var query = GetEDICommunicationPartyConfigSchemaQuery(EDICommunicationPartyConfigDirectionsList.Codes.Outbound);
			var partySubQuery = GetEDICommunicationPartySchemaSubQuery();
			query.AddSubQuery(partySubQuery, JoinCondition.And);

			query.AddToFilter(EDICommunicationPartyConfigSchema.PK, communicationPartyConfigPk);
			var subquery = new ZDBOnlySubQuery(typeof(EDICommunicationAuth), EDICommunicationAuthSchema.PK);
			query.AddSubQuery(EDICommunicationPartyConfigSchema.ECC_ECA_Auth, subquery, JoinCondition.And);

			var cacheKey = communicationPartyConfigPk.ToString();

			communicationPartyConfig = GetCommunicationPartyConfig(cacheKey, _outboundMemoryCache, query);
			return communicationPartyConfig != null;
		}

		ZDBOnlyQuery GetInboundEDICommunicationPartyConfigSchemaQuery(string applicationCode)
		{
			var query = GetEDICommunicationPartyConfigSchemaQuery(EDICommunicationPartyConfigDirectionsList.Codes.Inbound);
			var partySubQuery = GetEDICommunicationPartySchemaSubQuery();
			partySubQuery.AddToFilter(EDICommunicationPartySchema.ECP_ApplicationCode, applicationCode);
			query.AddSubQuery(partySubQuery, JoinCondition.And);

			return query;
		}

		ZDBOnlyQuery GetEDICommunicationPartyConfigSchemaQuery(string ediCommunicationPartyConfigDirection)
		{
			var query = new ZDBOnlyQuery(typeof(EDICommunicationPartyConfig));
			query.AddToFilter(EDICommunicationPartyConfigSchema.ECC_IsActive, true);
			query.AddToFilter(EDICommunicationPartyConfigSchema.ECC_Direction, ediCommunicationPartyConfigDirection);

			return query;
		}

		ZDBOnlySubQuery GetEDICommunicationPartySchemaSubQuery()
		{
			var partySubQuery = new ZDBOnlySubQuery(typeof(EDICommunicationParty), EDICommunicationPartyConfigSchema.ECC_ECP_Party);
			partySubQuery.AddToFilter(EDICommunicationPartySchema.ECP_IsActive, true);
			return partySubQuery;
		}

		IEDICommunicationPartyConfig GetCommunicationPartyConfig(string key, MemoryCache cache, ZQuery filterQuery)
		{
			if (ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.EAdaptorNextFeature) == null)
			{
				return null;
			}

			// Note that this will cache a null if the record is not found - you can check the result and eject the key instead not desired
			return cache.GetOrAdd(key, () =>
			{
				var fetchedCommunicationPartyConfig = new ReadOnlyBusinessObjectFactory().Load<EDICommunicationPartyConfig>(filterQuery);
				return ImmutableCommunicationPartyConfig.FromFactoryObject(fetchedCommunicationPartyConfig
					.FirstOrDefault());
			}, new CacheItemPolicy()
			{
				AbsoluteExpiration = GetExpirationDate()
			});
		}

		public CommunicationPartyConfigCache()
		{
			var config = new NameValueCollection() {
				{ "CacheMemoryLimitMegabytes", CACHE_SIZE_MAX_MB.ToString() }
			};
			_inboundMemoryCache = new MemoryCache($"{GetType().Name}_Inbound", config);
			_outboundMemoryCache = new MemoryCache($"{GetType().Name}_Outbound", config);
		}
	}
}
