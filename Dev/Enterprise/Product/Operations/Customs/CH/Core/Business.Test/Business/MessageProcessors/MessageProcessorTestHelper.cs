using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.CH.Business.Testing;

public static class MessageProcessorTestHelper
{
	public static (CusEntryHeader, EDIMessage) CreateHeaderAndMessage(BusinessObjectFactory factory, string messageType, string messageSubType, ZString messageResponse, ZString referenceBGM, ZGuid? messageBranchPK = null)
	{
		var entryHeader = CreateEntryHeader(factory, messageType, referenceBGM);
		var ediMessage = CreateEDIMessage(factory, messageType: messageType, messageSubType: messageSubType, messageText: messageResponse, messageBranchPK: messageBranchPK);
		return (entryHeader, ediMessage);
	}

	public static (CusEntryHeader, EDIMessage) CreateHeaderAndMessageWithCusEntryNum(BusinessObjectFactory factory, string messageType, string messageSubType, ZString messageResponse, ZString cusEntryNum)
	{
		var entryHeader = CreateEntryHeader(factory, messageType, ZString.Empty);
		entryHeader.MovementReferenceNumberSetter(cusEntryNum);
		var ediMessage = CreateEDIMessage(factory, messageType: messageType, messageSubType: messageSubType, messageText: messageResponse);
		return (entryHeader, ediMessage);
	}

	public static EDIMessage CreateEDIMessage(BusinessObjectFactory factory, string messageType = null, string messageSubType = null, string applicationCode = ApplicationCodeList.Codes.CHCustomsEdec, string direction = EDIMessage.Direction.Receive, string status = EDIMessage.Status.Queued, string messageText = "", string applicationReference = null, string messageNum = null, BusinessObject linkedObject = null, ZGuid? messageBranchPK = null)
	{
		var message = factory.New<CHEDIMessage>();
		message.EM_ApplicationCode = applicationCode;
		message.EM_ReceiveTransmit = direction;
		message.EM_MessageType = messageType;
		message.EM_MessageSubType = messageSubType;
		message.EM_IsTestMessage = true;
		message.EM_GB = messageBranchPK ?? GlbBranch.CurrentBranch.PK;
		message.EM_MessageNum = string.IsNullOrEmpty(messageNum) ? "001" : messageNum;
		message.EM_Status = status;
		message.EM_MessageText = messageText;
		message.EM_ApplicationReference = applicationReference;
		message.EM_IsTestMessage = true;
		message.EM_LinkedObject = linkedObject;
		if (direction == EDIMessage.Direction.Receive)
		{
			message.EM_EI = CreateEDIInterchange(factory, new ZGuid(), applicationCode, direction, null).PK;
		}
		return message;
	}

	public static CHEDIMessage CreateSentEdiMessage(BusinessObjectFactory factory, string applicationReference = null, string messageType = "", string messageSubType = "")
	{
		var sessionGuid = ZGuid.NewZGuid();

		var sentEdiInterchange = factory.New<EDIInterchange>();
		sentEdiInterchange.EI_From = "CW1";
		sentEdiInterchange.EI_To = "Customs";
		sentEdiInterchange.EI_SessionGUID = sessionGuid;
		sentEdiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;

		var sentEdiMessage = factory.New<CHEDIMessage>();
		sentEdiMessage.EM_Status = EDIMessage.Status.Sent;
		sentEdiMessage.EM_ApplicationCode = ApplicationCodeList.Codes.CHCustomsPassar;
		sentEdiMessage.EM_MessageType = messageType;
		sentEdiMessage.EM_MessageSubType = messageSubType;
		sentEdiMessage.EM_ApplicationReference = applicationReference;
		sentEdiMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;

		sentEdiInterchange.ContainedMessages.Add(sentEdiMessage);

		return sentEdiMessage;
	}

	static CusEntryHeader CreateEntryHeader(BusinessObjectFactory factory, ZString type, ZString reference)
	{
		var jobDeclaration = factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = type;
		var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		entryHeader.CH_Status = CHLogicalStatusList.Codes.Sent;
		entryHeader.CH_BGMReference = reference;
		return entryHeader;
	}

