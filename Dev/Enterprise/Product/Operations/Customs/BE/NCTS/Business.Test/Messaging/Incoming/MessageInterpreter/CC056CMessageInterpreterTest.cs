using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC056CMessageInterpreter))]
	sealed class CC056CMessageInterpreterTest : MessageInterpreterTestCase<CC056CMessageInterpreter, ICC056CDataProvider>
	{
		public override void TestInterpret()
		{
			var functionalError = new List<FunctionalErrorXmlProvider>
			{
				FunctionalErrorXmlProvider.New(new FunctionalErrorType01
				{
					ErrorPointer = "EP1", ErrorCode = "21", ErrorReason = "bad type one", OriginalAttributeValue = "11"
				}),
				FunctionalErrorXmlProvider.New(new FunctionalErrorType01
				{
					ErrorPointer = "EP2", ErrorCode = "22", ErrorReason = "AR013", OriginalAttributeValue = "12"
				})
			};
			var controlDateAndTime = new DateTime(2022, 6, 1, 12, 34, 56, DateTimeKind.Utc);
			var mockCC056C = new Mock<ICC056CDataProvider>();
			mockCC056C.Setup(m => m.BusinessRejectionType).Returns("015");
			mockCC056C.Setup(m => m.RejectionDateAndTimeUtc).Returns(controlDateAndTime);
			mockCC056C.Setup(m => m.RejectionCode).Returns("4");
			mockCC056C.Setup(m => m.RejectionReason).Returns("ABC");
			mockCC056C.Setup(m => m.FunctionalErrorList).Returns(functionalError);
			var result = Interpreter.Interpret(mockCC056C.Object, null);
			AssertEquals("Declaration received an error for type 015 on 01-06-2022 14:34:56 UTC+2</br>Reason: 4 ABC</br>Functional error code: 21</br>Reason: bad type one</br>Attribute: EP1</br>Element in declaration contains now the value: 11</br></br>Functional error code: 22</br>Reason: AR013 == Amendment is not supported yet by Customs</br>Attribute: EP2</br>Element in declaration contains now the value: 12</br>", result);
		}
	}
}
