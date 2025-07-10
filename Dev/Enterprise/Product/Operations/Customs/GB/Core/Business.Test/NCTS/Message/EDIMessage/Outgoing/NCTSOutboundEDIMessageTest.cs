using System.Globalization;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(NCTSOutboundEDIMessage))]
	class NCTSOutboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<NCTSOutboundEDIMessage>();
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.GbCustomsNCTS, message.EM_ApplicationCode);
		}

		public void TestGetMessageReferenceNumber()
		{
			var message1 = Factory.New<NCTSOutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(001) from IMessageNumberStrategy sequence", "1", message1.EM_MessageNum);

			var message2 = Factory.New<NCTSOutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(002) from IMessageNumberStrategy sequence", "2", message2.EM_MessageNum);
		}

		public void TestCorrectlyTypeDeciding()
		{
			var message = Factory.New<NCTSOutboundEDIMessage>();
			Factory.Save();
			AssertType<NCTSOutboundEDIMessage>("Using base EDIMessage", NewFactory().Load<EDIMessage>(message.PK));
			AssertType<NCTSOutboundEDIMessage>("Using GB EDIMessage", NewFactory().Load<GbEDIMessage>(message.PK));
		}

		public void TestSendersReferencePlaceHolderReplacement()
		{
			var company = GlbCompany.CurrentCompany;
			var branch = GlbBranch.CurrentBranch;
			var nctsHeader1 = Factory.New<NctsHeader>();
			nctsHeader1.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader1.BH_GB = branch.PK;

			var message1 = Factory.New<NCTSOutboundEDIMessage>();
			message1.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message1.EM_MessageType = GB_NCTS5DeparturePhaseList.Codes.DepartureDeclaration;
			message1.EM_MessageText = $"<messageIdentification>{NCTSOutboundEDIMessage.SendersReferencePlaceHolderHtml}</messageIdentification>";
			nctsHeader1.LinkedMessages.Add(message1);
			Factory.Save();

			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeader.BH_GB = branch.PK;

			var message2 = Factory.New<NCTSOutboundEDIMessage>();
			message2.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			message2.EM_MessageType = GB_NCTS5ArrivalPhaseList.Codes.ArrivalNotification;
			message2.EM_MessageText = $"<messageIdentification>{NCTSOutboundEDIMessage.SendersReferencePlaceHolderHtml}</messageIdentification>";
			arrivalHeader.LinkedMessages.Add(message2);
			Factory.Save();

			var replaceValue1 = string.Format(CultureInfo.InvariantCulture, "<messageIdentification>{0}/{1}</messageIdentification>", nctsHeader1.BH_JobReference, message1.EM_MessageNum);
			var replaceValue2 = string.Format(CultureInfo.InvariantCulture, "<messageIdentification>{0}/{1}</messageIdentification>", arrivalHeader.BH_JobReference, message2.EM_MessageNum);

			CombineAssertions(() =>
			{
				AssertEquals("Should replace message 1 placeholder with '{BH_JHobReferenece}/{EM_MessageNum}', e.g. 'NCT0001/123'", replaceValue1, message1.EM_MessageText);
				AssertEquals("Should replace message 1 placeholder with '{BH_JHobReferenece}/{EM_MessageNum}', e.g. 'NCT0001/123'", replaceValue2, message2.EM_MessageText);
			});
		}

		public void TestLinkedObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var message = Factory.New<NCTSOutboundEDIMessage>();
			nctsHeader.LinkedMessages.Add(message);
			var movementHeader = nctsHeader.MovementHeader;
			Factory.Save();
			AssertEquals("Pre-requisite", expected: true, nctsHeader.IsPhase5Departure);
			AssertSame(message.EM_LinkedObject, movementHeader);

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = EDIMessage.ApplicationCodes.EuNcts;
			message = Factory.New<NCTSOutboundEDIMessage>();
			nctsHeader.LinkedMessages.Add(message);
			Factory.Save();
			AssertEquals("Pre-requisite", expected: false, nctsHeader.IsPhase5Departure);
			AssertSame(message.EM_LinkedObject, nctsHeader);
		}
	}
}
