using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class InvoiceLineOtherDetailsLayoutBuilder : ColumnLayoutBuilder<JobDeclaration, InvoiceLineOtherDetailsControlBag>
	{
		protected override int MaxColumns => 1;
		public override InvoiceLineOtherDetailsControlBag CommonBag => InvoiceLineOtherDetailsControlBag.Instance;
	}
}
