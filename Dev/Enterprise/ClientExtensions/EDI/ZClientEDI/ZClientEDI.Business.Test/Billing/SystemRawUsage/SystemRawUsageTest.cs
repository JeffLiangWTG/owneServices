using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class SystemRawUsageTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			AssertEquals(Organisation.OH_Code, DummyRawUsage.OrgCode);
			AssertEquals(Organisation.OH_FullName, DummyRawUsage.OrgName);
			AssertEquals(new ZDateTime(2015, 9, 1), DummyRawUsage.PeriodStart);
			AssertEquals("DUM", DummyRawUsage.SystemCode);
		}

		public void TestGetRawUsageSummarySections()
		{
			AssertEquals(true, DummyRawUsage.GetRawUsageSummarySections() is SummarySection[]);
		}

		#region Implementation

		DummyRawUsage DummyRawUsage;
		EDIOrgHeader Organisation;

		protected override void SetUp()
		{
			base.SetUp();

			Organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			Organisation.OH_FullName = "AAA Company";
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 9, 1), Organisation.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			DummyRawUsage = new DummyRawUsage(context);
		}

		#endregion
	}

	#region DummyRawUsage class

	internal class DummyRawUsage : SystemRawUsage
	{
		public DummyRawUsage(BillingLoadRawUsageContext context)
			: base(context)
		{
		}

		public override ZString SystemCode
		{
			get { return "DUM"; }
		}

		public override SummarySection[] GetRawUsageSummarySections()
		{
			return System.Array.Empty<SummarySection>();
		}

		public List<ZString> RawLines
		{
			get { return rawLines ?? (rawLines = new List<ZString>()); }
		}
		List<ZString> rawLines;
	}

	#endregion
}