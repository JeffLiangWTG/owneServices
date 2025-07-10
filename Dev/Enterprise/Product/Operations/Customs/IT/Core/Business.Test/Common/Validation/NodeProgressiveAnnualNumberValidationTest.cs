using System;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class NodeProgressiveAnnualNumberValidationTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NodeProgressiveAnnualNumberValidation(null));
		AssertNoExceptionThrown(() => new NodeProgressiveAnnualNumberValidation(new Mock<ICustomsMessageFountainProvider>().Object));
	}

	public void TestArguments()
	{
		var nodeValidation = new NodeProgressiveAnnualNumberValidation(new Mock<ICustomsMessageFountainProvider>().Object);
		AssertExceptionThrown<ArgumentNullException>(() => nodeValidation.Validate(null));
	}
}
