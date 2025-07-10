using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(PortMessagingUsage))]
	internal class PortMessagingUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = new PortMessagingUsage("PM1", Factory, new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals(BillingConstants.BillingSystem.PortMessaging, usage.SystemCode);
		}

		public void TestGetGeneralSummarySections()
		{
			Action<PortMessagingUsage, ZString> assertDescription = (usage, mainDescription) =>
			{
				var secs = usage.GetGeneralSummarySections();
				AssertEquals(1, secs.Length);
				AssertEquals(1, secs[0].Lines.Count);
				AssertEquals(mainDescription, secs[0].Lines[0].MainDescription);
			};

			var usagePM1 = new PortMessagingUsage("PM1", Factory, new UsingParty(), EdiDateTest.MonthToday);
			var usagePM2 = new PortMessagingUsage("PM2", Factory, new UsingParty(), EdiDateTest.MonthToday);
			var usagePM3 = new PortMessagingUsage("PM3", Factory, new UsingParty(), EdiDateTest.MonthToday);

			assertDescription(usagePM1, "Port Order with HDS");
			assertDescription(usagePM2, "Other Message");
			assertDescription(usagePM3, "Status Message");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PortMessagingUsage("PM1", Factory, new UsingParty(), EdiDateTest.MonthToday);
		}
	}
}
