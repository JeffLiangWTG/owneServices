using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAIROUTMessage : CMRMessage
	{
		public CMRAIROUTMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.AIROUT;
		}

		#endregion
	}
}
