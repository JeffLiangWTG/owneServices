using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportPartyContactPersonProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportPartyContactPersonProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", ImportPartyContactPersonProvider.NewOrNull(null));
				AssertNotNull("Not NULL", ImportPartyContactPersonProvider.NewOrNull(Factory.NewWithValidTestData<GlbStaff>()));
			});
		}

		public void TestPosition()
		{
			CombineAssertions(() =>
			{
				AssertNull("Empty", Provider.Position);

				staff.GS_Title = "Sachbearbeiter";
				AssertEquals("Not empty", "Sachbearbeiter", Provider.Position);

				staff.GS_Title = "123456789012345678901234567890123456";
				AssertEquals("Truncate", "12345678901234567890123456789012345", Provider.Position);
			});
		}

		public void TestPersonName()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.PersonName);

				staff.GS_FullName = "Bob Baumeister";
				AssertEquals("Not empty", "Bob Baumeister", Provider.PersonName);
			});
		}

		public void TestPhoneNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.PhoneNumber);

				staff.GS_WorkPhone = "06131474747";
				AssertEquals("Not empty", "06131474747", Provider.PhoneNumber);
			});
		}

		public void TestMailAddress()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.MailAddress);

				staff.GS_EmailAddress = "bob.baumeister@samplefreight.de";
				AssertEquals("Not empty", "bob.baumeister@samplefreight.de", Provider.MailAddress);
			});
		}

		protected override ImportPartyContactPersonProvider GetProvider() => ImportPartyContactPersonProvider.NewOrNull(staff);

		protected override void SetUp()
		{
			base.SetUp();
			staff = Factory.New<GlbStaff>();
		}
		GlbStaff staff;
	}
}
