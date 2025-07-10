using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC022CMessageInterpreter))]
	sealed class CC022CMessageInterpreterTest : MessageInterpreterTestCase<CC022CMessageInterpreter, ICC022CDataProvider>
	{
		public override void TestInterpret()
		{
			FunctionalErrorXmlProvider CreateFunctionalErrorMock(int number)
			{
				var functionalError = FunctionalErrorXmlProvider.New(new FunctionalErrorType01());
				functionalError.SequenceNumber = $"{number}1";
				functionalError.ErrorCode = $"{number}2";
				functionalError.ErrorReason = "No clue";
				functionalError.ErrorPointer = $"{number}3";
				functionalError.OriginalAttributeValue = $"{number}4";
				return functionalError;
			}

			var currentDateTime = DateTime.Now;

			var expectedResult = $"Declaration received a request to amend the declaration on {currentDateTime.ToString("dd-MMM-y HH:mm:ss")}</br>" +
				$"11. Functional error code: 12</br>" +
				$"Reason: No clue</br>" +
				$"Attribute: 13</br>" +
				$"Element in declaration contains now the value: 14</br></br>" +
				$"21. Functional error code: 22</br>" +
				$"Reason: No clue</br>" +
				$"Attribute: 23</br>" +
				$"Element in declaration contains now the value: 24</br>";

			Mock<ICC022CDataProvider> mockProvider = new Mock<ICC022CDataProvider>();
			mockProvider.Setup(x => x.AmendmentNotificationDateAndTime).Returns(currentDateTime);
			mockProvider.Setup(x => x.FunctionalErrors).Returns(new[] { CreateFunctionalErrorMock(1), CreateFunctionalErrorMock(2) });

			AssertEquals(expectedResult, Interpreter.Interpret(mockProvider.Object, null));
		}
	}
}
