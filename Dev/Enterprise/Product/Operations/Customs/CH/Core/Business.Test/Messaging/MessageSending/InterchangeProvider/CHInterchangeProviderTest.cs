using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class CHInterchangeProviderTest : InterchangeProviderTestCase
{
	protected abstract string ApplicationCode { get; }

	public override void TestMessagesPopulateNewInterchange() => CombineAssertions(() =>
	{
		GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, CompanyBID);

		var linkedObject = Factory.NewWithValidTestData<JobDeclaration>();

		foreach (var (messageType, messageSubType, expectedDestinationCode) in GetDataForTest())
		{
			AssertMessagesPopulateNewInterchange(messageType, messageSubType, expectedDestinationCode);
		}

		void AssertMessagesPopulateNewInterchange(string messageType, string messageSubType, string expectedDestinationCode)
		{
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(CreateAndPopulateMessage(linkedObject, ApplicationCode, messageType, messageSubType));

			var provider = GetInterchangeProvider(messages);
			provider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();

			var interchange = provider.Interchanges.Single();
			var message = interchange.ContainedMessages.Single() as EDIMessage;

			var interchangeNo = $"ApplicationCode={message.EM_ApplicationCode} MessageType={message.EM_MessageType}";
			AssertEquals($"{interchangeNo} message is queued", CHEDIMessage.Status.Sent, message.EM_Status);
			AssertNotNull($"{interchangeNo} linked to message", interchange);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_ApplicationCode)}", ApplicationCode, interchange.EI_ApplicationCode);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_InterchangeType)}", message.EM_MessageType, interchange.EI_InterchangeType);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_ReceiveTransmit)}", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_From)}", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_To)}", expectedDestinationCode, interchange.EI_To);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_TransportType)}", EDIInterchange.TransportType.xT, interchange.EI_TransportType);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_Status)}", EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_Status)}", EDIInterchangePriorityList.Codes.High, interchange.EI_Priority);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_IsActive)}", true, interchange.EI_IsActive);
			AssertHeaderText($"{interchangeNo} {nameof(interchange.EI_HeaderText)}", interchange.EI_HeaderText);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_BodyText)}", GetExpectedEI_BodyText(message), interchange.EI_BodyText);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_FooterText)}", ZString.Empty, interchange.EI_FooterText);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_GP)}", message.EM_GP, interchange.EI_GP);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_Priority)}", EDIInterchangePriorityList.Codes.High, interchange.EI_Priority);
			Assert($"{interchangeNo} {nameof(interchange.EI_InterchangeNum)}", !interchange.EI_InterchangeNum.IsEmpty);
			Assert($"{interchangeNo} {nameof(interchange.EI_SessionGUID)}", !interchange.EI_SessionGUID.IsEmpty);
			AssertEquals($"{interchangeNo} {nameof(interchange.EI_GB)}", message.EM_GB, interchange.EI_GB);
			void AssertHeaderText(string assertionMessage, ZString actualHeaderText)
			{
				var expectedHeaderJson = GetExpectedEI_HeaderAttributes(message);
				if (expectedHeaderJson == null)
				{
					AssertEquals(assertionMessage, ZString.Empty, actualHeaderText);
				}
				else
				{
					JsonTestHelper.AssertEquals(assertionMessage, expectedHeaderJson, actualHeaderText);
				}
			}
		}
	});

	protected abstract List<(string messageType, string messageSubType, string expectedDestinationCode)> GetDataForTest();

	protected virtual ZString GetExpectedEI_BodyText(EDIMessage message) => message.EM_MessageText;

	protected virtual Dictionary<string, string> GetExpectedEI_HeaderAttributes(EDIMessage message) => null;

	public void TestNoInterchangeOnFailedMessages()
	{
		var linkedObject = Factory.NewWithValidTestData<JobDeclaration>();
		var message = CreateAndPopulateMessage(linkedObject, "XXX", string.Empty, string.Empty);
		message.EM_MessageText = string.Empty;
		var messages = new NonDependentEDIMessageCollection(Factory);
		messages.AddRange(new[] { message });
		var provider = GetInterchangeProvider(messages);

		AssertNoExceptionThrown(() =>
		{
			provider.PackCollatedMessagesIntoInterchanges();
			Factory.Save();
		});

		AssertEquals("No Interchanges created", 0, provider.Interchanges.Length);
	}

	protected CHEDIMessage CreateAndPopulateMessage(BusinessObject linkedObject, string applicationCode, string messageType, string messageSubType)
	{
		var message = Factory.New<CHEDIMessage>();
		message.EM_ApplicationCode = applicationCode;
		message.EM_GB = UserBranchPK;
		message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
		message.EM_MessageOwner = ZString.Empty;
		message.EM_MessageText = $"<Sample Message Text {Guid.NewGuid()}>";
		message.EM_MessageType = messageType;
		message.EM_ReceiveTransmit = CHEDIMessage.Direction.Transmit;
		message.EM_Status = CHEDIMessage.Status.Sent;
		message.EM_GP = Password.PK;
		message.EM_LinkedObject = linkedObject;
		message.EM_ApplicationReference = "APREF000001";
		if (!string.IsNullOrEmpty(messageSubType))
		{
			message.EM_MessageSubType = messageSubType;
		}
		return message;
	}

	protected const string CompanyBID = "BID123456";

	GlbExternalPassword Password => password ?? (password = Factory.NewWithValidTestData<GlbExternalPassword>());
	GlbExternalPassword password;

	ZGuid UserBranchPK => userBranchPK ??= CreateUserBranch().PK;
	ZGuid? userBranchPK;

	GlbBranch CreateUserBranch()
	{
		var branch = GlbCompany.CurrentCompany.Branches.AddNew();
		branch.GB_Code = "ZZZ";
		branch.GB_IsActive = true;
		branch.Factory.Save();
		return branch;
	}
}

public class CHInterchangeProviderBaseOnlyTest : TestCaseWithFactory
{
	public void TestNew()
	{
		var message = Factory.New<CHEDIMessage>();
		var messages = new NonDependentEDIMessageCollection(Factory);
		messages.Add(message);

		message.EM_ApplicationCode = ApplicationCodeList.Codes.CHCustomsEdec;
		AssertType<CHCInterchangeProvider>(CHInterchangeProvider.New(messages));
		message.EM_ApplicationCode = ApplicationCodeList.Codes.CHCustomsPassar;
		AssertType<CHPInterchangeProvider>(CHInterchangeProvider.New(messages));
		message.EM_ApplicationCode = ApplicationCodeList.Codes.CHCustomsCharteraOutput;
		AssertType<CHOInterchangeProvider>(CHInterchangeProvider.New(messages));
		message.EM_ApplicationCode = "XXX";
		AssertNull(CHInterchangeProvider.New(messages));
	}
}
