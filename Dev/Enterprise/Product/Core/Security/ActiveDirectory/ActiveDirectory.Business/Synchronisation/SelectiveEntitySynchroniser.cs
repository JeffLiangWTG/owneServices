using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Security.ActiveDirectory.Synchronisation
{
	public class SelectiveEntitySynchroniser : EntitySynchroniser
	{
		public SelectiveEntitySynchroniser(BusinessObjectFactory factory, IEnumerable<BusinessObject> objectsToSync)
			: base(factory)
		{
			Argument.NotNull(objectsToSync, "objectsToSync");

			this.objectsToSync = objectsToSync.ToArray();
		}

		readonly BusinessObject[] objectsToSync;

		IEnumerable<GlbStaff> Staff
		{
			get { return objectsToSync.OfType<GlbStaff>(); }
		}

		IEnumerable<GlbGroup> Groups
		{
			get { return objectsToSync.OfType<GlbGroup>(); }
		}

		#region EntitySynchroniser overrides

		protected override IEnumerable<GlbStaff> GetStaffToSync(SyncMode? preferredSyncMode)
		{
			return objectsToSync.Any() ? Staff.Where(s =>
			{
				s.ReloadSafe(); // ensure syncing latest data from DB
				return !s.GS_IsSystemAccount && (s.GS_IsActive || s.GS_ActiveDirectoryObjectGuid.IsValid);
			}).ToArray() : base.GetStaffToSync(preferredSyncMode);
		}

		protected override IEnumerable<GlbGroup> GetGroupsToSync(SyncMode? preferredSyncMode)
		{
			return objectsToSync.Any() ? Groups.Where(g =>
			{
				g.ReloadSafe(); // ensure syncing latest data from DB
				return !g.GG_IsSystemDefined && (g.GG_IsActive || g.GG_ActiveDirectoryObjectGuid.IsValid);
			}).ToArray() : base.GetGroupsToSync(preferredSyncMode);
		}

		#endregion
	}
}
