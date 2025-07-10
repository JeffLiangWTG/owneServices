using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSEAAARRMessage : CMRCUSRESMessage
	{
		public CMRSEAAARRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			ZString reference = GetReferenceFromSendersReference(SendersReference);

			ZQuery filter = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, reference);
			filter.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, CusSCAOceanBill.ApplicationCodes);
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
			EM_MessageType = CMRMessageTypes.SEAAAR;
		}
	}
}
