using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC023C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	sealed class CC023CProviderTest : TestCaseWithFactory
	{
		public void TestMRN()
		{
			AssertEquals("MRN", "21IEDUB11A782454R2", provider.MRN);
		}

		public void TestDeclarationAcceptanceDate()
		{
			AssertEquals("DeclarationAcceptanceDate", new ZDateTime(2023, 6, 20), provider.DeclarationAcceptanceDate);
			AssertEquals("Empty DeclarationAcceptanceDate", ZDateTime.Empty, emptyProvider.DeclarationAcceptanceDate);
		}
		public void TestCustomsOfficeOfRecoveryAtdeparture()
		{
			AssertEquals("CustomsOfficeOfRecoveryAtdeparture", "REF123", provider.CustomsOfficeOfRecoveryAtdeparture);
		}

		public void TestNameOfGuarantor()
		{
			AssertEquals("NameOfGuarantor", "BOB THE BUILDER", provider.NameOfGuarantor);
		}

		public void TestAddressOfGuarantor()
		{
			AssertEquals("AddressOfGuarantor", "123 WHERE ST, City, IE", provider.AddressOfGuarantor);
			AssertEquals("Empty AddressOfGuarantor", ZString.Empty, emptyProvider.AddressOfGuarantor);
		}

		public void TestGuarantorNotificationText()
		{
			AssertEquals("GuarantorNotificationText", "Guarantor Notification Text", provider.GuarantorNotificationText);
		}

		public void TestGuarantorNotificationDate()
		{
			AssertEquals("GuarantorNotificationDate", new ZDateTime(2022, 5, 2), provider.GuarantorNotificationDate);
			AssertEquals("Empty GuarantorNotificationDate", ZDateTime.Empty, emptyProvider.GuarantorNotificationDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC023CProvider(new Cc023CType
			{
				TransitOperation = new TransitOperationType48
				{
					Mrn = "21IEDUB11A782454R2",
					DeclarationAcceptanceDate = new DateTime(2023, 6, 20)
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "REF123"
				},
				Guarantor = new GuarantorType06
				{
					Name = "BOB THE BUILDER",
					Address = new AddressType16
					{
						StreetAndNumber = "123 WHERE ST",
						City = "City",
						Country = CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl.CountryCodesCustomsOfficeLists.Ie
					}
				},
				GuarantorNotification = new GuarantorNotificationType
				{
					GuarantorNotificationDate = new DateTime(2022, 5, 2),
					GuarantorNotificationText = "Guarantor Notification Text"
				}
			});
			emptyProvider = new CC023CProvider(new Cc023CType());
		}
		CC023CProvider provider;
		CC023CProvider emptyProvider;
	}
}
