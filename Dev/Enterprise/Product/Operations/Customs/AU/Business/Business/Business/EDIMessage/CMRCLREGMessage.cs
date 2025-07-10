using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCLREGMessage : CMRMessage
	{
		public CMRCLREGMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.CLREG;
		}

		protected override string GetSendersReference()
		{
			OrgHeader orgHeader = EM_LinkedObject as OrgHeader;
			if (orgHeader != null)
			{
				return orgHeader.OH_Code;
			}

			return base.GetSendersReference();
		}

		#endregion
	}
}
