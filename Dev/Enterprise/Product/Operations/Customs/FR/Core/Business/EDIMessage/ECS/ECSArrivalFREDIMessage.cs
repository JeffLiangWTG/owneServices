using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class ECSArrivalFREDIMessage : ECSFREDIMessage
	{
		public ECSArrivalFREDIMessage(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageSubType = Business.MessageSubTypeList.Codes.ARR;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}
	}
}
