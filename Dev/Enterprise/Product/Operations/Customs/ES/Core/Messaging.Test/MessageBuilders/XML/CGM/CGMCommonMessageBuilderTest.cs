using System;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

public abstract class CGMCommonMessageBuilderTest<TMessageBuilder, TProvider, T> : XMLMessageBuilderTest<TMessageBuilder, TProvider, T>
	where TProvider : class, ICGMCommonDataProvider
	where TMessageBuilder : CGMCommonMessageBuilder<TProvider, T>
{
	public override sealed void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When provider is null", () => CreateMessageBuilderWithNullProvider());
	}

	[TestDate(2020, 1, 9, 15, 13, 23, 456)]
	public override sealed void TestCreateEDIMessage()
	{
		var messageBuilder = CreateMessageBuilder();

		CombineAssertions(() =>
		{
			AssertEquals("messageBuilder.MessageType", ExpectedMessageType, messageBuilder.MessageType);
			AssertEquals("messageBuilder.MessageSubType", ExpectedMessageSubType, messageBuilder.MessageSubType);
			AssertEquals("messageBuilder.Provider", mockProvider.Object, messageBuilder.Provider);
			AssertUnsignedMessageText(messageBuilder.UnsignedMessageText);
			AssertSignedMessageText(messageBuilder.GetSignedMessageText());
		});
	}

	protected override sealed ZString GetSignedMessageTestFileContent() => GetTestFile();
	protected override sealed ZString GetUnsignedMessageTestFileContent() => GetTestFile();
	protected abstract ZString GetTestFile();

	#region Structures SetUp

	protected ICGMPartyProviderWithAddressAndContactPerson SetUpCGMPartyProviderWithAddressAndContactPerson(ZString id, ZString name, ZString address, ZString city,
		ZString postCode, ZString country, ZString contactPersonName, ZString email, ZString phoneNumber)
	{
		var partyProvider = new Mock<ICGMPartyProviderWithAddressAndContactPerson>();
		partyProvider.Setup(m => m.Id).Returns(id);
		partyProvider.Setup(m => m.Name).Returns(name);
		partyProvider.Setup(m => m.Address).Returns(BuilderHelperTest.SetUpAddress(address, city, postCode, country));
		partyProvider.Setup(m => m.ContactPerson).Returns(BuilderHelperTest.SetUpContactInformation(contactPersonName, email, phoneNumber));
		return partyProvider.Object;
	}

	#endregion
}
