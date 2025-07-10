using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	class UpdateCA_K84AccountingDateOnLVXDeclaration : DataTransformation
	{
		public override string UserDescription => "Online Update CA_K84AccountingDate On LVX Declaration";

		public UpdateCA_K84AccountingDateOnLVXDeclaration()
			: this(5000)
		{
		}

		internal UpdateCA_K84AccountingDateOnLVXDeclaration(int batchSize)
		{
			this.batchSize = batchSize;
		}
		readonly int batchSize;
		internal const string LastClusterKeyWaterMark = "UpdateCA_K84AccountingDateOnLVXDeclaration_LastClusterKey";
		internal const string UpdatedDeclarationNumMark = "UpdateCA_K84AccountingDateOnLVXDeclaration_UpdatedDeclarationNum";
		internal const string InsertGenAddOnColumnNumMark = "UpdateCA_K84AccountingDateOnLVXDeclaration_InsertGenAddOnColumnNum";
		internal const string UpdatedGenAddOnColumnNumMark = "UpdateCA_K84AccountingDateOnLVXDeclaration_UpdatedGenAddOnColumnNum";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var updatedDeclarationNum = 0;
			var insertGenAddOnColumnNum = 0;
			var updatedGenAddOnColumnNum = 0;
			if (!int.TryParse(ExtProperty.Database.Select(Db.Connection, LastClusterKeyWaterMark), out var lastClusterKey))
			{
				lastClusterKey = GetMaxClusterKey();
				if (lastClusterKey > 0)
				{
					var createIndexQuery = @"
IF NOT EXISTS (SELECT Name FROM sys.indexes WHERE name='GenAddOnColumn_Index_For_Online_Update_K84AccountingDate' AND object_id = OBJECT_ID('dbo.GenAddOnColumn'))
	CREATE INDEX [GenAddOnColumn_Index_For_Online_Update_K84AccountingDate] ON [dbo].[GenAddOnColumn] ([XA_ParentID], [XA_ParentTableCode], [XA_Name]) INCLUDE ([XA_Data])
	WHERE ([XA_ParentTableCode]='JE' AND [XA_Name]='CA_K84AccountingDate') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON);
";

					Db.Connection.ExecuteNonQuery(createIndexQuery);
				}
			}
			else
			{
				int.TryParse(ExtProperty.Database.Select(Db.Connection, UpdatedDeclarationNumMark), out updatedDeclarationNum);
				int.TryParse(ExtProperty.Database.Select(Db.Connection, InsertGenAddOnColumnNumMark), out insertGenAddOnColumnNum);
				int.TryParse(ExtProperty.Database.Select(Db.Connection, UpdatedGenAddOnColumnNumMark), out updatedGenAddOnColumnNum);
			}

			while (lastClusterKey > 0)
			{
				var processResult = ProcessBatch(lastClusterKey);

				updatedDeclarationNum += processResult.UpdatedDeclarationNum;
				insertGenAddOnColumnNum += processResult.InsertGenAddOnColumnNum;
				updatedGenAddOnColumnNum += processResult.UpdatedGenAddOnColumnNum;

				lastClusterKey = Math.Max(lastClusterKey - batchSize, 0);
				if (lastClusterKey > 0)
				{
					ExtProperty.Database.Update(Db.Connection, LastClusterKeyWaterMark, lastClusterKey.ToString());
					ExtProperty.Database.Update(Db.Connection, UpdatedDeclarationNumMark, updatedDeclarationNum.ToString());
					ExtProperty.Database.Update(Db.Connection, InsertGenAddOnColumnNumMark, insertGenAddOnColumnNum.ToString());
					ExtProperty.Database.Update(Db.Connection, UpdatedGenAddOnColumnNumMark, updatedGenAddOnColumnNum.ToString());
				}
				token.ThrowIfCancellationRequested();
			}

			manager?.ShowInfoMessage($"updated declarations [{updatedDeclarationNum}], insert GenAddOnColumn [{insertGenAddOnColumnNum}], updated GenAddOnColumn [{updatedGenAddOnColumnNum}].");

			ExtProperty.Database.Delete(Db.Connection, LastClusterKeyWaterMark);
			ExtProperty.Database.Delete(Db.Connection, UpdatedDeclarationNumMark);
			ExtProperty.Database.Delete(Db.Connection, InsertGenAddOnColumnNumMark);
			ExtProperty.Database.Delete(Db.Connection, UpdatedGenAddOnColumnNumMark);
			DropIndexIfExists();
		}

		(int UpdatedDeclarationNum, int InsertGenAddOnColumnNum, int UpdatedGenAddOnColumnNum) ProcessBatch(int lastClusterKey)
		{
			var sql = $@"
DECLARE @DeclarationUpdateRows INT;
DECLARE @GenAddOnColumnNewRows INT;
DECLARE @GenAddOnColumnUpdateRows INT;

CREATE TABLE #LVXNeedsUpdate (
    PK UNIQUEIDENTIFIER,
    LVSDate VARCHAR(30) COLLATE SQL_Latin1_General_CP1_CI_AS,
    LVXDate VARCHAR(30)
);

INSERT INTO #LVXNeedsUpdate (PK, LVSDate, LVXDate)
SELECT 
    LVXDeclaration.JE_PK,
    LVSAddOnColumn.XA_Data,
    LVXAddOnColumn.XA_Data
