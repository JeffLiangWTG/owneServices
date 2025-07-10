using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC004CMessageInterpreter))]
	sealed class CC004CMessageInterpreterTest : MessageInterpreterTestCase<CC004CMessageInterpreter, ICC004CDataProvider>
	{
		public override void TestInterpret()
		{
			var mockCC004C = new Mock<ICC004CDataProvider>();
			mockCC004C.Setup(m => m.EntryDate).Returns(new DateTime(2022, 4, 1, 12, 34, 56));
			mockCC004C.Setup(m => m.SubmissionDate).Returns(new DateTime(2022, 4, 2, 12, 34, 56));
			mockCC004C.Setup(m => m.CorrelationIdentifier).Returns("123");
			var result = Interpreter.Interpret(mockCC004C.Object, null);
			AssertEquals("New detailed status: 'Amendment acceptance'</br>Status granted on: 01-Apr-22 12:34:56</br>Amendment submission date and time: 02-Apr-22 12:34:56</br>Correlation id: 123", result);
		}
	}
}
