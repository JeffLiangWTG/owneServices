using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class ILDLO122ResponseMessage : ILEDIResponseMessage
	{
		public ILDLO122ResponseMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.DLO;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.DeliveryOrderResponse;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILDLO122ResponseMessageDataObject(this);
	}
}
