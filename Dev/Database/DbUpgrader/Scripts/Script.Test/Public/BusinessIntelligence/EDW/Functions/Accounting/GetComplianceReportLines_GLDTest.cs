using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ComplianceReport;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GetComplianceReportLines_GLD))]
	class GetComplianceReportLines_GLDTest : GetComplianceReportLines_BaseTest
	{
		public override string ScriptSql => $@"SELECT * FROM {Db.EdwDatabaseName}.dbo.GetComplianceReportLines_GLD('{reportPK}') ORDER by ACL_ReportSequence";

		public override Action<DbCommand> SetParameters => command =>
		{
			command.AddParameterBasedOnDbColumn("@PK", reportPK, AccComplianceReportSchema.PK);
		};
	}
}
