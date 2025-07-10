using System.Data;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles
{
	public class ConvertJobServiceCompletedDateTimeToDateTimeOffset : ConvertDateTimeToDateTimeOffsetTransform
	{
		public override string UserDescription => "Convert JobService ES_Completed DateTime to DateTimeOffset and store the new value as ES_CompletedDateTimeOffset.";

		protected override SchemaDateTimeOffsetColumn ColumnToConvert => JobServiceSchema.ES_CompletedDateTimeOffset;

		protected override SchemaDateTimeColumn SourceColumn => JobServiceSchema.ES_Completed;

		protected override SqlDbType DateTimeTypeBeforeConversion => SqlDbType.SmallDateTime;

		protected override bool HasTransformationRanBasedOnSchema(SchemaColumn dateColumn)
		{
			return DbObjectCreator.ComputedColumnExists(Db.Connection, JobServiceSchema.Constants.SqlSchemaName, dateColumn.TableName, dateColumn.Name);
		}

		protected override string GetSubqueryForTimeZoneColumn(string timeZoneColumnName)
		{
			return Invariant($@"
SELECT 
	CASE
		WHEN ES_OA_Location IS NOT NULL
		THEN
		(
			SELECT
				OA_RL_NKRelatedPortCode
			FROM
				dbo.OrgAddress
			WHERE
				OA_PK = ES_OA_Location
		)
		WHEN ES_ParentTableCode = 'WD'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.WhsDocket
				JOIN dbo.WhsWarehouse ON WD_WW_Whs = WW_PK
				JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
			WHERE
				WD_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'WVO'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.WhsVASOrder
				JOIN dbo.WhsArea ON WA_PK = WVO_WA_ServiceArea
				JOIN dbo.WhsWarehouse ON WA_WW_Whs = WW_PK
				JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
			WHERE
				WVO_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'WSJ'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.WhsAdHocServiceJob
				JOIN dbo.WhsWarehouse ON WSJ_WW_Whs = WW_PK
				JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
			WHERE
				WSJ_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'CO'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.CusContainer
				JOIN dbo.JobDeclaration ON CO_ClusterKey = JE_ClusterKey
				JOIN dbo.GlbBranch ON JE_GB = GB_PK
			WHERE
				CO_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'BH'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.CusInBondHeader
				JOIN dbo.GlbBranch ON BH_GB = GB_PK
			WHERE
				BH_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'KM'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.DtbBooking
				JOIN dbo.GlbBranch ON KM_GB_Branch = GB_PK
			WHERE
				KM_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'LTC'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.DtbConsignment
				JOIN dbo.GlbBranch ON LTC_GB_Branch = GB_PK
			WHERE
				LTC_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'KG'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.DtbConsignmentRunSheet
				JOIN dbo.GlbBranch ON KG_GB_Branch = GB_PK
			WHERE
				KG_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'EW'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.JobBookedCtgMove
				JOIN dbo.JobCartage ON EW_JJ = JJ_PK
				JOIN dbo.GlbBranch ON JJ_GB = GB_PK
			WHERE
				EW_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'WDC'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.WhsItemDispatchConsignment
				JOIN dbo.WhsWarehouse ON WDC_WW_Warehouse = WW_PK
				JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
			WHERE
				WDC_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'WRC'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.WhsItemReceiveConsignment
				JOIN dbo.WhsWarehouse ON WRC_WW_IntendedWarehouse = WW_PK
				JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
			WHERE
				WRC_PK = ES_ParentID
		)
		WHEN ES_ParentTableCode = 'WKI'
		THEN
		(
			SELECT
				GB_RL_NKHomePort
			FROM
				dbo.WorkItem
				JOIN dbo.GlbBranch ON WKI_GB_AssignedBranch = GB_PK
			WHERE
				WKI_PK = ES_ParentID
		)
	END AS {timeZoneColumnName}");
		}

		protected override IndexInfo GetSupportingIndex(TransformationIndexProvider indexProvider)
		{
			return indexProvider.New(JobServiceSchema.Instance)
				.Key(JobServiceSchema.Constants.ES_Completed)
				.Include(JobServiceSchema.Constants.ES_OA_Location, JobServiceSchema.Constants.ES_ParentID, JobServiceSchema.Constants.ES_ParentTableCode, "ES_AutoVersion")
				.Where("[ES_Completed] IS NOT NULL")
				.GetInfo();
		}
	}
}
