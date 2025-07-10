using System;
using System.IO;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(BaseInboundMessageProcessor<>))]
public class BaseInboundMessageProcessorBaseOnlyTest : TestCaseWithFactory
{
	public void TestPreProcessMessageCore() => CombineAssertions(() =>
	{
		CusEntryHeader entryHeader;
		using (DisposableEnvironment.ForBranch(UserBranchPK.ToGuid()))
		{
			entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			Factory.Save();
		}
		AssertNotEquals("Pre-condition: Current branch", UserBranchPK, GlbBranch.CurrentBranch.PK);

		var messageProcessor = new InboundMessageProcessorForTesting();

		var message = CreateEdiMessage(entryHeader.TablePrefix, entryHeader.PK);
		messageProcessor.PreProcessMessageExposed(message);
		AssertEquals($"{entryHeader.TableName}.EM_LinkTable", entryHeader.TableName, message.EM_LinkTable);
		AssertEquals($"{entryHeader.TableName}.EM_LinkUniqueID", entryHeader.PK, message.EM_LinkUniqueID);
		AssertEquals($"{entryHeader.TableName}.EM_GB", UserBranchPK, message.EM_GB);

		message = CreateEdiMessage(ZString.Empty, ZGuid.Empty);
		messageProcessor.PreProcessMessageExposed(message);
		AssertEquals("Not linked: EM_LinkTable", ZString.Empty, message.EM_LinkTable);
		AssertEquals("Not linked: EM_LinkTable", ZGuid.Empty, message.EM_LinkUniqueID);

		var ediMessage = CreateEdiMessage(entryHeader.TablePrefix, new ZGuid(Guid.NewGuid().ToString()));

		EDIMessage CreateEdiMessage(string tablePrefix, ZGuid pk)
		{
			var message = Factory.New<CHEDIMessage>();
			message.EM_EI = MessageProcessorTestHelper.CreateEDIInterchange(Factory, new ZGuid(), EDIInterchange.ApplicationCodes.CHCustomsEdec, ReceiveTransmitList.Codes.Receive, null).PK;
			var writer = new StringWriter();
			var testMessage = new MessageObjectForTesting { TablePrefix = tablePrefix, PK = pk.IsEmpty ? Guid.Empty : pk.ToGuid() };
			new XmlSerializer(typeof(MessageObjectForTesting)).Serialize(writer, testMessage);
			message.EM_MessageText = writer.ToString();
			return message;
		}
	});

	ZGuid UserBranchPK => userBranchPK ??= CreateUserBranch().PK;
	ZGuid? userBranchPK;

	GlbBranch CreateUserBranch()
	{
		var branch = GlbCompany.CurrentCompany.Branches.AddNew();
		branch.GB_Code = "ZZZ";
		branch.GB_IsActive = ZBool.True;
		branch.Factory.Save();
		return branch;
	}

	class InboundMessageProcessorForTesting : BaseInboundMessageProcessor<MessageObjectForTesting>
	{
		public InboundMessageProcessorForTesting() : base(new LoggingInformationForTesting())
		{
		}

		internal void PreProcessMessageExposed(EDIMessage ediMessage) => PreProcessMessage(ediMessage);

		protected override void ProcessResponseMessage(CHEDIMessage message, MessageObjectForTesting customsResponse)
		{
		}

		protected override MessageObjectForTesting DeserializeResponse(CHEDIMessage message)
		{
			return (MessageObjectForTesting)new XmlSerializer(typeof(MessageObjectForTesting)).Deserialize(message.GetEM_MessageTextReader());
		}

		protected override BusinessObject FindLinkedObject(EDIMessage message, MessageObjectForTesting testMessage)
		{
			return message.Factory.Load(testMessage.TablePrefix, testMessage.PK);
		}

		protected override string MessageFriendlyNameCore { get; }
	}

	public class MessageObjectForTesting
	{
		public string TablePrefix { get; set; }
		public Guid PK { get; set; }
	}
}
