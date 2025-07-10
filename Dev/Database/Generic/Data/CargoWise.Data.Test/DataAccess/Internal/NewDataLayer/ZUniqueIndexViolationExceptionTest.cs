using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;

namespace CargoWise.Data.Test.DataAccess;

sealed class ZUniqueIndexViolationExceptionTest : TestCase
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
	public void TestZUniqueIndexViolationException_BulkCopyWithDuplicatedClusterKey_CanConstructTheCorrectException()
	{
		var consignmentHeaderTable = GetDataTableForHVLVConsignmentHeader();
		var row1 = consignmentHeaderTable.NewRow();
		row1[HVLVConsignmentHeaderSchema.Constants.PK] = Guid.NewGuid();
		row1[HVLVConsignmentHeaderSchema.Constants.HCH_ClusterKey] = 1;
		row1[HVLVConsignmentHeaderSchema.Constants.HCH_JS_Shipment] = Guid.NewGuid();
		row1[HVLVConsignmentHeaderSchema.Constants.HCH_SystemCreateTimeUtc] = DateTime.UtcNow;
		row1[HVLVConsignmentHeaderSchema.Constants.HCH_SystemLastEditTimeUtc] = DateTime.UtcNow;
		consignmentHeaderTable.Rows.Add(row1);

		var row2 = consignmentHeaderTable.NewRow();
		row2[HVLVConsignmentHeaderSchema.Constants.PK] = Guid.NewGuid();
		row2[HVLVConsignmentHeaderSchema.Constants.HCH_ClusterKey] = 1;
		row2[HVLVConsignmentHeaderSchema.Constants.HCH_JS_Shipment] = Guid.NewGuid();
		row2[HVLVConsignmentHeaderSchema.Constants.HCH_SystemCreateTimeUtc] = DateTime.UtcNow;
		row2[HVLVConsignmentHeaderSchema.Constants.HCH_SystemLastEditTimeUtc] = DateTime.UtcNow;
		consignmentHeaderTable.Rows.Add(row2);

		using (var bulkCopy = Db.Connection.GetSqlBulkCopy(SqlBulkCopyOptions.Default, ((IDbConnectionInternals)Db.Connection).InternalDbTransaction))
		{
			bulkCopy.BulkCopyTimeout = 0;
			bulkCopy.BatchSize = 1000;
			bulkCopy.DestinationTableName = consignmentHeaderTable.TableName;
			foreach (DataColumn column in consignmentHeaderTable.Columns)
			{
				bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
			}

			var exception = AssertExceptionThrown<SqlException>("SqlException thrown from bulkCopy.WriteToServer ", () => bulkCopy.WriteToServer(consignmentHeaderTable));

			Assert(DbErrorMatch.GetExceptionType(exception) == DbErrorType.CannotInsertDuplicateUniqueIndexKey);
			ZUniqueIndexViolationException uniqueIndexViolationException = null;
			AssertNoExceptionThrown("Should throw no exception when constructing ZUniqueIndexViolationException",
				() => uniqueIndexViolationException =
					new ZUniqueIndexViolationException(exception, Db.Connection, consignmentHeaderTable, "HCH"));
			AssertEquals(1, uniqueIndexViolationException.Row[HVLVConsignmentHeaderSchema.Constants.HCH_ClusterKey]);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
	public void TestZUniqueIndexViolationException_BulkCopyWithDuplicatedGuidKey_CanConstructTheCorrectException()
	{
		var consignmentHeaderTable = GetDataTableForHVLVConsignmentHeader();
		var row1 = consignmentHeaderTable.NewRow();
		var shipmentPK = Guid.NewGuid();
		row1[HVLVConsignmentHeaderSchema.Constants.PK] = Guid.NewGuid();
		row1[HVLVConsignmentHeaderSchema.Constants.HCH_ClusterKey] = 1;
		row1[HVLVConsignmentHeaderSchema.Constants.HCH_JS_Shipment] = shipmentPK;
		row1[HVLVConsignmentHeaderSchema.Constants.HCH_SystemCreateTimeUtc] = DateTime.UtcNow;
		row1[HVLVConsignmentHeaderSchema.Constants.HCH_SystemLastEditTimeUtc] = DateTime.UtcNow;
		consignmentHeaderTable.Rows.Add(row1);

		var row2 = consignmentHeaderTable.NewRow();
		row2[HVLVConsignmentHeaderSchema.Constants.PK] = Guid.NewGuid();
		row2[HVLVConsignmentHeaderSchema.Constants.HCH_ClusterKey] = 2;
		row2[HVLVConsignmentHeaderSchema.Constants.HCH_JS_Shipment] = shipmentPK;
		row2[HVLVConsignmentHeaderSchema.Constants.HCH_SystemCreateTimeUtc] = DateTime.UtcNow;
		row2[HVLVConsignmentHeaderSchema.Constants.HCH_SystemLastEditTimeUtc] = DateTime.UtcNow;
		consignmentHeaderTable.Rows.Add(row2);

		using (var bulkCopy = Db.Connection.GetSqlBulkCopy(SqlBulkCopyOptions.Default, ((IDbConnectionInternals)Db.Connection).InternalDbTransaction))
		{
			bulkCopy.BulkCopyTimeout = 0;
			bulkCopy.BatchSize = 1000;
			bulkCopy.DestinationTableName = consignmentHeaderTable.TableName;
			foreach (DataColumn column in consignmentHeaderTable.Columns)
			{
				bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
			}

			var exception = AssertExceptionThrown<SqlException>("SqlException thrown from bulkCopy.WriteToServer", () => bulkCopy.WriteToServer(consignmentHeaderTable));

			Assert(DbErrorMatch.GetExceptionType(exception) == DbErrorType.CannotInsertDuplicateUniqueIndexKey);
			ZUniqueIndexViolationException uniqueIndexViolationException = null;
			AssertNoExceptionThrown("Should throw no exception when constructing ZUniqueIndexViolationException",
				() => uniqueIndexViolationException =
					new ZUniqueIndexViolationException(exception, Db.Connection, consignmentHeaderTable, "HCH"));

			AssertEquals(shipmentPK, uniqueIndexViolationException.Row[HVLVConsignmentHeaderSchema.Constants.HCH_JS_Shipment]);
		}
	}

	static DataTable GetDataTableForHVLVConsignmentHeader()
	{
		var table = new DataTable(HVLVConsignmentHeaderSchema.Constants.TableName);
		table.Columns.Add(HVLVConsignmentHeaderSchema.Constants.PK, typeof(Guid));
		table.Columns.Add(HVLVConsignmentHeaderSchema.Constants.HCH_ClusterKey, typeof(int));
		table.Columns.Add(HVLVConsignmentHeaderSchema.Constants.HCH_JS_Shipment, typeof(Guid));
		table.Columns.Add(HVLVConsignmentHeaderSchema.Constants.HCH_SystemCreateTimeUtc, typeof(DateTime));
		table.Columns.Add(HVLVConsignmentHeaderSchema.Constants.HCH_SystemLastEditTimeUtc, typeof(DateTime));

		return table;
	}
}
