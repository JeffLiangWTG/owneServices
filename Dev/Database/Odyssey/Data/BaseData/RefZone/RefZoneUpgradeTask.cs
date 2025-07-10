namespace Enterprise.DbUpgrader.Data.BaseData.RefZone
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using Enterprise.DbUpgrader.Data.BaseData.Common;
	using Enterprise.ZArchitecture.Schema;

	public class RefZoneUpgradeTask : SystemInstallDataUpgradeTask
	{
		public RefZoneUpgradeTask()
			: base(new RefZoneDataFile())
		{
		}

		/// <summary>
		/// Existing records are not updated
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
				case RefZoneHeaderSchema.Constants.TableName:
					// NK = FZ_Code, FZ_Description
					// FK = *FZ_OH_RelatedParty
					if (
						SameNkExists(RefZoneHeaderSchema.FZ_Code, sourceRow[RefZoneHeaderSchema.FZ_Code.Name].ToString())
						|| SameNkExists(RefZoneHeaderSchema.FZ_Description, sourceRow[RefZoneHeaderSchema.FZ_Description.Name].ToString())
						)
					{
						nonInsertedZonePks.Add((Guid)sourceRow[RefZoneHeaderSchema.PK.Name]);
						shouldInsert = false;
					}
					else if (!FkIsNullOrReferencedPkExists(OrgHeaderSchema.PK, sourceRow[RefZoneHeaderSchema.FZ_OH_RelatedParty.Name]))
					{
						sourceRow[RefZoneHeaderSchema.FZ_OH_RelatedParty.Name] = DBNull.Value;
					}
					break;

				case RefZonePivotSchema.Constants.TableName:
					// NK = F2_ParentID + F2_FZ
					// FK = F2_FZ, *F2_ParentID
					Guid zoneFk = (Guid)sourceRow[RefZonePivotSchema.F2_FZ.Name];
					Guid externalParentFk = (Guid)sourceRow[RefZonePivotSchema.F2_ParentID.Name];
					if (
						nonInsertedZonePks.Contains(zoneFk) ||
						SamePivotNkExists(RefZonePivotSchema.F2_ParentID, externalParentFk, RefZonePivotSchema.F2_FZ, zoneFk)
						)
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

		readonly List<Guid> nonInsertedZonePks = new List<Guid>();
	}
}
