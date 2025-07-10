using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public class HeaderDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, HeaderDetailsControlBag> where T : CusReconDeclaration
	{
		public override HeaderDetailsControlBag CommonBag => HeaderDetailsControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}
