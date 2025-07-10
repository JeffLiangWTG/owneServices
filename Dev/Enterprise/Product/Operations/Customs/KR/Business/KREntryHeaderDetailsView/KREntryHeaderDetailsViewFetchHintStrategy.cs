using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class KREntryHeaderDetailsViewFetchHintStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public KREntryHeaderDetailsViewFetchHintStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
		}

		new KREntryHeaderDetailsView BusinessObject
		{
			get { return (KREntryHeaderDetailsView)base.BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(OrgAddressSchema.PK, BusinessObject.KEH_OA_SupplierAddress);
		}
	}
}
