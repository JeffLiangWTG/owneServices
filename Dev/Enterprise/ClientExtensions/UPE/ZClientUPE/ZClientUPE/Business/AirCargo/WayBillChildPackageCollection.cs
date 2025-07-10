using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class WayBillChildPackageCollection : JobRelatedWayBillCollection
	{
		public WayBillChildPackageCollection(UPECusHAWB houseBill)
			: base(houseBill.Factory)
		{
			this.HouseBill = houseBill;
		}

		public new UPEJobRelatedWayBill this[int index]
		{
			get { return (UPEJobRelatedWayBill)Elements[index]; }
		}

		public new UPEJobRelatedWayBill AddNew()
		{
			return (UPEJobRelatedWayBill)base.AddNew();
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(JobRelatedWayBillSchema.EB_ParentID, HouseBill.PK);
			result.AddToFilter(JobRelatedWayBillSchema.EB_ParentTableCode, HouseBill.TablePrefix);
			result.AddToFilter(JobRelatedWayBillSchema.EB_WaybillType, JobRelatedWayBill.Constants.RelatedWayBillType.Child);
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			JobRelatedWayBill wayBill = child as JobRelatedWayBill;
			wayBill.EB_ParentID = HouseBill.PK;
			wayBill.EB_ParentTableCode = HouseBill.TablePrefix;
			wayBill.EB_WaybillType = JobRelatedWayBill.Constants.RelatedWayBillType.Child;
		}

		readonly UPECusHAWB HouseBill;
	}
}
