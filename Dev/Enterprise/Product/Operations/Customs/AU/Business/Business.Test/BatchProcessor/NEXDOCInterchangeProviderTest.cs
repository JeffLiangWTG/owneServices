using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class NEXDOCInterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			var messageText = @"<RexAcknowledgeOwnership>
    <identification>
      <rexNumber>REX0000028829</rexNumber>
    </identification>
    <isAccepted>true</isAccepted>
  </RexAcknowledgeOwnership>";

#if NETFRAMEWORK
			var expectedPrefix = @"<UniversalInterchange xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">";
#else
			var expectedPrefix = @"<UniversalInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">";
#endif

			var expectedInterchangeText = $@"{expectedPrefix}
  <Header>
    <SenderID>{GlbCompany.CurrentCompany.LicenceKeyIdentifier}</SenderID>
    <RecipientID>NEXDOCSTest</RecipientID>
    <DeliveryMetadata>
      <ValueCollection>
        <Value>
          <Name>Submitter</Name>
          <Type>String</Type>
          <Data>{GlbStaff.CurrentUser.GS_Code}</Data>
        </Value>
        <Value>
          <Name>MessageType</Name>
          <Type>String</Type>
          <Data>RexAcknowledgeOwnership</Data>
        </Value>
        <Value>
          <Name>RexNumber</Name>
          <Type>String</Type>
          <Data>REX0000028829</Data>
        </Value>
        <Value>
          <Name>JobNumber</Name>
          <Type>String</Type>
          <Data />
        </Value>
      </ValueCollection>
    </DeliveryMetadata>
  </Header>
  <Body>
    <RexAcknowledgeOwnership>
      <identification>
        <rexNumber>REX0000028829</rexNumber>
      </identification>
      <isAccepted>true</isAccepted>
    </RexAcknowledgeOwnership>
  </Body>
</UniversalInterchange>";

			var messages = new NonDependentEDIMessageCollection(Factory);
			var message1 = messages.AddNew();
			message1.EM_MessageText = messageText;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.NEXDOCS;
			message1.EM_ApplicationReference = "REX0000028829";
			message1.EM_IsTestMessage = true;
			message1.EM_MessageType = EDIMessage.ApplicationCodes.NEXDOCS;
			message1.EM_Status = "QUE";
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;

			var message2 = messages.AddNew();
			message2.EM_MessageText = messageText;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.NEXDOCS;
			message2.EM_ApplicationReference = "REX0000028829";
			message2.EM_IsTestMessage = true;
			message2.EM_MessageType = EDIMessage.ApplicationCodes.NEXDOCS;
			message2.EM_Status = "QUE";
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;

			var provider = new NEXDOCInterchangeProvider(messages);
			var interchanges = provider.Interchanges;

			CombineAssertions(() =>
			{
				AssertEquals("Should have created 2 interchanges", 2, interchanges.Length);
				AssertEquals("EI_ApplicationCode", "NEX", interchanges[0].EI_ApplicationCode);
				AssertEquals("EI_InterchangeType", "NEX", interchanges[0].EI_InterchangeType);
				AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchanges[0].EI_From);
				AssertEquals("EI_To", "NEXDOCSTest", interchanges[0].EI_To);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchanges[0].EI_ReceiveTransmit);

				AssertEqualsIgnoreFormat(expectedInterchangeText, interchanges[0].EI_BodyText);
				AssertEqualsIgnoreFormat(expectedInterchangeText, interchanges[1].EI_BodyText);
			});
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new NEXDOCInterchangeProvider(collection);

		void AssertEqualsIgnoreFormat(string expected, string actual)
		{
			var expectedReplaced = Regex.Replace(expected, @"[\r\n]+\s+", string.Empty);
			var actualReplaced = Regex.Replace(actual, @"[\r\n]+\s+", string.Empty);
			AssertMultilineASCIIEquals(expectedReplaced, actualReplaced);
		}
	}
}
