using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class BMBoardFilterForCurrentUserProvider
	{
		public static ZQuery GetQuery(params IBMSystem[] systems)
		{
			var query = new ZQuery(BMBoardSchema.MB_FS_System, systems.Select(s => s.PK).ToArray());

			var subQuery = new ZQuery();
			subQuery.AddToFilter(new ZQuery(BMBoardSchema.MB_IsPublished, true), JoinCondition.Or);

			var currentUser = GlbStaff.CurrentUser;
			if (currentUser != null)
			{
				var factory = new BusinessObjectFactory { NameForDebugging = nameof(BMBoardFilterForCurrentUserProvider) + "Current User", RefreshEnabled = false };
				factory.SuspendValidation();
				currentUser = factory.Load<GlbStaff>(currentUser.PK);
			}

			if (currentUser != null)
			{
				subQuery.AddToFilter(new ZQuery(BMBoardSchema.MB_GS_NKStaffCode, currentUser.GS_Code), JoinCondition.Or);
				subQuery.AddToFilter(new ZQuery(BMBoardSchema.MB_GG_ReleaseGroup, currentUser.Groups.Select(g => g.PK).ToArray()), JoinCondition.Or);
			}

			query.AddToFilter(subQuery, JoinCondition.And);

			return query;
		}
	}
}
