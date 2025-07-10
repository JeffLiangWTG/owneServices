using System;
using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.TestFramework
{
	public class ViewStmNumsTestHelper
	{
		readonly DbConnection connection;

		public ViewStmNumsTestHelper(DbConnection connection)
		{
			this.connection = connection;
		}

		#region SELECT

		public bool ExistsInView(Guid stmNumsPK)
		{
			var commandText = string.Format("SELECT COUNT(*) FROM dbo.ViewStmNums WHERE SN_PK = '{0}'", stmNumsPK);
			var result = connection.ExecuteScalar(commandText);

			return (int)result != 0;
		}

		public bool ExistsInView(string type, string prefix)
		{
			var commandText = string.Format("SELECT COUNT(*) FROM dbo.ViewStmNums WHERE SN_Type = '{0}' AND SN_Prefix = '{1}'", type, prefix);
			var result = connection.ExecuteScalar(commandText);

			return (int)result != 0;
		}

		public bool ExistsInView(Guid owner, string name, string type, string prefix)
		{
			var commandText = string.Format("SELECT COUNT(*) FROM dbo.ViewStmNums WHERE SN_Owner = '{0}' AND SN_Name = '{1}' AND SN_Type = '{2}' AND SN_Prefix = '{3}'", owner, name, type, prefix);
			var result = connection.ExecuteScalar(commandText);

			return (int)result != 0;
		}

		public bool ExistsInTable(Guid ownerPK, string name, long minValue, long maxValue)
		{
			var commandText = string.Format("SELECT COUNT(*) FROM dbo.StmNums WHERE SN_Owner = '{0}' AND SN_Name = '{1}' AND SN_MinimumValue = {2} AND SN_MaximumValue = {3}", ownerPK, name, minValue, maxValue);
			var result = connection.ExecuteScalar(commandText);

			return (int)result != 0;
		}

		public bool ExistsInTable(string name)
		{
			var commandText = string.Format("SELECT COUNT(*) FROM dbo.StmNums WHERE SN_Name = '{0}' ", name);
			var result = connection.ExecuteScalar(commandText);

			return (int)result != 0;
		}

		public bool ExistsInTable(Guid ownerPK, string name, long minValue, long maxValue, long currentValue)
		{
			var commandText = string.Format("SELECT COUNT(*) FROM dbo.StmNums WHERE SN_Owner = '{0}' AND SN_Name = '{1}' AND SN_MinimumValue = {2} AND SN_MaximumValue = {3} AND SN_Value = {4}",
				ownerPK, name, minValue, maxValue, currentValue);
			var result = connection.ExecuteScalar(commandText);

			return (int)result != 0;
		}

		public int GetStmNumsId(Guid viewId)
		{
			const string sqlText = "SELECT TOP 1 SN_Id FROM dbo.ViewStmNums WHERE SN_PK=@viewId";
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@viewId", SqlDbType.UniqueIdentifier, viewId);
				return Convert.ToInt32(cmd.ExecuteScalar());
			}
		}

		#endregion

		#region INSERT

		public Guid InsertView(Guid ownerPK, string name, long minValue, long maxValue, int canRollover, long? value = null, short sequence = 0, bool expectCountExists = true)
		{
			const string insertCommandText = "INSERT dbo.ViewStmNums (SN_Owner, SN_Name, SN_Value, SN_MinimumValue, SN_MaximumValue, SN_CanRollover, SN_Sequence) VALUES (@ownerPK, @name, @value, @minValue, @maxValue, @canRollover, @sequnce)";

			using (var command = connection.Command(insertCommandText))
			{
				command.AddParameter("@ownerPK", SqlDbType.UniqueIdentifier, ownerPK);
				command.AddParameter("@name", SqlDbType.VarChar, name);
				command.AddParameter("@value", SqlDbType.BigInt, value ?? minValue);
				command.AddParameter("@minValue", SqlDbType.BigInt, minValue);
				command.AddParameter("@maxValue", SqlDbType.BigInt, maxValue);
				command.AddParameter("@canRollover", SqlDbType.Bit, canRollover);
				command.AddParameter("@sequnce", SqlDbType.SmallInt, sequence);

				command.ExecuteNonQuery();
			}

			var selectCommandText = string.Format("SELECT SN_PK FROM dbo.ViewStmNums WHERE SN_Owner = '{0}' AND SN_Name = '{1}' AND SN_MinimumValue = {2} AND SN_MaximumValue = {3}", ownerPK, name, minValue, maxValue);
			var table = DataUtils.GetDataTableFromQuery(connection, selectCommandText);
			if (expectCountExists)
			{
				if (table.Rows.Count != 1)
				{
					throw new InvalidOperationException("Expected row count to be 1 but was " + table.Rows.Count + ".");
				}

				return DataUtils.GetPk(table.Rows[0]);
			}
			else
			{
				if (table.Rows.Count != 0)
				{
					throw new InvalidOperationException("Expected row count to be 0 but was " + table.Rows.Count + ".");
				}
				return Guid.Empty;
			}
		}

		#endregion

		#region UPDATE

		public void UpdateView(Guid pK, int minValue, int maxValue)
		{
			var updateCommandText = "UPDATE dbo.ViewStmNums SET SN_MinimumValue = @minValue, SN_MaximumValue = @maxValue WHERE SN_PK = @PK";

			using (var command = connection.Command(updateCommandText))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pK);
				command.AddParameter("@minValue", SqlDbType.Int, minValue);
				command.AddParameter("@maxValue", SqlDbType.Int, maxValue);
				command.ExecuteNonQuery();
			}
		}

		public void UpdateStmNumsName(string @oldname, string name)
		{
			var updateCommandText = "UPDATE dbo.StmNums SET SN_Name = @name WHERE SN_Name = @oldname";

			if (!ExistsInTable(@oldname))
			{
				throw new InvalidOperationException("this name not exists to update.");
			}

			using (var command = connection.Command(updateCommandText))
			{
				command.AddParameter("@oldname", SqlDbType.VarChar, oldname);
				command.AddParameter("@name", SqlDbType.VarChar, name);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region DELETE

		public void DeleteView(Guid stmNumsPK)
		{
			var deleteCommandText = string.Format("DELETE dbo.ViewStmNums WHERE SN_PK = '{0}'", stmNumsPK);
			using (var command = connection.Command(deleteCommandText))
			{
				command.ExecuteNonQuery();
			}
		}
		public void DeleteView(string sn_name)
		{
			var deleteCommandText = string.Format("DELETE dbo.ViewStmNums WHERE SN_Name = '{0}'", sn_name);
			using (var command = connection.Command(deleteCommandText))
			{
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region CACHE

		public void InsertCacheNumber(Guid ownerPK, string name)
		{
			var selectFountainIDSQL = string.Format("SELECT SN_ID FROM dbo.StmNums WHERE SN_Owner = '{0}' AND SN_Name = '{1}'", ownerPK, name);
			int fountainID = (int)connection.ExecuteScalar(selectFountainIDSQL);

			Assertion.Assert("Number fountain exist", fountainID > 0);

			var insertNumberCacheSQL = "INSERT dbo.StmNumberCache (SG_Value, SG_SN) VALUES (@value, @fountainID)";
			using (var command = connection.Command(insertNumberCacheSQL))
			{
				command.AddParameter("@value", SqlDbType.BigInt, 10);
				command.AddParameter("@fountainID", SqlDbType.Int, fountainID);
				command.ExecuteNonQuery();
			}
		}

		public bool CheckIfHasCacheNumbers(Guid ownerPK, string name)
		{
			var selectNumberCacheSQL = string.Format(@"
SELECT COUNT(*) FROM dbo.StmNumberCache
JOIN dbo.StmNums ON SN_ID = SG_SN
WHERE SN_Owner = '{0}' AND SN_Name = '{1}'", ownerPK, name);

			var result = connection.ExecuteScalar(selectNumberCacheSQL);
			return (int)result != 0;
		}
		#endregion
	}
}

