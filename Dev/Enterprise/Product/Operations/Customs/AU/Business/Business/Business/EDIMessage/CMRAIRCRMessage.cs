using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAIRCRMessage : CMRMessage
	{
		public CMRAIRCRMessage(BusinessObjectFactory factory, DataRow row)
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
			EM_MessageType = CMRMessageTypes.AIRCR;
		}

		#endregion
	}
}
