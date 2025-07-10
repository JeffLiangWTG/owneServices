using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAIROUTRMessage : CMRCUSRESMessage
	{
		public CMRAIROUTRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			ZString reference = GetReferenceFromSendersReference(SendersReference);

			result = CusHAWBBase.Load(Factory, reference);
			if (result == null)
			{
				result = CusUnderbond.LoadFromSendersReference(Factory, reference);
			}

			if (result == null)
			{
				result = base.GetWrappedObject();
			}

			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.AIROUT;
		}
	}
}
