using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Security
{
	public class SecurityMap : ISecurityMap
	{
		public IEnumerable<string[]> GetStaffSecurity(GlbStaff staff)
		{
			SecurityIterator<string> iterator = new SecurityIterator<string>(new BusinessObjectFactory(), new StringSecuritySummaryGenerator(), treatLocalAdministratorCheckPointDifferently: true);
			iterator.Setup(staff);
			foreach (var summary in iterator.GetSummaries())
			{
				yield return new string[] { summary.Checkpoint.DisplayTextPathToSecurityRight, summary.Summary, summary.Explicit ? Res.GetString("234fd9da-94c4-40b3-92f2-e420c6178aa2", "Yes") : Res.GetString("d993b1a0-4ef5-4ce6-a083-37b5c6e8727b", "No") };
			}
		}

		public bool HasAccessForAllBranchesAndDepartments(BusinessObjectFactory factory, GlbSecurityCollection securities, SecurityCore securityCore, GlbStaff staff, GlbSecurity security)
		{
			return HasAccessForAllBranchesAndDepartments(factory, securities, securityCore, staff, security, security.GetCheckpointLookupKey());
		}

		public bool HasAccessForAllBranchesAndDepartments(BusinessObjectFactory factory, GlbSecurityCollection securities, SecurityCore securityCore, GlbStaff staff, GlbSecurity security, ZArchitecture.Modules.CheckpointLookupKey lookupKey)
		{
			var initialRefreshValue = factory.RefreshEnabled;
			factory.RefreshEnabled = false;
			try
			{
				CachedRightsKey key = new CachedRightsKey(staff, lookupKey);
				CachedRights rights;
				if (!cache.TryGetValue(key, out rights))
				{
					rights = new CachedRights(factory, securities, securityCore, key);
					cache.Add(key, rights);
				}

				string[] departments = null;
				string[] branches = null;
				if (security.GU_GE.IsValid)
				{
					departments = new string[] { security.DepartmentCode };
				}
				if (security.GU_GC.IsValid || security.GU_GB.IsValid)
				{
					List<ZString> branchList = new List<ZString>();
					if (security.GU_GC.IsValid)
					{
						branchList.AddRange(from b in security.Company.Branches where b.GB_IsActive select b.GB_Code);
					}
					if (security.GU_GB.IsValid && !branchList.Contains(security.BranchCode))
					{
						branchList.Add(security.BranchCode);
					}
					branches = branchList.Count > 0 ? branchList.ConvertAll(b => (string)b).ToArray() : null;
				}

				return rights.HasAccess(departments, branches);
			}
			finally
			{
				factory.RefreshEnabled = initialRefreshValue;
			}
		}

		readonly Dictionary<CachedRightsKey, CachedRights> cache = new Dictionary<CachedRightsKey, CachedRights>();

		class CachedRightsKey : IEquatable<CachedRightsKey>
		{
			public CachedRightsKey(GlbStaff staff, ZArchitecture.Modules.CheckpointLookupKey lookupKey)
			{
				Staff = staff;
				LookupKey = lookupKey;
			}

			public GlbStaff Staff { get; private set; }
			public ZArchitecture.Modules.CheckpointLookupKey LookupKey { get; private set; }

			public bool Equals(CachedRightsKey other)
			{
				return Staff.PK == other.Staff.PK && LookupKey.Equals(other.LookupKey);
			}

			public override bool Equals(object obj)
			{
				CachedRightsKey other = obj as CachedRightsKey;
				return other != null && Equals(other);
			}

			public override int GetHashCode()
			{
				return Staff.PK.GetHashCode() ^ LookupKey.GetHashCode();
			}
		}

		class CachedRights
		{
			public CachedRights(BusinessObjectFactory factory, GlbSecurityCollection securities, SecurityCore securityCore, CachedRightsKey key)
			{
				var iterator = new SecurityIterator<bool[,]>(factory, new Generator(), treatLocalAdministratorCheckPointDifferently: true);
				iterator.SetupNoInitialize(new GlbStaff[] { key.Staff }, key.LookupKey);
				foreach (var summary in iterator.GetSummaries(securities, securityCore))
				{
					var nodeRightsMatrix = summary.Summary;
					if (nodeRightsMatrix != null)
					{
						BitArray nodeRights = new BitArray(nodeRightsMatrix.Length, false);
						for (int i = 0; i < nodeRightsMatrix.GetLength(0); i++)
						{
							for (int j = 0; j < nodeRightsMatrix.GetLength(1); j++)
							{
								if (nodeRightsMatrix[i, j])
								{
									nodeRights[j * nodeRightsMatrix.GetLength(0) + i] = true;
								}
							}
						}

						if (rights == null)
						{
							rights = nodeRights;
						}
						else if (rights.Length == nodeRights.Length)
						{
							rights = rights.And(nodeRights);
						}
						else
						{
							bool previousRights = rights[0];
							rights = nodeRights;
							for (int i = 0; i < rights.Length; ++i)
							{
								rights[i] &= previousRights;
							}
						}
					}
				}
				departments = iterator.Departments;
				branches = iterator.Branches;
			}

			public bool HasAccess(IEnumerable<string> departmentsToCheck, IEnumerable<string> branchesToCheck)
			{
				if (rights != null)
				{
					IEnumerable<int> di;
					if (departmentsToCheck == null)
					{
						di = Enumerable.Range(0, departments.Length);
					}
					else
					{
						di = from dep in departmentsToCheck select Array.BinarySearch(departments, dep);
					}

					IEnumerable<int> bi;
					if (branchesToCheck == null)
					{
						bi = Enumerable.Range(0, branches.Length);
					}
					else
					{
						bi = from brn in branchesToCheck select Array.BinarySearch(branches, brn);
					}

					return (from i in di from j in bi where i >= 0 && j >= 0 select j * departments.Length + i).All(index => index >= rights.Length || rights[index]);
				}

				return true;
			}

			readonly BitArray rights;
			readonly string[] departments;
			readonly string[] branches;
		}

		class Generator : SecuritySummaryGenerator<bool[,]>
		{
			public override bool[,] GrantedValue
			{
				get { return null; }
			}

			public override bool[,] DeniedValue
			{
				get { return null; }
			}

			protected override bool[,] GenerateSummaryCore(string[] departments, string[] branches, bool[,] rights)
			{
				return rights;
			}

			protected override bool[,] GenerateLocalAdministratorSummaryCore(IEnumerable<IGlbSecurity> securities)
			{
				return null;
			}
		}
	}
}
