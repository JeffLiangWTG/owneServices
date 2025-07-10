using System;
using System.Text;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestsSubclassesOf(typeof(InboundMessageDocumentWrapper<>))]
abstract class InboundMessageDocumentWrapperTest<T1, T2> : NonPersistentBusinessObjectTestCase
	where T1 : InboundMessageDocumentWrapper<T2>
	where T2 : IJPInboundMessageDataProvider
{
	protected virtual T1 CreateNewMessageDocumentWrapper(IJPInboundMessageParseResult inboundMessageParseResult)
	{
		return (T1)Activator.CreateInstance(typeof(T1), inboundMessageParseResult, Factory);
	}

	protected T1 MessageDocumentWrapper => messageDocumentWrapper ??= CreateNewMessageDocumentWrapper(GetInboundMessageParseResult());
	T1 messageDocumentWrapper;

	IJPInboundMessageParseResult GetInboundMessageParseResult()
	{
		var parser = NACCSFactoryService.GetInboundMessageParser(Factory);
		return parser.Parse(Encoding.ASCII.GetBytes(TestDataHelper.GetResourceStream(GetDefaultMessageTestFile())));
	}

	protected abstract string GetDefaultMessageTestFile();

	protected override BusinessObject GetNewBusinessObject()
	{
		return MessageDocumentWrapper;
	}
}

[TestedType(typeof(ExportPermitMessageDocumentWrapper))]
sealed class InboundMessageDocumentWrapperBaseTest : InboundMessageDocumentWrapperTest<ExportPermitMessageDocumentWrapper, IExportClearancePermit>
{
	public void TestCommonProperties()
	{
		var wrapper = MessageDocumentWrapper;

		CombineAssertions(() =>
		{
			AssertEquals("Procedure Code", "EDC", wrapper.ProcedureCode);
			AssertEquals("Output Information Code", "SAE1LF3", wrapper.OutputInformationCode);
			AssertEquals("Received Date Time", "2023/11/06 14:33", wrapper.ReceivedDateTime);
			AssertEquals("User Code", "XXXXX", wrapper.UserCode);
			AssertEquals("User Mail Address", "XXX70201@MAIL.PROD.NACCS6", wrapper.UserMailAddress);
			AssertEquals("Subject", "21233784930", wrapper.Subject);
			AssertEquals("Message Tag", "EDC00000000003200000000353", wrapper.MessageTag);
			AssertEquals("Division Number", "001", wrapper.DivisionNumber);
			AssertEquals("Last Message", "E", wrapper.LastMessage);
			AssertEquals("Message Type", "P", wrapper.MessageType);
			AssertEquals("Input Reference", "0000000020", wrapper.InputReference);
			AssertEquals("Split Reference", "123", wrapper.SplitReference);
			AssertEquals("Management Type", "1", wrapper.ManagementType);
			AssertEquals("Message Length", "123456", wrapper.MessageLength);
			AssertEquals("TransportMode", "S", wrapper.TransportMode);
		});
	}

	protected override string GetDefaultMessageTestFile() => "ExportClearancePermitTestMessage.txt";
}
