using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class GeneralLedgerDataRecover : IGeneralLedgerDataRecover
	{
		public void RecoverPartOfGLD(SchemaColumn pkSchemaColumn, BusinessObject[] recoverGldTransactions)
		{
			var selectedPks = recoverGldTransactions.Select(x => x.PK.ToGuid()).Distinct();

			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				HandleGLDComplianceReport(pkSchemaColumn, selectedPks);

				var deleteSql = $@"
DELETE AccGeneralLedgerData
WHERE {pkSchemaColumn.Name} IN (SELECT value FROM @PKs)
";
				Db.Connection.ExecuteNonQuery(deleteSql, command =>
				{
					command.AddTableValuedParameter("@PKs", "dbo.TVP_uniqueidentifier", selectedPks);
				});

				GeneralLedgerDataProcessor.ProcessData(recoverGldTransactions.Select(x => ((IBusinessObjectInternals)x).Row).ToArray());
				manager.CommitTransaction();
			}
		}

		void HandleGLDComplianceReport(SchemaColumn pkSchemaColumn, IEnumerable<Guid> selectedPks)
		{
			var queryComplianceReportSql = $@"SELECT DISTINCT ACL_ACR_Report AS Compliance_Report_PK FROM dbo.AccComplianceReportTransactionPivot
					INNER JOIN dbo.AccGeneralLedgerData ON ACL_ParentID = GLD_PK
					WHERE {pkSchemaColumn.Name} IN (SELECT value FROM @PKs)";

			Action<DbCommand> commandAction = command =>
			{
				command.AddTableValuedParameter("@PKs", TVPHelper.TVP_uniqueidentifier, selectedPks);
			};

			GeneralLedgerDataScriptHelper.HandleComplianceReportFromGLD(queryComplianceReportSql, commandAction);
		}

		IGeneralLedgerDataProcessor GeneralLedgerDataProcessor => generalLedgerDataProcessor ?? (generalLedgerDataProcessor = new GeneralLedgerDataProcessor());
		IGeneralLedgerDataProcessor generalLedgerDataProcessor;
	}
}
