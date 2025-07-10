using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public class TransactionDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, TransactionDetailsControlBag>
		where T : CusIntrastatHeader
	{
		public override TransactionDetailsControlBag CommonBag => TransactionDetailsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
