using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AddressDetailsExtensionsTest : TestCaseWithFactory
	{
		public void TestAreEmpty()
		{
			AssertEquals("null", true, ((IAddressDetails)null).AreEmpty());
			AssertEquals("empty", true, new AddressDetailsForTest().AreEmpty());

			AssertEquals("empty", true, new AddressDetailsForTest().AreEmpty());

			AssertEquals("CompanyName", false, new AddressDetailsForTest { CompanyName = "TST" }.AreEmpty());
			AssertEquals("ContactName", false, new AddressDetailsForTest { ContactName = "TST" }.AreEmpty());
			AssertEquals("Phone", false, new AddressDetailsForTest { Phone = "TST" }.AreEmpty());
			AssertEquals("Fax", false, new AddressDetailsForTest { Fax = "TST" }.AreEmpty());
			AssertEquals("Email", false, new AddressDetailsForTest { Email = "TST" }.AreEmpty());
			AssertEquals("AddressLine1", false, new AddressDetailsForTest { AddressLine1 = "TST" }.AreEmpty());
			AssertEquals("AddressLine2", false, new AddressDetailsForTest { AddressLine2 = "TST" }.AreEmpty());
			AssertEquals("City", false, new AddressDetailsForTest { City = "TST" }.AreEmpty());
			AssertEquals("State", false, new AddressDetailsForTest { State = "TST" }.AreEmpty());
			AssertEquals("PostCode", false, new AddressDetailsForTest { PostCode = "TST" }.AreEmpty());
			AssertEquals("Country/Region", false, new AddressDetailsForTest { Country = "TST" }.AreEmpty());
		}

		public void TestCopyTo()
		{
			// Nothing should happen if address is copied to nowhere.
			new AddressDetailsForTest { City = "TEST" }.CopyTo(null);

			OrgHeader org = Factory.New<OrgHeader>();
			OrgAddress orgAddr = org.MainAddress;

			// It is wrong to try to copy null, regardless of the destination.
			AssertExceptionThrown<ArgumentNullException>("copy null to null", () => ((IAddressDetails)null).CopyTo(null));
			AssertExceptionThrown<ArgumentNullException>("copy null to object", () => ((IAddressDetails)null).CopyTo(org));

			IAddressDetails address = new AddressDetailsForTest
			{
				CompanyName = "ADE COMPANY",
				ContactName = "ADE CONTACT",
				Phone = "ADE PHONE",
				Fax = "ADE FAX",
				Email = "ADE EMAIL",
				AddressLine1 = "ADE ADDR1",
				AddressLine2 = "ADE ADDR2",
				City = "ADE CITY",
				State = "ADE STATE",
				PostCode = "ADE POSTCD",
				Country = "AD"
			};

			address.CopyTo(org);

			// All values are copied, except for contact.

			AssertEquals("ADE COMPANY", org.OH_FullName);
			// AssertEquals("ADE CONTACT", org.Contact);
			AssertEquals("ADE PHONE", orgAddr.OA_Phone);
			AssertEquals("ADE FAX", orgAddr.OA_Fax);
			AssertEquals("ADE EMAIL", orgAddr.OA_Email);
			AssertEquals("ADE ADDR1", orgAddr.OA_Address1);
			AssertEquals("ADE ADDR2", orgAddr.OA_Address2);
			AssertEquals("ADE CITY", orgAddr.OA_City);
			AssertEquals("ADE STATE", orgAddr.OA_State);
			AssertEquals("ADE POSTCD", orgAddr.OA_PostCode);
			AssertEquals("AD", orgAddr.OA_RN_NKCountryCode);
		}

		sealed class AddressDetailsForTest : IAddressDetails
		{
			public ZString CompanyName { get; set; }
			public ZString ContactName { get; set; }
			public ZString Phone { get; set; }
			public ZString Fax { get; set; }
			public ZString Email { get; set; }
			public ZString AddressLine1 { get; set; }
			public ZString AddressLine2 { get; set; }
			public ZString City { get; set; }
			public ZString State { get; set; }
			public ZString PostCode { get; set; }
			public ZString Country { get; set; }
		}
	}
}
