using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class ILDEC274ResponseMessage : ILEDIResponseMessage
	{
		public ILDEC274ResponseMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.DEC;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.ImportDeclarationResponse;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILDEC274ResponseMessageDataObject(this);
	}
}
