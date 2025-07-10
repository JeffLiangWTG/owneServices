using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CUSRESMessageInterpreterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new CUSRESMessageInterpreter(null), "Null argument");
		AssertNoExceptionThrown("Valid Arguments", () => new CUSRESMessageInterpreter(Factory.New<EDIMessage>()));
	}

	public void TestInterpretation_ResponseWithoutRequest()
	{
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupCustomsStatuses(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest);
		var mockDataProvider = CreateMockDataProvider("ABC");
		mockDataProvider.Setup(x => x.InformationRequests).Returns(Array.Empty<IInformationRequest>());

		var result = Interpreter.GetMessageInterpretation(mockDataProvider.Object);
		var expected = @"Document Reference Number: DOCUMENT001
Entry Status: ABC - ABC DESC";
		AssertEquals(expected, result);
	}

	public void TestInterpretation_ResponseWithSingleRequest()
	{
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupCustomsStatuses(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest);
		var mockInformationRequest = new Mock<IInformationRequest>();
		mockInformationRequest.Setup(x => x.ResponseDetails).Returns(new ZString[] { "Processing Response" });

		var mockDataProvider = CreateMockDataProvider("XYZ");
		mockDataProvider.Setup(x => x.InformationRequests).Returns(new IInformationRequest[] { mockInformationRequest.Object });

		var result = Interpreter.GetMessageInterpretation(mockDataProvider.Object);
		var expected = @"Document Reference Number: DOCUMENT001
Entry Status: XYZ - N/A
Processing Response";
		AssertEquals(expected, result);
	}

	public void TestInterpretation_ResponseWithMultipleRequests()
	{
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupCustomsStatuses(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest);
		var mockInformationRequest1 = new Mock<IInformationRequest>();
		mockInformationRequest1.Setup(x => x.RequestType).Returns("1");
		mockInformationRequest1.Setup(x => x.ErrorSegment).Returns("NAD");
		mockInformationRequest1.Setup(x => x.ResponseDetails).Returns(new ZString[] { "Incorrect value provided", "Vague value provided" });
		var mockInformationRequest2 = new Mock<IInformationRequest>();
		mockInformationRequest2.Setup(x => x.RequestType).Returns("2");
		mockInformationRequest2.Setup(x => x.ResponseDetails).Returns(new ZString[] { "Commercial invoice" });

		var mockDataProvider = CreateMockDataProvider("ABC");
		mockDataProvider.Setup(x => x.InformationRequests).Returns(new IInformationRequest[] { mockInformationRequest1.Object, mockInformationRequest2.Object });

		var result = Interpreter.GetMessageInterpretation(mockDataProvider.Object);
		var expected = @"Document Reference Number: DOCUMENT001
Entry Status: ABC - ABC DESC
Information requested
Error in segment NAD
Incorrect value provided
Vague value provided
Attachment requested
Commercial invoice";
		AssertEquals(expected, result);
	}

	Mock<ICUSRESDataProvider> CreateMockDataProvider(string entryStatus)
	{
		var mockResponse = new Mock<ICUSRESDataProvider>();
		mockResponse.Setup(x => x.DocumentIdentifier).Returns("DOCUMENT001");
		mockResponse.Setup(x => x.EntryStatus).Returns(entryStatus);
		return mockResponse;
	}

	CUSRESMessageInterpreter Interpreter => interpreter ??= new CUSRESMessageInterpreter(Factory.New<EDIMessage>());
	CUSRESMessageInterpreter interpreter;
}
