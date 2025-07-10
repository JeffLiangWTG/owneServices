using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815MovementGuaranteeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815MovementGuarantee(null));
			AssertNoExceptionThrown(() => new IE815MovementGuarantee(new Mock<IMovementGuarantee>().Object));
		});
	}
}
