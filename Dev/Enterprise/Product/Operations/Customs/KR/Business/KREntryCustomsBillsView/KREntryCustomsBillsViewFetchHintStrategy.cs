using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class KREntryCustomsBillsViewFetchHintStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public KREntryCustomsBillsViewFetchHintStrategy(KREntryCustomsBillsView view)
			: base(view)
		{
		}

		new KREntryCustomsBillsView BusinessObject
		{
			get { return (KREntryCustomsBillsView)base.BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.KEB_OH_Importer);
		}
	}
}