	public static EDIInterchange CreateEDIInterchange(BusinessObjectFactory factory, ZGuid sessionGuid, string applicationCode, string direction, EDIMessage containedMessage, string bodyText = null)
	{
		var ediInterchange = factory.New<EDIInterchange>();
		ediInterchange.EI_ApplicationCode = applicationCode;
		ediInterchange.EI_From = direction == EDIMessage.Direction.Receive ? "Customs" : "CW1";
		ediInterchange.EI_To = direction == EDIMessage.Direction.Receive ? "CW1" : "Customs";
		ediInterchange.EI_SessionGUID = sessionGuid;
		ediInterchange.EI_ReceiveTransmit = direction;
		ediInterchange.EI_BodyText = bodyText;
		if (containedMessage != null)
		{
			ediInterchange.ContainedMessages.Add(containedMessage);
		}
		return ediInterchange;
	}

	public static (CusEntryHeader, EDIMessage) CreateHeaderMessagesAndInterchanges(BusinessObjectFactory factory, string applicationCode, string messageType, string outgoingMessageSubType, string incomingMessageSubType, ZString messageResponse, string applicationReference = "")
	{
		var jobDeclaration = factory.New<JobDeclaration>();
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_Status = "AWO";

		var sentEdiMessage = CreateEDIMessage(factory, messageType, outgoingMessageSubType, applicationCode, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, applicationReference: applicationReference);
		entryHeader.Messages.Add(sentEdiMessage);

		var sessionGuid = ZGuid.NewZGuid();

		CreateEDIInterchange(factory, sessionGuid, applicationCode, EDIInterchange.Direction.Transmit, sentEdiMessage);

		var receivedEdiMessage = CreateEDIMessage(factory, messageType, incomingMessageSubType, ApplicationCodeList.Codes.CHCustomsEdec, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, messageResponse, messageNum: "002");

		CreateEDIInterchange(factory, sessionGuid, applicationCode, EDIInterchange.Direction.Receive, receivedEdiMessage);

		factory.Save();

		return (entryHeader, receivedEdiMessage);
	}

	public static (ForwardingShipment, EDIMessage) CreateShipmentMessagesAndInterchanges(BusinessObjectFactory factory, string applicationCode, string messageType, string outgoingMessageSubType, string incomingMessageSubType, ZString messageResponse)
	{
		var shipment = factory.New<ForwardingShipment>();

		var sentEdiMessage = CreateEDIMessage(factory, messageType, outgoingMessageSubType, applicationCode, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent);
		shipment.Messages.Add(sentEdiMessage);

		var sessionGuid = ZGuid.NewZGuid();

		CreateEDIInterchange(factory, sessionGuid, applicationCode, EDIInterchange.Direction.Transmit, sentEdiMessage);

		var receivedEdiMessage = CreateEDIMessage(factory, messageType, incomingMessageSubType, ApplicationCodeList.Codes.CHCustomsEdec, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, messageResponse, messageNum: "002");

		CreateEDIInterchange(factory, sessionGuid, applicationCode, EDIInterchange.Direction.Receive, receivedEdiMessage);

		factory.Save();

		return (shipment, receivedEdiMessage);
	}

	public static (GlbCompany, EDIMessage) CreateCompanyMessagesAndInterchanges(BusinessObjectFactory factory, string applicationCode, string messageType, string messageSubType, ZString messageResponse, string sentApplicationReference = null, string outgoingInterchangeBodyText = null, string companyCode = null, bool certificateCredential = false, string customsRegNo = null)
	{
		var company = CreateCompany(factory, companyCode: companyCode, tokenCredential: !certificateCredential, certificateCredential: certificateCredential, customsRegNo: customsRegNo);

		var sentEdiMessage = CreateEDIMessage(factory, messageType, applicationCode: applicationCode, direction: EDIMessage.Direction.Transmit, status: EDIMessage.Status.Sent, applicationReference: sentApplicationReference);
		sentEdiMessage.EM_LinkedObject = company;

		var sentEdiInterchange = CreateEDIInterchange(factory, ZGuid.NewZGuid(), applicationCode, EDIInterchange.Direction.Transmit, sentEdiMessage, outgoingInterchangeBodyText);

		var receivedEdiInterchange = CreateEDIInterchange(factory, sentEdiInterchange.EI_SessionGUID, sentEdiInterchange.EI_ApplicationCode, EDIInterchange.Direction.Receive, null);

		var receivedEdiMessage = CreateEDIMessage(factory, messageType, messageSubType, receivedEdiInterchange.EI_ApplicationCode, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, messageResponse, string.Empty, "002");
		receivedEdiMessage.EM_GB = GlbBranch.CurrentBranch.PK;

		receivedEdiInterchange.ContainedMessages.Add(receivedEdiMessage);
		factory.Save();

		return (company, receivedEdiMessage);
	}

