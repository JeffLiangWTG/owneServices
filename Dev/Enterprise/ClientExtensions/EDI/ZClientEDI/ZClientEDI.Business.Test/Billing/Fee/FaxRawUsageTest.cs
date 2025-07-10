using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Fee.Test
{
	[TestedType(typeof(FeeRawUsage))]
	internal class FaxRawUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var context = new BillingLoadRawUsageContext(Factory, EdiDateTest.MonthToday, ZGuid.NewZGuid(), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = new FeeRawUsage(context);
			AssertEquals(BillingConstants.BillingSystem.Fee, rawUsage.SystemCode);
		}

		public void TestGetRawUsageSummarySections()
		{
			var periodStart = new ZDateTime(2010, 12, 1);

			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ClientLicenceFee fee1 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "AAA", 10.24m);
			ClientLicenceFee fee2 = BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "BBB", 20.48m);
			BillingTestHelper.CreateLicenceFee(organisation.LicCompany, "XXX", "XXX - should be unmatched", 200m, "XXXCODE", periodStart.AddMonths(1), ZDateTime.Empty);

			var context = new BillingLoadRawUsageContext(Factory, periodStart, organisation.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			FeeRawUsage rawUsage = new FeeRawUsage(context);

			SummarySection[] summarySections = rawUsage.GetRawUsageSummarySections();
			AssertEquals("Single summary section", 1, summarySections.Length);

			SummaryLine summaryHeader = summarySections[0].Header;
			AssertEquals("Product Fees", summaryHeader.MainDescription);
			AssertEquals("Amount", summaryHeader.Amount);
			AssertEquals("30.72", summaryHeader.TotalAmount);
			AssertEquals("Total (AUD)", summaryHeader.TotalDescription);

			AssertEquals("2 summary lines", 2, summarySections[0].Lines.Count);

			AssertEquals("AAA fee description", summarySections[0].Lines[0].MainDescription);
			AssertEquals("10.24", summarySections[0].Lines[0].Amount);

			AssertEquals("BBB fee description", summarySections[0].Lines[1].MainDescription);
			AssertEquals("20.48", summarySections[0].Lines[1].Amount);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new BillingLoadRawUsageContext(Factory, EdiDateTest.MonthToday, ZGuid.NewZGuid(), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			return new FeeRawUsage(context);
		}

		#endregion
	}
}
