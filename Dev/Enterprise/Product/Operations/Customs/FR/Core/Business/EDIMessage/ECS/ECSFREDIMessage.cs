using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class ECSFREDIMessage : FREDIMessage
	{
		public ECSFREDIMessage(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.ECS;
		}
	}
}
