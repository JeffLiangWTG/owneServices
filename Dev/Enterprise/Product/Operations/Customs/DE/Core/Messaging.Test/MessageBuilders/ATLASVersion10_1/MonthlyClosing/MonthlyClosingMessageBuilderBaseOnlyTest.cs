using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	sealed class MonthlyClosingMessageBuilderBaseOnlyTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLineNumbersInMessage() => NUnit.Framework.Assert.That(new MonthlyClosingMessageBuilderForTest().LineNumbersInMessage, Is.Not.EqualTo(default(System.Collections.Generic.List<int>)));
	}

	sealed class MonthlyClosingMessageBuilderForTest : MonthlyClosingMessageBuilder<ECFCPF>
	{
		protected override ECFCPF GetMessageCore() => new ECFCPF();
	}
}
