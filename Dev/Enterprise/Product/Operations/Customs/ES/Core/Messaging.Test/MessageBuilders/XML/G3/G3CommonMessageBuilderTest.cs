using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public abstract class G3CommonMessageBuilderTest<TMessageBuilder, TProvider, T> : XMLMessageBuilderTest<TMessageBuilder, TProvider, T>
		where TProvider : class, IG3CommonMessageDataProvider
		where TMessageBuilder : G3CommonMessageBuilder<TProvider, T>
	{
		protected override sealed ZString GetSignedMessageTestFileContent() => GetTestFile();
		protected override sealed ZString GetUnsignedMessageTestFileContent() => GetTestFile();
		protected abstract ZString GetTestFile();

		public void TestPopulateTestIndicator()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			CombineAssertions(() =>
			{
				var messageText = CreateMessageBuilder().GetSignedMessageText();
				AssertContains("Recipient is correct", $"<{XMLTestFileConstants.XmlElementNamespace}Recipient>ES.AEAT</{XMLTestFileConstants.XmlElementNamespace}Recipient>", messageText);
			});
		}

		protected IG3Message SetupMessage()
		{
			var mockMessage = new Mock<IG3Message>();
			mockMessage.Setup(m => m.Sender).Returns("sender");

			return mockMessage.Object;
		}

		protected IG3Declarant SetupDeclarant()
		{
			var mockDeclarant = new Mock<IG3Declarant>();
			mockDeclarant.Setup(m => m.IdNumber).Returns("ESA78587268");
			mockDeclarant.Setup(m => m.Name).Returns("TARIC, S.A.U.");

			var fullAddress = SetupFullAddress();
			mockDeclarant.Setup(m => m.FullAddress).Returns(fullAddress);
			var communication = SetupCommunication();
			mockDeclarant.Setup(m => m.Communication).Returns(communication);

			return mockDeclarant.Object;
		}

		protected IG3Representative SetupRepresentative()
		{
			var mockRepresentative = new Mock<IG3Representative>();
			mockRepresentative.Setup(m => m.IdNumber).Returns("321654987A");
			mockRepresentative.Setup(m => m.Status).Returns("2");
			mockRepresentative.Setup(m => m.Name).Returns("Name");
			var communication = SetupCommunication();
			mockRepresentative.Setup(m => m.Communication).Returns(communication);

			return mockRepresentative.Object;
		}

		protected IG3FullAddress SetupFullAddress()
		{
			var mockFullAddress = new Mock<IG3FullAddress>();
			mockFullAddress.Setup(m => m.Street).Returns("BOIX Y MORER 6 6");
			mockFullAddress.Setup(m => m.Number).Returns("6");
			mockFullAddress.Setup(m => m.POBox).Returns("120");
			mockFullAddress.Setup(m => m.SubDivision).Returns("subdiv");
			mockFullAddress.Setup(m => m.Country).Returns("ES");
			mockFullAddress.Setup(m => m.PostCode).Returns("28003");
			mockFullAddress.Setup(m => m.City).Returns("MADRID");

			return mockFullAddress.Object;
		}

		protected IG3Communication SetupCommunication()
		{
			var mockCommunication = new Mock<IG3Communication>();
			mockCommunication.Setup(m => m.CommunicationType).Returns("EM");
			mockCommunication.Setup(m => m.CommunicationId).Returns("informatica@taric.es");

			return mockCommunication.Object;
		}
	}
}
