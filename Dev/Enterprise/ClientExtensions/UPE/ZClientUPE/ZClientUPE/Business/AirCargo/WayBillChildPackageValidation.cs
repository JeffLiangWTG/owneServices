using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEJobRelatedWayBillValidation : JobRelatedWayBillValidation
	{
		public UPEJobRelatedWayBillValidation(UPEJobRelatedWayBill parent)
			: base(parent)
		{
		}

		protected override void CheckEB_WaybillNumber()
		{
			base.CheckEB_WaybillNumber();
			if (Parent.EB_WaybillType == JobRelatedWayBill.Constants.RelatedWayBillType.Child)
			{
				MandatoryValidation.CheckEntered(Parent.EB_WaybillNumberInfo);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.EB_WaybillNumberInfo, Parent.Factory.Load<JobRelatedWayBill>(new ZQuery(JobRelatedWayBillSchema.EB_ParentID, Parent.EB_ParentID)));
			}
		}
	}
}
