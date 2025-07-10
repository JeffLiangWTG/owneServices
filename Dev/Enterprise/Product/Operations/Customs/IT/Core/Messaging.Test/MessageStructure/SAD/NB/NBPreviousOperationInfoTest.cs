using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class NBPreviousOperationInfoTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => new NBPreviousOperationInfo(new Mock<IPreviousOperationInfo>().Object));
		});
	}
}
