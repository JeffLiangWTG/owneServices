using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSEQRMessage : CMRCUSRESMessage
	{
		public CMRSEQRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.SEQ;
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			ZString reference = GetReferenceFromSendersReference(SendersReference);
			result = CusOutturnHeader.LoadFromSendersReference(Factory, reference);
			if (result == null)
			{
				result = base.GetWrappedObject();
			}

			return result;
		}
	}
}
