using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Moq;
using ActionCodedList = Enterprise.Customs.AE.Business.AEConstants.Messaging.ActionCodedList;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CONTRLMessageInterpreterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new CONTRLMessageInterpreter(null), "Null argument");
		AssertNoExceptionThrown("Valid Arguments", () => new CONTRLMessageInterpreter(Factory.New<EDIMessage>()));
	}

	public void TestInterpretation_InterchangeAcknowledged()
	{
		var mockInterchangeResponse = CreateMockResponse<ICONTRLInterchangeResponseProvider>("INT001", ActionCodedList.ActionCoded8);
		var mockDataProvider = new Mock<ICONTRLDataProvider>();
		mockDataProvider.Setup(x => x.InterchangeResponse).Returns(mockInterchangeResponse.Object);

		var result = Interpreter.GetMessageInterpretation(mockDataProvider.Object);
		var expected = "<td>1</td><td>Interchange Reference Number: INT001<br>Interchange was successful with no errors</td>";

		AssertContains(expected, result);
	}

	public void TestInterpretation_InvalidInterchangeSegment()
	{
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupCustomsStatuses(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest);
		var mockInterchangeResponse = CreateMockResponse<ICONTRLInterchangeResponseProvider>("INT001", ActionCodedList.ActionCoded4);
		mockInterchangeResponse.Setup(x => x.SyntaxErrorCode).Returns("ERR");
		mockInterchangeResponse.Setup(x => x.ErrorDataElementPosition).Returns("1");
		mockInterchangeResponse.Setup(x => x.ErrorDataElementComponentPosition).Returns("2");
		var mockDataProvider = new Mock<ICONTRLDataProvider>();
		mockDataProvider.Setup(x => x.InterchangeResponse).Returns(mockInterchangeResponse.Object);

		var result = Interpreter.GetMessageInterpretation(mockDataProvider.Object);
		var expected = "<td>1</td><td>Interchange Reference Number: INT001<br>The UNB/UNZ segment is missing or has a validation error<br>Error Code: ERR - ERR DESC<br>Error occurred at data element 1, component position 2</td>";
		AssertContains(expected, result);
	}

	public void TestInterpretation_InvalidMessageSegment()
	{
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupCustomsStatuses(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest);
		var mockInterchangeResponse = CreateMockResponse<ICONTRLInterchangeResponseProvider>("INT001", ActionCodedList.ActionCoded7);
		var mockMessageResponse = CreateMockResponse<ICONTRLMessageResponseProvider>("MSG001", ActionCodedList.ActionCoded4);
		mockMessageResponse.Setup(x => x.SyntaxErrorCode).Returns("ERR");
		mockMessageResponse.Setup(x => x.ErrorDataElementPosition).Returns("1");
		mockMessageResponse.Setup(x => x.ErrorDataElementComponentPosition).Returns("2");
		var mockSegmentError = new Mock<ISegmentErrorProvider>();
		mockMessageResponse.Setup(x => x.SegmentErrors).Returns(Array.Empty<ISegmentErrorProvider>());

		var mockDataProvider = new Mock<ICONTRLDataProvider>();
		mockDataProvider.Setup(x => x.InterchangeResponse).Returns(mockInterchangeResponse.Object);
		mockDataProvider.Setup(x => x.MessageResponse).Returns(mockMessageResponse.Object);

		var result = Interpreter.GetMessageInterpretation(mockDataProvider.Object);
		var expected = "<td>1</td><td>Interchange Reference Number: INT001<br>No errors in the UNB/UNZ segment, but one or more validation errors in the message within the interchange</td>";
		var expected1 = "<td>2</td><td>Message Reference Number: MSG001<br>The UNH/UNT segment is missing or has a validation error<br>Error Code: ERR - ERR DESC<br>Error occurred at data element 1, component position 2</td>";

		CombineAssertions(() =>
		{
			AssertContains(expected, result);
			AssertContains(expected1, result);
		});
	}

	public void TestInterpretation_InvalidMessageContent_SingleError()
	{
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupCustomsStatuses(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest);
		var mockInterchangeResponse = CreateMockResponse<ICONTRLInterchangeResponseProvider>("INT001", ActionCodedList.ActionCoded7);
		var mockMessageResponse = CreateMockResponse<ICONTRLMessageResponseProvider>("MSG001", ActionCodedList.ActionCoded7);

		var mockSegmentError = new Mock<ISegmentErrorProvider>();
		mockSegmentError.Setup(x => x.SegmentPosition).Returns("3");
		mockSegmentError.Setup(x => x.SegmentSyntaxErrorCode).Returns("ERR");
		mockSegmentError.Setup(x => x.DataElementErrors).Returns(Array.Empty<ISyntaxErrorProvider>());
		mockMessageResponse.Setup(x => x.SegmentErrors).Returns(new[] { mockSegmentError.Object });

		var mockDataProvider = new Mock<ICONTRLDataProvider>();
		mockDataProvider.Setup(x => x.InterchangeResponse).Returns(mockInterchangeResponse.Object);
		mockDataProvider.Setup(x => x.MessageResponse).Returns(mockMessageResponse.Object);

		var result = Interpreter.GetMessageInterpretation(mockDataProvider.Object);
		var expected = "<td>1</td><td>Interchange Reference Number: INT001<br>No errors in the UNB/UNZ segment, but one or more validation errors in the message within the interchange</td>";
		var expected1 = "<td>3</td><td>Error occurred in segment 3<br>Error Code: ERR - ERR DESC";

		CombineAssertions(() =>
		{
			AssertContains(expected, result);
			AssertContains(expected1, result);
		});
	}

	public void TestInterpretation_InvalidMessageContent_MultipleErrors()
	{
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupCustomsStatuses(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest);
		var mockInterchangeResponse = CreateMockResponse<ICONTRLInterchangeResponseProvider>("INT001", ActionCodedList.ActionCoded7);
		var mockMessageResponse = CreateMockResponse<ICONTRLMessageResponseProvider>("MSG001", ActionCodedList.ActionCoded7);
		mockMessageResponse.Setup(x => x.SegmentErrors).Returns(new[] { CreateErrorSegment("3", "ERR"), CreateErrorSegment("4", "UNK", "2") });

		var mockDataProvider = new Mock<ICONTRLDataProvider>();
		mockDataProvider.Setup(x => x.InterchangeResponse).Returns(mockInterchangeResponse.Object);
		mockDataProvider.Setup(x => x.MessageResponse).Returns(mockMessageResponse.Object);

		var result = Interpreter.GetMessageInterpretation(mockDataProvider.Object);
		var expected = "<td>3</td><td>Error occurred in segment 3<br>Error Code: ERR - ERR DESC<br>Error occurred at data element 1";
		var expected1 = "<td>4</td><td>Error occurred in segment 4<br>Error Code: UNK - N/A<br>Error occurred at data element 1, component position 2";

		CombineAssertions(() =>
		{
			AssertContains(expected, result);
			AssertContains(expected1, result);
		});

		ISegmentErrorProvider CreateErrorSegment(string segmentPosition, string errorCode, string componentPosition = "-1")
		{
			var mockSyntaxError = new Mock<ISyntaxErrorProvider>();
			mockSyntaxError.Setup(x => x.SyntaxErrorCode).Returns(errorCode);
			mockSyntaxError.Setup(x => x.ErrorDataElementPosition).Returns("1");
			mockSyntaxError.Setup(x => x.ErrorDataElementComponentPosition).Returns(componentPosition);

			var mockSegmentError = new Mock<ISegmentErrorProvider>();
			mockSegmentError.Setup(x => x.SegmentPosition).Returns(segmentPosition);
			mockSegmentError.Setup(x => x.DataElementErrors).Returns(new[] { mockSyntaxError.Object });

			return mockSegmentError.Object;
		}
	}

	Mock<T> CreateMockResponse<T>(string reference, string actionCode) where T : class, ICONTRLInterchangeResponseProvider
	{
		var mockResponse = new Mock<T>();
		mockResponse.Setup(x => x.OutgoingReference).Returns(reference);
		mockResponse.Setup(x => x.ActionCode).Returns(actionCode);
		return mockResponse;
	}

	CONTRLMessageInterpreter Interpreter => interpreter ??= new CONTRLMessageInterpreter(Factory.New<EDIMessage>());
	CONTRLMessageInterpreter interpreter;
}
