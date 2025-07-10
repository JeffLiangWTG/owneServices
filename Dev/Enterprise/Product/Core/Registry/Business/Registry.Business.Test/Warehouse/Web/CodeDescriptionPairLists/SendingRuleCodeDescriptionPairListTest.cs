using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class SendingRuleCodeDescriptionPairListTest : TestCase
	{
		public void TestList()
		{
			SendingRuleCodeDescriptionPairList list = new SendingRuleCodeDescriptionPairList();
			AssertEquals(4, list.Count);

			AssertNotNull(list["ALL"]);
			AssertNotNull(list["GRP"]);
			AssertNotNull(list["ROL"]);
			AssertNotNull(list["NON"]);

			AssertEquals("Send to staff roles AND notification group", list["ALL"].Description);
			AssertEquals("Send to notification group, send to staff roles if there is no notification group", list["GRP"].Description);
			AssertEquals("Send to staff roles, send to notification group if there are no staff roles", list["ROL"].Description);
			AssertEquals("Do not send email notifications", list["NON"].Description);
		}
	}
}
