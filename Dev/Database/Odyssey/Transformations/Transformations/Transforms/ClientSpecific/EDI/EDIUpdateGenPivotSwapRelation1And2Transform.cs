using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	class EDIUpdateGenPivotSwapRelation1And2Transform : DataTransformation
	{
		public override string UserDescription => "Swap XX_Relation1TableCode, XX_Relation1ID and XX_Relation2TableCode, XX_Relation2ID column's value when [XX_RelationType = 'WRK' and XX_Relation1TableCode = 'WKI' and XX_Relation2TableCode = 'IM'] in Table:GenPivot";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, "GenPivot") &&
				DbObjectCreator.TableExists(Db.Connection, "EdiBilledUsage"))
			{
				var sql = @"
	BEGIN TRY
		-- total 88 lines (ediprod), the SQL query executes quickly.
		delete from GenPivot where XX_PK in (
			select a.XX_PK from GenPivot a
			left join GenPivot b
			on a.XX_RelationType = 'WRK' 
			AND a.XX_Relation1TableCode = 'WKI' 
			AND a.XX_Relation2TableCode = 'IM'
			and b.XX_RelationType = 'WRK'
			and b.XX_Relation1TableCode = 'IM'
			and b.XX_Relation2TableCode = 'WKI'
			and a.XX_Relation1ID = b.XX_Relation2ID
			and a.XX_Relation2ID = b.XX_Relation1ID
			where b.XX_PK is not null
		);

		-- total 30K+ lines (ediprod), the SQL query executes quickly.
		UPDATE 
		  GenPivot 
		SET 
		  XX_Relation1TableCode = XX_Relation2TableCode, 
		  XX_Relation2TableCode = XX_Relation1TableCode, 
		  XX_Relation1ID = XX_Relation2ID, 
		  XX_Relation2ID = XX_Relation1ID,
		  XX_SystemLastEditTimeUtc = GETUTCDATE(),
		  XX_SystemLastEditUser = '~BP',
		  XX_AutoVersion = (XX_AutoVersion + 1) % 32768
		WHERE 
		  XX_RelationType = 'WRK' 
		  AND XX_Relation1TableCode = 'WKI' 
		  AND XX_Relation2TableCode = 'IM';
	END TRY
	BEGIN CATCH
		THROW;
	END CATCH
";
				using (DataTransformationHelper.SuspendTriggerIfExists("TG_GenPivot_UpdateAutoVersion", GenPivotSchema.Constants.TableName))
				{
					Db.Connection.ExecuteNonQuery(sql);
				}
			}
		}
	}
}
