using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class NCTSMessage : BEMessage
	{
		public NCTSMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = SendMessageTypes.Codes.NCT;
		}
	}
}
