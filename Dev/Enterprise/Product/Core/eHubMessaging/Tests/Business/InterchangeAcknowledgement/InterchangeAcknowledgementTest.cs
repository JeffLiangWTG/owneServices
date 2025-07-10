using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.eHubMessaging.Tests
{
	class InterchangeAcknowledgementTest : TestCaseWithFactory
	{
		public void TestInterchangeAcknowledgementSetsCommunicationPartyConfig_ForInterchange()
		{
			using (Factory.AddDisposableService())
			{
				(var inboundConfig, var outboundConfig) = CreateConfigurations();

				var interchange = CreateTestInterchange(inboundConfig);

				var interchangesBefore = Factory.Load<EDIInterchange>(new ZQuery());
				new InterchangeAcknowledgement().Send(Factory, new Mock<INotifications>().Object, interchange, string.Empty, Enumerable.Empty<IValidationRule>(), InterchangeAcknowledgementType.Success);

				Factory.Save();

				var newInterchanges = Factory.Load<EDIInterchange>(new ZQuery()).Except(interchangesBefore).ToList();

				AssertEquals(1, newInterchanges.Count);
				AssertEquals(outboundConfig.PK, newInterchanges[0].EI_ECC_CommunicationPartyConfig);
			}
		}

		public void TestEDICommunicationsModeNotSaved()
		{
			using (Factory.AddDisposableService())
			{
				(var inboundConfig, var outboundConfig) = CreateConfigurations();
				var interchange = CreateTestInterchange(inboundConfig);
				new InterchangeAcknowledgement().Send(Factory, new Mock<INotifications>().Object, interchange, string.Empty, Enumerable.Empty<IValidationRule>(), InterchangeAcknowledgementType.Success);

				Factory.Save();

				var newCommunicationsModes = Factory.Load<EDICommunicationsMode>(new ZQuery()).ToList();
				AssertEquals(0, newCommunicationsModes.Count);
			}
		}

		public void TestInterchangeAcknowledgementSetsCommunicationPartyConfig_ForMessage()
		{
			using (Factory.AddDisposableService())
			{
				(var inboundConfig, var outboundConfig) = CreateConfigurations();

				var interchange = CreateTestInterchange(inboundConfig, withMessage: true);

				var interchangesBefore = Factory.Load<EDIInterchange>(new ZQuery());
				new InterchangeAcknowledgement().Send(Factory, new Mock<INotifications>().Object, interchange.ContainedMessages[0], string.Empty, Enumerable.Empty<IValidationRule>(), InterchangeAcknowledgementType.Success);

				Factory.Save();

				var newInterchanges = Factory.Load<EDIInterchange>(new ZQuery()).Except(interchangesBefore).ToList();

				AssertEquals(1, newInterchanges.Count);
				AssertEquals(outboundConfig.PK, newInterchanges[0].EI_ECC_CommunicationPartyConfig);
			}
		}

		public void TestInterchangeAcknowledgementDoesNotCreateInterchange_AndGeneratesCorrectMessageLog_WithoutOutboundConfig()
		{
			using (Factory.AddDisposableService())
			{
				(var inboundConfig, var outboundConfig) = CreateConfigurations(withOutbound: false);

				var interchange = CreateTestInterchange(inboundConfig, withMessage: true);
				var interchangesBefore = Factory.Load<EDIInterchange>(new ZQuery());

				new InterchangeAcknowledgement().Send(Factory, new Mock<INotifications>().Object, interchange.ContainedMessages[0], string.Empty, Enumerable.Empty<IValidationRule>(), InterchangeAcknowledgementType.Success);

				Factory.Save();

				var newInterchanges = Factory.Load<EDIInterchange>(new ZQuery()).Except(interchangesBefore).ToList();

				AssertEquals(0, newInterchanges.Count);
				var logNote = interchange.ContainedMessages[0].GetNotes().FindByDescription("Data Import Log Text");
				AssertEquals(1, logNote.Length);
				AssertEquals("Acknowledgement message requested, but acknowledgement can’t be generated as outbound configuration is missing.", logNote[0].ST_NoteDataAsText);
			}
		}

		public void TestInterchangeAcknowledgementSetsCorrectEM_EM_RequestMessage_ForResponseMessage()
		{
			using (Factory.AddDisposableService())
			{
				(var inboundConfig, var outboundConfig) = CreateConfigurations();

				var interchange = CreateTestInterchange(inboundConfig, withMessage: true);
				new InterchangeAcknowledgement().Send(Factory, new Mock<INotifications>().Object, interchange.ContainedMessages[0], string.Empty, Enumerable.Empty<IValidationRule>(), InterchangeAcknowledgementType.Success);

				Factory.Save();

				AssertNotNull(Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit).AddToFilter(new ZQuery(EDIMessageSchema.EM_EM_RequestMessage, interchange.ContainedMessages[0].PK))));
			}
		}

		public void TestInterchangeAcknowledgementSetsCorrectEM_ExternalReferenceNumber_ForResponseMessage()
		{
			using (Factory.AddDisposableService())
			{
				(var inboundConfig, var outboundConfig) = CreateConfigurations();

				var interchange = CreateTestInterchange(inboundConfig, withMessage: true);
				var externalReferenceNumber = "F9723E66-B989-4010-AD3A-F7EAA9FF13F8";
				interchange.ContainedMessages[0].EM_ExternalReferenceNumber = externalReferenceNumber;

				Factory.Save();

				new InterchangeAcknowledgement().Send(Factory, new Mock<INotifications>().Object, interchange.ContainedMessages[0], string.Empty, Enumerable.Empty<IValidationRule>(), InterchangeAcknowledgementType.Success);

				Factory.Save();

				AssertNotNull("Response message with correct EM_ExternalReferenceNumber should have been created", Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit).AddToFilter(new ZQuery(EDIMessageSchema.EM_ExternalReferenceNumber, externalReferenceNumber))));
			}
		}

		(EDICommunicationPartyConfig, EDICommunicationPartyConfig) CreateConfigurations(bool withOutbound = true)
		{
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();

			var inboundConfig = Factory.New<EDICommunicationPartyConfig>();
			inboundConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;
			party.Configs.Add(inboundConfig);

			EDICommunicationPartyConfig outboundConfig = null;
			if (withOutbound)
			{
				outboundConfig = Factory.New<EDICommunicationPartyConfig>();
				outboundConfig.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;
				party.Configs.Add(outboundConfig);
			}

			Factory.Save();

			return (inboundConfig, outboundConfig);
		}

		EDIInterchange CreateTestInterchange(EDICommunicationPartyConfig config, bool withMessage = false)
		{
			var interchangeWithACK = Factory.New<EDIInterchange>();
			interchangeWithACK.EI_InterchangeType = EDIInterchangeTypeList.Codes.XMS;
			interchangeWithACK.EI_From = "Sender";
			interchangeWithACK.EI_To = "Recipient";
			interchangeWithACK.EI_ApplicationCode = "XMS";
			interchangeWithACK.EI_ECC_CommunicationPartyConfig = config.PK;

			interchangeWithACK.EI_HeaderText = @"<InterchangeInfo xmlns=""http://www.edi.com.au/EnterpriseService/"">
	<Date>2013-11-28T15:37:56.893+11:00</Date>
	<XmlType>Verbose</XmlType>
	<Source/>
	<Target />
	  <Acknowledgement>
		<Required>OnAll</Required>
		<Channel>eAdaptor</Channel>
		<RecipientID>HYEDAUIKB</RecipientID>
	  </Acknowledgement>
  </InterchangeInfo>";

			if (withMessage)
			{
				var message = (EDIMessage)Factory.New<IXmlEDIMessage>();
				message.EM_ECC_CommunicationPartyConfig = config.PK;
				interchangeWithACK.ContainedMessages.Add(message);
			}

			Factory.Save();

			return interchangeWithACK;
		}
	}
}
