using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DeferredlB3SendActionListWithCancelTests : TestCase
	{
		public void TestDeferredlB3SendActionListWithCancelContent()
		{
			var list = new DeferredB3SendActionListWithCancel();
			CombineAssertions(() =>
			{
				AssertEquals("List Count", 3, list.Count);

				Assert("Code - CAN", list.ContainsCode(DeferredB3SendActionListWithCancel.Codes.Cancel));
				Assert("Code - DLY", list.ContainsCode(DeferredB3SendActionListWithCancel.Codes.Defer));
				Assert("Code - NOW", list.ContainsCode(DeferredB3SendActionListWithCancel.Codes.Now));

				AssertEquals("Desc - CAN", DeferredB3SendActionListWithCancel.Descriptions.Cancel, list.GetDescriptionFromCode(DeferredB3SendActionListWithCancel.Codes.Cancel));
				AssertEquals("Desc - DLY", DeferredB3SendActionListWithCancel.Descriptions.Defer, list.GetDescriptionFromCode(DeferredB3SendActionListWithCancel.Codes.Defer));
				AssertEquals("Desc - NOW", DeferredB3SendActionListWithCancel.Descriptions.Now, list.GetDescriptionFromCode(DeferredB3SendActionListWithCancel.Codes.Now));
			});
		}
	}
}
