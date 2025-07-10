using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderDeclarationTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderDeclaration(null));
			AssertNoExceptionThrown(() => new ETHeaderDeclaration(new Mock<IDeclaration>().Object));
		});
	}
}
