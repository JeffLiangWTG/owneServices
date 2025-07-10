using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCLREGRMessage : BaseCLREGResponseMessage
	{
		public CMRCLREGRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessage.CMRMessageTypes.CLREG;
		}
	}
}
