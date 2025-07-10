using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC928CMessageInterpreter))]
sealed class CC928CMessageInterpreterTest : MessageInterpreterTestCase<CC928CMessageInterpreter, ICC928CDataProvider>
{
	public override void TestInterpret()
	{
		var currentDateTime = new DateTime(2022, 4, 1);
		var mockCC928C = new Mock<ICC928CDataProvider>();
		var correlationId = ZGuid.NewZGuid().ToString();
		mockCC928C.Setup(m => m.CorrelationId).Returns(correlationId);

		var ediMessage = Factory.NewWithValidTestData<EDIMessage>();
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		entryHeader.CH_EntryStatus = "EXT";
		ediMessage.EM_LinkedObject = entryHeader;

		var result = Interpreter.Interpret(mockCC928C.Object, ediMessage);

		CombineAssertions(() =>
		{
			AssertEquals($"Correlation Id: {correlationId}</br>Status remains EXT", result);

			entryHeader.CH_EntryStatus = "ACC";
			result = Interpreter.Interpret(mockCC928C.Object, ediMessage);
			AssertEquals($"Correlation Id: {correlationId}</br>Status is set to ACC", result);
		});
	}
}
