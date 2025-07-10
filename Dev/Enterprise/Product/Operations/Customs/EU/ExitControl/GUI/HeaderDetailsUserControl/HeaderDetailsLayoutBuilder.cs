using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public class HeaderDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, HeaderDetailsControlBag> where T : Business.CusExitHeader
	{
		public override HeaderDetailsControlBag CommonBag => HeaderDetailsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
