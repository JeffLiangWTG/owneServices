using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT141DataProvider))]
sealed class NT141DataProviderTest : BaseDepartureDataProviderTest<NT141DataProvider, NctsHeaderDepartureMessageSendingObject>
{
	protected override NT141DataProvider CreateDataProvider() => new NT141DataProvider(MessageSendingObject);

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new NT141DataProvider(null));
	}

	public void TestTransitOperation() => CombineAssertions(() =>
	{
		AssertNotNull("Instance provided", DataProvider.TransitOperation);
		AssertSame("Instance cached", DataProvider.TransitOperation, DataProvider.TransitOperation);
	});

	public void TestEnquiry() => CombineAssertions(() =>
	{
		AssertNotNull("Instance provided", DataProvider.Enquiry);
		AssertSame("Instance cached", DataProvider.Enquiry, DataProvider.Enquiry);
	});

	public void TestCustomsOfficeOfDestinationActual() => CombineAssertions(() =>
	{
		MessageSendingObject.ActualDestinationCustomsOffice = "office";
		MessageSendingObject.ReasonText = "some text";
		AssertNotNull("Instance provided", DataProvider.CustomsOfficeOfDestinationActual);
		AssertEquals("Office provided", "office", DataProvider.CustomsOfficeOfDestinationActual?.ReferenceNumber);
		AssertSame("Instance cached", DataProvider.CustomsOfficeOfDestinationActual, DataProvider.CustomsOfficeOfDestinationActual);

		ResetDataProvider();
		MessageSendingObject.ActualDestinationCustomsOffice = ZString.Empty;
		AssertNull("No office", DataProvider.CustomsOfficeOfDestinationActual);

		ResetDataProvider();
		MessageSendingObject.ActualDestinationCustomsOffice = "office";
		MessageSendingObject.ReasonText = ZString.Empty;
		AssertNull("Not provided if no ReasonText", DataProvider.CustomsOfficeOfDestinationActual);
	});

	public void TestConsigneeActual() => CombineAssertions(() =>
	{
		MessageSendingObject.ActualConsignee.E2_AddressOverride = ZBool.True;
		MessageSendingObject.ActualConsignee.E2_CompanyName = "consignee";
		MessageSendingObject.ReasonText = "some text";
		AssertNotNull("Instance provided", DataProvider.ConsigneeActual);
		AssertEquals("Address provided", "consignee", DataProvider.ConsigneeActual?.Name);
		AssertSame("Instance cached", DataProvider.ConsigneeActual, DataProvider.ConsigneeActual);

		ResetDataProvider();
		MessageSendingObject.ReasonText = ZString.Empty;
		AssertNull("Not provided if no ReasonText", DataProvider.ConsigneeActual);
	});

	public void TestConsigneeAddressOmittedWithIdentificationNumber() => CombineAssertions(() =>
	{
		var orgAddress = Factory.New<OrgHeader>().MainAddress;
		orgAddress.CompanyName = "name";
		MessageSendingObject.ActualConsignee.E2_OA_Address = orgAddress.PK;
		MessageSendingObject.ReasonText = "some text";

		AssertNull("Without IdentificationNumber: IdentificationNumber", DataProvider.ConsigneeActual.IdentificationNumber);
		AssertNotNullOrEmpty("Without IdentificationNumber: Name", DataProvider.ConsigneeActual.Name);
		AssertNotNull("Without IdentificationNumber: Address", DataProvider.ConsigneeActual.Address);

		ResetDataProvider();
		MessageSendingObject.ActualConsignee.Organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123");
		AssertNotNullOrEmpty("With IdentificationNumber: IdentificationNumber", DataProvider.ConsigneeActual.IdentificationNumber);
		AssertNull("With IdentificationNumber: Name", DataProvider.ConsigneeActual.Name);
		AssertNull("With IdentificationNumber: Address", DataProvider.ConsigneeActual.Address);
	});

	[TestDateIncremental(seconds: 1)]
	public void TestCorrelationIdentifier() => CombineAssertions(() =>
	{
		var otherMovement = Factory.New<NctsHeader>();
		otherMovement.SetMovementType(NctsMovementType.Codes.Departure);

		CreateIncomingMessage("NOT_LATEST");
		CreateIncomingMessage("EXPECTED");
		CreateIncomingMessage("OTHER_APPCODE", applicationCode: EDIMessage.ApplicationCodes.CHCustomsEdec);
		CreateIncomingMessage("OTHER_MSGTYPE", messageType: MessageTypeCodeList.Codes.MSL);
		CreateIncomingMessage("OTHER_MSGSUBTYPE", messageType: MessageSubTypeCodeList.Codes.PassarActivationResponse);
		CreateIncomingMessage("NOT_RECEIVE", receiveTransmit: ReceiveTransmitList.Codes.Transmit);
		CreateIncomingMessage("NOT_PROCESSED", status: EDIMessage.Status.Queued);
		CreateIncomingMessage("OTHER_MOVEMENT", linkedObject: otherMovement.MovementHeader);
		Factory.Save();

		AssertEquals("CorrelationIdentifier", "EXPECTED", DataProvider.CorrelationIdentifier);
		AssertSame("cached", DataProvider.CorrelationIdentifier, DataProvider.CorrelationIdentifier);

		void CreateIncomingMessage(string messageIdentification, string applicationCode = EDIMessage.ApplicationCodes.CHCustomsPassar, string messageType = MessageTypeCodeList.Codes.MSG, string messageSubType = MessageSubTypeCodeList.Codes.PassarEnquiryOfNotArrivedTransit, string receiveTransmit = ReceiveTransmitList.Codes.Receive, string status = EDIMessage.Status.ProcessedOK, BusinessObject linkedObject = null)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT140(messageIdentification: messageIdentification));
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_ReceiveTransmit = receiveTransmit;
			message.EM_Status = status;
			message.EM_LinkedObject = linkedObject ?? NctsHeader.MovementHeader;
			message.MessageNumberStrategy = new MessageNumberStrategyForTesting("1");
		}
	});
}
