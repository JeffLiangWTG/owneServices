using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccGLDComplianceReport : AccComplianceReport
	{
		public AccGLDComplianceReport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override string GetLoadBalanceDetailsSql()
		{
			return string.Format(LoadBalanceDetails, string.Empty, GLDAggregateQuery, HeaderDetailsQuery);
		}

		protected override void AddParametersForLoadBalanceDetails(DbCommand dbCommand)
		{
			base.AddParametersForLoadBalanceDetails(dbCommand);
			dbCommand.AddTableValuedParameter("@BranchList", "dbo.TVP_uniqueidentifier", Enumerable.Empty<Guid>());
			dbCommand.AddTableValuedParameter("@DepartmentList", "dbo.TVP_uniqueidentifier", Enumerable.Empty<Guid>());
		}

		string GLDAggregateQuery => @"
			SELECT
				GLAccountPK AS AG_PK,
				Amount AS AA_Amount,
				AG_AccountType,
				PostPeriod AS AA_Period
			FROM 
				dbo.GetGLAggregate(@CompanyPK, @BranchList, 1, @DepartmentList, 1) Aggregate
				INNER JOIN dbo.AccGLHeader ON AG_PK = GLAccountPK";
	}
}
