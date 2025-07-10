using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.ServiceTasks.Testing
{
	public class BillingDatabaseTest : TestCaseWithFactory
	{
		[TestDate(2016, 8, 1)]
		public void TestGetFirstStlPeriod()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			Factory.Save();
			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2014, 1, 15, 0, 0, 0), lic1, "", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2016, 1, 15, 0, 0, 0), lic1, "", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2016, 7, 15, 0, 0, 0), lic1, "", "", "", ""));
			EServicesBillingTestHelper.AddTransactions(infoList);
			var db = new BillingDatabase();
			AssertEquals(new DateTime(2016, 1, 1), db.GetFirstStlPeriod(new DateTime(2016, 8, 1)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			EServicesBillingTestHelper.CreateTable();
		}

		protected override void TearDown()
		{
			EServicesBillingTestHelper.DropTable();
			base.TearDown();
		}
	}
}
