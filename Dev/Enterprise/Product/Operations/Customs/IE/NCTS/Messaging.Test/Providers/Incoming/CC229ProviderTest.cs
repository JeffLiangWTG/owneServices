using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC229C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC229ProviderTest : TestCaseWithFactory
	{
		public void TestGRN()
		{
			AssertEquals("GRN", "GRN123", provider.GRN);
		}

		public void TestInvalidityDate()
		{
			AssertEquals("Invalidity Date", new DateTime(2024, 1, 1), provider.InvalidityDate);
		}

		public void TestGuarantorName()
		{
			AssertEquals("Guarantor Name", "Joe Bloggs", provider.GuarantorName);
		}

		public void TestGuarantorAddress()
		{
			AssertEquals("Gurantor Address", "123 Test Street, Dublin, A12B3C4, IE", provider.GuarantorAddress);
		}

		public void TestCustomsOfficeOfGuarantee()
		{
			AssertEquals("Office of Guarantee", "IEDUB100", provider.CustomsOfficeOfGuarantee);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC229CProvider(new Cc229CType()
			{
				GuaranteeReference = new GuaranteeReferenceType11
				{
					Grn = "GRN123",
					InvalidityDate = new DateTime(2024, 1, 1),
					CustomsOfficeOfGuarantee = new CustomsOfficeOfGuaranteeType02
					{
						ReferenceNumber = "IEDUB100"
					},
				},
				Guarantor = new GuarantorType04
				{
					Name = "Joe Bloggs",
					Address = new AddressType16
					{
						StreetAndNumber = "123 Test Street",
						City = "Dublin",
						Postcode = "A12B3C4",
						Country = CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl.CountryCodesCustomsOfficeLists.Ie,
					},
				}
			});
		}
		CC229CProvider provider;
	}
}
