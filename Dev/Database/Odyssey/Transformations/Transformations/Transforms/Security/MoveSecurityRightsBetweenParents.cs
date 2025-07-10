using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("An abstract data transformation that will continue to be useful so long as security rights exist and change over time.")]
	abstract class MoveSecurityRightsBetweenParents : DataTransformation
	{
		/// <summary>
		/// Moves a list of security items from one parent to another by creating new GlbSecurity records for the child items
		/// such that the effective security of the security items is the same before and after (no contexts denied or approved that weren't before).
		/// To do this requires comparing parent and child security trees.
		/// Currently does everything correct except for the case of a staff who's permissions for the security used to be determined by looking at group,
		/// but under the new parent it doesn't go to group. (See the big comment in TransformSecurityRight for more info) And still strictly more correct then doing no transformation.
		/// If this happens on a group itself - we cannot in general say what the new securities would be, because it would depend on what other groups we would check (and thus on the staff member).
		/// <param name="oldParents">The parents of securityRightsToMove, sorted from eldest to youngest.</param>
		/// <param name="newParents">The parent being moved to, sorted from eldest to youngest. Currently used to 1) determine if we need to bother making security rights at all (source+dest empty). 2) Display description of the task.</param>
		/// </summary>
		public MoveSecurityRightsBetweenParents(IUpgradeManager manager, string[] oldParents, string[] newParents, params string[] securityRightsToMove)
		{
			this.securityRights = securityRightsToMove;
			this.oldParents = oldParents;
			this.newParents = newParents;
		}

		internal readonly IEnumerable<string> securityRights;
		internal readonly IEnumerable<string> oldParents;
		internal readonly IEnumerable<string> newParents;
		internal Dictionary<Guid, Guid> companyForBranch;

		public override string UserDescription
		{
			get { return string.Format(CultureInfo.InvariantCulture, "Moving {0} from {1} to {2}.", string.Join(", ", securityRights), string.Join("|", oldParents), newParents.First()); }
		}

		void InitializeDictionary()
		{
			companyForBranch = new Dictionary<Guid, Guid>();

			var command = Db.Connection.Command(getBranchCompanySql);
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var branchPK = reader.GetGuid(0);
					var companyPK = reader.GetGuid(1);

					companyForBranch.Add(branchPK, companyPK);
				}
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			InitializeDictionary();

			TransformForTable(getStaffPksSql, "GU_GS");
			TransformForTable(getGroupPksSql, "GU_GG");
		}

		protected void TransformForTable(string selectSql, string fk)
		{
			var pks = new List<Guid>();
			var command = Db.Connection.Command(selectSql);
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					pks.Add(reader.GetGuid(0));
				}
			}
			foreach (var right in securityRights)
			{
				foreach (var pk in pks)
				{
					TransformSecurityRight(right, pk, fk);
				}
			}
		}

		struct Security
		{
			public Security(bool fGU_SecurityItemIsAllowed, Guid fGU_GC, Guid fGU_GB, Guid fGU_GE, int specificity)
			{
				this.GU_SecurityItemIsAllowed = fGU_SecurityItemIsAllowed;
				this.GU_GC = fGU_GC;
				this.GU_GB = fGU_GB;
				this.GU_GE = fGU_GE;
				this.specificity = specificity;
			}

			public Security(object[] input)
			{
				this.GU_SecurityItemIsAllowed = (bool)input[0];
				this.GU_GC = input[1] is Guid ? (Guid)input[1] : Guid.Empty;
				this.GU_GB = input[2] is Guid ? (Guid)input[2] : Guid.Empty;
				this.GU_GE = input[3] is Guid ? (Guid)input[3] : Guid.Empty;
				this.specificity = (int)input[4];
			}

			public bool GU_SecurityItemIsAllowed;
			public Guid GU_GC;
			public Guid GU_GB;
			public Guid GU_GE;
			public int specificity;
		}

		/*
The logic of how security trees and security inheritance works is not 100% obvious, so let me explain it here:

*   -> *   -> *
DUS -> CHI -> CEA
C   -> B   -> D

a security is either

-*|0 all companies
-C/1 a company
-B/2 a branch of a company
-C+D/3 a department while under a company
-B+D/4 a department while under a branch of a company

if you are DUS-CHI-CEA and all above securities are to do with DUS-CHI-CEA, this is the order of precedence they have.

for a security item, the most specific right applies. else if there are none applicable, the most specific right in the parent applies. and so on.

if child has *|0:
don't copy over anything from parent.
for each child C/1:
don't copy over anything from parent that is that C or under it (C, B belonging to C, C+D, B+D belonging to C)
for each child B/2:
don't copy over anything from parent that is that B or under it (B, C+D owning B, B+D)
for each child C+D/3:
don't copy over anything from parent that is that C+D or under it (C+D, B+D belonging to C)
for each child B+D/4:
don't copy over anything from parent that is that B+D
		*/

		IEnumerable<Security> ParentSecuritiesVisibleInChild(IEnumerable<Security> oldParentRows, IEnumerable<Security> childRows)
		{
			var toRemove = new List<Security>();
			var parentRows = new List<Security>(oldParentRows); //so we don't clobber data sent to us!

			foreach (var child in childRows)
			{
				if (child.specificity == 0) //*
				{
					return new List<Security>();
				}
				else if (child.specificity == 1) //C
				{
					foreach (var parent in parentRows.SkipWhile(x => x.specificity < 1))
					{
						if (parent.GU_GC == child.GU_GC
							|| (parent.GU_GB != Guid.Empty && companyForBranch[parent.GU_GB] == child.GU_GC))
						{
							toRemove.Add(parent);
						}
					}
					foreach (var remove in toRemove)
					{
						parentRows.Remove(remove);
					}
					toRemove.Clear();
				}
				else if (child.specificity == 2) //B
				{
					foreach (var parent in parentRows.SkipWhile(x => x.specificity < 2))
					{
						if (parent.GU_GB == child.GU_GB
							|| parent.GU_GE != Guid.Empty && companyForBranch[child.GU_GB] == parent.GU_GC)
						{
							toRemove.Add(parent);
						}
					}
					foreach (var remove in toRemove)
					{
						parentRows.Remove(remove);
					}
					toRemove.Clear();
				}
				else if (child.specificity == 3) //C+D
				{
					foreach (var parent in parentRows.SkipWhile(x => x.specificity < 3))
					{
						if (parent.GU_GE == child.GU_GE &&
							(parent.GU_GC == child.GU_GC
							|| (parent.GU_GB != Guid.Empty && companyForBranch[parent.GU_GB] == child.GU_GC)))
						{
							toRemove.Add(parent);
						}
					}
					foreach (var remove in toRemove)
					{
						parentRows.Remove(remove);
					}
					toRemove.Clear();
				}
				else if (child.specificity == 4) //B+D
				{
					foreach (var parent in parentRows.SkipWhile(x => x.specificity < 4))
					{
						if (parent.GU_GE == child.GU_GE &&
							parent.GU_GB == child.GU_GB)
						{
							toRemove.Add(parent);
						}
					}
					foreach (var remove in toRemove)
					{
						parentRows.Remove(remove);
					}
					toRemove.Clear();
				}
			}

			return parentRows;
		}

		IEnumerable<Security> MergeParentAndChild(IEnumerable<Security> parentRows, IEnumerable<Security> childRows)
		{
			var visible = ParentSecuritiesVisibleInChild(parentRows, childRows);
			var result = new List<Security>(childRows);
			result.AddRange(visible);
			return result.OrderBy(x => x.specificity);
		}

		List<Security> RowsForSecurityRight(string sql, Guid staffOrGroupPK, string right, string fk)
		{
			var result = new List<Security>();

			var command = Db.Connection.Command(string.Format(sql, staffOrGroupPK, right, fk));
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var row = new object[5];
					reader.GetValues(row);
					result.Add(new Security(row));
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1110:DoNotUseColumnNamesDirectly", Justification = "Baseline")]
		void TransformSecurityRight(string securityRight, Guid staffOrGroupPK, string fk)
		{
			//1) for each parent, get all securities for that parent
			var rowsForEachParent = new List<IEnumerable<Security>>();
			foreach (string oldParent in oldParents)
			{
				rowsForEachParent.Add(RowsForSecurityRight(selectSql, staffOrGroupPK, oldParent, fk));
			}

			//and the child!
			var childRows = RowsForSecurityRight(selectSql, staffOrGroupPK, securityRight, fk);

			//1.5) sanity check - if the oldParent hierarchy + child is devoid of rights, and so is the newParent hierarchy, nothing is needed to do here
			if (rowsForEachParent.Sum(x => x.Count()) == 0)
			{
				var rowsForEachNewParent = new List<IEnumerable<Security>>();
				foreach (string newParent in newParents)
				{
					rowsForEachNewParent.Add(RowsForSecurityRight(selectSql, staffOrGroupPK, newParent, fk));
				}

				if (rowsForEachNewParent.Sum(x => x.Count()) == 0)
				{
					return;
				}
			}

			//2) repeatedly merge a grandparent and parent (merge all visible rights from the grandparent into the parent) until only one is left
			//For groups, we can assume a base case *-*-* Y because if you're searching groups for a security and fall off the top, the implicit result is actually to grant it outright,
			//even if all other groups deny it (see ZSecurity.cs IsGroupAllowed. Only one exception: LocalAdminCheckpoint which uses IsGroupExplicitly allowed instead)
			//For staff, the base case is to do with the groups - for each given B/C/D if even a single group accepts it, then the base case should include a security accepting it.
			//(And of course then, if even a single group has *-*-* Y or nothing at all - then the base case becomes *-*-* Y)

			//Note that not having a base case still leads to strictly better behaviour then not doing any transformation at all -
			//we're correct except in cases where we don't account for the base case, and if we just moved securities and didn't do a transformation,
			//then we do the same thing in this case (nothing) anyway!

			//So for now I'll just do the easier part (the group base case) and if the staff base case is needed, let's make a WI for it.

			IEnumerable<Security> parentRows;

			if (fk == "GU_GG")
			{
				var baseCase = new List<Security>();
				baseCase.Add(new Security(true, Guid.Empty, Guid.Empty, Guid.Empty, 0));

				parentRows = rowsForEachParent.Aggregate((IEnumerable<Security>)baseCase, (parent, child) => MergeParentAndChild(parent, child));
			}
			else
			{
				parentRows = rowsForEachParent.Aggregate((parent, child) => MergeParentAndChild(parent, child));
			}

			//3) for every (aggregated) parent security that has visible effect in child, insert a new row for it on the child
			foreach (var parent in ParentSecuritiesVisibleInChild(parentRows, childRows))
			{
				try
				{
					using (var insertCommand = Db.Connection.Command(string.Format(insertSql, fk)))
					{
						insertCommand.AddParameter("@ItemIsAllowed", SqlDbType.Bit, parent.GU_SecurityItemIsAllowed);
						insertCommand.AddParameter("@SecurityRight", SqlDbType.VarChar, securityRight);
						insertCommand.AddParameter("@GC", SqlDbType.UniqueIdentifier, parent.GU_GC != Guid.Empty ? parent.GU_GC : DBNull.Value);
						insertCommand.AddParameter("@GB", SqlDbType.UniqueIdentifier, parent.GU_GB != Guid.Empty ? parent.GU_GB : DBNull.Value);
						insertCommand.AddParameter("@GE", SqlDbType.UniqueIdentifier, parent.GU_GE != Guid.Empty ? parent.GU_GE : DBNull.Value);
						insertCommand.AddParameter("@PK", SqlDbType.UniqueIdentifier, staffOrGroupPK);
						insertCommand.ExecuteNonQuery();
					}
				}
				catch (SqlException ex)
				{
					//Already exists perhaps?
					base.manager.ShowInfoMessage(string.Format(
						"Insertion of row for Security Right: {0} PK: {1} Group/Staff: {2} failed with message: {3}. You may want to manually verify that their security rights are correct.",
						securityRight, fk, staffOrGroupPK, ex.Message));
					continue;
				}
			}
		}

		#region Implementation

		const string getStaffPksSql = @"SELECT GS_PK FROM dbo.GlbStaff";

		const string getGroupPksSql = @"SELECT GG_PK FROM dbo.GlbGroup";

		const string getBranchCompanySql = @"SELECT GB_PK, GB_GC FROM dbo.GlbBranch";

		const string selectSql = @"
DECLARE @PK VARCHAR(36)
DECLARE @right VARCHAR(50)

SELECT @PK = '{0}', @right = '{1}'

select GU_SecurityItemIsAllowed, GU_GC, GU_GB, GU_GE,
	CASE
		WHEN GU_GC is null and GU_GB is null and GU_GE is null then 0
		WHEN GU_GC is not null and GU_GE is null then 1
		WHEN GU_GC is null and GU_GB is not null and GU_GE is null then 2
		WHEN GU_GC is not null and GU_GE is not null then 3
		WHEN GU_GC is null and GU_GB is not null and GU_GE is not null then 4
		else 5
	END as specificity
from dbo.GlbSecurity
WHERE GU_SecurityRight = @right
AND {2} = @PK
order by specificity";

		const string insertSql = @"INSERT INTO dbo.GlbSecurity
(GU_PK, GU_IsValid, GU_SecurityItemIsAllowed, GU_SecurityRight, GU_GC, GU_GB, GU_GE, {0})
VALUES (NEWID(), 1, @ItemIsAllowed, @SecurityRight, @GC, @GB, @GE, @PK)";

		#endregion
	}
}
