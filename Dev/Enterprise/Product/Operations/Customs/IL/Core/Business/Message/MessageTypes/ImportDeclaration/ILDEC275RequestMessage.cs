using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class ILDEC275RequestMessage : ILEDIRequestMessage
	{
		public ILDEC275RequestMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.DEC;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.ImportDeclarationRequest;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => new ILDEC275RequestMessageDataObject(this);
	}
}
