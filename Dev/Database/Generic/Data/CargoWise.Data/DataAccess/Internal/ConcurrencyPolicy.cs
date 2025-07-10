using System.ComponentModel;
using System.Data;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework
{
	[Immutable]
	[ImmutableObject(true)]
	public class ConcurrencyPolicy
	{
		public static readonly ConcurrencyPolicy Default = new ConcurrencyPolicy(true, CollisionCheck.Always);      // NotifyAndMerge
		public static readonly ConcurrencyPolicy Strict = new ConcurrencyPolicy(false, CollisionCheck.Always);      // Notify
		public static readonly ConcurrencyPolicy Ignore = new ConcurrencyPolicy(true, CollisionCheck.Never);        // OverwriteOtherUser
		public static readonly ConcurrencyPolicy Observe = new ConcurrencyPolicy(true, CollisionCheck.OnChange);    // NotifyAndMergeOnlyIfChangedInThisBusinessObject
		public static readonly ConcurrencyPolicy Protect = new ConcurrencyPolicy(false, CollisionCheck.OnChange);   // NotifyOnlyIfChangedInThisBusinessObject
		public static readonly ConcurrencyPolicy DefaultWithoutDatabaseMerge = new ConcurrencyPolicy(true, CollisionCheck.Always, false);    // NotifyAndMergeWithoutDatabaseAutomaticMerge

		readonly bool allowMerge = true;
		readonly CollisionCheck collisionCheck = CollisionCheck.Always;
		readonly bool allowDatabaseMerge = true;

		protected ConcurrencyPolicy()
		{ }

		protected ConcurrencyPolicy(bool allowMerge, CollisionCheck collisionCheck)
		{
			this.allowMerge = allowMerge;
			this.collisionCheck = collisionCheck;
			this.allowDatabaseMerge = allowMerge;
		}

		protected ConcurrencyPolicy(bool allowMerge, CollisionCheck collisionCheck, bool allowDatabaseMerge)
		{
			this.allowMerge = allowMerge;
			this.collisionCheck = collisionCheck;
			this.allowDatabaseMerge = allowDatabaseMerge;
		}

		public bool AllowMerge
		{
			get { return allowMerge; }
		}

		public virtual bool AllowAutomaticMergeIfDatabaseValuesAreEqual(DataRow row, DataColumn column)
		{
			return allowDatabaseMerge;
		}

		public CollisionCheck CollisionCheck
		{
			get { return collisionCheck; }
		}

		#region SuppressResourceStringsCheckRegion
		public string Strategy()
		{
			if (ConcurrencyPolicy.Default == this)
			{
				return "Default - NotifyAndMerge";
			}
			else if (ConcurrencyPolicy.Strict == this)
			{
				return "Strict - Notify";
			}
			else if (ConcurrencyPolicy.Ignore == this)
			{
				return "Ignore - OverwriteOtherUser";
			}
			else if (ConcurrencyPolicy.Observe == this)
			{
				return "Observe - NotifyAndMergeOnlyIfChangedInThisBusinessObject";
			}
			else if (ConcurrencyPolicy.Protect == this)
			{
				return "Protect - NotifyOnlyIfChangedInThisBusinessObject";
			}
			if (ConcurrencyPolicy.DefaultWithoutDatabaseMerge == this)
			{
				return "Overwrite - OverwriteOtherUserViaApplicationConcurrencyMergeHandling";
			}
			return string.Empty;
		}
		#endregion

		public virtual bool IsSilent
		{
			get { return CollisionCheck == CollisionCheck.Never; }
		}

		public virtual bool ShouldCheck(DataRow row, DataColumn column)
		{
#if DEBUG
			if (SuppressConcurrencyPolicyForTestPurposes)
			{
				return false;
			}
#endif

			return
				collisionCheck == CollisionCheck.Always ||
				collisionCheck == CollisionCheck.OnChange && (row.RowState == DataRowState.Deleted || HasChanged(row, column));
		}

#if DEBUG
		public static bool SuppressConcurrencyPolicyForTestPurposes;
#endif

		public static bool HasChanged(DataRow row, DataColumn column)
		{
			return !row[column, DataRowVersion.Original].Equals(row[column, DataRowVersion.Current]);
		}

		public override string ToString()
		{
			return Strategy();
		}
	}
}
