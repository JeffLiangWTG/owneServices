using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC504CMessageInterpreter))]
sealed class CC504CMessageInterpreterTest : MessageInterpreterTestCase<CC504CMessageInterpreter, ICC504CDataProvider>
{
	public override void TestInterpret()
	{
		var mockProvider = new Mock<ICC504CDataProvider>();
		mockProvider.CallBase = true;
		mockProvider.Setup(m => m.AmendmentAcceptanceDateAndTime).Returns(new DateTime(2023, 1, 16, 16, 55, 24));
		var provider = mockProvider.Object;
		var ediMessage = Factory.NewWithValidTestData<EDIMessage>();
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		entryHeader.CH_EntryStatus = "DRL";
		ediMessage.EM_LinkedObject = entryHeader;
		var mockProcessor = new Mock<CC504CMessageProcessor>(new BatchProcessor.LoggingInformation());
		var result = Interpreter.Interpret(mockProvider.Object, ediMessage);
		AssertEquals("Declaration is amended on 16/01/2023 4:55:24 PM<br />Status is set to DRL<br />", result);
	}
}
