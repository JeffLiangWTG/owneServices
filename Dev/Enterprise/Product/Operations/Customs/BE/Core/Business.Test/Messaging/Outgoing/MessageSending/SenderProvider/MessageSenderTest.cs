using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Customs.BE.Business.Testing;

public abstract class MessageSenderTest<TMessageSender, TProvider> : TestCaseWithFactory
	where TMessageSender : MessageSender
	where TProvider : class, IMessageHeader
{
	public void TestSendMessage()
	{
		SetUpMockProviderData(mockProvider);
		messageSender.Send();

		AssertEquals("Test-message is not checked in SendingObject (live)", false, messageSender.IsTestMessage);
		AssertMessage((BEMessage)((IAllowPermitProcessing)messageSender.MessageObject).Messages.First());
	}

	public void TestSendMessage_TestDeclaration()
	{
		SetUp_TestDeclaration();
		SetUpMockProviderData(mockProvider);
		messageSender.Send();

		AssertEquals("Test-message is checked in SendingObject (test)", true, messageSender.IsTestMessage);
		AssertMessage((BEMessage)((IAllowPermitProcessing)messageSender.MessageObject).Messages.First());
	}

	protected virtual void AssertMessage(BEMessage message)
	{
		AssertMessageStatusAndEdiMessageDetails(message);
		AssertMessageContent(message);
	}

	void AssertMessageStatusAndEdiMessageDetails(BEMessage message)
	{
		CombineAssertions(() =>
		{
			AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.BECustoms, message.EM_ApplicationCode);
			AssertNullOrEmpty("EM_MessageOwner", message.EM_MessageOwner);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_MessageSubType", messageSender.MessageSubType, message.EM_MessageSubType);
			AssertEquals("EM_MessageType", MessageType, message.EM_MessageType);
			AssertNullOrEmpty("EM_ApplicationReference", message.EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals("EM_GB", EnvProxy.Instance.CurrentBranch.PK, message.EM_GB);
			AssertEquals("EM_GE", Env.CurrentDepartment.PK, message.EM_GE);
			AssertNotNullOrEmpty("EM_MessageText", message.EM_MessageText);
			Assert("EM_IsActive", message.EM_IsActive);
			AssertEquals("EM_LinkTable", ParentTableName, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", LinkedObjectId, message.EM_LinkUniqueID);
			AssertEquals("EM_IsTestMessage", messageSender.IsTestMessage, message.EM_IsTestMessage);
		});
	}

	protected virtual IEnumerable<PropertyInfo> IgnorePropertiesFromTest => Enumerable.Empty<PropertyInfo>();

	void AssertMessageContent(BEMessage message)
	{
		var dataProvider = (TProvider)messageSender.DataProvider;
		CombineAssertions($"Please make this test worth it by providing the basic properties for message {dataProvider.MessageType} in dataprovider {typeof(TProvider).Name}", () =>
		{
			var properties = typeof(TProvider).GetProperties().Except(IgnorePropertiesFromTest).ToArray();
			foreach (PropertyInfo property in properties)
			{
				AssertNotNullOrEmpty($"Property: '{property.Name}'", property.GetValue(dataProvider)?.ToString());
			}
		});
		var expectedXmlFile = MessageDirectory + $"{dataProvider.MessageType}.xml";
		using (var inStream = XmlContentAssembly.GetManifestResourceStream(expectedXmlFile))
		{
			ZString expectedMessage = new StreamReader(inStream).ReadToEnd();

			CombineAssertions($"Please provide a test xml with the expected content: {expectedXmlFile}", () =>
			{
				AssertNotNullOrEmpty("File not found", expectedMessage);
				if (!string.IsNullOrEmpty(expectedMessage))
				{
					AssertXMLEquals(expectedMessage, message.EM_MessageText);
				}
			});
		}
	}

	protected abstract ZString EntryType { get; }

	protected abstract ZString MessageType { get; }

	protected abstract ZString ParentTableName { get; }

	protected abstract void SetUpMockProviderData(Mock<TProvider> mockProvider);

	protected virtual void SetUp_TestDeclaration() { }

	protected abstract string MessageDirectory { get; }

	protected abstract Assembly XmlContentAssembly { get; }

	protected virtual ZGuid LinkedObjectId => messageSender.MessageObject.PK;

	protected TMessageSender messageSender;
	protected Mock<TProvider> mockProvider;
}
