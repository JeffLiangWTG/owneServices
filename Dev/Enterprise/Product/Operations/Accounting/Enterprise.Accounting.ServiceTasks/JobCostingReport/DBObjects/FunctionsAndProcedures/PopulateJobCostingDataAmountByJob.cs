using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects
{
	class PopulateJobCostingDataAmountByJob : JCDDependentDBObject, IDeleteDataWhenRemovePartition
	{
		public PopulateJobCostingDataAmountByJob()
			: base(JCDDBObjectInfoList.Instance["PopulateJobCostingDataAmountByJob"])
		{
		}

		public override string Name => FormattableString.Invariant($"INSERT INTO RptDtJobCostingDataAmountByJob");

		public override string CreateSQLText
			=> GetEmbeddedResourceText("Enterprise.Accounting.ServiceTasks.JobCostingReport.DBObjects.FunctionsAndProcedures.PopulateJobCostingDataAmountByJob.sql");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL command")]
		public override string CheckAndDropSQLText => @"
IF OBJECT_ID(N'dbo.RptDtJobCostingDataAmountByJob') IS NOT NULL
BEGIN
  TRUNCATE TABLE dbo.RptDtJobCostingDataAmountByJob;
END
";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL command")]
		void IDeleteDataWhenRemovePartition.SetCommandForRemovePartition(DbCommand command, PartitionInfo pi)
		{
			command.CommandType = CommandType.Text;
			command.CommandText = "DELETE FROM RptDtJobCostingDataAmountByJob WHERE JCA_GC = @companyPk AND JCA_PostPeriod = @period";
			command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, pi.AM_GC_Company);
			command.AddParameter("@period", SqlDbType.Int, pi.AM_Period);
		}
	}
}
