using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(SystemCodeRawUsage))]
	internal class SystemCodeRawUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var context = new BillingLoadRawUsageContext(Factory, EdiDateTest.MonthToday, ZGuid.NewZGuid(), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			SystemCodeRawUsage rawUsage = new SystemCodeRawUsage(context, "AAA");
			AssertEquals("System code set in construction", "AAA", rawUsage.SystemCode);

			rawUsage = new SystemCodeRawUsage(context, "BBB");
			AssertEquals("System code set in construction", "BBB", rawUsage.SystemCode);
		}

		public void TestSummary()
		{
			var context = new BillingLoadRawUsageContext(Factory, EdiDateTest.MonthToday, ZGuid.NewZGuid(), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			SystemCodeRawUsage rawUsage = new SystemCodeRawUsage(context, "AAA");
			AssertNotNull(rawUsage.Summary);
		}

		public void TestGetRawUsageSummarySections()
		{
			var context = new BillingLoadRawUsageContext(Factory, EdiDateTest.MonthToday, ZGuid.NewZGuid(), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			SystemCodeRawUsage rawUsage = new SystemCodeRawUsage(context, "AAA");
			rawUsage.Summary.Lines.AddNew().MainDescription = "hello";
			rawUsage.Summary.Lines.AddNew().MainDescription = "world";

			SummarySection[] summarySections = rawUsage.GetRawUsageSummarySections();
			AssertEquals("Single summary section", 1, summarySections.Length);
			AssertEquals("Two lines in section", 2, summarySections[0].Lines.Count);

			AssertEquals("hello", summarySections[0].Lines[0].MainDescription);
			AssertEquals("world", summarySections[0].Lines[1].MainDescription);

			AssertContains("Usage Summary", summarySections[0].Header.TopLevelDescription);
		}

		public void TestSummaryHeaderDescription()
		{
			var context = new BillingLoadRawUsageContext(Factory, EdiDateTest.MonthToday, ZGuid.NewZGuid(), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			SystemCodeRawUsage rawUsage = new SystemCodeRawUsage(context, BillingConstants.BillingSystem.eBACCA);
			ZString expectedHeaderTopLevelDescription = BillingConstants.BillingSystemList.GetDescriptionFromCode(BillingConstants.BillingSystem.eBACCA) + " Usage Summary";
			AssertEquals(expectedHeaderTopLevelDescription, rawUsage.GetRawUsageSummarySections()[0].Header.TopLevelDescription);

			rawUsage.SummaryHeaderDescription = "PREVED MEDVED!";
			AssertEquals("PREVED MEDVED!", rawUsage.GetRawUsageSummarySections()[0].Header.TopLevelDescription);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new BillingLoadRawUsageContext(Factory, EdiDateTest.MonthToday, ZGuid.NewZGuid(), ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			return new SystemCodeRawUsage(context, "DUM");
		}

		#endregion
	}
}
