using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC035C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC035CProviderTest : TestCaseWithFactory
	{
		public void TestNullValue()
		{
			CombineAssertions(() =>
			{
				var emptyProvider = new CC035CProvider(new Cc035CType());
				AssertEquals("Null value should return empty", ZString.Empty, emptyProvider.MRN);
				AssertEquals("Null value should return empty", ZDateTime.Empty, emptyProvider.DeclarationAcceptanceDate);
				AssertEquals("Null value should return empty", ZDateTime.Empty, emptyProvider.RecoveryNotificationDate);
				AssertEquals("Null value should return empty", ZString.Empty, emptyProvider.RecoveryNotificationText);
				AssertEquals("Null value should return empty", ZString.Empty, emptyProvider.AmountClaimed);
				AssertEquals("Null value should return empty", ZString.Empty, emptyProvider.CustomsOfficeOfRecoveryAtDeparture);
			});
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "19AA12345678901230", provider.MRN);
		}

		public void TestDeclarationAcceptanceDate()
		{
			AssertEquals("DeclarationAcceptanceDate", new ZDate(2023, 1, 30), provider.DeclarationAcceptanceDate);
		}

		public void TestRecoveryNotificationDate()
		{
			AssertEquals("RecoveryNotificationDate", new ZDate(2023, 2, 13), provider.RecoveryNotificationDate);
		}

		public void TestRecoveryNotificationText()
		{
			AssertEquals("RecoveryNotificationText", "Test Recovery Notification", provider.RecoveryNotificationText);
		}

		public void TestAmountClaimed()
		{
			AssertEquals("AmountClaimed", "32.6EUR", provider.AmountClaimed);
		}

		public void TestCustomsOfficeOfRecoveryAtDeparture()
		{
			AssertEquals("CustomsOfficeOfRecoveryAtDeparture", "REF123", provider.CustomsOfficeOfRecoveryAtDeparture);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC035CProvider(new Cc035CType
			{
				TransitOperation = new TransitOperationType48
				{
					Mrn = "19AA12345678901230",
					DeclarationAcceptanceDate = new DateTime(2023, 1, 30, 15, 30, 0),
				},
				RecoveryNotification = new RecoveryNotificationType
				{
					RecoveryNotificationDate = new DateTime(2023, 2, 13, 14, 39, 22),
					RecoveryNotificationText = "Test Recovery Notification",
					AmountClaimed = 32.6m,
					Currency = "EUR"
				},
				CustomsOfficeOfRecoveryAtDeparture = new CustomsOfficeOfRecoveryAtDepartureType01
				{
					ReferenceNumber = "REF123"
				}
			});
		}
		CC035CProvider provider;
	}
}
