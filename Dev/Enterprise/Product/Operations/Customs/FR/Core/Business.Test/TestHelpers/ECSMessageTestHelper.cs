using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
	public class ECSMessageTestHelper
	{
		public TestEDIMessage CreateTestEDIMessage(BusinessObjectFactory factory, ZGuid branchPK, ZString applicationCode, ZString receiveTransmit, ZString type, ZString subType, ZString status, ZString text)
		{
			var message = factory.NewWithValidTestData<TestEDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = receiveTransmit;
			message.EM_Status = status;
			message.EM_GB = branchPK;
			message.EM_MessageText = text;
			message.EM_MessageType = type;
			message.EM_MessageSubType = subType;

			return message;
		}

		public EDIMessage CreateFREDIMessage(BusinessObjectFactory factory, ZGuid branchPK, ZString applicationCode, ZString receiveTransmit, ZString type, ZString subType, ZString status, ZString text)
		{
			var message = factory.NewWithValidTestData<FREDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = receiveTransmit;
			message.EM_Status = status;
			message.EM_GB = branchPK;
			message.EM_MessageText = text;
			message.EM_MessageType = type;
			message.EM_MessageSubType = subType;
			message.MessageNumberStrategy = new MockMessageNumberStrategy();
			return message;
		}

		public void AssertEDIMessage(EDIMessage message, EDIInterchange interchange, ZString applicationCode, ZString receiveTransmit, ZString type, ZString subType, ZString num, ZString reference, ZDateTime date, ZString status, ZString linkTable, ZGuid linkTablePK, ZString messageText)
		{
			Assertion.AssertEquals("message linked to interchange", interchange.PK, message.EM_EI);
			Assertion.AssertEquals("ApplicationCode", applicationCode, message.EM_ApplicationCode);
			Assertion.AssertEquals("ReceiveTransmit", receiveTransmit, message.EM_ReceiveTransmit);
			Assertion.AssertEquals("MessageType", type, message.EM_MessageType);
			Assertion.AssertEquals("MessageSubType", subType, message.EM_MessageSubType);
			Assertion.AssertEquals("MessageNum", num, message.EM_MessageNum);
			Assertion.AssertEquals("ApplicationReference", reference, message.EM_ApplicationReference);
			Assertion.AssertEquals("Status", status, message.EM_Status);
			Assertion.AssertEquals("HeldUntilDate", date, message.EM_HeldUntilDate);
			Assertion.AssertEquals("MessageText", messageText, message.EM_MessageText);
			Assertion.AssertEquals("LinkTable - this will be set by message processor", linkTable, message.EM_LinkTable);
			Assertion.AssertEquals("LinkUniqueID - this will be set by message processor", linkTablePK, message.EM_LinkUniqueID);
		}

		public EDIInterchange CreateTestEDIInterchange(BusinessObjectFactory factory, ZGuid branchPK, ZString from, ZString to, ZString applicationCode, ZString receiveTransmit, ZString type, ZString num, ZString status, ZString messageText)
		{
			var interchange = factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_GB = branchPK;
			interchange.EI_From = from;
			interchange.EI_To = to;
			interchange.EI_ApplicationCode = applicationCode;
			interchange.EI_ReceiveTransmit = receiveTransmit;
			interchange.EI_InterchangeType = type;
			interchange.EI_InterchangeNum = num;
			interchange.EI_Status = status;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = messageText;

			return interchange;
		}

		public void AssertEDIInterchange(EDIInterchange interchange, ZString applicationCode, ZString receiveTransmit, ZString type, ZString from, ZString to, ZString status, ZString header, ZString body)
		{
			Assertion.AssertEquals("ApplicationCode", applicationCode, interchange.EI_ApplicationCode);
			Assertion.AssertEquals("InterchangeType", type, interchange.EI_InterchangeType);
			Assertion.AssertEquals("ReceiveTransmit", receiveTransmit, interchange.EI_ReceiveTransmit);
			Assertion.AssertEquals("Status", status, interchange.EI_Status);
			Assertion.AssertEquals("Sender", from, interchange.EI_From);
			Assertion.AssertEquals("Recepient", to, interchange.EI_To);
			Assertion.AssertContains("BodyText", body, interchange.EI_BodyText);
			Assertion.AssertContains("HeaderText", header, interchange.EI_HeaderText);
		}

		public CusExitDetail CreateCusExitDetail(BusinessObjectFactory factory, ZString cehReferenceNumber, ZString cedMRN, ZString status)
		{
			var dec = factory.New<JobDeclaration>();
			var exitHeader = factory.New<CusExitControlHeader>();
			exitHeader.CEH_ReferenceNumber = cehReferenceNumber;
			exitHeader.CEH_ParentTableCode = "JE";
			exitHeader.CEH_ParentID = dec.PK;
			var exitDetail = factory.New<CusExitDetail>();
			exitDetail.CED_CEH = exitHeader.PK;
			exitDetail.CED_MovementReferenceNumber = cedMRN;
			exitDetail.CED_Status = status;

			return exitDetail;
		}

		public void AssertCusExitDetail(CusExitDetail exitDetail, ZString status, bool isFinalized, ZDate date)
		{
			Assertion.AssertEquals(status, exitDetail.CED_Status);
			Assertion.AssertEquals(isFinalized, exitDetail.CED_IsFinalized);
			Assertion.AssertEquals(date, exitDetail.CED_ExitDate);
		}

		public ZString CreateEIMessageBodyWithReponseDatasNode(ZString schemaID, ZString mrn, ZString etat, ZDate date)
		{
			return $@"
<Message>
	<EnveloppeMessage>
		<schemaID>{schemaID}</schemaID>
		<schemaVersion>01012012</schemaVersion>
		<partyId>32582075100080</partyId>
		<transactionId>FORMATION+ECS+26</transactionId>
		<numseq>0</numseq>
	</EnveloppeMessage>
	<ReponseDeclaration>
		<Entete>
			<mrnecs>{mrn}</mrnecs>
		</Entete>
		<ReponseDatas>
			<Notification>
				<Etat>
					<etat>{etat}</etat>
					<etatDate>{date}</etatDate>
					<etatHeure>11:49</etatHeure>
				</Etat>
			</Notification>
		</ReponseDatas>
	</ReponseDeclaration>
</Message>
";
		}

		public ZString CreateEIMessageBodyWithReponseEtatNode(ZString schemaID, ZString mrn, ZString etat, ZDate date)
		{
			return $@"
<Message>
	<EnveloppeMessage>
		<schemaID>{schemaID}</schemaID>
		<schemaVersion>01012012</schemaVersion>
		<partyId>32582075100080</partyId>
		<transactionId>FORMATION+ECS+26</transactionId>
		<numseq>0</numseq>
	</EnveloppeMessage>
	<ReponseDeclaration>
		<Entete>
			<mrnecs>{mrn}</mrnecs>
		</Entete>
		<ReponseEtat>
			<etat>{etat}</etat>
			<etatDate>{date}</etatDate>
			<etatHeure>11:49</etatHeure>
		</ReponseEtat>
	</ReponseDeclaration>
</Message>
";
		}

		public ZString CresteEMMessageText(ZString opeben)
		{
			return $@"
   <Declaration>
      <Entete>
         <codact>1</codact>
      </Entete>
      <Gen>
         <mrnecs>20FRD4440053891481</mrnecs>
         <bureausortie>FR002651</bureausortie>
         <opeben>{opeben}</opeben>
         <numagr>00001910</numagr>
         <locagr>39433691100042/2</locagr>
      </Gen>
   </Declaration>
";
		}
	}

	class MockMessageNumberStrategy : IMessageNumberStrategy
	{
		public string GetMessageReferenceNumber() => Guid.NewGuid().ToString("N");
	}
}
