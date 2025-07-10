using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEJobRelatedWayBill : JobRelatedWayBill
	{
		public UPEJobRelatedWayBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static void CreateJobRelatedWaybill(BusinessObjectFactory factory, ZGuid parentPK, string longTrackingNumber, string shortTrackingNumber, string waybillType)
		{
			JobRelatedWayBill jobRelatedWayBill = factory.New<JobRelatedWayBill>();
			jobRelatedWayBill.EB_ParentID = parentPK;
			jobRelatedWayBill.EB_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			jobRelatedWayBill.EB_WaybillNumber = longTrackingNumber;
			jobRelatedWayBill.EB_WaybillShortNumber = shortTrackingNumber;
			jobRelatedWayBill.EB_WaybillType = waybillType;
		}

		protected override JobRelatedWayBillValidation GetNewValidation()
		{
			return new UPEJobRelatedWayBillValidation(this);
		}
	}
}
