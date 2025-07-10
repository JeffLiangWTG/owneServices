using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC599CMessageInterpreter))]
sealed class CC599CMessageInterpreterTest : MessageInterpreterTestCase<CC599CMessageInterpreter, ICC599CDataProvider>
{
	public override void TestInterpret()
	{
		var currentDateTime = new DateTime(2022, 4, 1);
		var mockCC599C = new Mock<ICC599CDataProvider>();
		mockCC599C.Setup(m => m.StateOfSeals).Returns("1");
		mockCC599C.Setup(m => m.ControlResultCode).Returns("A2");
		mockCC599C.Setup(m => m.CustomsOfficeOfExit).Returns("Antwerp office");
		mockCC599C.Setup(m => m.ExitDate).Returns(currentDateTime);

		var ediMessage = Factory.NewWithValidTestData<EDIMessage>();
		var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
		entryHeader.CH_EntryStatus = "EXT";
		ediMessage.EM_LinkedObject = entryHeader;

		var result = Interpreter.Interpret(mockCC599C.Object, ediMessage);
		CombineAssertions(() =>
		{
			AssertEquals("Declaration has exited the EU on 01-Apr-22 according customs office Antwerp office</br>Control result code is A2 - Considered satisfactory</br>State of seals is OK</br>Status is set to EXT", result);

			mockCC599C.Setup(m => m.StateOfSeals).Returns("0");
			mockCC599C.Setup(m => m.ExitStoppedDate).Returns(currentDateTime);
			entryHeader.CH_EntryStatus = "NXT";
			result = Interpreter.Interpret(mockCC599C.Object, ediMessage);
			AssertEquals("Declaration has not exited the EU. The exit has stopped on 01-Apr-22 according customs office Antwerp office</br>Control result code is A2 - Considered satisfactory</br>State of seals is NOK</br>Status is set to NXT", result);
		});
	}
}
