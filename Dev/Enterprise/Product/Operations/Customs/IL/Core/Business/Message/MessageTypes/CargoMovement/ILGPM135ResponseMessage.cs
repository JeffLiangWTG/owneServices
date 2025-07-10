using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class ILGPM135ResponseMessage : ILEDIResponseMessage
	{
		public ILGPM135ResponseMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.GPM;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.GatepassMovementResponse;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILGPM135ResponseMessageDataObject(this);
	}
}
