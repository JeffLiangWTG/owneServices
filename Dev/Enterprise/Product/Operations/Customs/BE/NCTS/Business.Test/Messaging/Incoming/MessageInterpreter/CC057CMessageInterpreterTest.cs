using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC057CMessageInterpreter))]
	sealed class CC057CMessageInterpreterTest : MessageInterpreterTestCase<CC057CMessageInterpreter, ICC057CDataProvider>
	{
		public void TestInterpret_NullTransitOperation()
		{
			var mockCC057C = new Mock<ICC057CDataProvider>();
			mockCC057C.Setup(x => x.FunctionalErrors).Returns([]);

			var result = Interpreter.Interpret(mockCC057C.Object, null);
			AssertEquals(ZString.Empty, result);
		}

		public override void TestInterpret()
		{
			var mockCC057C = new Mock<ICC057CDataProvider>();

			var transitOperationProvider = TransitOperationXmlProvider.New(new TransitOperationType21
			{
				BusinessRejectionType = BEOutgoingMessageTypes.Codes.CC007C,
				RejectionCode = EU.NCTS.Business.RejectionCodes.Codes.Code4,
				RejectionDateAndTime = new DateTime(2022, 06, 01, 12, 34, 56),
				RejectionReason = "Invalid arrival date",
			});
			mockCC057C.Setup(x => x.TransitOperation).Returns(transitOperationProvider);

			var functionalError = FunctionalErrorXmlProvider.New(new FunctionalErrorType01());
			functionalError.ErrorPointer = "cc015c.DepartureTransportMeans(2).typeOfIdentification";
			functionalError.ErrorCode = EU.NCTS.Business.FunctionalErrorCodes.Codes.Code12;
			functionalError.ErrorReason = "Type of transport does not exist";
			functionalError.OriginalAttributeValue = "32";
			var list = new Collection<FunctionalErrorXmlProvider> { functionalError };
			mockCC057C.Setup(x => x.FunctionalErrors).Returns(new ReadOnlyCollection<FunctionalErrorXmlProvider>(list));

			var result = Interpreter.Interpret(mockCC057C.Object, null);

			var expectedResult = new ZStringBuilder();
			expectedResult.Append("Declaration received an error for type 007 (Arrival notification rejection) on 01-06-2022 14:34:56 UTC+2.<br />");
			expectedResult.Append("Reason: 4 (Other reasons) Invalid arrival date<br />");
			expectedResult.Append("Functional error code: 12 (Code list violation (incorrect enumeration))<br />");
			expectedResult.Append("Reason: Type of transport does not exist<br />");
			expectedResult.Append("Attribute: cc015c.DepartureTransportMeans(2).typeOfIdentification<br />");
			expectedResult.Append("Element in declaration contains now the value: 32<br /><br />");

			AssertEquals(expectedResult.ToString(), result);
		}
	}
}
