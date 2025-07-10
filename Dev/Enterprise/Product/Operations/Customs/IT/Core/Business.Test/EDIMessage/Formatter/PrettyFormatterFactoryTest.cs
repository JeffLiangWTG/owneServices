using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class PrettyFormatterFactoryTest : TestCaseWithFactory
{
	public void TestGetMessagePrettyFormatterType_NullFactory()
	{
		AssertExceptionThrown<ArgumentNullException>(() => PrettyFormatterFactory.GetMessagePrettyFormatter(null, messageEDI));
	}

	public void TestGetMessagePrettyFormatterType_NullMessageEDI()
	{
		var messagePrettyFormatter = PrettyFormatterFactory.GetMessagePrettyFormatter(Factory, null);
		AssertEquals("When Not Implemented Formatter", null, messagePrettyFormatter);
	}

	public void TestGetMessagePrettyFormatterType_Default()
	{
		messageEDI.EM_MessageType = "BAD";
		var messagePrettyFormatter = PrettyFormatterFactory.GetMessagePrettyFormatter(Factory, messageEDI);
		AssertEquals("When Not Implemented Formatter", null, messagePrettyFormatter);
	}

	public void TestGetMessagePrettyFormatterType_IrispX()
	{
		AssertGetTypeForMessageType(SADConstants.CustomsInterchangeType.IrispX, typeof(SadIrispEDIMessagePrettyFormatter));
	}

	public void TestGetMessagePrettyFormatterType_SingleWindowRequest()
	{
		AssertGetTypeForMessageType(MessageProcessorConstants.InterchangeTypes.SingleWindowRequest, typeof(SingleWindowEDIMessagePrettyFormatter));
	}

	public void TestGetMessagePrettyFormatterType_SingleWindowStatusResponseMessageType()
	{
		AssertGetTypeForMessageType(MessageProcessorConstants.InterchangeTypes.SingleWindowStatusResponseMessageType, typeof(SingleWindowEDIMessagePrettyFormatter));
	}

	public void TestGetMessagePrettyFormatterType_Ucc6ResponseMessageType()
	{
		AssertGetTypeForMessageType(MessageProcessorConstants.InterchangeTypes.Ucc6ResponseMessageType, typeof(Ucc6ResponseEDIMessagePrettyFormatter));
	}

	public void TestGetMessagePrettyFormatterType_Ucc6RequestEDIMessagePrettyFormatter()
	{
		CombineAssertions(() =>
		{
			messageEDI.EM_ApplicationReference = EDIMessageApplicationReferenceList.Codes.Import;
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.NewDeclaration, typeof(SoapMessageInputPrettyFormatter));
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.Amendment, typeof(SoapMessageInputPrettyFormatter));
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.Cancellation, typeof(SoapMessageInputPrettyFormatter));

			messageEDI.EM_ApplicationReference = EDIMessageApplicationReferenceList.Codes.Export;
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.NewDeclaration, typeof(SoapMessageInputPrettyFormatter));
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.Amendment, typeof(SoapMessageInputPrettyFormatter));
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.Cancellation, typeof(SoapMessageInputPrettyFormatter));
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.SignatureResponse, typeof(SoapMessageInputPrettyFormatter));

			messageEDI.EM_ApplicationReference = EDIMessageApplicationReferenceList.Codes.Ncts;
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.NewDeclaration, typeof(SoapMessageInputPrettyFormatter));
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.Amendment, typeof(SoapMessageInputPrettyFormatter));
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.Cancellation, typeof(SoapMessageInputPrettyFormatter));

			messageEDI.EM_ApplicationReference = EDIMessageApplicationReferenceList.Codes.TemporaryStorage;
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.NewDeclaration, typeof(SoapMessageInputPrettyFormatter));
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.Amendment, typeof(SoapMessageInputPrettyFormatter));
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.Cancellation, typeof(SoapMessageInputPrettyFormatter));

			messageEDI.EM_ApplicationReference = Common.EU.EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			messageEDI.EM_MessageType = EDIMessageTypeList.Codes.NewDeclaration;
			var messagePrettyFormatter = PrettyFormatterFactory.GetMessagePrettyFormatter(Factory, messageEDI);
			AssertNull("When EM_ApplicationCode is not Import Or Export ", messagePrettyFormatter);
		});
	}

	public void TestGetMessagePrettyFormatterType_ElectronicFolderQueryType()
	{
		AssertGetTypeForMessageType(MessageProcessorConstants.InterchangeTypes.ElectronicFolderQueryType, typeof(SoapMessageInputPrettyFormatter));
	}

	public void TestGetMessagePrettyFormatterType_ElectronicFolderResponseType()
	{
		AssertGetTypeForMessageType(MessageProcessorConstants.InterchangeTypes.ElectronicFolderResponseType, typeof(SoapMessageOutputPrettyFormatter));
	}

	public void TestGetMessagePrettyFormatterType_IvistoRequest()
	{
		AssertGetTypeForMessageType(EDIMessageTypeList.Codes.IvistoRequest, typeof(SoapMessageInputPrettyFormatter));
	}

	public void TestGetMessagePrettyFormatterType_IvistoResponse()
	{
		AssertGetTypeForMessageType(EDIMessageTypeList.Codes.IvistoResponse, typeof(Ucc6IvistoResponseEDIMessagePrettyFormatter));
	}

	public void TestGetMessagePrettyFormatterType_IrildesRequest()
	{
		AssertGetTypeForMessageType(EDIMessageTypeList.Codes.IrildesRequest, typeof(SoapMessageInputPrettyFormatter));
	}

	public void TestGetMessagePrettyFormatterType_IrildesResponse()
	{
		AssertGetTypeForMessageType(EDIMessageTypeList.Codes.IrildesResponse, typeof(IrildesResponseEDIMessagePrettyFormatter));
	}

	public void TestGetMessagePrettyFormatterType_AccountingSummaryRequestType()
	{
		AssertGetTypeForMessageType(EDIMessageTypeList.Codes.AccountingSummaryRequest, typeof(SoapMessageInputPrettyFormatter));
	}

	public void TestGetMessagePrettyFormatterType_SummaryProspectusRequestType()
	{
		CombineAssertions(() =>
		{
			messageEDI.IsTransmitMessage = true;
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.SummaryProspectusRequest, typeof(SoapMessageInputPrettyFormatter));

			messageEDI.IsTransmitMessage = false;
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.SummaryProspectusRequest, typeof(SoapMessageOutputPrettyFormatter));
		});
	}

	public void TestGetMessagePrettyFormatterType_SummaryProspectusDownloadType()
	{
		CombineAssertions(() =>
		{
			messageEDI.IsTransmitMessage = true;
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.SummaryProspectusDownload, typeof(SoapMessageInputPrettyFormatter));

			messageEDI.IsTransmitMessage = false;
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.SummaryProspectusDownload, typeof(SoapMessageOutputPrettyFormatter));
		});
	}

	public void TestGetMessagePrettyFormatterType_ReleaseProspectusRequestType()
	{
		CombineAssertions(() =>
		{
			messageEDI.IsTransmitMessage = true;
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.ReleaseProspectusRequest, typeof(SoapMessageInputPrettyFormatter));

			messageEDI.IsTransmitMessage = false;
			AssertGetTypeForMessageType(EDIMessageTypeList.Codes.ReleaseProspectusRequest, typeof(SoapMessageOutputPrettyFormatter));
		});
	}

	void AssertGetTypeForMessageType(ZString messageType, Type expectedPrettyFormatterType)
	{
		messageEDI.EM_MessageType = messageType;
		var messagePrettyFormatter = PrettyFormatterFactory.GetMessagePrettyFormatter(Factory, messageEDI);
		AssertEquals($"Message Pretty Formatter Type for '{messageType}'", expectedPrettyFormatterType, messagePrettyFormatter?.GetType());
	}

	protected override void SetUp()
	{
		base.SetUp();
		messageEDI = Factory.New<ITEDIMessage>();
	}

	ITEDIMessage messageEDI;
}
