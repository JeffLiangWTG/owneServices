using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class SupplementaryCodeCollection : BaseSupplementaryCodeCollection<SupplementaryCode>
	{
		protected SupplementaryCodeCollection(ZPropertyInfo info, short size, short startOrder = 1)
			: base(info, size, startOrder)
		{
		}

		public new static SupplementaryCodeCollection New(ZPropertyInfo info)
		{
			var master = info.BizObj as ISupplementaryCodeSupporter;
			var provider = BaseSupplementaryCodeProvider.GetBySupplementaryCodeSupporter(master);
			var supplementaryCodeCollection = new SupplementaryCodeCollection(info, provider.NumberOfCodes, provider.CodesStartingOrder);
			supplementaryCodeCollection.Load();
			return supplementaryCodeCollection;
		}
	}
}
