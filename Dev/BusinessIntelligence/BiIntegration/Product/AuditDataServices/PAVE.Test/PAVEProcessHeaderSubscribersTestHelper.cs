using System;
using System.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.PAVE.Test
{
	static class PAVEProcessHeaderSubscribersTestHelper
	{
		public static DataTable CreateProcessHeaderTable(int numEntries = 0)
		{
			var changeTable = new DataTable();

			changeTable.Columns.Add(ProcessHeaderSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_Status, typeof(string));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_IsActive, typeof(bool));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_ParentId, typeof(Guid));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_P0_Template, typeof(Guid));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_FH_ParentHeader, typeof(Guid));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_GG_ReleaseGroup, typeof(Guid));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_ReleaseDateTime, typeof(DateTime));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_LastTransferType, typeof(string));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_FC_CurrentComponent, typeof(Guid));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_FC_DedicatedBuffer, typeof(Guid));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_CompletionStatement, typeof(string));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_AllowTaskAutoAssignment, typeof(bool));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_SystemLastEditTimeUtc, typeof(DateTime));
			changeTable.Columns.Add(ProcessHeaderSchema.Constants.FH_SystemLastEditUser, typeof(string));
			changeTable.Columns.Add("TranEndTimeUtc", typeof(DateTime));

			for (var i = 0; i < numEntries; i++)
			{
				var row = CreateProcessHeaderRow(changeTable, Guid.NewGuid(), currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid());
				row.AcceptChanges();
				row.SetModified();
			}

			return changeTable;
		}

		public static DataRow CreateProcessHeaderRow(DataTable dataTable, Guid pk, Guid? currentComponentPK, Guid? parentHeaderPK, bool active = true, Guid? dedicatedBufferPK = null, DateTime? tranEndTimeUtc = null)
		{
			var row = dataTable.NewRow();

			row["TranEndTimeUtc"] = tranEndTimeUtc ?? ZDateTime.UtcNow.ToDateTime();
			row[ProcessHeaderSchema.Constants.FH_SystemLastEditTimeUtc] = ZDateTime.UtcNow.ToDateTime();
			row[ProcessHeaderSchema.Constants.FH_SystemLastEditUser] = "~BP";

			row[ProcessHeaderSchema.Constants.PK] = pk;
			row[ProcessHeaderSchema.Constants.FH_Status] = "STS";
			row[ProcessHeaderSchema.Constants.FH_IsActive] = active;
			row[ProcessHeaderSchema.Constants.FH_ParentId] = Guid.NewGuid();
			row[ProcessHeaderSchema.Constants.FH_P0_Template] = DBNull.Value;
			row[ProcessHeaderSchema.Constants.FH_FH_ParentHeader] = (object)parentHeaderPK ?? DBNull.Value;
			row[ProcessHeaderSchema.Constants.FH_GG_ReleaseGroup] = Guid.NewGuid();
			row[ProcessHeaderSchema.Constants.FH_ReleaseDateTime] = ZDateTime.UtcNow.ToDateTime();
			row[ProcessHeaderSchema.Constants.FH_LastTransferType] = string.Empty;
			row[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] = currentComponentPK;
			row[ProcessHeaderSchema.Constants.FH_FC_DedicatedBuffer] = (object)dedicatedBufferPK ?? DBNull.Value;
			row[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "Test";
			row[ProcessHeaderSchema.Constants.FH_AllowTaskAutoAssignment] = true;

			dataTable.Rows.Add(row);
			return row;
		}
	}
}
