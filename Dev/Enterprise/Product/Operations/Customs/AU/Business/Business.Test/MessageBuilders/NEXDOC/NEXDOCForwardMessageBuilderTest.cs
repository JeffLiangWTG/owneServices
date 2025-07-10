using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class NEXDOCForwardMessageBuilderTest : TestCaseWithFactory
	{
		public void TestCreateNewMessage()
		{
			var header = Factory.New<QuarantineExDocHeader>();
			header.QH_RequestForPermitNumber = "REX00001";
			header.QH_ForwardeeEDIUserIdentifier = "CGAA02417";
			header.QH_ForwardStatus = "CTRD";
			header.QH_ForwardRequiresAcceptance = true;

			new NEXDOCForwardMessageBuilder(header).CreateNewMessage();

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, header.PK));
			AssertEquals("Should have created 1 EDIMessage", 1, messages.Length);
			AssertEquals("EM_Status", "QUE", messages[0].EM_Status);
			AssertEquals("EM_ReceiveTransmit", "TRX", messages[0].EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationCode", "NEX", messages[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", "NEX", messages[0].EM_MessageType);
			AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, messages[0].EM_GB);

			var expectedXml = @"<NST1:RexForwardOwnership xmlns:NST2=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"" xmlns:NST3=""http://agriculture.gov.au/nexdoc/common/EnumTypes_1.0"" xmlns:NST1=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"">
  <NST1:identification>
    <NST2:rexNumber>REX00001</NST2:rexNumber>
  </NST1:identification>
  <NST1:clientGroup>CGAA02417</NST1:clientGroup>
  <NST1:requiresAcceptance>true</NST1:requiresAcceptance>
  <NST1:holdUntilStatus>CTRD</NST1:holdUntilStatus>
</NST1:RexForwardOwnership>";
			AssertXMLEquals(expectedXml, messages[0].EM_MessageText);
			AssertEquals("EM_MessageSubType", NEXDOCMessageType.Codes.REXForward, messages[0].EM_MessageSubType);
		}

		public void TestCreateNewMessageWithoutHoldUntilStatus()
		{
			var header = Factory.New<QuarantineExDocHeader>();
			header.QH_RequestForPermitNumber = "REX00001";
			header.QH_ForwardeeEDIUserIdentifier = "CGAA02417";
			header.QH_ForwardStatus = "XXXX";

			new NEXDOCForwardMessageBuilder(header).CreateNewMessage();

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, header.PK));
			AssertEquals("Should have created 1 EDIMessage", 1, messages.Length);
			AssertEquals("EM_Status", "QUE", messages[0].EM_Status);
			AssertEquals("EM_ReceiveTransmit", "TRX", messages[0].EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationCode", "NEX", messages[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", "NEX", messages[0].EM_MessageType);
			AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, messages[0].EM_GB);

			var expectedXml = @"<NST1:RexForwardOwnership xmlns:NST2=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"" xmlns:NST3=""http://agriculture.gov.au/nexdoc/common/EnumTypes_1.0"" xmlns:NST1=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"">
  <NST1:identification>
    <NST2:rexNumber>REX00001</NST2:rexNumber>
  </NST1:identification>
  <NST1:clientGroup>CGAA02417</NST1:clientGroup>
  <NST1:requiresAcceptance>false</NST1:requiresAcceptance>
</NST1:RexForwardOwnership>";
			AssertXMLEquals(expectedXml, messages[0].EM_MessageText);
		}
	}
}
