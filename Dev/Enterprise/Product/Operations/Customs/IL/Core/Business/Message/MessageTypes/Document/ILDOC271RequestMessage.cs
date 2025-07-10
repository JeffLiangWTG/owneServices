using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class ILDOC271RequestMessage : ILEDIRequestMessage
	{
		public ILDOC271RequestMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.DOC;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.SupportingDocumentsRequest;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILDOC271RequestMessageDataObject(this);
	}
}
