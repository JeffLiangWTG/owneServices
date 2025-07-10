using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class NEXDOCWithdrawMessageBuilderTest : TestCaseWithFactory
	{
		public void TestCreateNewMessage()
		{
			var header = Factory.New<QuarantineExDocHeader>();
			header.QH_RequestForPermitNumber = "REX00001";

			new NEXDOCWithdrawMessageBuilder(header).CreateNewMessage();

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, header.PK));
			AssertEquals("Should have created 1 EDIMessage", 1, messages.Length);
			AssertEquals("EM_Status", "QUE", messages[0].EM_Status);
			AssertEquals("EM_ReceiveTransmit", "TRX", messages[0].EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationCode", "NEX", messages[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", "NEX", messages[0].EM_MessageType);
			AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, messages[0].EM_GB);

			var expectedXml = @"<NST1:RexWithdrawOwnership xmlns:NST2=""http://agriculture.gov.au/nexdoc/common/CommonTypes_1.0"" xmlns:NST1=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"">
  <NST1:identification>
    <NST2:rexNumber>REX00001</NST2:rexNumber>
  </NST1:identification>
</NST1:RexWithdrawOwnership>";
			AssertXMLEquals(expectedXml, messages[0].EM_MessageText);
			AssertEquals("EM_MessageSubType", NEXDOCMessageType.Codes.WithdrawalOwnership, messages[0].EM_MessageSubType);
		}
	}
}
