using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class NBPreviousAdministrativeReferenceTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => new NBPreviousAdministrativeReference(new Mock<IPreviousAdministrativeReference>().Object));
		});
	}
}
