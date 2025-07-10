using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ModuleIDAttributeTest : TestCase
	{
		public void TestModuleIdentifier()
		{
			var attribute = new ModuleIDAttribute(ModuleId.Opportunity);
			AssertEquals(attribute.ModuleIdentifier, ModuleIDs.Opportunity);
			AssertEquals(attribute.ModuleIdName, nameof(ModuleId.Opportunity));
			AssertEquals(attribute.UniversalCopyMenuIgnoreModuleId, false);

			var clientAttribute = new ModuleIDAttribute("Opportunity");
			AssertEquals(clientAttribute.ModuleIdentifier, ModuleIDs.Opportunity);
			AssertEquals(clientAttribute.ModuleIdName, nameof(ModuleId.Opportunity));
			AssertEquals(clientAttribute.UniversalCopyMenuIgnoreModuleId, false);

			using (ClientHookLoader.Instance.OverrideClientHookForTest(new MockClientHookForModuleIDAttribute()))
			{
				var overridenClientModuleAttribute = new ModuleIDAttribute("Organisation", true);
				AssertEquals("Should get overriden module first", overridenClientModuleAttribute.ModuleIdentifier.Description, "ModuleOverrides");
				AssertEquals("UniversalCopyMenuIgnoreModuleId should be true", overridenClientModuleAttribute.UniversalCopyMenuIgnoreModuleId, true);

				var newClientModuleAttribute = new ModuleIDAttribute("Opportunity", false);
				AssertEquals("Should get New module", newClientModuleAttribute.ModuleIdentifier.Description, "TestName");
				AssertEquals("UniversalCopyMenuIgnoreModuleId should be false", newClientModuleAttribute.UniversalCopyMenuIgnoreModuleId, false);

				var baseModuleAttribute = new ModuleIDAttribute(ModuleId.OrgAddresses, true);
				AssertEquals("should get from ModuleIDs.All", ModuleIDs.OrgAddresses, baseModuleAttribute.ModuleIdentifier);
				AssertEquals("UniversalCopyMenuIgnoreModuleId should be true", true, baseModuleAttribute.UniversalCopyMenuIgnoreModuleId);
			}
		}
	}
}
