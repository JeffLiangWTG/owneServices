using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.PostUpgrade.Public.Customs.ES
{
	public class ChangeCusInBondMoveHeaderBMPhaseInArrivalToTSAWhereInTS : DataTransformation
	{
		public override string UserDescription => "Update BM_Phase to TSA CusInBondMoveHeader table.";

		bool HasESCompany => Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'ES'");

		void CreateAdditionalIndex()
		{
			var sql = @"IF NOT EXISTS (SELECT Name FROM sys.indexes WHERE name='IX_UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival_BH_ApplicationCode_BH_HeaderType' AND object_id = OBJECT_ID('dbo.CusInbondHeader'))
				CREATE INDEX IX_UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival_BH_ApplicationCode_BH_HeaderType ON dbo.CusInbondHeader (BH_ApplicationCode, BH_HeaderType) INCLUDE ([BH_PK],[BH_GB])
				WHERE BH_ApplicationCode = 'NC5' AND BH_HeaderType = 'A'
				WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)";
			Db.Connection.Command(sql).ExecuteScalar();
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (HasESCompany)
			{
				CreateAdditionalIndex();

				var sql = @"UPDATE dbo.CusInBondMoveHeader SET BM_Phase = 'TSA',
					BM_SystemLastEditTimeUtc = GetUtcDate(), BM_SystemLastEditUser = 'E'
					FROM dbo.CusInBondMoveHeader
					INNER JOIN dbo.CusInBondHeader ON BM_BH = BH_PK AND BH_ApplicationCode = 'NC5' AND BH_HeaderType = 'A'
					INNER JOIN dbo.GlbBranch ON GB_PK = BH_GB AND GB_RN_NKCountryCode = 'ES'
					INNER JOIN dbo.CusEntryNum ON CE_ParentID = BM_BH AND CE_EntryType = 'SUM' 
					INNER JOIN dbo.CusTempStorageRegHeader ON SRH_Reference = CE_EntryNum
					WHERE BM_Phase <> 'TSA'";
				Db.Connection.Command(sql).ExecuteScalar();

				DropIndexIfExists();
			}
		}

		void DropIndexIfExists()
		{
			var dropIndexQuery = "DROP INDEX IF EXISTS IX_UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival_BH_ApplicationCode_BH_HeaderType ON dbo.CusInbondHeader";
			Db.Connection.Command(dropIndexQuery).ExecuteScalar();
		}
	}
}
