using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching
{
	class ProductEntityMatchingOrgCache
	{
		public ProductEntityMatchingOrgCache(BusinessObjectFactory factory)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
		}
		BusinessObjectFactory Factory { get; }
		Dictionary<int, OrgHeader> entityToOrgHeaderMapping = new Dictionary<int, OrgHeader>();

		public (IEntity entity, OrgHeader org) GetOrganisationFromEntityParent(IEntity entity)
		{
			Argument.NotNull(entity, nameof(entity));

			OrgHeader org = null;
			var orgEntity = entity.Parents.FirstOrDefault(p => p.EntityName == XmlConstants.EntityNames.OrgHeader);
			org = GetOrgHeader(orgEntity);
			return (orgEntity, org);
		}

		public void UpdateOrgHeader(IEntity entity, OrgHeader org)
		{
			Argument.NotNull(entity, nameof(entity));

			if (org != null && entity.InternalPK == Guid.Empty)
			{
				entity.InternalPK = org.PK.ToGuid();
			}
			var key = ProductEntityMatchingInterceptor.GetInstanceKey(entity);
			if (entityToOrgHeaderMapping.ContainsKey(key))
			{
				entityToOrgHeaderMapping[key] = org;
			}
			else
			{
				entityToOrgHeaderMapping.Add(key, org);
			}
		}

		public OrgHeader GetOrgHeader(IEntity orgEntity)
		{
			OrgHeader org = null;
			if (orgEntity != null)
			{
				var key = ProductEntityMatchingInterceptor.GetInstanceKey(orgEntity);
				if (!entityToOrgHeaderMapping.TryGetValue(key, out org))
				{
					var orgToMatchAgainst = new EntityToOrgMatchingConverter(Factory).Convert(orgEntity);
					org = new OrganisationMatcher(Factory, IfUnmatched.ReturnNull).GetMatchingOrganization(orgToMatchAgainst);
					if (org != null)
					{
						var matchedOrgPK = org.PK.ToGuid();
						if (orgEntity.InternalPK != matchedOrgPK)
						{
							orgEntity.InternalPK = matchedOrgPK;
						}
					}

					entityToOrgHeaderMapping.Add(key, org);
				}
			}

			return org;
		}

		public void ClearCache()
		{
			entityToOrgHeaderMapping = new Dictionary<int, OrgHeader>();
		}
	}
}
