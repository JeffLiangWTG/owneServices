using System;
using System.Data;
using CargoWise.Data;

namespace CargoWise.Bi.Common.Testing
{
	public class BiTemporaryMasterState : IDisposable
	{
		readonly string previousValue;
		readonly DbConnection connection;
		readonly string paramName;

		BiTemporaryMasterState(DbConnection connection, string paramName, string paramValue)
		{
			this.connection = connection;
			this.paramName = paramName;
			this.previousValue = BiMasterState.GetParameter(connection, paramName);
			SetParameter(connection, paramName, paramValue);
		}

		void SetParameter(DbConnection connection, string paramName, string paramValue)
		{
			if (paramValue != null)
			{
				BiMasterState.SetParameter(connection, paramName, paramValue);
			}
			else
			{
				var sqlText = @"
					IF EXISTS (SELECT * FROM [biadmin].[MasterState] WHERE ParamName = @ParamName)
					BEGIN
						UPDATE [biadmin].[MasterState] SET ParamValue = NULL WHERE ParamName = @ParamName
					END ELSE
					BEGIN
						INSERT INTO [biadmin].[MasterState] (ParamName, ParamValue)
						VALUES(@ParamName, NULL)
					END
				";

				using (var cmd = connection.Command(sqlText))
				{
					cmd.AddParameter("@ParamName", SqlDbType.NVarChar, paramName);
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void Dispose()
		{
			BiMasterState.SetParameter(this.connection, this.paramName, this.previousValue);
		}

		internal static BiTemporaryMasterState NewTemporaryValue(DbConnection connection, string paramName, string paramValue)
		{
			return new BiTemporaryMasterState(connection, paramName, paramValue);
		}

		public static BiTemporaryMasterState SetParameterTemporaryValue(DbConnection connection, string paramName, string paramValue)
		{
			return NewTemporaryValue(connection, paramName, paramValue);
		}
	}
}