	public static EDIMessage CreateMessagesAndInterchangesWithEmptyCompany(BusinessObjectFactory factory, string applicationCode, string messageType, string messageSubType, ZString messageResponse, string outgoingInterchangeBodyText = null)
	{
		var sentEdiMessage = CreateEDIMessage(factory, messageType, applicationCode: applicationCode, direction: EDIMessage.Direction.Transmit, status: EDIMessage.Status.Sent);

		var sentEdiInterchange = CreateEDIInterchange(factory, ZGuid.NewZGuid(), applicationCode, EDIInterchange.Direction.Transmit, sentEdiMessage, outgoingInterchangeBodyText);

		var receivedEdiInterchange = CreateEDIInterchange(factory, sentEdiInterchange.EI_SessionGUID, sentEdiInterchange.EI_ApplicationCode, EDIInterchange.Direction.Receive, null);

		var receivedEdiMessage = CreateEDIMessage(factory, messageType, messageSubType, receivedEdiInterchange.EI_ApplicationCode, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, messageResponse, messageNum: "003");

		receivedEdiInterchange.ContainedMessages.Add(receivedEdiMessage);
		factory.Save();

		return receivedEdiMessage;
	}

	public static EDIMessage GetOutgoingMessage(EDIMessage incomingMessage)
	{
		var factory = incomingMessage.Factory;
		var outgoingInterchange = factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SessionGUID, incomingMessage.Interchange.EI_SessionGUID));
		var outgoingMessage = factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, outgoingInterchange.PK));
		return outgoingMessage;
	}

	public static GlbCompany CreateCompany(BusinessObjectFactory factory, string companyCode = null, string branchCode = null, string bpid = null, string cad = null, string customsRegNo = null, bool tokenCredential = false, bool certificateCredential = false)
	{
		var cCode = string.IsNullOrEmpty(companyCode) ? "C01" : companyCode;
		var bCode = string.IsNullOrEmpty(branchCode) ? "B01" : branchCode;

		var company = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, cCode)) ?? factory.New<GlbCompany>();
		company.GC_Code = cCode;
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		company.GC_OH_OrgProxy = factory.NewWithValidTestData<OrgHeader>().PK;
		company.GC_IsActive = ZBool.True;

		if (!company.Branches.Any(b => b.GB_Code == bCode))
		{
			var branch = company.Branches.AddNew();
			branch.GB_Code = bCode;
			branch.GB_IsActive = ZBool.True;
		}

		if (tokenCredential)
		{
			CredentialsTestHelper.CreateCompanyTokenCredential(company);
		}

		if (certificateCredential)
		{
			CredentialsTestHelper.CreateCompanyCertificateCredential(company);
		}

		if (customsRegNo != null)
		{
			company.GC_CustomsRegistrationNo = customsRegNo;
		}

		if (bpid != null)
		{
			company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, bpid);
		}

		if (cad != null)
		{
			company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.CAD, cad);
		}

		factory.Save();
		return company;
	}

	public static void ProcessMessage(ApplicationTypeMessageProcessor processor, EDIMessage message)
	{
		if (processor.RequiresPreProcessing)
		{
			processor.PreProcessMessage(message);
			AssertEquals("PreProcessedOK", EDIMessage.Status.PreProcessedOK, message.EM_Status);
		}
		processor.ProcessMessage(message);
	}
}
