namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.Data;
	class TimeDimensionLoader
	{
		public TimeDimensionLoader(DateTime loadEndDateExclusive)
		{
			this.loadEndDateExclusive = loadEndDateExclusive;
		}

		readonly DateTime loadEndDateExclusive;

		public void PopulateDates()
		{
			using (var stlDwConnection = DbManager.OpenNewStlAnalysisDataWarehouseConnection())
			{
				try
				{
					using (var stlDwTransaction = stlDwConnection.BeginTransaction())
					{
						InsertNewDates(stlDwTransaction);
						stlDwTransaction.Commit();
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Error populating date dimension. " + ex.Message);
				}
			}
		}

		void InsertNewDates(SqlTransaction stlDwTransaction)
		{
			string sqlText = EtlController.GetLoadClientScript("PopulateDimDate.sql");

			using (var insertCmd = DbManager.NewSqlCommand(sqlText, stlDwTransaction))
			{
				insertCmd.Parameters.Add("@StartDateInclusive", SqlDbType.Date).Value = new DateTime(2015, 1, 1);
				insertCmd.Parameters.Add("@EndDateExclusive", SqlDbType.Date).Value = loadEndDateExclusive;
				insertCmd.ExecuteNonQuery();
			}
		}
	}
}
