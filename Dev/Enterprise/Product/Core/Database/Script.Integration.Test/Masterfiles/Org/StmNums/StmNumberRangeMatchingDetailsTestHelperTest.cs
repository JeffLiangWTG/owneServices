using System;
using System.Data;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.StmNums.Testing
{
	class StmNumberRangeMatchingDetailsTestHelper
	{
		readonly DbConnection connection;

		public StmNumberRangeMatchingDetailsTestHelper(DbConnection connection)
		{
			this.connection = connection;
		}

		#region ExistsInTable

		public bool ExistsInTable(Guid ownerPK, string ownerTableCode, string rangeType, string prefix)
		{
			var commandText = string.Format("SELECT COUNT(*) FROM dbo.StmNumberRangeMatchingDetail WHERE NRM_OwnerId = '{0}' AND NRM_OwnerTableCode = '{1}' AND NRM_RangeType = '{2}' AND NRM_Prefix = '{3}'", ownerPK, ownerTableCode, rangeType, prefix);
			var result = connection.ExecuteScalar(commandText);

			return (int)result != 0;
		}

		#endregion

		#region InsertStmNumberRangeMatchingDetails

		public Guid InsertStmNumberRangeMatchingDetails(Guid ownerPK, string ownerTableCode, string rangeType, string prefix)
		{
			var pk = Guid.NewGuid();
			string insertCommandText = "INSERT INTO dbo.StmNumberRangeMatchingDetail (NRM_PK, NRM_OwnerId, NRM_OwnerTableCode, NRM_RangeType, NRM_Prefix) VALUES (@PK, @ownerPK, @ownerTableCode, @rangeType, @prefix)";

			using (var command = connection.Command(insertCommandText))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@ownerPK", SqlDbType.UniqueIdentifier, ownerPK);
				command.AddParameter("@ownerTableCode", SqlDbType.VarChar, ownerTableCode);
				command.AddParameter("@rangeType", SqlDbType.VarChar, rangeType);
				command.AddParameter("@prefix", SqlDbType.VarChar, prefix);

				command.ExecuteNonQuery();
			}

			if (!ExistsInTable(ownerPK, ownerTableCode, rangeType, prefix))
			{
				throw new InvalidOperationException("Problem to insert InsertStmNumberRangeMatchingDetails");
			}

			return pk;
		}

		#endregion

		#region UpdateStmNumberRangeMatchingDetails_Prefix

		public void UpdateStmNumberRangeMatchingDetails_Prefix(Guid pK, string rangeType, string prefix)
		{
			var updateCommandText = "UPDATE dbo.StmNumberRangeMatchingDetail SET NRM_Prefix = @prefix , NRM_RangeType = @rangeType  WHERE NRM_PK = @PK";

			using (var command = connection.Command(updateCommandText))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pK);
				command.AddParameter("@rangeType", SqlDbType.VarChar, rangeType);
				command.AddParameter("@prefix", SqlDbType.VarChar, prefix);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region DropConstraintRangeType

		public void DropConstraintRangeType()
		{
			var sqlDropConstraint = @"
IF OBJECTPROPERTY(OBJECT_ID('Constraint_NRM_RangeType'), 'IsConstraint') = 1
	BEGIN
		ALTER Table dbo.StmNumberRangeMatchingDetail DROP CONSTRAINT Constraint_NRM_RangeType
	END
";
			using (var command = connection.Command(sqlDropConstraint))
			{
				command.ExecuteNonQuery();
			}
		}
		#endregion
	}
}

