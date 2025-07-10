using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

public abstract class BasePassarNctsMSGMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;

	protected virtual string ExpectedMovementType => TestedMovementType;

	protected abstract string TestedMovementType { get; }

	protected virtual bool FindLinkedObjectByMRNAnyVersion => false;

	protected virtual ZString GetResponseMessageWithMRN(string mrn, string mrnVersion) => ZString.Empty;

	public void TestMovementType()
	{
		if (ExpectedMovementType == null)
		{
			Assert(true);
		}
		else
		{
			var messageProcessor = CreateMessageProcessor();
			var movementTypeProperty = messageProcessor.GetType().GetProperty("MovementType", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertEquals(ExpectedMovementType, movementTypeProperty.GetValue(messageProcessor));
		}
	}

	public void TestFindLinkedObjectByMRN()
	{
		const string MRN1 = "22CHVL2525YYN7IZJ7";
		const string MRNVersion1 = "1";
		const string MRN2 = "22CHVL2525YYN7IZJ8";
		const string MRNVersion2 = "2";

		var responseWithMRN = GetResponseMessageWithMRN(mrn: MRN1, mrnVersion: MRNVersion1);
		if (!responseWithMRN.IsEmpty)
		{
			var sessionGuid = ZGuid.NewZGuid();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(TestedMovementType);
			nctsHeader.MovementReferenceNumberSetter($"{MRN1}");
			Factory.Save();

			CHEDIMessage receivedMessage;

			AssertProcessMessage(
				(correlationIdentifier) => receivedMessage = Helper.CreateReceivedMessage(sessionGuid),
				(correlationIdentifier) => responseWithMRN,
				(ediMessage) =>
				{
					AssertEquals("Linked Object", GetExpectedLinkedObject(nctsHeader), ediMessage.EM_LinkedObject);
				}, assertEDIMessageLinked: false, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);

			nctsHeader.MovementReferenceNumberSetter($"{MRN1}.{MRNVersion1}");
			Factory.Save();

			AssertProcessMessage(
				(correlationIdentifier) => receivedMessage = Helper.CreateReceivedMessage(sessionGuid),
				(correlationIdentifier) => responseWithMRN,
				(ediMessage) =>
				{
					AssertEquals("Linked Object", GetExpectedLinkedObject(nctsHeader), ediMessage.EM_LinkedObject);
				}, assertEDIMessageLinked: false, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);

			AssertProcessMessage(
				(correlationIdentifier) => receivedMessage = Helper.CreateReceivedMessage(sessionGuid),
				(correlationIdentifier) => GetResponseMessageWithMRN(mrn: MRN1, mrnVersion: MRNVersion2),
				(ediMessage) =>
				{
					AssertEquals("Linked Object", GetExpectedLinkedObject(nctsHeader), ediMessage.EM_LinkedObject);
				}, assertEDIMessageLinked: false, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);

			AssertProcessMessage(
				(correlationIdentifier) => receivedMessage = Helper.CreateReceivedMessage(sessionGuid),
				(correlationIdentifier) => GetResponseMessageWithMRN(mrn: MRN2, mrnVersion: MRNVersion2),
				(ediMessage) =>
				{
					AssertNull("Linked Object", ediMessage.EM_LinkedObject);
				}, assertEDIMessageLinked: false, expectedMessageStatus: EDIMessageStatusList.Codes.Discarded);
		}
		else
		{
			Assert(true);
		}
	}

	protected NctsMessageProcessorTestHelper Helper => helper ?? (helper = new NctsMessageProcessorTestHelper(Factory));
	NctsMessageProcessorTestHelper helper;

	BusinessObject GetExpectedLinkedObject(NctsHeader nctsHeader) => TestedMovementType == NctsMovementType.Codes.Departure ? nctsHeader.MovementHeader : nctsHeader;
}
