using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.FetchStrategies
{
	public class InvoiceLineViewCollectionFetchStrategy : Customs.Business.FetchStrategies.InvoiceLineViewCollectionFetchStrategy
	{
		public InvoiceLineViewCollectionFetchStrategy(IInvoiceLineViewCollection<JobComInvoiceLine> collection)
			: base(collection)
		{
		}

		protected new IInvoiceLineViewCollection<JobComInvoiceLine> Collection
		{
			get { return (IInvoiceLineViewCollection<JobComInvoiceLine>)base.Collection; }
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);
			var hasJI_SupplementaryCode1Column = columns.Any(x => x.ColumnName == JobComInvoiceLine.Schema.JI_SupplementaryCode1);
			var hasJI_SupplementaryCode2Column = columns.Any(x => x.ColumnName == JobComInvoiceLine.Schema.JI_SupplementaryCode2);
			if (hasJI_SupplementaryCode1Column || hasJI_SupplementaryCode2Column)
			{
				var factory = Collection.Factory;
				var loader = new BaseSupplementaryCode.Loader(factory);

				foreach (var pk in businessObjects.Where(x => x.IsInDatabase).Select(x => x.PK))
				{
					factory.AddFetchHint(CusCodeDataSchema.Instance, loader.GetZQuery(pk, CusCodeDataTypeList.Codes.SupplementaryCode));
				}
			}
		}
	}
}
