using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using GlbCompanyWrapper = Enterprise.Customs.CH.Business.GlbCompanyWrapper;
using IMessageManager = Enterprise.Customs.CH.Business.IMessageManager;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

public abstract class BasePassarMessageManagerTest<TMessageManager, TMessageSendingObject> : TestCaseWithFactory
	where TMessageManager : BasePassarMessageManager<TMessageSendingObject>, IMessageManager
	where TMessageSendingObject : BusinessObject, IMessageSendingObjectParent, IMessageSendingObject, INctsMessageSendingObject
{
	public void TestConstructorNullArgument()
	{
		AssertExceptionThrown<ArgumentNullException>(() => CreateMessageManager(null));
	}

	public void TestMessageFriendlyName()
	{
		var manager = CreateMessageManager(NctsHeader);
		AssertEquals("message friendly name", ExpectedMessageFriendlyName, manager.MessageFriendlyName);
	}

	public void TestGenerateMessage() => CombineAssertions(() =>
	{
		var companyWrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.GetCurrentCompany(Factory));
		companyWrapper.TokenCredentialsEnabled = true;
		var tokenCredentials = companyWrapper.TokenCredentials;
		tokenCredentials.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
		tokenCredentials.GP_UserID = "USERNAME";
		tokenCredentials.CurrentDecryptedPassword = "PASSWORD";
		tokenCredentials.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		NctsHeader.CommonMovementHeader.BM_Phase = NCTS5DeparturePhaseList.Codes.Declaration;
		NctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Accepted;
		Factory.Save();

		var manager = CreateMessageManager(NctsHeader);
		var message = manager.GenerateMessages().First();
		Factory.Save();

		AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.CHCustomsPassar, message.EM_ApplicationCode);
		AssertEquals("EM_MessageType", ExpectedMessageType, message.EM_MessageType);
		AssertEquals("EM_MessageSubType", ExpectedMessageSubType, message.EM_MessageSubType);
		AssertEquals("EM_LinkedObject", ExpectedLinkedObject, message.EM_LinkedObject);
		AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
		AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
		AssertEquals("EM_ApplicationReference", (manager.SendingObject).MessageIdentification, message.EM_ApplicationReference);
		AssertEquals("EM_GP", tokenCredentials.PK, message.EM_GP);
		Assert("EM_MessageText", message.EM_MessageText.Contains(message.EM_ApplicationReference));

		AssertEquals("Messages has EDI message", 1, ExpectedEDIMessageCollection.Count);
		AssertEquals("CommonMovementHeader should have status: ", ExpectedMovementHeaderPhase, NctsHeader.CommonMovementHeader.BM_Phase);
		AssertEquals("NctsHeader should have status: ", NctsMessageStatusList.Codes.SentToCustoms, NctsHeader.EffectiveMessageStatus);

		EventsTestHelper.AssertEventAdded(NctsHeader, ExpectedEventType, expectedReference: ExpectedEventReference);

		AssertAfterGenerateMessage();
	});

	public void TestRollbackOnSaveFailed()
	{
		NctsHeader.CommonMovementHeader.BM_Phase = "XXX";
		NctsHeader.EffectiveMessageStatus = "XXX";
		Factory.Save();

		Factory.Saving += (f) => throw new ZSaveException(new ZDataExceptionForTesting("*** TEST ERROR ***"), f);
		var manager = CreateMessageManager(NctsHeader);
		manager.SendingObject.SendMessagesAndSave(_ => manager);

		CombineAssertions(() =>
		{
			AssertEquals("Messages deleted on saving failed", 0, NctsHeader.Messages.Count);
			AssertEquals("CommonMovementHeader status should not be updated on saving failed", "XXX", NctsHeader.CommonMovementHeader.BM_Phase);
			AssertEquals("NctsHeader status should not be updated on saving failed", "XXX", NctsHeader.EffectiveMessageStatus);
			AssertEquals("NctsHeader has no logs on saving failed", false, NctsHeader.Logs.LogsNotInDB.Any());
		});

		AssertRollbackOnSaveFailed();
	}

	protected abstract string MessageSendingObjectMessageType { get; }
	protected abstract ZString MovementType { get; }
	protected abstract TMessageManager CreateMessageManager(NctsHeader nctsHeader);
	protected virtual void AssertAfterGenerateMessage() { }
	protected virtual void AssertRollbackOnSaveFailed() { }

	string ExpectedMessageFriendlyName => new PassarMessageTypeList().GetDescriptionFromCode(MessageSendingObjectMessageType);
	protected virtual string ExpectedMessageType => MessageTypeCodeList.Codes.PassarNcts;
	protected abstract string ExpectedMovementHeaderPhase { get; }
	protected abstract string ExpectedMessageSubType { get; }
	protected abstract Event ExpectedEventType { get; }
	protected abstract string ExpectedEventReference { get; }
	protected virtual ZString InitialCustomsStatus => ZString.Empty;

	protected NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(MovementType);
		nctsHeader.CommonMovementHeader.BM_CustomsStatus = InitialCustomsStatus;
		return nctsHeader;
	}

	EDIMessageCollection ExpectedEDIMessageCollection => MovementType == NctsMovementType.Codes.Departure ? NctsHeader.MovementHeader.Messages : NctsHeader.Messages;

	BusinessObject ExpectedLinkedObject => MovementType == NctsMovementType.Codes.Departure ? NctsHeader.MovementHeader : NctsHeader;
}
