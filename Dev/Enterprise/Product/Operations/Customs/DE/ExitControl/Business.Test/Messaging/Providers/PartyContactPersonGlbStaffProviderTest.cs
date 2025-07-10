using CargoWise.Customs.DE.MessageContracts;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	sealed class PartyContactPersonGlbStaffProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyContactPersonGlbStaffProvider>
	{
		public void TestNew()
		{
			AssertNull(PartyContactPersonGlbStaffProvider.NewOrNull(null));
		}

		public void TestPosition()
		{
			AssertEquals("Sachbearbeiter", dataProvider.Position);
		}

		public void TestPersonName()
		{
			AssertEquals("Bob Baumeister", dataProvider.PersonName);
		}

		public void TestPhoneNumber()
		{
			AssertEquals("06131474747", dataProvider.PhoneNumber);
		}

		public void TestFacsimileNumber()
		{
			AssertEquals("12345678", dataProvider.FacsimileNumber);
		}

		public void TestMailAddress()
		{
			AssertEquals("bob.baumeister@samplefreight.de", dataProvider.MailAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var staff = Factory.New<GlbStaff>();
			staff.GS_Title = "Sachbearbeiter";
			staff.GS_FullName = "Bob Baumeister";
			staff.GS_WorkPhone = "06131474747";
			staff.GS_FaxNum = "12345678";
			staff.GS_EmailAddress = "bob.baumeister@samplefreight.de";
			dataProvider = PartyContactPersonGlbStaffProvider.NewOrNull(staff);
		}
		IAESPartyContactPerson dataProvider;

		protected override PartyContactPersonGlbStaffProvider GetProvider() => (PartyContactPersonGlbStaffProvider)dataProvider;
	}
}
