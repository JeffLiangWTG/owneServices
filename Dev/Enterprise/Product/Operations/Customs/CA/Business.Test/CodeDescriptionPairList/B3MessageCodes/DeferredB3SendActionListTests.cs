using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DeferredB3SendActionListTests : TestCase
	{
		public void TestIsValidCode()
		{
			var list = new DeferredB3SendActionList();
			CombineAssertions(() =>
			{
				AssertEquals("List Count", 2, list.Count);

				Assert("Code - DLY", list.ContainsCode(DeferredB3SendActionList.Codes.Defer));
				Assert("Code - NOW", list.ContainsCode(DeferredB3SendActionList.Codes.Now));

				AssertEquals("Desc - DLY", DeferredB3SendActionList.Descriptions.Defer, list.GetDescriptionFromCode(DeferredB3SendActionList.Codes.Defer));
				AssertEquals("Desc - NOW", DeferredB3SendActionList.Descriptions.Now, list.GetDescriptionFromCode(DeferredB3SendActionList.Codes.Now));
			});

			Assert(DeferredB3SendActionList.IsValidCode("DFR"));
			Assert(DeferredB3SendActionList.IsValidCode("NOW"));
			Assert(!DeferredB3SendActionList.IsValidCode("CAN"));
			Assert(!DeferredB3SendActionList.IsValidCode("DEF"));
		}
	}
}
