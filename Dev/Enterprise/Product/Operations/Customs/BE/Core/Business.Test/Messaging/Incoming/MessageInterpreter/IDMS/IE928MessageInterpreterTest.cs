using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(IE928MessageInterpreter))]
sealed class IE928MessageInterpreterTest : MessageInterpreterTestCase<IE928MessageInterpreter, IIE928DataProvider>
{
	public override void TestInterpret()
	{
		var currentDateTime = new DateTime(2022, 4, 1);
		var mockIE928 = new Mock<IIE928DataProvider>();
		var correlationId = ZGuid.NewZGuid().ToString();
		mockIE928.Setup(m => m.CorrelationId).Returns(correlationId);

		var ediMessage = Factory.NewWithValidTestData<EDIMessage>();
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		entryHeader.CH_EntryStatus = "EXT";
		ediMessage.EM_LinkedObject = entryHeader;

		var result = Interpreter.Interpret(mockIE928.Object, ediMessage);

		CombineAssertions(() =>
		{
			AssertEquals($"Correlation Id: {correlationId}</br>Status remains EXT", result);

			entryHeader.CH_EntryStatus = "ACC";
			result = Interpreter.Interpret(mockIE928.Object, ediMessage);
			AssertEquals($"Correlation Id: {correlationId}</br>Status is set to ACC", result);
		});
	}
}
