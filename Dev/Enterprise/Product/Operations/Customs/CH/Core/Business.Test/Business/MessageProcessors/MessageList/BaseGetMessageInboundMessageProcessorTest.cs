using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business.Testing;

[TestDate(2023, 5, 8, 0, 0, 0)]
public abstract class BaseGetMessageInboundMessageProcessorTest : TestCaseWithFactory
{
	public void TestFriendlyName()
	{
		AssertEquals(ExpectedFriendlyName, MessageProcessor.MessageFriendlyName);
	}

	public void TestApplicationCode()
	{
		AssertEquals(ApplicationCode, MessageProcessor.ApplicationCode);
	}

	public void TestMessageFilter() => CombineAssertions(() =>
	{
		var messageSubTypesCondition = MessageFilterSubTypes.Length == 1
			? $"EM_MessageSubType = '{MessageSubType}'"
			: $"(EM_MessageSubType in ('{string.Join("', '", MessageFilterSubTypes)}'))";
		AssertEquals("MessageFilter", $"EM_ApplicationCode = '{ApplicationCode}' and EM_MessageType = 'MSG' and {messageSubTypesCondition}", MessageProcessor.MessageFilter.LiteralTextADO);
	});

	protected void AssertProcessMessage(Func<string, BusinessObject> prepareOutgoingMessage, Func<string, string> getResponseMessage, Action<EDIMessage> assertMessageProcessed, string expectedMessageStatus = EDIMessage.Status.ProcessedOK, bool assertEDIMessageLinked = true)
	{
		var passarTestHelper = new CustomsMessageProcessorTestHelper(Factory);

		var messageId = Guid.NewGuid().ToString();
		var correlationIdentifier = Guid.NewGuid().ToString();
		BusinessObject businessObject;
		using (DisposableEnvironment.ForBranch(Branch.PK.ToGuid()))
		{
			businessObject = prepareOutgoingMessage?.Invoke(correlationIdentifier);
		}
		var (company, ediMessage, transaction) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, eventType: EventType, messageSubType: MessageSubType, responseMessage: getResponseMessage(correlationIdentifier), messageId: messageId, cptType: TransactionType, companyCode: GlbCompany.CurrentCompany.GC_Code);
		Factory.Save();

		AssertNotEquals("Pre-condition: Initial branch", Branch.PK, ediMessage.EM_GB);

		TestDateAttribute.AddMinutes(1);
		MessageProcessor.ProcessMessage(ediMessage);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", expectedMessageStatus ?? EDIMessage.Status.ProcessedOK, ediMessage.EM_Status);

			if (messageProcessor.UpdateApplicationReference)
			{
				AssertEquals("EM_ApplicationReference", messageId, ediMessage.EM_ApplicationReference);
			}

			if (assertEDIMessageLinked)
			{
				businessObject = MessageProcessor.LinkMessageToCompany ? GlbCompany.CurrentCompany : businessObject;
				if (businessObject != null)
				{
					AssertEquals("EM_LinkTable", businessObject.TableName, ediMessage.EM_LinkTable);
					AssertEquals("EM_LinkUniqueID", businessObject.PK, ediMessage.EM_LinkUniqueID);
					if (!MessageProcessor.LinkMessageToCompany)
					{
						AssertEquals("EM_GB", Branch.PK, ediMessage.EM_GB);
						AssertEquals("EI_GB", Branch.PK, ediMessage.Interchange?.EI_GB);
					}
				}
			}

			AssertTransactionUpdated();

