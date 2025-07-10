using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public class ReportsGridFieldsLayoutBuilder<T> : ColumnLayoutBuilder<T, ReportsGridFieldsControlBag> where T : Business.CusExitReport
	{
		public override ReportsGridFieldsControlBag CommonBag => ReportsGridFieldsControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}
