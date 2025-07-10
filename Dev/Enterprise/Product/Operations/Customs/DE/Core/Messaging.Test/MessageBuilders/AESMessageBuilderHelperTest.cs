using System;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Messaging.Testing
{
	sealed class AESMessageBuilderHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCreateCommonEoriIdentification_EORI()
		{
			var partyID = new Mock<IPartyID>();
			partyID.Setup(m => m.EoriNumber).Returns("GREOR1");
			partyID.Setup(m => m.EoriBranchSuffix).Returns("EBS1");

			var result = AESMessageBuilderHelper.CreateCommonEoriIdentification<DummyEoriIdentification>(partyID.Object);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(result, Is.TypeOf<DummyEoriIdentification>(), "Type");
				NUnit.Framework.Assert.That(result.identificationNumber, Is.EqualTo("GREOR1"), "identificationNumber");
				NUnit.Framework.Assert.That(result.subsidiaryNumber, Is.EqualTo("EBS1"), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestCreateCommonEoriIdentification_TCU()
		{
			var partyID = new Mock<IPartyID>();
			partyID.Setup(m => m.EoriNumber).Returns(string.Empty);
			partyID.Setup(m => m.TCUNumber).Returns("GRTCU1");
			partyID.Setup(m => m.EoriBranchSuffix).Returns("EBS1");

			var result = AESMessageBuilderHelper.CreateCommonEoriIdentification<DummyEoriIdentification>(partyID.Object);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(result, Is.TypeOf<DummyEoriIdentification>(), "Type");
				NUnit.Framework.Assert.That(result.identificationNumber, Is.EqualTo("GRTCU1"), "identificationNumber");
				NUnit.Framework.Assert.That(result.subsidiaryNumber, Is.EqualTo("EBS1"), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestCreateCommonEoriIdentification_Truncate()
		{
			var partyID = new Mock<IPartyID>();
			partyID.Setup(m => m.EoriNumber).Returns("DETOOOOOOOOOOOOOOOOOLONG");
			partyID.Setup(m => m.EoriBranchSuffix).Returns("01234");

			var result = AESMessageBuilderHelper.CreateCommonEoriIdentification<DummyEoriIdentification>(partyID.Object);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(result.identificationNumber, Is.EqualTo("DETOOOOOOOOOOOOOO"), "identificationNumber");
				NUnit.Framework.Assert.That(result.subsidiaryNumber, Is.EqualTo("0123"), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestCreateCommonEoriIdentification_Default()
		{
			NUnit.Framework.Assert.That(AESMessageBuilderHelper.CreateCommonEoriIdentification<DummyEoriIdentification>(null), Is.EqualTo(default(DummyEoriIdentification)), "NULL PartyID - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestCreateCommonEoriIdentificationWithContact()
		{
			var contactPerson = new Mock<IAESPartyContactPerson>();
			contactPerson.Setup(p => p.PersonName).Returns("Max Meier");
			contactPerson.Setup(p => p.PhoneNumber).Returns("+49 1234 5678901234");
			contactPerson.Setup(p => p.MailAddress).Returns("max@meier.de");

			var partyID = new Mock<IAESParty>();
			partyID.Setup(m => m.EoriNumber).Returns("DEEOR1");
			partyID.Setup(m => m.EoriBranchSuffix).Returns("EBS1");
			partyID.Setup(m => m.ContactPerson).Returns(contactPerson.Object);

			var result = AESMessageBuilderHelper.CreateCommonEoriIdentificationWithContact<DummyEoriIdentificationWithContact, DummyContact>(partyID.Object);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(result, Is.TypeOf<DummyEoriIdentificationWithContact>(), "Type");
				NUnit.Framework.Assert.That(result.identificationNumber, Is.EqualTo("DEEOR1"), "identificationNumber");
				NUnit.Framework.Assert.That(result.subsidiaryNumber, Is.EqualTo("EBS1"), "subsidiaryNumber");
				NUnit.Framework.Assert.That(result.ContactPerson.name, Is.EqualTo("Max Meier"), "name");
				NUnit.Framework.Assert.That(result.ContactPerson.phoneNumber, Is.EqualTo("+49 1234 5678901234"), "phone");
				NUnit.Framework.Assert.That(result.ContactPerson.eMailAddress, Is.EqualTo("max@meier.de"), "mail");
			});
		}

		[ExpectNoExceptions]
		public void TestCreateCommonEoriIdentificationWithContact_Truncate()
		{
			var contactPerson = new Mock<IAESPartyContactPerson>();
			contactPerson.Setup(p => p.PersonName).Returns("Max Meier0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
			contactPerson.Setup(p => p.PhoneNumber).Returns("+49 1234 567890123412345678901234567890123456789012345678901234567890123456789012345678901234567890");
			contactPerson.Setup(p => p.MailAddress).Returns("max@meier.de12345678901234567890123456789012345678901234567890123456789012345678901234567890");

			var partyID = new Mock<IAESParty>();
			partyID.Setup(m => m.EoriNumber).Returns("DEEOR1234567890123");
			partyID.Setup(m => m.EoriBranchSuffix).Returns("EBS123456");
			partyID.Setup(m => m.ContactPerson).Returns(contactPerson.Object);

			var result = AESMessageBuilderHelper.CreateCommonEoriIdentificationWithContact<DummyEoriIdentificationWithContact, DummyContact>(partyID.Object);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(result, Is.TypeOf<DummyEoriIdentificationWithContact>(), "Type");
				NUnit.Framework.Assert.That(result.identificationNumber, Is.EqualTo("DEEOR123456789012"), "identificationNumber");
				NUnit.Framework.Assert.That(result.subsidiaryNumber, Is.EqualTo("EBS1"), "subsidiaryNumber");
				NUnit.Framework.Assert.That(result.ContactPerson.name, Is.EqualTo("Max Meier0123456789012345678901234567890123456789012345678901234567890"), "name");
				NUnit.Framework.Assert.That(result.ContactPerson.phoneNumber, Is.EqualTo("+49 1234 56789012341234567890123456"), "phone");
				NUnit.Framework.Assert.That(result.ContactPerson.eMailAddress, Is.EqualTo("max@meier.de12345678901234567890123456789012345678901234567890123456789012345678901234567890"), "mail");
			});
		}

		[ExpectNoExceptions]
		public void TestCreateAESMessageSender()
		{
			var result = AESMessageBuilderHelper.CreateMessageSender<DummyAESMessageSender>("DEEOR001", "0001", "1234567890000000000000000");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(result, Is.TypeOf<DummyAESMessageSender>(), "Type");
				NUnit.Framework.Assert.That(result.identificationNumber, Is.EqualTo("DEEOR001"), "Identification Number");
				NUnit.Framework.Assert.That(result.subsidiaryNumber, Is.EqualTo("0001"), "Subsidiary Number");
				NUnit.Framework.Assert.That(result.authenticationNumber, Is.EqualTo("1234567890000000000000000"), "Authentication Number");
			});
		}

		[ExpectNoExceptions]
		public void TestCreateAESMessageSender_Truncate()
		{
			var eoriMaxLength = CargoWise.Customs.DE.MessageContracts.MessageSchema.AESMessageSchema.EoriCodeMaxLength;
			var branchMaxLength = CargoWise.Customs.DE.MessageContracts.MessageSchema.AESMessageSchema.EoriBranchCodeMaxLength;
			var authorisationNumberMaxLength = CargoWise.Customs.DE.MessageContracts.MessageSchema.AESMessageSchema.AuthorisationNumberMaxLength;
			var result = AESMessageBuilderHelper.CreateMessageSender<DummyAESMessageSender>(ZString.Replicate('1', eoriMaxLength + 1), ZString.Replicate('1', branchMaxLength + 1), ZString.Replicate('1', authorisationNumberMaxLength + 1));
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(result.identificationNumber.Length, Is.EqualTo(eoriMaxLength), "Identification Number length");
				NUnit.Framework.Assert.That(result.subsidiaryNumber.Length, Is.EqualTo(branchMaxLength), "Subsidiary Number length");
				NUnit.Framework.Assert.That(result.authenticationNumber.Length, Is.EqualTo(authorisationNumberMaxLength), "Authentication Number length");
			});
		}

		[ExpectNoExceptions]
		public void TestCreateAESMessageSender_NullAndEmptyString()
		{
			NUnit.Framework.Assert.That(AESMessageBuilderHelper.CreateMessageSender<DummyAESMessageSender>(null, string.Empty, null), Is.EqualTo(default(DummyAESMessageSender)));
		}

		[ExpectNoExceptions]
		public void TestCreateAESMessageRecipient()
		{
			var result = AESMessageBuilderHelper.CreateMessageRecipient<DummyAESMessageRecipient>("DE000011");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(result, Is.TypeOf<DummyAESMessageRecipient>(), "Type");
				NUnit.Framework.Assert.That(result.referenceNumber, Is.EqualTo("DE000011"), "Reference Number");
			});
		}

		[ExpectNoExceptions]
		public void TestCreateAESMessageRecipient_NullAndEmptyString()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(AESMessageBuilderHelper.CreateMessageRecipient<DummyAESMessageRecipient>(string.Empty), Is.EqualTo(default(DummyAESMessageRecipient)), "Empty - should be [null]");
				NUnit.Framework.Assert.That(AESMessageBuilderHelper.CreateMessageRecipient<DummyAESMessageRecipient>(null), Is.EqualTo(default(DummyAESMessageRecipient)), "Null - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestCreateAESMessage()
		{
			var preparationDateAndTimeMock = new Mock<IDateAndTime>();
			preparationDateAndTimeMock.Setup(dt => dt.DateAndTime).Returns(new DateTime(2021, 07, 29, 14, 05, 00));

			var interchangeSender = new Mock<IPartyID>();
			interchangeSender.Setup(p => p.EoriNumber).Returns("DE1234567");
			interchangeSender.Setup(p => p.EoriBranchSuffix).Returns("1234");

			var mockAESMessageHeader = new Mock<IAESMessageHeader>();
			mockAESMessageHeader.Setup(h => h.MessageIdentification).Returns("<<SENDERS REFERENCE PLACE HOLDER>>");
			mockAESMessageHeader.Setup(h => h.PreparationDateAndTimeUtc).Returns(preparationDateAndTimeMock.Object);
			mockAESMessageHeader.Setup(h => h.InterchangeSender).Returns(interchangeSender.Object);
			mockAESMessageHeader.Setup(h => h.AuthorizationNumber).Returns("DEAUTHNUMBER");
			mockAESMessageHeader.Setup(h => h.InterchangeRecipientID).Returns("DE003302");

			var message = AESMessageBuilderHelper.CreateMessage<DummyAesMessage, DummyAESMessageSender, DummyAESMessageRecipient>(mockAESMessageHeader.Object, "A.1.2");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message, Is.TypeOf<DummyAesMessage>(), "Type");
				NUnit.Framework.Assert.That(message.messageGroup, Is.EqualTo(DummyAESMessageGroup.ABC), "messageGroup");
				NUnit.Framework.Assert.That(message.messageType, Is.EqualTo(DummyAESMessageType.ABCDEF), "messageType");
				NUnit.Framework.Assert.That(message.messageIdentification, Is.EqualTo("<<SENDERS REFERENCE PLACE HOLDER>>"), "messageIdentification");
				NUnit.Framework.Assert.That(message.messageVersion, Is.EqualTo("A.1.2"), "messageVersion");
				NUnit.Framework.Assert.That(message.preparationDateAndTime, Is.EqualTo(new DateTime(2021, 07, 29, 14, 05, 00)), "messagePreparation");
				NUnit.Framework.Assert.That(message.MessageSender.identificationNumber, Is.EqualTo("DE1234567"), "MessageSender.identificationNumber");
				NUnit.Framework.Assert.That(message.MessageSender.subsidiaryNumber, Is.EqualTo("1234"), "MessageSender.subsidiaryNumber");
				NUnit.Framework.Assert.That(message.MessageSender.authenticationNumber, Is.EqualTo("DEAUTHNUMBER"), "MessageSender.authenticationNumber");
				NUnit.Framework.Assert.That(message.MessageRecipient.referenceNumber, Is.EqualTo("DE003302"), "MessageRecipient.referenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestCreateAESMessageWithAction()
		{
			var preparationDateAndTimeMock = new Mock<IDateAndTime>();
			preparationDateAndTimeMock.Setup(dt => dt.DateAndTime).Returns(new DateTime(2021, 07, 29, 14, 05, 00));

			var interchangeSender = new Mock<IPartyID>();
			interchangeSender.Setup(p => p.EoriNumber).Returns("DE1234567");
			interchangeSender.Setup(p => p.EoriBranchSuffix).Returns("1234");

			var mockAESMessageHeader = new Mock<IAESMessageHeader>();
			mockAESMessageHeader.Setup(h => h.MessageIdentification).Returns("<<SENDERS REFERENCE PLACE HOLDER>>");
			mockAESMessageHeader.Setup(h => h.PreparationDateAndTimeUtc).Returns(preparationDateAndTimeMock.Object);
			mockAESMessageHeader.Setup(h => h.InterchangeSender).Returns(interchangeSender.Object);
			mockAESMessageHeader.Setup(h => h.AuthorizationNumber).Returns("DEAUTHNUMBER");
			mockAESMessageHeader.Setup(h => h.InterchangeRecipientID).Returns("DE003302");

			var message = AESMessageBuilderHelper.CreateMessage<DummyAesMessage, DummyAESMessageSender, DummyAESMessageRecipient>(mockAESMessageHeader.Object, "A.1.2", m => m.AdditionalProperty = 42);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message, Is.TypeOf<DummyAesMessage>(), "Type");
				NUnit.Framework.Assert.That(message.messageGroup, Is.EqualTo(DummyAESMessageGroup.ABC), "messageGroup");
				NUnit.Framework.Assert.That(message.messageType, Is.EqualTo(DummyAESMessageType.ABCDEF), "messageType");
				NUnit.Framework.Assert.That(message.messageIdentification, Is.EqualTo("<<SENDERS REFERENCE PLACE HOLDER>>"), "messageIdentification");
				NUnit.Framework.Assert.That(message.messageVersion, Is.EqualTo("A.1.2"), "messageVersion");
				NUnit.Framework.Assert.That(message.preparationDateAndTime, Is.EqualTo(new DateTime(2021, 07, 29, 14, 05, 00)), "messagePreparation");
				NUnit.Framework.Assert.That(message.MessageSender.identificationNumber, Is.EqualTo("DE1234567"), "MessageSender.identificationNumber");
				NUnit.Framework.Assert.That(message.MessageSender.subsidiaryNumber, Is.EqualTo("1234"), "MessageSender.subsidiaryNumber");
				NUnit.Framework.Assert.That(message.MessageSender.authenticationNumber, Is.EqualTo("DEAUTHNUMBER"), "MessageSender.authenticationNumber");
				NUnit.Framework.Assert.That(message.MessageRecipient.referenceNumber, Is.EqualTo("DE003302"), "MessageRecipient.referenceNumber");
				NUnit.Framework.Assert.That(message.AdditionalProperty, Is.EqualTo(42), "AdditionalProperty");
			});
		}
	}

	class DummyEoriIdentification : IEoriIdentification
	{
		public string identificationNumber { get; set; }
		public string subsidiaryNumber { get; set; }
	}

	class DummyEoriIdentificationWithContact : IEoriIdentificationWithContact<DummyContact>
	{
		public string identificationNumber { get; set; }
		public string subsidiaryNumber { get; set; }
		public string name { get; set; }
		public DummyContact ContactPerson { get; set; }
	}

	class DummyContact : IContactPerson
	{
		public string name { get; set; }
		public string phoneNumber { get; set; }
		public string eMailAddress { get; set; }
	}

	class DummyAESMessageSender : IAESMessageSender
	{
		public string identificationNumber { get; set; }
		public string subsidiaryNumber { get; set; }
		public string authenticationNumber { get; set; }
	}

	class DummyAESMessageRecipient : IAESMessageRecipient
	{
		public string referenceNumber { get; set; }
	}

	class DummyAesMessage : IAESMessage<DummyAESMessageSender, DummyAESMessageRecipient>
	{
		public DateTime preparationDateAndTime { get; set; }
		public string messageIdentification { get; set; }
		public DummyAESMessageGroup messageGroup { get; set; }
		public DummyAESMessageType messageType { get; set; }
		public string messageVersion { get; set; }
		public DummyAESMessageSender MessageSender { get; set; }
		public DummyAESMessageRecipient MessageRecipient { get; set; }
		public int AdditionalProperty { get; set; }
	}

	enum DummyAESMessageGroup
	{
		// for all AES messages, we have just one MessageGroup per outbound message. The Runtime supplies the field with the first enum value, which is the right one
		ABC
	}

	enum DummyAESMessageType
	{
		// for all AES messages, we have just one MessageType per outbound message. The Runtime supplies the field with the first enum value, which is the right one
		ABCDEF
	}
}
