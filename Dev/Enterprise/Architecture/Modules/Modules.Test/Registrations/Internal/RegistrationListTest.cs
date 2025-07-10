using System;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class RegistrationListTest : TestCase
	{
		public RegistrationListTest()
			: base()
		{
		}

		public void TestGetRegisteredIdentifierByName()
		{
			RegistrationIdentifier iD = RegistrationList.GetRegisteredIdentifierByName(DummyRegistrationIDs.DummyCustoms.Name);
			AssertEquals("GetIDByName", DummyRegistrationIDs.DummyCustoms, iD);
		}

		public void TestGetCountryOverridesRegistered()
		{
			string[] countryList = RegistrationList.GetCountryOverridesRegistered(DummyRegistrationIDs.DummyCustoms);
			AssertEquals("Coutry List", true, countryList.Length > 0);
		}

		public void TestWithAllCountryRego()
		{
			AssertEquals(RegistrationList[DummyRegistrationIDs.DummyShipment, null], RegistrationList.DummyShipmentInfo);
			AssertEquals(RegistrationList[DummyRegistrationIDs.DummyShipment, ""], RegistrationList.DummyShipmentInfo);
			AssertEquals(RegistrationList[DummyRegistrationIDs.DummyShipment, "ZZ"], RegistrationList.DummyShipmentInfo);
		}

		public void TestWithCountrySpecificRegoFallthroughToNonCountrySpecific()
		{
			AssertEquals(RegistrationList[DummyRegistrationIDs.DummyCustoms, null], RegistrationList.DummyCustomsInfo);
			AssertEquals(RegistrationList[DummyRegistrationIDs.DummyCustoms, "ZZ"], RegistrationList.DummyCustomsInfo);
		}

		public void TestCountrySpecific()
		{
			AssertEquals(RegistrationList[DummyRegistrationIDs.DummyCustoms, "AU"], RegistrationList.DummyAUCustomsInfo);
			AssertEquals(RegistrationList[DummyRegistrationIDs.DummyCustoms, "SG"], RegistrationList.DummySGCustomsInfo);
		}

		public void TestRegisteringSameIDTwiceShouldThrowApplicationException_ClientOverride()
		{
			var moduleDescription = (NoResString)"moduleDescription";
			RegistrationList.Add(new RegistrationInfo(new ModuleIdentifier(ModuleId.Dummy, moduleDescription), "", "", "AU"));

			var exception = AssertExceptionThrown<ApplicationException>(
				"Should not allow registering a ClientOverride with the same CountryCode",
				"Sorry, you cannot have a country-specific and a non-country-specific identifier with a non-country-specific client override identifier",
				() => RegistrationList.Add(new ClientOverrideModuleInfo(new ClientOverrideModuleIdentifier(new ModuleIdentifier(ModuleId.Dummy, moduleDescription)), "", "", "AU")));

			AssertEquals(exception.Data["ID"], "Dummy");
			AssertEquals(exception.Data["CountryCode"], "AU");
			AssertEquals(exception.Data["TypePath"], ",");
			AssertEquals("We don't care about PreviousStackTrace other than MFI JobConsol, but it should not be null",
				exception.Data["PreviousStackTrace"].ToString().Length, 0);

			AssertNoExceptionThrown(
				"Should allow registering a ClientOverride with a different CountryCode",
				() => RegistrationList.Add(new ClientOverrideModuleInfo(new ClientOverrideModuleIdentifier(new ModuleIdentifier(ModuleId.Dummy, moduleDescription)), "", "", "")));
		}

		public void TestRegisteringSameIDTwiceShouldThrowApplicationExceptionWithPreviousStackTrace_ClientOverride_MFIConsolModuleOverride()
		{
			var moduleDescription = (NoResString)"moduleDescription";
			RegistrationList.Add(new RegistrationInfo(
				new ModuleIdentifier(ModuleId.JobConsol, moduleDescription),
				"SomeAssembly",
				"SomeClass",
				""));

			var exception = AssertExceptionThrown<ApplicationException>(
				"Should not allow registering a ClientOverride with the same CountryCode",
				"Sorry, you cannot have a country-specific and a non-country-specific identifier with a non-country-specific client override identifier",
				() => RegistrationList.Add(new ClientOverrideModuleInfo(
					new ClientOverrideModuleIdentifier(new ModuleIdentifier(ModuleId.JobConsol, moduleDescription)),
					"ZClientMFI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350",
					"Enterprise.Client.MFI.MFIConsolModuleOverride",
					"")));

			AssertEquals(exception.Data["ID"], "JobConsol");
			AssertEquals(exception.Data["CountryCode"], "");
			AssertEquals(exception.Data["TypePath"], "Enterprise.Client.MFI.MFIConsolModuleOverride,ZClientMFI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350");
			Assert(exception.Data["PreviousStackTrace"].ToString().Length > 0);

			AssertNoExceptionThrown(
				"Should allow registering a ClientOverride with a different CountryCode",
				() => RegistrationList.Add(new ClientOverrideModuleInfo(
					new ClientOverrideModuleIdentifier(new ModuleIdentifier(ModuleId.JobConsol, moduleDescription)),
					"SomeAssembly",
					"SomeClass",
					"AU")));
		}

		public void TestRegisteringSameIDTwiceShouldThrowApplicationException()
		{
			RegistrationList.Add(new ControllerInfo(new ControllerID("xxx"), "SomeAssembly", "SomeClass"));

			var exception = AssertExceptionThrown<ApplicationException>(
				"Should not allow registering a second non-ClientOverride",
				"Sorry, can't register second entry with ID 'xxx' and CountryCode ''",
				() => RegistrationList.Add(new ControllerInfo(new ControllerID("xxx"), "SomeAssembly", "SomeClass")));
		}

		public void TestAddThrowsArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => RegistrationList.Add(null));
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: info", () => RegistrationList.Add(null));

			var info = new RegistrationInfo(new ModuleIdentifier(ModuleId.Dummy, (NoResString)"xxx"), "", "", "AU");
			info.CountryCode = null;
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: CountryCode", () => RegistrationList.Add(info));

			info.ID = null;
			info.CountryCode = "AU";
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: ID", () => RegistrationList.Add(info));
		}

		#region Implementation

		DummyRegistrationList RegistrationList
		{
			get
			{
				if (registrationList == null)
				{
					registrationList = new DummyRegistrationList();
				}
				return registrationList;
			}
		}
		DummyRegistrationList registrationList;

		#endregion
	}
}
