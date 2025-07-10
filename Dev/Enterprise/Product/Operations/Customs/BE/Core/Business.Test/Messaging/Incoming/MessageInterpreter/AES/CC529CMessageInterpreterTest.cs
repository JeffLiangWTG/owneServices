using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC529CMessageInterpreter))]
sealed class CC529CMessageInterpreterTest : MessageInterpreterTestCase<CC529CMessageInterpreter, ICC529CDataProvider>
{
	public override void TestInterpret()
	{
		var mockCC529C = new Mock<ICC529CDataProvider>();
		mockCC529C.Setup(m => m.ReleaseDate).Returns(new DateTime(2022, 4, 1));

		var ediMessage = Factory.NewWithValidTestData<EDIMessage>();
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		entryHeader.CH_EntryStatus = "DRL";
		ediMessage.EM_LinkedObject = entryHeader;

		var result = Interpreter.Interpret(mockCC529C.Object, ediMessage);
		AssertEquals("Declaration is released on 01-Apr-22</br>Status is changed to DRL", result);
	}
}
