using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class ILGEN910RequestMessage : ILEDIRequestMessage
	{
		public ILGEN910RequestMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = ILMessageTypeList.Codes.GEN;
			EM_MessageSubType = ILEDIMessageSubTypeList.Codes.SyncOutgoingMessageRequest;
		}
	}
}
