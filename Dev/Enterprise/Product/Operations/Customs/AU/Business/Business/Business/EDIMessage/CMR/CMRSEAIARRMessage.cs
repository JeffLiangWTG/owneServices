using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSEAIARRMessage : CMRCUSRESMessage
	{
		public CMRSEAIARRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			ZString reference = GetReferenceFromSendersReference(SendersReference);

			ZQuery filter = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, reference);
			result = Factory.LoadTop1<CusSCAOceanBill>(filter);
			if (result == null)
			{
				result = base.GetWrappedObject();
			}

			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.SEAIAR;
		}
	}
}