			assertMessageProcessed?.Invoke(ediMessage);
		});

		void AssertTransactionUpdated()
		{
			AssertEquals("CPT_Status", StatusCodes.Closed, transaction.CPT_Status);
			AssertEquals("CPT_StatusTimeUtc", ZDateTime.UtcNow, transaction.CPT_StatusTimeUtc);
			AssertEquals("CPT_Type", TransactionType, transaction.CPT_Type);
			AssertEquals("CPT_TransactionID", messageId, transaction.CPT_TransactionID);
			AssertEquals("ParentObject", company.PK, transaction.ParentObject.PK);
		}
	}

	public virtual void TestUnableLinkMessageToParent()
	{
		if (!MessageProcessor.LinkMessageToCompany)
		{
			AssertProcessMessage(null, _ => GetResponseMessage(), (ediMessage) =>
			{
				AssertNull("EM_LinkedObject", ediMessage.EM_LinkedObject);
				AssertEquals($"Warning - Unable to link EDI Message '{ediMessage.EM_MessageNum}' to an existing business object.", Logger.Logs.Last().ToString());
			}, Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Discarded);
		}
		else
		{
			Assert(true);
		}
	}

	public void TestErrorLoggedIfNoMessageId() => CombineAssertions(() =>
	{
		var incomingMessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(eventType: EventType, responseMessage: GetResponseMessage());
		var (_, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCode, messageType: MessageTypeCodeList.Codes.MSG, messageSubType: MessageSubType, incomingMessageText, sentApplicationReference: string.Empty);

		Factory.Save();

		MessageProcessor.ProcessMessage(ediMessage);

		AssertEquals("Log message", true, Logger.ContainsLogEntry("Cannot get Message Id (EM_ApplicationReference) from outgoing Message:"));
	});

	public void TestErrorLoggedIfNoTransaction() => CombineAssertions(() =>
	{
		var messageId = Guid.NewGuid().ToString();
		var incomingMessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(eventType: EventType, responseMessage: GetResponseMessage());
		var (_, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCode, messageType: MessageTypeCodeList.Codes.MSG, messageSubType: MessageSubType, incomingMessageText, sentApplicationReference: messageId);

		Factory.Save();

		MessageProcessor.ProcessMessage(ediMessage);

		AssertEquals("Log message", true, Logger.ContainsLogEntry($"Cannot find Transaction by Message Id {messageId}"));
	});

	public void TestPostProcessOnException() => CombineAssertions(() =>
	{
		var initialStatus = StatusCodes.New;

		var expectedStatus = MessageProcessor.MustProcessInOrder ? StatusCodes.Skip : initialStatus;
		var expectedStatusReason = MessageProcessor.MustProcessInOrder ? (ZString)"Message processing failed" : ZString.Empty;

		var passarTestHelper = new CustomsMessageProcessorTestHelper(Factory);
		var (_, incommingEdiMessage, transaction) = passarTestHelper.CreateGetMessageResponseObjects(ApplicationCode, messageSubType: MessageSubTypeCodeList.Codes.PassarTechnicalError, messageId: "123", cptStatus: initialStatus, cptType: TransactionType);

		Factory.Save();

		MessageProcessor.PostProcessOnException(incommingEdiMessage);

		AssertEquals("After PostProcessOnExceptionCore CPT_Status", expectedStatus, transaction.CPT_Status);
		AssertEquals("PostProcessOnExceptionCore CPT_StatusReason", expectedStatusReason, transaction.CPT_StatusReason);
	});

	protected abstract string ExpectedFriendlyName { get; }

	protected abstract string ApplicationCode { get; }

	protected virtual string EventType => AutoEvents.InterchangeAcknowledgedCode;

	protected abstract string MessageSubType { get; }

	protected virtual string[] MessageFilterSubTypes => new[] { MessageSubType };

	protected virtual string TransactionType => TransactionTypes.MessageId;

	protected abstract string GetResponseMessage();

	protected BaseGetMessageInboundMessageProcessor MessageProcessor => messageProcessor ?? (messageProcessor = CreateMessageProcessor());
	BaseGetMessageInboundMessageProcessor messageProcessor;

	protected abstract BaseGetMessageInboundMessageProcessor CreateMessageProcessor();

	protected LoggingInformationForTesting Logger => logger ?? (logger = new LoggingInformationForTesting());
	LoggingInformationForTesting logger;

	protected GlbBranch Branch => branch ??= CreateUserBranch();
	GlbBranch branch;

	GlbBranch CreateUserBranch()
	{
		var branch = GlbCompany.CurrentCompany.Branches.AddNew();
		branch.GB_Code = "ZZZ";
		branch.GB_IsActive = ZBool.True;
		branch.Factory.Save();
		return branch;
	}
}
