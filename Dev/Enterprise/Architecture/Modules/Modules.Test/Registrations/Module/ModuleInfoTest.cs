using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ModuleInfoTest : TestCase
	{
		public void TestIsClientOverride()
		{
			AssertEquals("Non-client override", false, new ModuleInfo(ModuleIDs.GlbStaff, "", "").IsClientOverride);
			AssertEquals("Client override", true, new ClientOverrideModuleInfo(new ClientOverrideModuleIdentifier(ModuleIDs.GlbStaff), "", "").IsClientOverride);
		}

		public void TestMenuDescription()
		{
			ModuleInfo moduleInfo1 = new ModuleInfo(DummyModuleIDs.Dummy, "Enterprise.ZArchitecture.GUI", "Enterprise.ZArchitecture.Modules.Testing.DummyFilterGridModule", Constants.CountryCodes._TemplateCountryName_);
			ModuleInfo moduleInfo2 = new ModuleInfo(DummyModuleIDs.Dummy, "Enterprise.ZArchitecture.GUI", "Enterprise.ZArchitecture.Modules.Testing.DummyFilterGridModule", Constants.CountryCodes._TemplateCountryName_, (NoResString)"Dummy Menu Name");
			AssertEquals("moduleInfo1.MenuName", DummyModuleIDs.Dummy.Description, moduleInfo1.Description);

			AssertEquals("moduleInfo2.MenuName", "Dummy Menu Name", moduleInfo2.Description);
			AssertNotEquals("moduleInfo2.MenuName", DummyModuleIDs.Dummy.Description, moduleInfo2.Description);
		}

		public void TestIDProperty()
		{
			ModuleId iDForModule = ModuleId.AccBankAccount;
			ModuleIdentifier module = new ModuleIdentifier(iDForModule, (NoResString)"");
			AssertEquals(iDForModule, module.ID);
		}

		public void TestModuleIDExtentions()
		{
			ModuleId iDForModule = ModuleId.AccBankAccount;
			AssertEquals(iDForModule.GetModuleID(), ModuleIDs.AccBankAccount);

			AssertEquals(iDForModule.ToIdentifier(), new RegistrationIdentifier(nameof(ModuleId.AccBankAccount)));
		}

		public void TestExtendedDescription()
		{
			var module = new ModuleIdentifier(ModuleId.Dummy, (NoResString)"description");
			AssertEquals((NoResString)"description", module.Description);
			AssertEquals((NoResString)"description", module.ExtendedDescription);

			module = new ModuleIdentifier(ModuleId.Dummy, (NoResString)"description", (NoResString)"extended description");
			AssertEquals((NoResString)"description", module.Description);
			AssertEquals((NoResString)"extended description", module.ExtendedDescription);
		}
	}
}
