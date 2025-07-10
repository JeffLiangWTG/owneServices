using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DeclarationMessageManager))]
public abstract class DeclarationMessageManagerTest<TMessageSendingObjectParent, TMessageSendingObject> : TestCaseWithFactory
														where TMessageSendingObject : DeclarationMessageSendingObject
														where TMessageSendingObjectParent : DeclarationMessageSendingObjectParent<TMessageSendingObject>
{
	protected abstract string DeclarationType { get; }

	protected abstract string MessageType { get; }

	protected abstract string ExpectedApplicationCode { get; }

	protected abstract string ExpectedMessageType { get; }

	protected abstract string ExpectedMessageSubType { get; }

	protected abstract string ExpectedEntryHeaderStatus { get; }

	protected virtual string ExpectedEntryHeaderPhaseStatus => string.Empty;

	protected abstract Event ExpectedCustomsCommencedEvent { get; }

	protected virtual Event ExpectedDeclarationSentEvent => Events.DeclarationSentToCustoms;

	protected virtual ZString ExpectedDeclarationSentEventReference => ZString.Empty;

	protected abstract TMessageSendingObjectParent CreateMessageSendingObjectParent(JobDeclaration declaration);

	protected abstract DeclarationMessageManager GetSpecificMessageManager(DeclarationMessageSendingObject sender);

	protected abstract GlbExternalPassword CreateCurrentCompanyCredential();

	protected DeclarationMessageSendingObject Sender => sender ??= GetMessageSender();
	DeclarationMessageSendingObject sender;

	protected DeclarationMessageManager Manager => manager ??= GetSpecificMessageManager(Sender);
	DeclarationMessageManager manager;

	DeclarationMessageSendingObject GetMessageSender(bool withEntryNum = true)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = DeclarationType;
		declaration.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_Description = "DESCRIPTION";
		var cusHeader = declaration.CustomsEntryHeaders.AddNew();
		cusHeader.CH_CEI_Instruction = instruction.PK;
		if (withEntryNum)
		{
			cusHeader.EntryNumber = "NO1";
		}
		var sendingObjectParent = CreateMessageSendingObjectParent(declaration);
		return (DeclarationMessageSendingObject)sendingObjectParent.SendingObjectsCollection.First();
	}

	public void TestMessageFriendlyName() => AssertEquals("MessageFriendlyName", Sender.FriendlyNameForMessageManager, Manager.MessageFriendlyName);

	public void TestBusinessObject() => AssertEquals(Sender, Manager.BusinessObject);

	public void TestCanSendOriginal() => AssertEquals(true, Manager.CanSendOriginal);

	public void TestCanSendWithdrawal() => AssertEquals(false, Manager.CanSendWithdrawal);

	public void TestHasActiveMessages() => CombineAssertions(() =>
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
		{
			var importDeclarationMessageManager = GetSpecificMessageManager(GetMessageSender(withEntryNum: false));
			AssertEquals("HasActiveMessages is false", false, importDeclarationMessageManager.HasActiveMessages);
			importDeclarationMessageManager = GetSpecificMessageManager(Sender);
			AssertEquals("HasActiveMessages is true", true, importDeclarationMessageManager.HasActiveMessages);
		}
	});

	public void TestGenerateMessages() => CombineAssertions(() =>
	{
		Sender.ShouldSend = true;

		var testHelper = new SendingObjectsTestHelper();
		var credential = CreateCurrentCompanyCredential();

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
		{
			AssertEquals("HasActiveMessages is true", true, Manager.HasActiveMessages);

			var entryHeader = (CusEntryHeader)Sender.Header;
			Sender.MessageType = MessageType;
			Factory.Save();

			var messages = Manager.GenerateMessages();

			Factory.Save();

			testHelper.AssertEDIMessages(messages, ExpectedMessageType, ExpectedMessageSubType, entryHeader.PK, credential.PK, ExpectedApplicationCode);

			testHelper.AssertCusEntryHeader(entryHeader, ExpectedEntryHeaderStatus, ExpectedEntryHeaderPhaseStatus);

			if (ExpectedCustomsCommencedEvent != null)
			{
				EventsTestHelper.AssertEventAdded(entryHeader.Declaration, ExpectedCustomsCommencedEvent);
			}

			if (ExpectedDeclarationSentEvent != null)
			{
				if (ExpectedDeclarationSentEventReference.IsEmpty)
				{
					EventsTestHelper.AssertEventAdded(entryHeader, ExpectedDeclarationSentEvent);
				}
				else
				{
					EventsTestHelper.AssertEventAdded(entryHeader, ExpectedDeclarationSentEvent, ExpectedDeclarationSentEventReference);
				}
			}
			else
			{
				EventsTestHelper.AssertEventNotAdded(entryHeader.Declaration, Events.DeclarationSentToCustoms);
				EventsTestHelper.AssertEventNotAdded(entryHeader.Declaration, Events.DeclarationCancellationSent);
			}

			if (ExpectedEntryHeaderStatus != null)
			{
				EventsTestHelper.AssertEventAdded(entryHeader, Events.MessageStatusChange, $"|NEW={entryHeader.CH_Status}");
			}
		}
	});

	public void TestRollbackOnSaveFailed() => CombineAssertions(() =>
	{
		Sender.ShouldSend = true;

		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
		{
			AssertEquals("HasActiveMessages is true", true, Manager.HasActiveMessages);

			var entryHeader = (CusEntryHeader)Sender.Header;
			var declaration = entryHeader.Declaration;

			var declarationLogCountBefore = declaration.Logs.GetAllLogs().Count;
			var entryHeaderLogCountBefore = entryHeader.Logs.GetAllLogs().Count;

			entryHeader.Logs.AddNew(Events.DeclarationSentToCustoms);
			declaration.Logs.AddNew(Events.DeclarationSentToCustoms);

			Manager.RollbackOnSaveFailed();
			AssertEquals("# of created log events on declaration", 0, declaration.Logs.GetAllLogs().Count - declarationLogCountBefore);
			AssertEquals("# of created log events on entry header", 0, entryHeader.Logs.GetAllLogs().Count - entryHeaderLogCountBefore);
		}
	});

	[TestDate]
	public void TestCH_EntrySubmittedDateChangeAfterSendingIfEmpty() => CombineAssertions(() =>
	{
		Sender.ShouldSend = true;

		var testHelper = new SendingObjectsTestHelper();
		var credential = CreateCurrentCompanyCredential();

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
		{
			var entryHeader = (CusEntryHeader)Sender.Header;
			Sender.MessageType = MessageType;
			Factory.Save();

			AssertEquals("Pre-condition: CH_EntrySubmittedDate is initially empty", true, entryHeader.CH_EntrySubmittedDate.IsEmpty);

			var messages = Manager.GenerateMessages();

			Factory.Save();

			AssertEquals("CH_EntrySubmittedDate has been set", false, entryHeader.CH_EntrySubmittedDate.IsEmpty);
			var date = entryHeader.CH_EntrySubmittedDate;

			TestDateAttribute.AddMinutes(1);
			messages = Manager.GenerateMessages();

			Factory.Save();

			AssertEquals("CH_EntrySubmittedDate has not been changed", date, entryHeader.CH_EntrySubmittedDate);
		}
	});
}
