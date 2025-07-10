using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRUBMREQMessage : CMRMessage
	{
		public CMRUBMREQMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool AssumeMessageClearIfAcknowledgedAndNoResponse
		{
			get
			{
				return true;
			}
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.UBMREQ;
		}

		#endregion
	}
}
