using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class RegistrationInfoTest : TestCase
	{
		public void TestRegistrationInfo()
		{
			RegistrationInfo info = new RegistrationInfo(DummyRegistrationIDs.DummyShipment, "Assembly", "ClassFullName");
			AssertEquals("", info.CountryCode);
			AssertEquals(DummyRegistrationIDs.DummyShipment, info.ID);
			AssertEquals("ClassFullName,Assembly", info.TypePath);

			info = new RegistrationInfo(DummyRegistrationIDs.DummyShipment, "Assembly", "ClassFullName", "AU");
			AssertEquals("AU", info.CountryCode);
		}

		public void TestCanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist()
		{
			RegistrationInfo info = new RegistrationInfo(DummyRegistrationIDs.DummyShipment, "Assembly", "ClassFullName");
			AssertEquals("Default should be false", false, info.CanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist);
		}

		public void TestEquals()
		{
			RegistrationInfo info1 = new RegistrationInfo(new RegistrationIdentifier("xxx"), "", "", "");
			RegistrationInfo info2 = new RegistrationInfo(new RegistrationIdentifier("xxx"), "", "", "AU");
			RegistrationInfo info3 = new RegistrationInfo(new RegistrationIdentifier("yyy"), "", "", "");
			RegistrationInfo info4 = new RegistrationInfo(new RegistrationIdentifier("yyy"), "", "", "AU");

			RegistrationInfo info5 = new RegistrationInfo(new RegistrationIdentifier("yyy"), "", "", "", new TableRegistrationInfo("Table1"));
			RegistrationInfo info6 = new RegistrationInfo(new RegistrationIdentifier("yyy"), "", "", "", new TableRegistrationInfo("Table2"));

			AssertEquals(true, info1.Equals(info1));
			AssertEquals(true, info2.Equals(info2));

			AssertEquals(false, info1.Equals(info2));
			AssertEquals(false, info2.Equals(info1));
			AssertEquals(false, info2.Equals(info3));
			AssertEquals(false, info3.Equals(info2));
			AssertEquals(false, info3.Equals(info4));
			AssertEquals(false, info4.Equals(info3));
			AssertEquals(false, info1.Equals(info3));
			AssertEquals(false, info3.Equals(info1));
			AssertEquals(false, info1.Equals(info4));
			AssertEquals(false, info4.Equals(info1));

			AssertEquals(true, info5.Equals(info5));

			AssertEquals(false, info5.Equals(info6));
			AssertEquals(false, info6.Equals(info5));
			AssertEquals(false, info5.Equals(info3));
			AssertEquals(false, info3.Equals(info5));
		}
	}
}
