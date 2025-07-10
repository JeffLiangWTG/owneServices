using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Security
{
	class SecurityIteratorNodeContext<T>
	{
		public SecurityIteratorNodeContext(SecurityIteratorNodeContext<T> prevCtx, IEnumerable<IGlbSecurity> securities)
		{
			if (prevCtx == null || prevCtx.Securities == null)
			{
				if (securities != null)
				{
					securityList = GetSecurityList(securities);
				}
			}
			else if (securities == null)
			{
				securityList = prevCtx.securityList;
			}
			else
			{
				securityList = GetSecurityList(securities, FilterPreviousSecurities(prevCtx.securityList, securities));
			}
		}

		public IEnumerable<IGlbSecurity> Securities
		{
			get { return securityList; }
		}
		readonly List<IGlbSecurity> securityList;

		public T Summary { get; set; }

		static IEnumerable<IGlbSecurity> FilterPreviousSecurities(IEnumerable<IGlbSecurity> previousSecurities, IEnumerable<IGlbSecurity> securities)
		{
			var comparer = new CompanyBranchDepartmentWithStaffOrGroupComparer();
			return previousSecurities.Where(previousSecurity => !securities.Contains(previousSecurity, comparer));
		}

		List<IGlbSecurity> GetSecurityList(params IEnumerable<IGlbSecurity>[] securitiesArray)
		{
			List<IGlbSecurity> list = new List<IGlbSecurity>();

			var comparer = new CompanyBranchDepartmentComparer();
			foreach (var securities in securitiesArray)
			{
				foreach (var security in securities.Where(s => s.GU_GS.IsValid).OrderByDescending(s => s.GU_SecurityItemIsAllowed))
				{
					if (!list.Contains(security, comparer))
					{
						list.Add(security);
					}
				}
			}

			var comparerForGroupWithEmptyStaff = new CompanyBranchDepartmentComparerForGroupWithEmptyStaff();
			foreach (var securities in securitiesArray)
			{
				foreach (var security in securities.Where(s => s.GU_GG.IsValid).OrderByDescending(s => s.GU_SecurityItemIsAllowed))
				{
					if (!list.Contains(security, comparer) ||
						(!list.Contains(security, comparerForGroupWithEmptyStaff) && security.GU_SecurityItemIsAllowed)) // Force copying granted security for different group
					{
						list.Add(security);
					}
				}
			}

			return list.Count > 0 ? list : null;
		}

		class CompanyBranchDepartmentComparer : IEqualityComparer<IGlbSecurity>
		{
			public bool Equals(IGlbSecurity x, IGlbSecurity y)
			{
				return x.GU_GC == y.GU_GC && x.GU_GB == y.GU_GB && x.GU_GE == y.GU_GE;
			}

			public int GetHashCode(IGlbSecurity obj)
			{
				return obj.GU_GC.GetHashCode() ^ obj.GU_GB.GetHashCode() ^ obj.GU_GE.GetHashCode();
			}
		}

		class CompanyBranchDepartmentComparerForGroupWithEmptyStaff : IEqualityComparer<IGlbSecurity>
		{
			public bool Equals(IGlbSecurity x, IGlbSecurity y)
			{
				return x.GU_GC == y.GU_GC && x.GU_GB == y.GU_GB && x.GU_GE == y.GU_GE && (x.GU_GG == y.GU_GG || !x.GU_GS.IsEmpty || !y.GU_GS.IsEmpty);
			}

			public int GetHashCode(IGlbSecurity obj)
			{
				return obj.GU_GC.GetHashCode() ^ obj.GU_GB.GetHashCode() ^ obj.GU_GE.GetHashCode();
			}
		}

		class CompanyBranchDepartmentWithStaffOrGroupComparer : IEqualityComparer<IGlbSecurity>
		{
			public bool Equals(IGlbSecurity x, IGlbSecurity y)
			{
				return x.GU_GC == y.GU_GC && x.GU_GB == y.GU_GB && x.GU_GE == y.GU_GE &&
					(x.GU_GS.IsValid && x.GU_GS == y.GU_GS || x.GU_GG.IsValid && x.GU_GG == y.GU_GG);
			}

			public int GetHashCode(IGlbSecurity obj)
			{
				return obj.GU_GC.GetHashCode() ^ obj.GU_GB.GetHashCode() ^ obj.GU_GE.GetHashCode() ^ obj.GU_GS.GetHashCode() ^ obj.GU_GG.GetHashCode();
			}
		}
	}
}
