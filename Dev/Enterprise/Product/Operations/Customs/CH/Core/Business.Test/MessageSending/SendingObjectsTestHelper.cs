using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business.Testing;

public class SendingObjectsTestHelper : TestCaseWithFactory
{
	public void AssertEDIMessages(EDIMessage[] messages, ZString messageType, ZString messageSubType, ZGuid entryPK, ZGuid passwordPK, string applicationCode)
	{
		AssertEquals("Messages count", 1, messages.Length);

		var message = messages[0];
		AssertEquals("EM_ApplicationCode", applicationCode, message.EM_ApplicationCode);
		AssertEquals("EM_MessageType", messageType, message.EM_MessageType);
		AssertEquals("EM_MessageSubType", messageSubType, message.EM_MessageSubType);
		AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
		AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
		Assert("EM_MessageNum", !message.EM_MessageNum.IsEmpty);
		Assert("EM_MessageText", !message.EM_MessageText.IsEmpty);
		AssertEquals("EM_LinkTable", CusEntryHeaderSchema.Constants.TableName, message.EM_LinkTable);
		AssertEquals("EM_LinkUniqueID", entryPK, message.EM_LinkUniqueID);
		AssertEquals("EM_GP", passwordPK, message.EM_GP);
	}

	public void AssertCusEntryHeader(CusEntryHeader header, string expectedStatus, string expectedPhaseStatus)
	{
		AssertNotNull(header);
		if (expectedStatus != null)
		{
			AssertEquals("CH_Status", expectedStatus, header.CH_Status);
		}
		if (expectedPhaseStatus != null)
		{
			AssertEquals("CH_PhaseStatus", expectedPhaseStatus, header.CH_PhaseStatus);
		}
	}

	public void AssertEvent(StmALog logEntry, BusinessObject linkedObject, string expectedReference = null)
	{
		AssertNotNull(logEntry);
		AssertEquals("SL_Table", linkedObject.TableName, logEntry.SL_Table);
		AssertEquals("SL_Parent", linkedObject.PK, logEntry.SL_Parent);
		if (!string.IsNullOrEmpty(expectedReference))
		{
			AssertEquals("SL_Reference", expectedReference, logEntry.SL_Reference);
		}
	}
}
