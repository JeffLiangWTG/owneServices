using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	sealed class LandedCostTables_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on Landed Cost tables XX_ParentTableCode column";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, LandCostInputSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, LandedCostHeaderSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, LandedCostHistorySchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(TransformScript);
			}
		}

		const string TransformScript = """
		                               BEGIN TRY
		                               
		                               	DELETE
		                               	FROM [dbo].[LandCostInput]
		                               	WHERE LI_ParentTableCode NOT IN ('CO', 'JD', 'JI', 'JO', 'JZ');
		                               
		                               	DELETE
		                               	FROM [dbo].[LandedCostHistory]
		                               	WHERE LH_ParentTableCode NOT IN ('JI', 'JO');
		                               
		                               	DELETE
		                               	FROM [dbo].[LandedCostHistory]
		                               	WHERE LH_ParentID IS NULL;
		                               
		                               	DROP TABLE IF EXISTS #LandedCostHeaderToDelete;
		                               
		                               	SELECT LT_PK
		                               	INTO #LandedCostHeaderToDelete
		                               	FROM [dbo].[LandedCostHeader]
		                               	WHERE LT_ParentTableCode NOT IN ('JD', 'JE');
		                               
		                                IF EXISTS (SELECT NULL FROM #LandedCostHeaderToDelete)
		                                BEGIN
		                               
		                               	  DELETE
		                               	  FROM [dbo].[LandCostInput]
		                               	  WHERE LI_LT IN (SELECT LT_PK
		                               					FROM #LandedCostHeaderToDelete);
		                               
		                               	  DELETE
		                               	  FROM [dbo].[LandedCostHistory]
		                               	  WHERE LH_LT IN (SELECT LT_PK
		                               					FROM #LandedCostHeaderToDelete);
		                               
		                               	  DELETE
		                               	  FROM [dbo].[LandedCostHeader]
		                               	  WHERE LT_PK IN (SELECT LT_PK
		                               					FROM #LandedCostHeaderToDelete);
		                                END
		                               
		                               END TRY
		                               BEGIN CATCH
		                                THROW;
		                               END CATCH
		                               """;

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(LandCostInputSchema.Instance)
					.Key(LandCostInputSchema.Constants.LI_ParentID)
					.Include(LandCostInputSchema.Constants.LI_SystemCreateTimeUtc)
					.Include(LandCostInputSchema.Constants.LI_SystemLastEditTimeUtc)
					.Include(LandCostInputSchema.Constants.LI_AC_ChargeCode)
					.Where("[LI_ParentTableCode]<>'CO' AND [LI_ParentTableCode]<>'JD' AND [LI_ParentTableCode]<>'JI' AND [LI_ParentTableCode]<>'JO' AND [LI_ParentTableCode]<>'JZ'")
					.GetInfo();
				indexProvider.New(LandedCostHeaderSchema.Instance)
					.Key(LandedCostHeaderSchema.Constants.LT_ParentID)
					.Include(LandedCostHeaderSchema.Constants.LT_SystemCreateTimeUtc)
					.Include(LandedCostHeaderSchema.Constants.LT_SystemLastEditTimeUtc)
					.Where("[LT_ParentTableCode]<>'JD' AND [LT_ParentTableCode]<>'JE'")
					.GetInfo();
				indexProvider.New(LandedCostHistorySchema.Instance)
					.Key(LandedCostHistorySchema.Constants.LH_ParentID)
					.Include(LandedCostHistorySchema.Constants.LH_SystemCreateTimeUtc)
					.Include(LandedCostHistorySchema.Constants.LH_SystemLastEditTimeUtc)
					.Include(LandedCostHistorySchema.Constants.LH_OP)
					.Where("[LH_ParentTableCode]<>'JI' AND [LH_ParentTableCode]<>'JO'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
