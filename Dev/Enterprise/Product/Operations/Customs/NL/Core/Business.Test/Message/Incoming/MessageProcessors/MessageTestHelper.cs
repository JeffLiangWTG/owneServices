using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.NL.Business.Testing;

class MessageTestHelper
{
	public static NLEDIMessage SetupRFIMessage(BusinessObjectFactory factory, ZString bgmReference, ZString messageText, string initialPhaseStatus = "")
	{
		var declaration = factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_PhaseStatus = initialPhaseStatus;
		entryHeader.CH_BGMReference = bgmReference;
		entryHeader.MovementReferenceNumberSetter("MRN123");

		var testMessage = factory.New<NLEDIMessage>();
		testMessage.EM_ApplicationCode = ApplicationCodeList.Codes.NLCustoms;
		testMessage.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
		testMessage.EM_MessageSubType = NLIncomingMessageSubTypeList.Codes.CCRFIA;
		testMessage.EM_MessageNum = "1";
		testMessage.EM_ReceiveTransmit = NLEDIMessage.Direction.Receive;
		testMessage.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		testMessage.EM_LinkedObject = entryHeader;
		testMessage.EM_MessageText = messageText;

		factory.Save();

		return testMessage;
	}
}
