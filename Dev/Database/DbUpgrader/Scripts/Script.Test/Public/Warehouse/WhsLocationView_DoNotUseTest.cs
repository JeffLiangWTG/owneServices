using System.Collections.Generic;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Schema.WhsLocationViewSchema.Constants;

namespace Enterprise.Build.Database.Script.Public.Warehouse
{
	[TestedType(typeof(WhsLocationView_DoNotUse))]
	class WhsLocationView_DoNotUseTest : DbCreateIndexedViewScriptTest
	{
		protected override IEnumerable<IndexInfo> GetNonClusteredIndexes()
		{
			return new List<IndexInfo>()
			{
				IndexInfo.Builder.New(SqlSchemaName, Name, $"NR_UX__{PK}") // index name and column names for creating index
					.Unique(true)
					.Clustered(false)
					.Key(PK)
					.Include(WLV_WW_Whs, WLV_LocationString, WLV_LocationString_UserFriendly, WLV_WLT_LocationType, WLV_LocationClass, WLV_IsValidLocationForProductWarehousePutaway, WLV_LocationTypeCode)
					.GetInfo(),

				IndexInfo.Builder.New(SqlSchemaName, Name, FKRXIndexName(WLV_RowName))
					.Unique(false)
					.Clustered(false)
					.Key(WLV_RowName)
					.GetInfo(),

				IndexInfo.Builder.New(SqlSchemaName, TableName, $"NR_UX__{WLV_WW_Whs}_{WLV_TransitDischargeLRC}_{WLV_RS_NKTransitServiceLevel}_{WLV_TransitDischargeAndServiceLevelFakeColumnForUX}") // index name and column names for creating index
					.Unique(true)
					.Clustered(false)
					.Key(WLV_WW_Whs)
					.Key(WLV_TransitDischargeLRC)
					.Key(WLV_RS_NKTransitServiceLevel)
					.Key("WLV_TransitDischargeAndServiceLevelFakeColumnForUX") // index name and column names for creating index
					.GetInfo(),

				IndexInfo.Builder.New(SqlSchemaName, Name, $"NR_RX__{WLV_IsValidLocationForProductWarehousePutaway}_{WLV_WW_Whs}")
					.Unique(false)
					.Clustered(false)
					.Key(WLV_IsValidLocationForProductWarehousePutaway)
					.Key(WLV_WW_Whs)
					.Include(PK, WLV_WA_PickingArea, WLV_LocationClass, WLV_LastConfigChangedUtc, WLV_SystemCreateTimeUtc)
					.GetInfo(),

				IndexInfo.Builder.New(SqlSchemaName, Name, FKRXIndexName(WLV_WLT_LocationType))
					.Unique(false)
					.Clustered(false)
					.Key(WLV_WLT_LocationType)
					.GetInfo(),

				IndexInfo.Builder.New(SqlSchemaName, Name, FKRXIndexName(WLV_WA_PutawayArea))
					.Unique(false)
					.Clustered(false)
					.Key(WLV_WA_PutawayArea)
					.GetInfo(),

				IndexInfo.Builder.New(SqlSchemaName, Name, FKRXIndexName(WLV_WA_PickingArea))
					.Unique(false)
					.Clustered(false)
					.Key(WLV_WA_PickingArea)
					.GetInfo(),

				IndexInfo.Builder.New(SqlSchemaName, Name, FKRXIndexName(WLV_WR))
					.Unique(false)
					.Clustered(false)
					.Key(WLV_WR)
					.GetInfo(),

				IndexInfo.Builder.New(SqlSchemaName, Name, FKRXIndexName(WLV_SQ_DefaultPrintQueue))
					.Unique(false)
					.Clustered(false)
					.Key(WLV_SQ_DefaultPrintQueue)
					.GetInfo(),
			};
		}

		string FKRXIndexName(string columnName) => $"FK_RX__{columnName}"; // index name and column names for creating index
	}
}
