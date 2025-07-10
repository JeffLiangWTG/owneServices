using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Edifact;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(Exit2ManifestStatus))]
	sealed class Exit2ManifestStatusTest : AUCustomsManifestStatusTest
	{
		public EDIMessage AddValidExit2Message(ForwardingConsol consol)
		{
			EDIMessage message = AddMessageToConsol(consol, UNOBCharacterSet.FromUNOA(TestConsolValidEX2), "ORG");
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.Logs.AddNew(Events.DeclarationQueued);

			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			return message;
		}

		public EDIMessage AddValidEX2Response(ForwardingConsol consol)
		{
			EDIMessage message = AddMessageToConsol(consol, UNOBCharacterSet.FromUNOA(TestConsolValidEX2Response), "IAC");
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.Logs.AddNew(Events.CustomsCommenced);

			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			return message;
		}

		public void TestValidateEnvironmentForSendingManifests()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Exit2ManifestStatusForTest status = new Exit2ManifestStatusForTest(consol);
			AssertEquals("Error", "Unable to send Exit2 message as Exit2 no longer exists.", status.ValidateEnvironmentForSendingManifests(null));
		}

		public override void TestDeclareManifestWhenWeHaveChanges()
		{
			Assert(true);
		}

		public override void TestDeclareManifestWhenWeDontHaveChanges()
		{
			Assert(true);
		}

		public override void TestDeclareManifestWhenWeHaveErrors()
		{
			Assert(true);
		}

		public override void TestDeclareManifestWhenWeAreWaitingForAResponse()
		{
			Assert(true);
		}

		protected override void TestWithdrawManifestHasChanges(bool hasChanges)
		{
			Assert(true);
		}

		public override void TestWithdrawWhenHasNoMessages()
		{
			Assert(true);
		}

		public override void TestWithdrawWhenWaitingForAResponse()
		{
			Assert(true);
		}

		public override void TestDeclareManifestWhenThereAreEnvironmentErrors()
		{
			Assert(true);
		}

		public override void TestWithdrawManifestWhenThereAreEnvironmentErrors()
		{
			Assert(true);
		}

		#region Implementation

		protected override EDIMessage AddValidMessage(IManifestProvider manifestProvider)
		{
			return AddValidExit2Message((ForwardingConsol)manifestProvider);
		}

		protected override EDIMessage AddValidResponse(IManifestProvider manifestProvider)
		{
			return AddValidEX2Response((ForwardingConsol)manifestProvider);
		}

		EDIMessage AddMessageToConsol(ForwardingConsol consol, ZString messageText, ZString messageSubType)
		{
			EDIMessage myZExit2Message = consol.Messages.AddNew(typeof(CMRESMMessage));
			myZExit2Message.EM_ApplicationCode = "EX2";
			myZExit2Message.EM_ApplicationReference = consol.JK_UniqueConsignRef;
			myZExit2Message.EM_GB = GlbBranch.CurrentBranch.PK;
			myZExit2Message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			myZExit2Message.EM_MessageNum = "TESTING321";
			myZExit2Message.EM_MessageType = "EX2";
			myZExit2Message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			myZExit2Message.EM_Status = EDIMessage.Status.Sent;

			myZExit2Message.EM_MessageText = messageText;
			myZExit2Message.EM_MessageSubType = messageSubType;
			return myZExit2Message;
		}

		protected override IManifestProvider NewManifestProvider()
		{
			return Factory.New<ForwardingConsol>();
		}

		protected override ZDateTime GetExpectedLastMessageDate(EDIMessage message)
		{
			return new ZDateTime(new DateTime(03, 12, 08, 19, 50, 04));
		}

		protected override ZString ExpectedCustomsEntryNumber
		{
			get { return "3B033421001KSK"; }
		}

		const string TestConsolValidEX2 = @"UNH+<<MSGNO PLACEHOLDER>>+CUSMAN:089:000:AC+2300+01:F'
BGM+CKM+C00001031+031208+0'
TDT++TV2222++10++CONTR'
RFF+VM+9134517'
LOC+CDN:NZ:66'
LOC+PDP:3B:ZZ'
LOC+POD::50::NZAKL'
DTM+DEP+031208'
QTY+48:0.056:51'
NAD+PP+++TEST EXIT 2 SEA SHIPPING PROVIDER'
NAD+CA+++EDI'
UNS+D'
RFF+AAE+1M033421010ZIC'
RFF+BN+M77777'
NAD+RC'
LOC+POD::50::NZAKL'
GID+1'
GDS++1064 GOODS TO MEECHR'
MEA+WT+G+50:48.3778'
MEA+UN++:1'
RFF+BN+M77777'
QTY+48:0:CH'
QTY+48:1:PK'
RFF+AAE+1M033421011ZLC'
RFF+BN+L11111'
NAD+RC'
LOC+POD::50::NZAKL'
GID+1'
GDS++TEST DESCRIPTION FOR CIGARS'
MEA+WT+G+50:8.5343'
MEA+UN++:1'
RFF+BN+L11111'
QTY+48:0:CH'
QTY+48:1:PK'
UNS+S'
UNT+36+2300'";

		const string TestConsolValidEX2Response = @"UNH+7300+CUSRES:189:0:UN+2300'
BGM+CKM+C00001031+031208+11+3B033421001KSK'
ERP+H:031208195004+410'
UNT+4+7300'";

		#endregion
	}

	class Exit2ManifestStatusForTest : Exit2ManifestStatus
	{
		public Exit2ManifestStatusForTest(ForwardingConsol consol) : base(consol)
		{
		}

		new internal string ValidateEnvironmentForSendingManifests(Customs.Business.ISendsMessagesToCustoms sender) => base.ValidateEnvironmentForSendingManifests(sender);
	}
}
