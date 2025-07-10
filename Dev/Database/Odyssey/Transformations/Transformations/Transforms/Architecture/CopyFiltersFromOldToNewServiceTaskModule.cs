using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Architecture
{
	public class CopyFiltersFromOldToNewServiceTaskModule : DataTransformation
	{
		public override string UserDescription => "Populate the module filters for the new StmServiceTask module from the old module.";

		protected override void OfflinePostUpgradeTransform()
		{
			DeleteExistingStmServiceTaskFilters();
			InsertExistingServiceTaskFiltersInStmServiceTaskFilters();
		}

		void InsertExistingServiceTaskFiltersInStmServiceTaskFilters()
		{
			var existingFilters = new List<(object s9_PK, object s9_GC, object s9_ModuleID, object s9_RelatedEntityID, object s9_IsPublished, object s9_FilterData, object s9_ColumnLayoutData, object s9_SaveColumnLayout, object s9_IsSystem, object s9_FilterName, object s9_FilterType, object s9_ParentTableCode, object s9_SaveGridColourLayout, object s9_IsIndexSearch, object s9_ParentID, object s9_GridColourLayoutID)>();
			var sql = @"
SELECT * 
FROM StmModuleFilter filterData
WHERE S9_ModuleID = 'ServiceTask'";
			Db.Connection.ExecuteReader(
				sql,
				reader =>
				{
					existingFilters.Add((
						reader["S9_PK"],
						reader["S9_GC"],
						reader["S9_ModuleID"],
						reader["S9_RelatedEntityID"],
						reader["S9_IsPublished"],
						reader["S9_FilterData"],
						reader["S9_ColumnLayoutData"],
						reader["S9_SaveColumnLayout"],
						reader["S9_IsSystem"],
						reader["S9_FilterName"],
						reader["S9_FilterType"],
						reader["S9_ParentTableCode"],
						reader["S9_SaveGridColourLayout"],
						reader["S9_IsIndexSearch"],
						reader["S9_ParentID"],
						reader["S9_GridColourLayoutID"]));
				});
			foreach (var existingFilter in existingFilters)
			{
				try
				{
					var pk = Guid.NewGuid();
					Db.Connection.ExecuteNonQuery(
						@$"
INSERT INTO StmModuleFilter (
	[S9_PK],
	{(existingFilter.s9_GC == null ? string.Empty : "[S9_GC],")}
	[S9_ModuleID],
	{(existingFilter.s9_RelatedEntityID == null ? string.Empty : "[S9_RelatedEntityID],")}
	[S9_IsPublished],
	{(existingFilter.s9_FilterData == null ? string.Empty : "[S9_FilterData],")}
	{(existingFilter.s9_ColumnLayoutData == null ? string.Empty : "[S9_ColumnLayoutData],")}
	[S9_SaveColumnLayout],
	[S9_IsSystem],
	[S9_FilterName],
	[S9_FilterType],
	{(existingFilter.s9_ParentID == null ? string.Empty : "[S9_ParentId],")}
	[S9_ParentTableCode],
	{(existingFilter.s9_GridColourLayoutID == null ? string.Empty : "[S9_GridColourLayoutID],")}
	[S9_SaveGridColourLayout],
	[S9_SystemCreateTimeUtc],
	[S9_SystemCreateUser],
	[S9_SystemLastEditTimeUtc],
	[S9_SystemLastEditUser],
	[S9_IsIndexSearch])
VALUES (
@S9_PK,
{(existingFilter.s9_GC == null ? string.Empty : "@S9_GC,")}
@S9_ModuleID,
{(existingFilter.s9_RelatedEntityID == null ? string.Empty : "@S9_RelatedEntityID,")}
@S9_IsPublished,
{(existingFilter.s9_FilterData == null ? string.Empty : "@S9_FilterData,")}
{(existingFilter.s9_ColumnLayoutData == null ? string.Empty : "@S9_ColumnLayoutData,")}
@S9_SaveColumnLayout,
@S9_IsSystem,
@S9_FilterName,
@S9_FilterType,
{(existingFilter.s9_ParentID == null ? string.Empty : "@S9_ParentId,")}
@S9_ParentTableCode,
{(existingFilter.s9_GridColourLayoutID == null ? string.Empty : "@S9_GridColourLayoutID,")}
@S9_SaveGridColourLayout,
GetUtcDate(),
'~BP',
GetUtcDate(),
'~BP',
@S9_IsIndexSearch)
",
						cmd =>
						{
							cmd.AddParameter("@S9_PK", SqlDbType.UniqueIdentifier, pk);
							if (existingFilter.s9_GC != null)
							{
								cmd.AddParameter("@S9_GC", SqlDbType.UniqueIdentifier, existingFilter.s9_GC);
							}
							cmd.AddParameter("@S9_ModuleID", SqlDbType.NVarChar, 200, "StmServiceTask");
							if (existingFilter.s9_RelatedEntityID != null)
							{
								cmd.AddParameter("@S9_RelatedEntityID", SqlDbType.UniqueIdentifier, existingFilter.s9_RelatedEntityID);
							}
							cmd.AddParameter("@S9_IsPublished", SqlDbType.Bit, existingFilter.s9_IsPublished);
							if (existingFilter.s9_FilterData != null)
							{
								cmd.AddParameter("@S9_FilterData", SqlDbType.Binary, existingFilter.s9_FilterData);
							}
							if (existingFilter.s9_ColumnLayoutData != null)
							{
								cmd.AddParameter("@S9_ColumnLayoutData", SqlDbType.Binary, ConvertLayoutData((byte[])existingFilter.s9_ColumnLayoutData));
							}
							cmd.AddParameter("@S9_SaveColumnLayout", SqlDbType.Bit, existingFilter.s9_SaveColumnLayout);
							cmd.AddParameter("@S9_IsSystem", SqlDbType.Bit, existingFilter.s9_IsSystem);
							cmd.AddParameter("@S9_FilterName", SqlDbType.NVarChar, 50, existingFilter.s9_FilterName);
							cmd.AddParameter("@S9_FilterType", SqlDbType.NVarChar, 3, existingFilter.s9_FilterType);
							if (existingFilter.s9_ParentID != null)
							{
								cmd.AddParameter("@S9_ParentID", SqlDbType.UniqueIdentifier, existingFilter.s9_ParentID);
							}
							if (existingFilter.s9_GridColourLayoutID != null)
							{
								cmd.AddParameter("@S9_GridColourLayoutID", SqlDbType.UniqueIdentifier, existingFilter.s9_GridColourLayoutID);
							}
							cmd.AddParameter("@S9_ParentTableCode", SqlDbType.NVarChar, 3, existingFilter.s9_ParentTableCode);
							cmd.AddParameter("@S9_SaveGridColourLayout", SqlDbType.Bit, existingFilter.s9_SaveGridColourLayout);
							cmd.AddParameter("@S9_IsIndexSearch", SqlDbType.Bit, existingFilter.s9_IsIndexSearch);
						});
					InsertStmFilterModuleFilterUserData((Guid)existingFilter.s9_PK, pk);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					manager.ShowInfoMessage($"Could not convert filter [{existingFilter.s9_FilterName}], skipping filter. Error: {e}");
					continue;
				}
			}
		}

		byte[] ConvertLayoutData(byte[] s9_ColumnLayoutData)
		{
			var customLayoutXml = Compressor.UncompressAsString(s9_ColumnLayoutData);
			customLayoutXml.Replace("S5_ScheduleType", "SST_ServiceTaskCode");
			customLayoutXml.Replace("S5_ScheduleDescriptionMultilingual", "Description");
			customLayoutXml.Replace("S5_IsActive", "SST_Active");
			customLayoutXml.Replace("S5_TypeOfDocument", "Category");
			customLayoutXml.Replace("S5_GB", "SST_GB_Branch");
			customLayoutXml.Replace("S5_SystemCreateTimeUtc", "SST_SystemCreateTimeUtc");
			customLayoutXml.Replace("S5_SystemCreateUser", "SST_SystemCreateUser");
			customLayoutXml.Replace("S5_SystemLastEditTimeUtc", "SST_SystemLastEditTimeUtc");
			customLayoutXml.Replace("S5_SystemLastEditUser", "SST_SystemLastEditUser");
			return Compressor.Compress(Encoding.ASCII.GetBytes(customLayoutXml));
		}

		void InsertStmFilterModuleFilterUserData(Guid originalFilterDataPk, Guid newFilterDataPk)
		{
			var existingUserFilters = new List<(string s0_RelatedEntityTableCode, Guid s0_RelatedEntityID, byte[] s0_FilterDataValues)>();
			var retrieveSql = @"
SELECT * 
FROM StmModuleFilterUserData
WHERE S0_S9 = @pk";
			Db.Connection.ExecuteReader(
				retrieveSql,
				cmd =>
				{
					cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, originalFilterDataPk);
				},
				(reader) =>
				{
					existingUserFilters.Add(((string)reader["S0_RelatedEntityTableCode"], (Guid)reader["S0_RelatedEntityID"], (byte[])reader["S0_FilterDataValues"]));
				});
			foreach (var filter in existingUserFilters)
			{
				var insertSql = @"
INSERT INTO StmModuleFilterUserData ([S0_PK],[S0_RelatedEntityTableCode],[S0_RelatedEntityID],[S0_FilterDataValues],[S0_S9],[S0_SystemCreateTimeUtc],[S0_SystemCreateUser],[S0_SystemLastEditTimeUtc],[S0_SystemLastEditUser])
VALUES (NEWID(), @s0_RelatedEntityTableCode, @s0_RelatedEntityID, @s0_FilterDataValues, @s0_S9, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
				Db.Connection.ExecuteNonQuery(
					insertSql,
					cmd =>
					{
						cmd.AddParameter("@s0_RelatedEntityTableCode", SqlDbType.NVarChar, 3, filter.s0_RelatedEntityTableCode);
						cmd.AddParameter("@s0_RelatedEntityID", SqlDbType.UniqueIdentifier, filter.s0_RelatedEntityID);
						cmd.AddParameter("@s0_FilterDataValues", SqlDbType.Binary, filter.s0_FilterDataValues);
						cmd.AddParameter("@s0_S9", SqlDbType.UniqueIdentifier, newFilterDataPk);
					});
			}
		}

		void DeleteExistingStmServiceTaskFilters()
		{
			var sql = @"
DELETE FROM StmModuleFilterUserData
WHERE S0_S9 in (
	SELECT S9_PK
	FROM StmModuleFilter
	WHERE S9_ModuleID = 'StmServiceTask')

DELETE FROM StmModuleFilter
WHERE S9_ModuleId = 'StmServiceTask'
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
