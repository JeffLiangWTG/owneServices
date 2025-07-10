using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public class TransactionLineDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, TransactionLineDetailsControlBag>
		where T : CusIntrastatLine
	{
		public override TransactionLineDetailsControlBag CommonBag => TransactionLineDetailsControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
