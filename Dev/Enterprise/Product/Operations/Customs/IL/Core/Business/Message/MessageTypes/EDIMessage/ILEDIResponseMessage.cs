using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IL.Business
{
	public class ILEDIResponseMessage : ILEDIMessage
	{
		public ILEDIResponseMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}
	}
}
