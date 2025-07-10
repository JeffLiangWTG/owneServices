using System;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Ucc6ExportPreviousDocumentReferenceNumberValidatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var providerMock = new Mock<IPreviousDocumentReferenceNumberProvider>();
		AssertExceptionThrown<ArgumentNullException>("When Factory is null", () => new Ucc6ExportPreviousDocumentReferenceNumberValidator(factory: null, providerMock.Object));
		AssertExceptionThrown<ArgumentNullException>("When Provider is null", () => new Ucc6ExportPreviousDocumentReferenceNumberValidator(Factory, previousDocumentReferenceNumberProvider: null));
		AssertNoExceptionThrown(() => new Ucc6ExportPreviousDocumentReferenceNumberValidator(Factory, providerMock.Object));
	}
}
