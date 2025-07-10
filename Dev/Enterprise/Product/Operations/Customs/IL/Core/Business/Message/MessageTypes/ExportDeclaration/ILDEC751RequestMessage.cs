using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class ILDEC751RequestMessage : ILEDIRequestMessage
	{
		public ILDEC751RequestMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.DEC;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.ExportDeclarationRequest;
		}

		protected override MessageDataObjectBase GenerateMessageDataObject() => null;
	}
}
