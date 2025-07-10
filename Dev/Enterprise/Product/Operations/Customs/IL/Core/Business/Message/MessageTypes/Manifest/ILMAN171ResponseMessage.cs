using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class ILMAN171ResponseMessage : ILEDIResponseMessage
	{
		public ILMAN171ResponseMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.MAN;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.ForwarderManifestResponse;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILMAN171ResponseMessageDataObject(this);
	}
}
