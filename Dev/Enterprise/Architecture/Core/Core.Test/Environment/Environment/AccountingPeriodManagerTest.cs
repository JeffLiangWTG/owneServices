using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class AccountingPeriodManagerTest : TransactionedTestCase
	{
		public void TestGetPeriod()
		{
			string insertSql = "INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (newid(), 200410, 2004, '2004-01-01', '2004-12-30', 0, 0, @CompanyPK, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (DbCommand command = Db.Connection.Command(insertSql))
			{
				command.AddParameterBasedOnDbColumn("@CompanyPK", EnvProxy.Instance.CurrentCompany.PK, AccPeriodManagementSchema.AM_GC_Company);
				command.ExecuteNonQuery();
			}
			int period = AccountingPeriodManager.GetPeriod(new DateTime(2004, 1, 1), EnvProxy.Instance.CurrentCompany.PK);
			AssertEquals("Period", 0, period);

			period = AccountingPeriodManager.GetPeriod(new DateTime(2004, 2, 1), EnvProxy.Instance.CurrentCompany.PK);
			AssertEquals("Period", 200410, period);
		}
	}
}
