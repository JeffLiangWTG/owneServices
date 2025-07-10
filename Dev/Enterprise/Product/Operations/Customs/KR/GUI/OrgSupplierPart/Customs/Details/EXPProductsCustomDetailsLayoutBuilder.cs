using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class EXPProductsCustomDetailsLayoutBuilder : ColumnLayoutBuilder<OrgSupplierPart, EXPProductsCustomDetailsControlBag>
	{
		public override EXPProductsCustomDetailsControlBag CommonBag => EXPProductsCustomDetailsControlBag.Instance;
	}
}
