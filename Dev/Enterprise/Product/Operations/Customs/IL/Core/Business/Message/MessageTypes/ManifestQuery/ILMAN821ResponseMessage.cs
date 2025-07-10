using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class ILMAN821ResponseMessage : ILEDIResponseMessage
	{
		public ILMAN821ResponseMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.MAN;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.ManifestQueryResponse;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILMAN821ResponseMessageDataObject(this);
	}
}
