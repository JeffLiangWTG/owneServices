using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.CH.Business.Testing;

class EVVPreviousDocumentWrapperTest : TestCaseWithFactory
{
	public void TestNew()
	{
		AssertExceptionThrown<ArgumentNullException>("Null argument", () => EVVPreviousDocumentWrapper.New(null, Factory));
	}

	public void TestProperties() => CombineAssertions(() =>
	{
		var previousDocumentMock = new Mock<IEvvPreviousDocument>();
		previousDocumentMock.Setup(p => p.Type).Returns("T01");
		previousDocumentMock.Setup(p => p.Reference).Returns("R123");
		previousDocumentMock.Setup(p => p.AdditionalInformation).Returns("Axyz");

		var wrapper = EVVPreviousDocumentWrapper.New(previousDocumentMock.Object, Factory);

		AssertEquals("Type", "T01", wrapper.Type);
		AssertEquals("Reference", "R123", wrapper.Reference);
		AssertEquals("AdditionalInformation", "Axyz", wrapper.AdditionalInformation);
	});
}
