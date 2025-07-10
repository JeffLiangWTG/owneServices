using System;
using CargoWise.Customs.IE.MessageContracts.Interfaces.PBN;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	sealed class PBNContactDetailsProviderTest : DataProviderTestCase<PBNContactDetailsProvider>
	{
		public void TestIPBNContactDetails()
		{
			var provider = Provider;
			Assert("Should implement IPBNContactDetails", provider is IPBNContactDetails);
		}

		public void TestConstructor()
		{
			AssertNull(PBNContactDetailsProvider.New(null));
			AssertNull("No contact details", PBNContactDetailsProvider.New(Factory.New<AsycudaManifestHeader>()));
		}

		public void TestEmail()
		{
			AssertEquals("bob@test.ie", Provider.Email);
		}

		public void TestMobileNum1()
		{
			AssertEquals("+353871234567", Provider.MobileNum1);
		}

		public void TestMobileNum2()
		{
			AssertEquals("+353873456789", Provider.MobileNum2);
		}

		public void TestMultipleContactDetails()
		{
			var person2 = Factory.New<GlbPerson>();
			person2.PER_EmailAddress = "joe@test.ie";
			person2.PER_MobilePhone = "+353871234599";
			person2.PER_HomePhone = "+353873456799";
			var contact2 = pbn.Persons.AddNew();
			contact2.CPN_SystemCreateTimeUtc = DateTime.Now.AddDays(-1);
			contact2.CPN_PER_Person = person2.PK;

			var person3 = Factory.New<GlbPerson>();
			person3.PER_EmailAddress = "paul@test.ie";
			person3.PER_MobilePhone = "+353871234500";
			person3.PER_HomePhone = "+353873456709";
			var contact3 = pbn.Persons.AddNew();
			contact3.CPN_SystemCreateTimeUtc = DateTime.Now;
			contact3.CPN_PER_Person = person3.PK;

			var provider = GetProvider();
			CombineAssertions("First created person should be used", () =>
			{
				AssertEquals("Email", "joe@test.ie", provider.Email);
				AssertEquals("MobileNum1", "+353871234599", provider.MobileNum1);
				AssertEquals("MobileNum2", "+353873456799", provider.MobileNum2);
			});
		}

		protected override PBNContactDetailsProvider GetProvider() => PBNContactDetailsProvider.New(pbn);

		protected override void SetUp()
		{
			base.SetUp();

			var person = Factory.New<GlbPerson>();
			person.PER_EmailAddress = "bob@test.ie";
			person.PER_MobilePhone = "+353871234567";
			person.PER_HomePhone = "+353873456789";

			pbn = Factory.New<AsycudaManifestHeader>();
			var contact = pbn.Persons.AddNew();
			contact.CPN_SystemCreateTimeUtc = DateTime.Now;
			contact.CPN_PER_Person = person.PK;
		}
		AsycudaManifestHeader pbn;
	}
}
