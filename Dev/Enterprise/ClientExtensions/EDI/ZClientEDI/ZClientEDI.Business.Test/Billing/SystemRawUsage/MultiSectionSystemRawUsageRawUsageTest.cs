using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(MultiSectionSystemRawUsage))]
	internal class MultiSectionSystemRawUsageRawUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetRawUsageSummarySections()
		{
			var context = new BillingLoadRawUsageContext(Factory, EdiDateTest.MonthToday, ZGuid.NewZGuid(), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = new MultiSectionSystemRawUsage(context, BillingConstants.BillingSystem.AirlineMessaging);

			var section1 = new SummarySection(Factory);
			rawUsage.SummarySections.Add(section1);
			var section2 = new SummarySection(Factory);
			rawUsage.SummarySections.Add(section2);

			section1.Lines.AddNew().MainDescription = "hello";
			section2.Lines.AddNew().MainDescription = "world";

			SummarySection[] summarySections = rawUsage.GetRawUsageSummarySections();
			AssertEquals("Two summary sections", 2, summarySections.Length);
			AssertEquals("One line in section 1", 1, summarySections[0].Lines.Count);
			AssertEquals("One line in section 2", 1, summarySections[1].Lines.Count);

			AssertEquals("hello", summarySections[0].Lines[0].MainDescription);
			AssertEquals("world", summarySections[1].Lines[0].MainDescription);

			AssertContains("Airline Messaging Usage Summary", summarySections[0].Header.TopLevelDescription);
			AssertContains("Airline Messaging Usage Summary", summarySections[0].Header.TopLevelDescription);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new BillingLoadRawUsageContext(Factory, EdiDateTest.MonthToday, ZGuid.NewZGuid(), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			return new MultiSectionSystemRawUsage(context, BillingConstants.BillingSystem.AirlineMessaging);
		}

		#endregion
	}
}
