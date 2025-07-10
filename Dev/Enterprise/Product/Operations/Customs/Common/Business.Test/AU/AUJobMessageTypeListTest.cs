using NUnit.Framework;

namespace Enterprise.Customs.Common.AU.Testing
{
	class AUJobMessageTypeListTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestAUJobMessageTypeList()
		{
			NUnit.Framework.Assert.That(new AUJobMessageTypeList().ContainsOnly("EXP", "EXX", "IMP", "IMX", "EXW", "WEA", "DRW", "MSC", "AQS"), Is.EqualTo(true), "Contains EXP");
		}
	}
}
