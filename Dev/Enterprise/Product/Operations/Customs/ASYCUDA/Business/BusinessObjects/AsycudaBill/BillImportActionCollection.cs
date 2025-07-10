using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class BillImportActionCollection : SailingBillImportActionCollection<BillImportAction>
	{
		public BillImportActionCollection(IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> billCollection)
			: base(billCollection.Factory)
		{
			foreach (var bill in billCollection.Cast<AsycudaBill>())
			{
				base.Add(new BillImportAction(bill));
			}
		}
	}
}
