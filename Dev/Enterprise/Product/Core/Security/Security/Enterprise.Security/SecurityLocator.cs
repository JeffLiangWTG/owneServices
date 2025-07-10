using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security
{
	public sealed class SecurityLocator : SecurityCalculator
	{
		public SecurityLocator(GlbStaff staff, BusinessObjectFactory factory)
		{
			this.staff = staff;
			this.factory = factory;
		}

		/// <summary>
		/// Returns all branches where the staff is permitted access to the given SecurityCheckPoint.
		/// A branch is considered to be permitting access if the staff has permissions to access the SecurityCheckPoint in any one department for that branch.
		/// </summary>
		/// <param name="checkPoint">Checkpoint to test</param>
		/// <param name="branchesWhereCheckpointIsGranted">
		/// If true - returns branches where checkpoint is granted or implicit.
		/// If false - returns branches where checkpoint is denied.
		/// </param>
		public GlbBranch[] GetAllBranchesThatStaffHasPermissionsFor(SecurityCheckpoint checkPoint, bool branchesWhereCheckpointIsGranted = true)
		{
			List<GlbBranch> result = new List<GlbBranch>();
			bool addAllBranches = false;

			if (staff.GS_IsController)
			{
				addAllBranches = branchesWhereCheckpointIsGranted;
			}
			else if (staff.GS_IsOperational)
			{
				bool noGroupsExist = (staff.ActiveGroups.Count == 0);

				foreach (GlbBranch branch in Branches)
				{
					SecurityState state = GetSecurityState(checkPoint, branch, GlbSecuritySchema.GU_GS, staff.PK);

					if (!noGroupsExist && state == SecurityState.Implicit)
					{
						foreach (GlbGroup group in staff.ActiveGroups)
						{
							SecurityState groupState = GetSecurityState(checkPoint, branch, GlbSecuritySchema.GU_GG, group.PK);
							state = (state == SecurityState.Implicit || groupState == SecurityState.Granted) ? groupState : state;
							if (state == SecurityState.Granted || state == SecurityState.Implicit)
							{
								break;
							}
						}
					}

					if (branchesWhereCheckpointIsGranted && (state == SecurityState.Granted || state == SecurityState.Implicit) ||
						!branchesWhereCheckpointIsGranted && state == SecurityState.Denied)
					{
						result.Add(branch);
					}
				}
			}

			if (addAllBranches)
			{
				result.AddRange((GlbBranch[])Branches.ToArray(typeof(GlbBranch)));
			}

			return result.ToArray();
		}

		#region IsSecurityAllowedForAllBranches

		class BranchWithDepartments
		{
			public GlbBranch Branch { get; set; }
			public List<ZGuid> NonDeniedDepartments { get; set; }
		}

		public bool IsSecurityAllowedForAllBranches(SecurityCheckpoint checkPoint)
		{
			if (staff.GS_IsController || !staff.GS_IsOperational)
			{
				return true;
			}

			var branchesToCheck = Branches.Cast<GlbBranch>().Select(b => new BranchWithDepartments { Branch = b, NonDeniedDepartments = AllDepartmentPks.ToList() }).ToList();
			var staffPk = staff.PK;
			var groupPks = staff.ActiveGroups.Select(group => group.PK).ToArray();

			while (branchesToCheck.Count > 0 && checkPoint != null)
			{
				List<BranchWithDepartments> implicitBranches;

				// Staff security has priority over any group security on same level (except implicit)
				if (HasDeniedBranchesOneLevel(checkPoint, branchesToCheck, out implicitBranches, GlbSecuritySchema.GU_GS, new[] { staffPk }))
				{
					return false;
				}
				branchesToCheck = implicitBranches;

				// Check all groups securities together for branches with implicit securities
				if (branchesToCheck.Count > 0 && groupPks.Length > 0)
				{
					if (HasDeniedBranchesOneLevel(checkPoint, branchesToCheck, out implicitBranches, GlbSecuritySchema.GU_GG, groupPks))
					{
						return false;
					}
					branchesToCheck = implicitBranches;
				}

				checkPoint = checkPoint.Parent;
			}

			return true;
		}

		bool HasDeniedBranchesOneLevel(SecurityCheckpoint checkPoint, List<BranchWithDepartments> branchesToCheck, out List<BranchWithDepartments> implicitBranches, SchemaColumn ownerColumn, ZGuid[] ownerPks)
		{
			var foundSecurities = (GlbSecurity[])Securities.Find(GetOwnersFilter(checkPoint, branchesToCheck.Select(b => b.Branch).ToArray(), ownerColumn, ownerPks));
			if (foundSecurities.Length == 0)
			{
				implicitBranches = branchesToCheck;
				return false;
			}

			var securityMatrix = new SecurityState[branchesToCheck.Count, ownerPks.Length, AllDepartmentPks.Length];
			UpdateStateMatrix(securityMatrix, -1, -1, -1, SecurityState.Implicit);

			foreach (var security in foundSecurities.OrderBy(GlbSecuritySortIndexCalculator.CalculateSortIndex))
			{
				var ownerIndex = Array.IndexOf(ownerPks, security[ownerColumn]);
				var departmentIndex = Array.IndexOf(AllDepartmentPks, security.GU_GE);
				var state = security.GU_SecurityItemIsAllowed ? SecurityState.Granted : SecurityState.Denied;

				if (security.GU_GC.IsEmpty && security.GU_GB.IsEmpty)
				{
					UpdateStateMatrix(securityMatrix, -1, ownerIndex, departmentIndex, state);
				}
				else if (!security.GU_GC.IsEmpty)
				{
					for (int i = 0; i < branchesToCheck.Count; i++)
					{
						if (branchesToCheck[i].Branch.GB_GC == security.GU_GC)
						{
							UpdateStateMatrix(securityMatrix, i, ownerIndex, departmentIndex, state);
						}
					}
				}
				else if (!security.GU_GB.IsEmpty)
				{
					for (int i = 0; i < branchesToCheck.Count; i++)
					{
						if (branchesToCheck[i].Branch.PK == security.GU_GB)
						{
							UpdateStateMatrix(securityMatrix, i, ownerIndex, departmentIndex, state);
							break;
						}
					}
				}
			}

			var hasDeniedBranches = false;
			implicitBranches = new List<BranchWithDepartments>(branchesToCheck.Count);
			for (int i = 0; i < branchesToCheck.Count; i++)
			{
				var branchWithDepartments = branchesToCheck[i];
				var state = SecurityState.Denied;
				for (int k = 0; k < AllDepartmentPks.Length; k++)
				{
					if (branchWithDepartments.NonDeniedDepartments.Contains(AllDepartmentPks[k]))
					{
						var departmentState = SecurityState.Denied;
						for (int j = 0; j < ownerPks.Length; j++)
						{
							if (securityMatrix[i, j, k] != SecurityState.Denied)
							{
								state = departmentState = securityMatrix[i, j, k];
							}
							if (state == SecurityState.Granted)
							{
								break;
							}
						}
						if (state == SecurityState.Granted)
						{
							break;
						}

						if (departmentState == SecurityState.Denied)
						{
							branchWithDepartments.NonDeniedDepartments.Remove(AllDepartmentPks[k]);
						}
					}
				}

				if (branchWithDepartments.NonDeniedDepartments.Count > 0 && state != SecurityState.Granted)
				{
					implicitBranches.Add(branchWithDepartments);
				}
				if (state == SecurityState.Denied && branchWithDepartments.NonDeniedDepartments.Count == 0)
				{
					hasDeniedBranches = true;
					break;
				}
			}

			return hasDeniedBranches;
		}

		ZQuery GetOwnersFilter(SecurityCheckpoint checkPoint, GlbBranch[] branchesToCheck, SchemaColumn ownerColumn, ZGuid[] ownerPks)
		{
			var filter = new ZQuery();
			filter.AddToFilter(GlbSecuritySchema.GU_SecurityRight, checkPoint.Code);
			filter.AddToFilter(GlbSecuritySchema.GU_ItemGUID, checkPoint.ItemGuid == Guid.Empty ? null : checkPoint.ItemGuid);
			filter.AddToFilter(ownerColumn, ownerPks);

			var branchesFilter = new ZQuery();
			branchesFilter.AddToFilter(JoinCondition.Or, GlbSecuritySchema.GU_GB, branchesToCheck.Select(branch => branch.PK));
			branchesFilter.AddToFilter(JoinCondition.Or, GlbSecuritySchema.GU_GC, branchesToCheck.Select(branch => branch.GB_GC).Distinct());
			branchesFilter.AddToFilter(new ZQuery(new ZQuery(GlbSecuritySchema.GU_GB, null), new ZQuery(GlbSecuritySchema.GU_GC, null)), JoinCondition.Or);
			filter.AddToFilter(branchesFilter);

			return filter;
		}

		void UpdateStateMatrix(SecurityState[,,] matrix, int branchIndex, int ownerIndex, int departmentIndex, SecurityState newState)
		{
			var fromBranch = branchIndex >= 0 ? branchIndex : 0;
			var toBranch = branchIndex >= 0 ? branchIndex + 1 : matrix.GetLength(0);

			var fromOwner = ownerIndex >= 0 ? ownerIndex : 0;
			var toOwner = ownerIndex >= 0 ? ownerIndex + 1 : matrix.GetLength(1);

			var fromDepartment = departmentIndex >= 0 ? departmentIndex : 0;
			var toDepartment = departmentIndex >= 0 ? departmentIndex + 1 : matrix.GetLength(2);

			for (int i = fromBranch; i < toBranch; i++)
			{
				for (int j = fromOwner; j < toOwner; j++)
				{
					for (int k = fromDepartment; k < toDepartment; k++)
					{
						matrix[i, j, k] = newState;
					}
				}
			}
		}

		#endregion

		readonly GlbStaff staff;
		readonly BusinessObjectFactory factory;

		#region Collections

		GlbBranchCollection Branches
		{
			get
			{
				if (branches == null)
				{
					branches = new GlbBranchCollection(factory);
					branches.Load();
				}
				return branches;
			}
		}

		GlbDepartmentCollection Departments
		{
			get
			{
				if (departments == null)
				{
					departments = new GlbDepartmentCollection(factory);
				}
				return departments;
			}
		}

		internal GlbSecurityCollection Securities
		{
			get
			{
				if (securities == null)
				{
					ZQuery filter = new ZQuery(GlbSecuritySchema.GU_GS, staff.PK);
					List<ZGuid> groupPks = new List<ZGuid>();

					foreach (GlbGroup group in staff.ActiveGroups)
					{
						groupPks.Add(group.PK);
					}

					filter.AddToFilter(JoinCondition.Or, GlbSecuritySchema.GU_GG, SQLComparisonOperator.Equal, groupPks);
					securities = new GlbSecurityCollection(factory);
					securities.Load(filter);
				}

				return securities;
			}
		}

		GlbBranchCollection branches;
		GlbDepartmentCollection departments;
		GlbSecurityCollection securities;

		#endregion

		#region Get Security State

		SecurityState GetSecurityState(SecurityCheckpoint checkPoint, GlbBranch branch, SchemaColumn ownerColumn, ZGuid ownerPk)
		{
			SecurityState result;

			ZQuery departmentFilter = GetOwnerFilter(checkPoint, ownerColumn, ownerPk);
			departmentFilter.AddToFilter(GlbSecuritySchema.GU_GE, SQLComparisonOperator.NotEqual, null);

			ZQuery filter = new ZQuery(departmentFilter);
			filter.AddToFilter(GlbSecuritySchema.GU_GB, branch.PK);

			ZGuid[] nonDeniedBranchDepartmentPks;
			result = GetSecurityStateForDepartments(filter, Array.Empty<ZGuid>(), out nonDeniedBranchDepartmentPks);

			if (result == SecurityState.Implicit)
			{
				ZGuid companyPk = branch.Company.PK;
				filter = new ZQuery(departmentFilter);
				filter.AddToFilter(GlbSecuritySchema.GU_GC, companyPk);

				if (nonDeniedBranchDepartmentPks.Length > 0 && nonDeniedBranchDepartmentPks.Length != Departments.Count)
				{
					filter.AddToFilter(GlbSecuritySchema.GU_GE, nonDeniedBranchDepartmentPks);
				}

				ZGuid[] nonDeniedCompanyDepartmentPks;
				result = GetSecurityStateForDepartments(filter, nonDeniedBranchDepartmentPks, out nonDeniedCompanyDepartmentPks);

				if (result == SecurityState.Implicit)
				{
					result = GetSecurityStateForOneLevel(Securities, checkPoint, ownerColumn == GlbSecuritySchema.GU_GG, ownerPk, ZGuid.Empty, branch.PK, ZGuid.Empty);

					if (result == SecurityState.Implicit)
					{
						result = GetSecurityStateForOneLevel(Securities, checkPoint, ownerColumn == GlbSecuritySchema.GU_GG, ownerPk, ZGuid.Empty, ZGuid.Empty, companyPk);

						if (result == SecurityState.Implicit)
						{
							result = GetSecurityStateForOneLevel(Securities, checkPoint, ownerColumn == GlbSecuritySchema.GU_GG, ownerPk, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
						}
					}
				}
			}

			if (result == SecurityState.Implicit && checkPoint.Parent != null)
			{
				result = GetSecurityState(checkPoint.Parent, branch, ownerColumn, ownerPk);
			}

			return result;
		}

		SecurityState GetSecurityStateForDepartments(ZQuery filter, ZGuid[] initialNonDeniedDepartmentPks, out ZGuid[] newNonDeniedDepartmentPks)
		{
			SecurityState result = SecurityState.Implicit;
			GlbSecurity[] filterResults = (GlbSecurity[])Securities.Find(filter);
			List<ZGuid> deniedDepartmentPks = new List<ZGuid>();

			foreach (GlbSecurity security in filterResults)
			{
				if (security.GU_SecurityItemIsAllowed)
				{
					result = SecurityState.Granted;
					break;
				}
				else
				{
					deniedDepartmentPks.Add(security.GU_GE);
				}
			}

			bool noDepartmentsDenied = (deniedDepartmentPks.Count == 0);

			if (noDepartmentsDenied)
			{
				newNonDeniedDepartmentPks = Array.Empty<ZGuid>();
			}
			else
			{
				if (initialNonDeniedDepartmentPks.Length == 0)
				{
					initialNonDeniedDepartmentPks = AllDepartmentPks;
				}
				newNonDeniedDepartmentPks = GetNonDeniedDepartmentPks(deniedDepartmentPks, initialNonDeniedDepartmentPks);
			}

			if (!noDepartmentsDenied && newNonDeniedDepartmentPks.Length == 0)
			{
				result = SecurityState.Denied;
			}

			return result;
		}

		ZGuid[] GetNonDeniedDepartmentPks(List<ZGuid> deniedDepartmentPks, ZGuid[] nonDeniedDepartmentPks)
		{
			List<ZGuid> result = new List<ZGuid>();
			if (deniedDepartmentPks.Count != nonDeniedDepartmentPks.Length)
			{
				foreach (ZGuid departmentPk in nonDeniedDepartmentPks)
				{
					if (!deniedDepartmentPks.Contains(departmentPk))
					{
						result.Add(departmentPk);
					}
				}
			}
			return result.ToArray();
		}

		ZGuid[] AllDepartmentPks
		{
			get
			{
				if (allDepartmentPks == null)
				{
					List<ZGuid> result = new List<ZGuid>();
					foreach (GlbDepartment department in Departments)
					{
						result.Add(department.PK);
					}
					allDepartmentPks = result.ToArray();
				}
				return allDepartmentPks;
			}
		}

		ZQuery GetOwnerFilter(SecurityCheckpoint checkPoint, SchemaColumn ownerColumn, ZGuid ownerPk)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(GlbSecuritySchema.GU_SecurityRight, checkPoint.Code);
			result.AddToFilter(GlbSecuritySchema.GU_ItemGUID, checkPoint.ItemGuid == Guid.Empty ? null : checkPoint.ItemGuid);
			result.AddToFilter(ownerColumn, ownerPk.IsEmpty ? null : ownerPk);
			return result;
		}

		ZGuid[] allDepartmentPks;

		#endregion
	}
}