FROM 
    dbo.JobDeclaration LVXDeclaration
    JOIN dbo.JobComInvoiceHeader ON JZ_JE = JE_PK
    JOIN dbo.GenPivot ON XX_Relation1ID = JZ_PK
    JOIN dbo.CAJobDeclaration LVSDeclaration ON XX_Relation2ID = LVSDeclaration.JE_PK
    JOIN dbo.GenAddOnColumn LVSAddOnColumn ON XA_ParentID = LVSDeclaration.JE_PK
    LEFT JOIN dbo.GenAddOnColumn LVXAddOnColumn ON LVXAddOnColumn.XA_ParentID = LVXDeclaration.JE_PK AND LVXAddOnColumn.XA_ParentTableCode = 'JE' AND LVXAddOnColumn.XA_Name = 'CA_K84AccountingDate'
WHERE
    LVSAddOnColumn.XA_ParentTableCode = 'JE'
    AND LVSAddOnColumn.XA_Name = 'CA_K84AccountingDate'
    AND LVSDeclaration.JE_MessageType = 'LVS'
    AND LVSDeclaration.JE_DataModel = 'CA'
    AND LVXDeclaration.JE_MessageType = 'LVX'
    AND LVXDeclaration.JE_DataModel = 'CA'
    AND XX_RelationType = 'ZE'
    AND XX_Relation1TableCode = 'JZ'
    AND XX_Relation2TableCode = 'JE'
    AND (LVXAddOnColumn.XA_Data IS NULL OR LVSAddOnColumn.XA_Data <> LVXAddOnColumn.XA_Data)
    AND LVXDeclaration.JE_ClusterKey BETWEEN (@endClusterKey - @batchSize + 1) AND @endClusterKey;

UPDATE LVXDeclaration
SET
    JE_AddInfo = CONCAT(
        LVXAddInfo.AddInfoValue, 
        IIF(LVXAddInfo.AddInfoValue = '', 'K84AccountingDate=', '*K84AccountingDate='), 
        CONVERT(VARCHAR, TableToUpdate.LVSDate, 121)
    ),
    JE_SystemLastEditUser = '~BP',
    JE_SystemLastEditTimeUTC = GETUTCDATE()
FROM 
    dbo.JobDeclaration LVXDeclaration
    JOIN #LVXNeedsUpdate AS TableToUpdate ON LVXDeclaration.JE_PK = TableToUpdate.PK
    CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(LVXDeclaration.JE_AddInfo, 'K84AccountingDate') AS LVXAddInfo
WHERE 
    LVXDeclaration.JE_MessageType = 'LVX'
    AND LVXDeclaration.JE_DataModel = 'CA';
SET @DeclarationUpdateRows = @@ROWCOUNT

INSERT INTO dbo.GenAddOnColumn
    (XA_PK, XA_Name, XA_Type, XA_Data, XA_ParentTableCode, XA_ParentID, XA_SystemCreateTimeUtc, XA_SystemCreateUser, XA_SystemLastEditTimeUtc, XA_SystemLastEditUser)
SELECT
    NEWID(), 'CA_K84AccountingDate', 'DAT', LVSDate, 'JE', PK, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'
FROM
    #LVXNeedsUpdate
WHERE
    LVXDate IS NULL;
SET @GenAddOnColumnNewRows = @@ROWCOUNT

UPDATE GenAddOnColumn
SET
    XA_Data = TableToUpdate.LVSDate,
    XA_SystemLastEditTimeUtc = GETUTCDATE(), 
    XA_SystemLastEditUser = '~BP'
FROM 
    #LVXNeedsUpdate AS TableToUpdate
WHERE
    LVXDate IS NOT NULL
    AND TableToUpdate.PK = XA_ParentID
    AND XA_ParentTableCode = 'JE' 
    AND XA_Name = 'CA_K84AccountingDate';
SET @GenAddOnColumnUpdateRows = @@ROWCOUNT

SELECT @DeclarationUpdateRows, @GenAddOnColumnNewRows, @GenAddOnColumnUpdateRows";
			int updatedDeclarationNum, insertGenAddOnColumnNum, updatedGenAddOnColumnNum;
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@batchSize", SqlDbType.Int, batchSize);
				cmd.AddParameter("@endClusterKey", SqlDbType.Int, lastClusterKey);
				using (var reader = cmd.ExecuteReader())
				{
					reader.Read();
					updatedDeclarationNum = (int)reader[0];
					insertGenAddOnColumnNum = (int)reader[1];
					updatedGenAddOnColumnNum = (int)reader[2];
				}
			}
			return (updatedDeclarationNum, insertGenAddOnColumnNum, updatedGenAddOnColumnNum);
		}

		int GetMaxClusterKey()
		{
			var result = 0;
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'"))
			{
				result = Db.Connection.ExecuteScalar<int>(@"
SELECT ISNULL(MAX(JE_ClusterKey), 0) maxClusterKey
FROM dbo.JobDeclaration WHERE JE_MessageType = 'LVX' AND JE_DataModel = 'CA'");
			}
			return result;
		}

		void DropIndexIfExists()
		{
			var dropIndexQuery = "DROP INDEX IF EXISTS GenAddOnColumn_Index_For_Online_Update_K84AccountingDate ON dbo.GenAddOnColumn";
			Db.Connection.ExecuteNonQuery(dropIndexQuery);
		}
	}
}
