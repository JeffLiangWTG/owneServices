using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security
{
	class SecurityCache
	{
		public SecurityCache(GlbSecurityCollection securities, SecurityCore securityCore)
		{
			LocalAdministratorLookupKey = securityCore.StaffLocalAdministratorPlaceholder.LookupKey;
			PopulateDictionary(securities);
		}

		public IEnumerable<IGlbSecurity> this[ISecurityCheckpoint checkpoint, GlbStaff staff]
		{
			get { return this[checkpoint.LookupKey, staff]; }
		}

		public IEnumerable<IGlbSecurity> this[CheckpointLookupKey lookupKey, GlbStaff staff]
		{
			get
			{
				SecuritiesCacheElement securitiesByTargetPK;
				if (securitiesDictionary.TryGetValue(lookupKey, out securitiesByTargetPK))
				{
					return securitiesByTargetPK.GetSecurities(staff);
				}
				else
				{
					return null;
				}
			}
		}

		public CheckpointLookupKey LocalAdministratorLookupKey { get; private set; }

		void PopulateDictionary(GlbSecurityCollection securities)
		{
			securitiesDictionary = new Dictionary<CheckpointLookupKey, SecuritiesCacheElement>();
			foreach (GlbSecurity glbSecurity in securities)
			{
				CheckpointLookupKey key;
				if (String.Equals(glbSecurity.GU_SecurityRight, GlbSecurity.ChangeOtherStaffSecurityRightName, StringComparison.OrdinalIgnoreCase) ||
					String.Equals(glbSecurity.GU_SecurityRight, GlbSecurity.ChangeOtherGroupSecurityRightName, StringComparison.OrdinalIgnoreCase))
				{
					key = LocalAdministratorLookupKey;
				}
				else if (glbSecurity.GU_ItemGUID.IsValid)
				{
					key = new CheckpointLookupKey(glbSecurity.GU_SecurityRight, glbSecurity.GU_ItemGUID.ToGuid());
				}
				else
				{
					key = new CheckpointLookupKey(glbSecurity.GU_SecurityRight);
				}
				SecuritiesCacheElement securitiesByTargetPK;
				if (!securitiesDictionary.TryGetValue(key, out securitiesByTargetPK))
				{
					securitiesByTargetPK = new SecuritiesCacheElement();
					securitiesDictionary.Add(key, securitiesByTargetPK);
				}

				if (glbSecurity.GU_GS.IsValid)
				{
					securitiesByTargetPK.AddStaffSecurity(glbSecurity.GU_GS, glbSecurity);
				}
				else if (glbSecurity.GU_GG.IsValid)
				{
					securitiesByTargetPK.AddGroupSecurity(glbSecurity.GU_GG, glbSecurity);
				}
			}
		}

		class SecuritiesCacheElement
		{
			public IEnumerable<IGlbSecurity> GetSecurities(GlbStaff staff)
			{
				IEnumerable<IGlbSecurity> result = null;
				if (staffSecurities != null)
				{
					List<IGlbSecurity> list;
					if (staffSecurities.TryGetValue(staff.PK, out list))
					{
						result = list;
					}
				}

				if (groupSecurities != null)
				{
					foreach (GlbGroup group in staff.ActiveGroups)
					{
						foreach (var kv in groupSecurities)
						{
							if (group.PK == kv.Key)
							{
								result = result == null ? kv.Value : result.Union(kv.Value);
							}
						}
					}
				}

				return result;
			}

			public void AddStaffSecurity(ZGuid staffPK, IGlbSecurity glbSecurity)
			{
				AddSecurity(ref staffSecurities, staffPK, glbSecurity);
			}

			public void AddGroupSecurity(ZGuid groupPK, IGlbSecurity glbSecurity)
			{
				AddSecurity(ref groupSecurities, groupPK, glbSecurity);
			}

			static void AddSecurity(ref Dictionary<ZGuid, List<IGlbSecurity>> dict, ZGuid targetPK, IGlbSecurity glbSecurity)
			{
				if (dict == null)
				{
					dict = new Dictionary<ZGuid, List<IGlbSecurity>>();
				}

				List<IGlbSecurity> list;
				if (!dict.TryGetValue(targetPK, out list))
				{
					list = new List<IGlbSecurity>();
					dict.Add(targetPK, list);
				}

				list.Add(glbSecurity);
			}

			Dictionary<ZGuid, List<IGlbSecurity>> staffSecurities;
			Dictionary<ZGuid, List<IGlbSecurity>> groupSecurities;
		}

		Dictionary<CheckpointLookupKey, SecuritiesCacheElement> securitiesDictionary;
	}
}
