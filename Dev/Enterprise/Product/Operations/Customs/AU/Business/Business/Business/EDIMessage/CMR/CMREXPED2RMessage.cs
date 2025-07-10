using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMREXPED2RMessage : CMRCUSRESMessage
	{
		public CMREXPED2RMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.EXPED2;
		}
	}
}
