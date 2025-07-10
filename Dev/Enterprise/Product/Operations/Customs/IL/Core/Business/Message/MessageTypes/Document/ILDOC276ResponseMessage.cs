using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class ILDOC276ResponseMessage : ILEDIResponseMessage
	{
		public ILDOC276ResponseMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.DOC;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.SupportingDocumentsResponse;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILDOC276ResponseMessageDataObject(this);
	}
}
