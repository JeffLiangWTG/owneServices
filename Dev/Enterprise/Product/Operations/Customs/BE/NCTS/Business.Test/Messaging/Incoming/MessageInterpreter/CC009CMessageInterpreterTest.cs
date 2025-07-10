using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC009CMessageInterpreter))]
	sealed class CC009CMessageInterpreterTest : MessageInterpreterTestCase<CC009CMessageInterpreter, ICC009CDataProvider>
	{
		public override void TestInterpret()
		{
			var decisionDateTimeUtc = DateTime.UtcNow.AddDays(-5);
			var requestDateTimeUtc = DateTime.UtcNow.AddDays(-10);
			const string justificationdescription = "JustificationDescription";
			var correlationId = ZGuid.NewZGuid().ToString();

			var mockCC009C = new Mock<ICC009CDataProvider>();
			mockCC009C.Setup(m => m.Decision).Returns(true);
			mockCC009C.Setup(m => m.DecisionDateTimeUtc).Returns(decisionDateTimeUtc);
			mockCC009C.Setup(m => m.RequestDateTimeUtc).Returns(requestDateTimeUtc);
			mockCC009C.Setup(m => m.InitiatedByCustoms).Returns(true);
			mockCC009C.Setup(m => m.Justification).Returns(justificationdescription);
			mockCC009C.Setup(m => m.CorrelationIdentifier).Returns(correlationId);

			var result = Interpreter.Interpret(mockCC009C.Object, null);

			AssertEquals(
				"New declaration status: Cancellation accepted</br>" +
				$"Status granted on: {NctsMessageHelper.GetUtcDateTimeToLocalBEBranchString(decisionDateTimeUtc)}</br>" +
				$"Request date and time to invalidate/cancel: {NctsMessageHelper.GetUtcDateTimeToLocalBEBranchString(requestDateTimeUtc)}</br>" +
				"Initiated by customs: yes</br>" +
				$"Justification: {justificationdescription}</br>" +
				$"Correlation id: {correlationId}", result);
		}

		public void TestInterpret_WhenDecisionIsFalse()
		{
			var mockCC009C = new Mock<ICC009CDataProvider>();
			mockCC009C.Setup(m => m.Decision).Returns(false);

			var result = Interpreter.Interpret(mockCC009C.Object, null);

			AssertContains("New declaration status: Cancellation refused", result);
		}

		public void TestInterpretWhenInitiatedByCustomsIsFalse()
		{
			var mockCC009C = new Mock<ICC009CDataProvider>();
			mockCC009C.Setup(m => m.InitiatedByCustoms).Returns(false);

			var result = Interpreter.Interpret(mockCC009C.Object, null);

			AssertContains("Initiated by customs: no", result);
		}
	}
}
