using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(ContactPersonProvider))]
	sealed class ContactPersonProviderTest : Customs.Business.Testing.DataProviderTestCase<ContactPersonProvider>
	{
		public void TestNewOrNull_AddressNull()
		{
			AssertNull(ContactPersonProvider.NewOrNull(null));
		}

		public void TestNewOrNull_AllPropertiesEmpty()
		{
			AssertNull(ContactPersonProvider.NewOrNull(Factory.New<JobDocAddress>()));
		}

		public void TestName()
		{
			AssertEquals("Name", provider.Name);
		}

		public void TestPhoneNumber()
		{
			AssertEquals("PhoneNumber", provider.PhoneNumber);
		}

		public void TestEMailAddress()
		{
			AssertEquals("EMailAddress", provider.EMailAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var data = Factory.CreateJobDocAddress(contactName: "Name", contactPhone: "PhoneNumber", contactEmail: "EMailAddress");
			provider = ContactPersonProvider.NewOrNull(data);
		}

		ContactPersonProvider provider;

		protected override ContactPersonProvider GetProvider() => provider;
	}
}
