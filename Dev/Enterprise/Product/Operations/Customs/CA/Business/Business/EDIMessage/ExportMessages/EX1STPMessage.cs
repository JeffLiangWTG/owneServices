using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class EX1STPMessage : EXPEDIMessage
	{
		public EX1STPMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.G7Export;
		}

		#endregion
	}
}
