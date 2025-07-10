using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DeferredlB3SendActionListOverrideTests : TestCase
	{
		public void TestDeferredlB3SendActionOrverrideListContent()
		{
			var list = new DeferredB3SendActionListOverride();
			CombineAssertions(() =>
			{
				AssertEquals("List Count", 3, list.Count);

				Assert("Code - DEF", list.ContainsCode(DeferredB3SendActionListOverride.Codes.RegistryDefault));
				Assert("Code - DLY", list.ContainsCode(DeferredB3SendActionListOverride.Codes.Defer));
				Assert("Code - NOW", list.ContainsCode(DeferredB3SendActionListOverride.Codes.Now));

				AssertEquals("Desc - DEF", DeferredB3SendActionListOverride.Descriptions.RegistryDefault, list.GetDescriptionFromCode(DeferredB3SendActionListOverride.Codes.RegistryDefault));
				AssertEquals("Desc - DLY", DeferredB3SendActionListOverride.Descriptions.Defer, list.GetDescriptionFromCode(DeferredB3SendActionListOverride.Codes.Defer));
				AssertEquals("Desc - NOW", DeferredB3SendActionListOverride.Descriptions.Now, list.GetDescriptionFromCode(DeferredB3SendActionListOverride.Codes.Now));
			});
		}
	}
}
