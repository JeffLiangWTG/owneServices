using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class BillImportActionCollection : SailingBillImportActionCollection<BillImportAction>
	{
		public BillImportActionCollection(JPAFRBillsCollection billCollection)
			: base(billCollection.Factory)
		{
			foreach (var bill in billCollection)
			{
				Add(new BillImportAction(bill));
			}
		}
	}
}
