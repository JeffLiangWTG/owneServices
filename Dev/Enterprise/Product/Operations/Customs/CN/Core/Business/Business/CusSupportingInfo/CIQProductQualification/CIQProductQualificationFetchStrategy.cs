using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class CIQProductQualificationFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CIQProductQualificationFetchStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
		}
	}
}
