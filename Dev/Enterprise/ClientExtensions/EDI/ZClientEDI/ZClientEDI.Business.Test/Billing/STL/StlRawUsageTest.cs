using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlRawUsage))]
	internal class StlRawUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			AssertEquals(new ZDateTime(2015, 9, 1), DummyRawUsage.PeriodStart);
			AssertEquals("SYD", DummyRawUsage.ServerCode);
		}

		public void TestGetRawUsageSummarySections()
		{
			AssertEquals(true, DummyRawUsage.GetRawUsageSummarySections() is SummarySection[]);
		}

		public void TestMergeSummarySections()
		{
			var org = EServicesBillingTestHelper.CreateClient(Factory, "DDD", "EIO", "SYD");
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 8201;
			db.LD_ServerCode = "SYD";
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 9, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var dummyRawUsage2 = new DummyStlRawUsage(context);

			DummyRawUsage.Summary.Lines.AddNew();
			AssertEquals(1, DummyRawUsage.SummarySections.Count);

			dummyRawUsage2.Summary.Lines.AddNew();
			dummyRawUsage2.SummarySections.Add(new SummarySection(Factory));
			DummyRawUsage.MergeSummarySections(dummyRawUsage2);
			AssertEquals(3, DummyRawUsage.SummarySections.Count);
		}

		#region Implementation

		DummyStlRawUsage DummyRawUsage;
		EDIOrgHeader Organisation;

		protected override void SetUp()
		{
			base.SetUp();

			Organisation = EServicesBillingTestHelper.CreateClient(Factory, "DDD", "ABC", "SYD");
			Organisation.OH_FullName = "AAA Company";
			var db = Organisation.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 8201;
			db.LD_ServerCode = "SYD";
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 9, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			DummyRawUsage = new DummyStlRawUsage(context);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var organisation = EServicesBillingTestHelper.CreateClient(Factory, "DDD", "ABC", "SYD");
			var db = Organisation.LicCompany.ActiveOrAllLicDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 9, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			return new DummyStlRawUsage(context);
		}

		#endregion
	}

	#region DummyStlRawUsage class

	internal class DummyStlRawUsage : StlRawUsage
	{
		public DummyStlRawUsage(BillingLoadRawUsageContext context)
			: base(context)
		{
		}

		public List<ZString> RawLines
		{
			get { return rawLines ?? (rawLines = new List<ZString>()); }
		}
		List<ZString> rawLines;
	}

	#endregion
}
