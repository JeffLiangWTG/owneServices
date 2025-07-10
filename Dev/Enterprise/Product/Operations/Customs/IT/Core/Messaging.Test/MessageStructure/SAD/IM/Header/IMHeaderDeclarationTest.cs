using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderDeclarationTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderDeclaration(null));
			AssertNoExceptionThrown(() => new IMHeaderDeclaration(new Mock<IDeclaration>().Object));
		});
	}
}
