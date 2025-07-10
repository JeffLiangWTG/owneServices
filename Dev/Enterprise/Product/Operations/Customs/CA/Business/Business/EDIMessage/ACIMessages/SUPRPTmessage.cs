using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class SUPRPTMessage : ACIEDIMessage
	{
		public SUPRPTMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
		}

		#endregion
	}
}
