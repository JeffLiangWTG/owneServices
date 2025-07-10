using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertNull(typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertNull(typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForEMCSDefault()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsEmcsSystem;
			message.EM_MessageType = DE.Messaging.EDIMessageTypeList.Codes.EMCS;
			AssertEquals(typeof(EmcsEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForEMCSED839C()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsEmcsSystem;
			message.EM_MessageType = DE.Messaging.EDIMessageTypeList.Codes.EMCS;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationReference = nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED839C);
			AssertEquals(typeof(EmcsInboundEDIMessage<IED839>), typeDecider.GetTypeForLoad(row, Factory));
		}

		protected override void SetUp()
		{
			base.SetUp();
			typeDecider = new EDIMessageTypeDecider();
			message = Factory.New<EDIMessage>();
			row = ((INeedRow)message).Row;
		}
		EDIMessageTypeDecider typeDecider;
		EDIMessage message;
		DataRow row;
	}
}
