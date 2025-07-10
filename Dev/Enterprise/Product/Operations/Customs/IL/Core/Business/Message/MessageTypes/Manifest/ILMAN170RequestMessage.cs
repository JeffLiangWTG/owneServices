using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class ILMAN170RequestMessage : ILEDIRequestMessage
	{
		public ILMAN170RequestMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.MAN;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.ForwarderManifestRequest;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILMAN170RequestMessageDataObject(this);
	}
}
