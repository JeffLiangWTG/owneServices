using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC028CMessageInterpreter))]
	sealed class CC028CMessageInterpreterTest : MessageInterpreterTestCase<CC028CMessageInterpreter, ICC028CDataProvider>
	{
		public override void TestInterpret()
		{
			var mockCC028C = new Mock<ICC028CDataProvider>();
			mockCC028C.Setup(m => m.EntryDate).Returns(new DateTime(2022, 4, 1, 12, 34, 56));
			mockCC028C.Setup(m => m.CorrelationIdentifier).Returns("123");
			var result = Interpreter.Interpret(mockCC028C.Object, null);
			AssertContains("New declaration status: Declaration MRN Allocated", result);
		}
	}
}
