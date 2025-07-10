namespace Enterprise.DbUpgrader.Data.BaseData.Company
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Enterprise.DbUpgrader.Data.BaseData.Common;
	using Enterprise.ZArchitecture.Schema;

	public class CompanyUpgradeTask : SystemInstallDataUpgradeTask
	{
		public CompanyUpgradeTask()
			: base(new CompanyDataFile())
		{
		}

		/// <summary>
		/// Does not update existing records
		/// </summary>
		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			// Skips update
		}

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			bool shouldInsert = true;

			switch (targetTable.TableName)
			{
				case GlbCompanySchema.Constants.TableName:
					// NK = GC_Code
					// FK = *GC_OH_OrgProxy
					if (SameNkExists(GlbCompanySchema.GC_Code, sourceRow[GlbCompanySchema.GC_Code.Name].ToString()))
					{
						nonInsertedCompanyPks.Add((Guid)sourceRow[GlbCompanySchema.PK.Name]);
						shouldInsert = false;
					}
					else if (!FkIsNullOrReferencedPkExists(OrgHeaderSchema.PK, sourceRow[GlbCompanySchema.GC_OH_OrgProxy.Name]))
					{
						sourceRow[GlbCompanySchema.GC_OH_OrgProxy.Name] = DBNull.Value;
					}
					break;

				case GlbBranchSchema.Constants.TableName:
					// NK = GB_Code
					// FK = GB_GC, *GB_OH_OrgProxy
					if (
						nonInsertedCompanyPks.Contains((Guid)sourceRow[GlbBranchSchema.GB_GC.Name]) ||
						SameNkExists(GlbBranchSchema.GB_Code, sourceRow[GlbBranchSchema.GB_Code.Name].ToString())
						)
					{
						shouldInsert = false;
					}
					else if (!FkIsNullOrReferencedPkExists(OrgHeaderSchema.PK, sourceRow[GlbBranchSchema.GB_OH_OrgProxy.Name]))
					{
						sourceRow[GlbBranchSchema.GB_OH_OrgProxy.Name] = DBNull.Value;
					}
					break;

				case GlbGroupSchema.Constants.TableName:
					// NK = GG_Code
					if (SameNkExists(GlbGroupSchema.GG_Code, sourceRow[GlbGroupSchema.GG_Code.Name].ToString()))
					{
						nonInsertedGroupPks.Add((Guid)sourceRow[GlbGroupSchema.PK.Name]);
						shouldInsert = false;
					}
					break;

				case GlbStaffSchema.Constants.TableName:
					// NK = GS_Code, GS_LoginName
					// FK = *GS_GB_HomeBranch, *GS_GE_HomeDepartment
					if (
						SameNkExists(GlbStaffSchema.GS_Code, sourceRow[GlbStaffSchema.GS_Code.Name].ToString())
						|| SameNkExists(GlbStaffSchema.GS_LoginName, sourceRow[GlbStaffSchema.GS_LoginName.Name].ToString())
						)
					{
						nonInsertedStaffPks.Add((Guid)sourceRow[GlbStaffSchema.PK.Name]);
						shouldInsert = false;
					}
					else
					{
						object gsBranchFk = sourceRow[GlbStaffSchema.GS_GB_HomeBranch.Name];

						if (
							gsBranchFk is Guid
							&& nonInsertedBranchPks.Contains((Guid)gsBranchFk))
						{
							sourceRow[GlbStaffSchema.GS_GB_HomeBranch.Name] = DBNull.Value;
						}

						if (!FkIsNullOrReferencedPkExists(GlbDepartmentSchema.PK, sourceRow[GlbStaffSchema.GS_GE_HomeDepartment.Name]))
						{
							sourceRow[GlbStaffSchema.GS_GE_HomeDepartment.Name] = DBNull.Value;
						}
					}
					break;

				case GlbGroupLinkSchema.Constants.TableName:
					// NK = GK_GG + GK_GS
					// FK = GK_GG, GK_GS
					Guid gkGroupFk = (Guid)sourceRow[GlbGroupLinkSchema.GK_GG.Name];
					Guid gkStaffFk = (Guid)sourceRow[GlbGroupLinkSchema.GK_GS.Name];
					if (
						nonInsertedGroupPks.Contains(gkGroupFk) ||
						nonInsertedStaffPks.Contains(gkStaffFk) ||
						SamePivotNkExists(GlbGroupLinkSchema.GK_GG, gkGroupFk, GlbGroupLinkSchema.GK_GS, gkStaffFk)
						)
					{
						shouldInsert = false;
					}
					break;

				case GlbGroupRoleSchema.Constants.TableName:
					var groupRoleGroupFk = (Guid)sourceRow[GlbGroupRoleSchema.GGR_GG_Group.Name];
					if (nonInsertedGroupPks.Contains(groupRoleGroupFk))
					{
						shouldInsert = false;
					}
					break;

				case GlbSecuritySchema.Constants.TableName:
					var glbSecurityGroupFk = (Guid)sourceRow[GlbSecuritySchema.GU_GG.Name];
					if (nonInsertedGroupPks.Contains(glbSecurityGroupFk))
					{
						shouldInsert = false;
					}
					break;
			}

			if (shouldInsert)
			{
				base.DoInsert(sourceRow, targetTable, ref targetIndex);
			}
		}

		readonly List<Guid> nonInsertedCompanyPks = new List<Guid>();
		readonly List<Guid> nonInsertedBranchPks = new List<Guid>();
		readonly List<Guid> nonInsertedGroupPks = new List<Guid>();
		readonly List<Guid> nonInsertedStaffPks = new List<Guid>();
	}
}
