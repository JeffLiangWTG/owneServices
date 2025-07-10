using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;
using NUnit.Framework;
using EMCSPhase4_1 = CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1;

namespace Enterprise.Customs.IE.EMCS.Business.Testing.Phase4_1
{
	sealed class EMCSResponseMessageDetailsTest : TestCase
	{
		public void TestResponseDetails()
		{
			AssertEquals(11, responseMessageDetails.ResponseDetails.Count);
		}

		public void Test704()
		{
			IE.Business.Testing.TestHelper.AssertResponseDetail(responseMessageDetails.GetResponseDetail(EMCSIncomingMessageTypeList.Codes.IE704, messageText), typeof(EMCSPhase4_1.IE704.Ie704Type), typeof(IE704MessageProcessor));
		}

		public void Test801()
		{
			IE.Business.Testing.TestHelper.AssertResponseDetail(responseMessageDetails.GetResponseDetail(EMCSIncomingMessageTypeList.Codes.IE801, messageText), typeof(EMCSPhase4_1.IE801.Ie801Type), typeof(IE801MessageProcessor));
		}

		public void Test802()
		{
			IE.Business.Testing.TestHelper.AssertResponseDetail(responseMessageDetails.GetResponseDetail(EMCSIncomingMessageTypeList.Codes.IE802, messageText), typeof(EMCSPhase4_1.IE802.Ie802Type), typeof(IE802MessageProcessor));
		}

		public void Test803()
		{
			IE.Business.Testing.TestHelper.AssertResponseDetail(responseMessageDetails.GetResponseDetail(EMCSIncomingMessageTypeList.Codes.IE803, messageText), typeof(EMCSPhase4_1.IE803.Ie803Type), typeof(IE803MessageProcessor));
		}

		public void Test810()
		{
			IE.Business.Testing.TestHelper.AssertResponseDetail(responseMessageDetails.GetResponseDetail(EMCSIncomingMessageTypeList.Codes.IE810, messageText), typeof(EMCSPhase4_1.IE810.Ie810Type), typeof(IE810MessageProcessor));
		}

		public void Test813()
		{
			IE.Business.Testing.TestHelper.AssertResponseDetail(responseMessageDetails.GetResponseDetail(EMCSIncomingMessageTypeList.Codes.IE813, messageText), typeof(EMCSPhase4_1.IE813.Ie813Type), typeof(IE813MessageProcessor));
		}

		public void Test818()
		{
			IE.Business.Testing.TestHelper.AssertResponseDetail(responseMessageDetails.GetResponseDetail(EMCSIncomingMessageTypeList.Codes.IE818, messageText), typeof(EMCSPhase4_1.IE818.Ie818Type), typeof(IE818MessageProcessor));
		}

		public void Test819()
		{
			IE.Business.Testing.TestHelper.AssertResponseDetail(responseMessageDetails.GetResponseDetail(EMCSIncomingMessageTypeList.Codes.IE819, messageText), typeof(EMCSPhase4_1.IE819.Ie819Type), typeof(IE819MessageProcessor));
		}

		public void Test829()
		{
			IE.Business.Testing.TestHelper.AssertResponseDetail(responseMessageDetails.GetResponseDetail(EMCSIncomingMessageTypeList.Codes.IE829, messageText), typeof(EMCSPhase4_1.IE829.Ie829Type), typeof(IE829MessageProcessor));
		}

		public void Test839()
		{
			IE.Business.Testing.TestHelper.AssertResponseDetail(responseMessageDetails.GetResponseDetail(EMCSIncomingMessageTypeList.Codes.IE839, messageText), typeof(EMCSPhase4_1.IE839.Ie839Type), typeof(IE839MessageProcessor));
		}

		public void Test917()
		{
			IE.Business.Testing.TestHelper.AssertResponseDetail(responseMessageDetails.GetResponseDetail(EMCSIncomingMessageTypeList.Codes.IE917, messageText), typeof(EMCSPhase4_1.IE917.Ie917Type), typeof(IE917MessageProcessor));
		}

		public void TestGetResponseDetail_MessageAcknowledge()
		{
			IE.Business.Testing.TestHelper.AssertResponseDetail(responseMessageDetails.AcknowledgementResponseDetail, null, typeof(MessageAcknowledgementProcessor));
		}

		public void TestGetResponseDetail_InvalidMessageType()
		{
			AssertEquals(ResponseDetail.Empty, responseMessageDetails.GetResponseDetail("!@#"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			responseMessageDetails = new EMCSResponseMessageDetails();
		}

		EMCSResponseMessageDetails responseMessageDetails;
		readonly string messageText = "<q1:IE801 xmlns:q1=\"urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE801:V3.13\">";
	}
}
