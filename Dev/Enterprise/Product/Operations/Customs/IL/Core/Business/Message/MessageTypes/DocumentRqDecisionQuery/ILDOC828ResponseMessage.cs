using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class ILDOC828ResponseMessage : ILEDIResponseMessage
	{
		public ILDOC828ResponseMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.DOC;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.SupportingDocumentsRqDecisionResponse;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILDOC828ResponseMessageDataObject(this);
	}
}
