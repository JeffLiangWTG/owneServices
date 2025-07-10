using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC019C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC019CProviderTest : TestCaseWithFactory
	{
		public void TestMRN()
		{
			var provider = new CC019CProvider(new Cc019CType
			{
				TransitOperation = new TransitOperationType08
				{
					Mrn = "21IEDUB11A782454R2"
				}
			});
			AssertEquals("MRN", "21IEDUB11A782454R2", provider.MRN);

			provider = new CC019CProvider(new Cc019CType());
			AssertEquals("MRN", ZString.Empty, provider.MRN);
		}

		public void TestDiscrepancyDate()
		{
			var provider = new CC019CProvider(new Cc019CType
			{
				TransitOperation = new TransitOperationType08
				{
					DiscrepanciesNotificationDate = new DateTime(2023, 2, 10),
				}
			});
			AssertEquals("DiscrepancyDate", new ZDateTime(2023, 2, 10), provider.DiscrepancyDate);

			provider = new CC019CProvider(new Cc019CType());
			AssertEquals("DiscrepancyDate", ZDateTime.Empty, provider.DiscrepancyDate);
		}

		public void TestDiscrepancyNotificationText()
		{
			var provider = new CC019CProvider(new Cc019CType
			{
				TransitOperation = new TransitOperationType08
				{
					DiscrepanciesNotificationText = "Text Discrepancies Notification"
				}
			});
			AssertEquals("DiscrepancyNotificationText", "Text Discrepancies Notification", provider.DiscrepancyNotificationText);

			provider = new CC019CProvider(new Cc019CType());
			AssertEquals("DiscrepancyNotificationText", ZString.Empty, provider.DiscrepancyNotificationText);
		}
	}
}
