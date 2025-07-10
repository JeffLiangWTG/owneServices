using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(AgencyContainerManager))]
	sealed class AgencyContainerManagerTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @RcsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @RcPk UNIQUEIDENTIFIER = (SELECT TOP(1) RC_PK FROM dbo.RefContainer);
				INSERT dbo.RefContainerStock (R6_PK, R6_ContainerNum, R6_RC, R6_SystemCreateTimeUtc, R6_SystemCreateUser) VALUES
					(@RcsPk01, 'RCS01', @RcPk, '2013-10-30', 'DN1'),
					(newid(), 'RCS02', @RcPk, '2013-10-29', 'DN2'),
					(newid(), 'RCS03', @RcPk, '2013-10-31', 'DN3');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("TransactionDateUtc", new DateTime(2013, 10, 30), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "DN1", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "RCS01", transaction1.Reference1);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				DateTime startDate = new DateTime(2013, 10, 30);
				return new RecurringRange(startDate, startDate.AddDays(1));
			}
		}
	}
}
