using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class SpecialProceduresLayoutBuilder<T> : ColumnLayoutBuilder<T, SpecialProceduresControlBag> where T : JobDeclaration
	{
		public override SpecialProceduresControlBag CommonBag => SpecialProceduresControlBag.Instance;

		protected override int MaxColumns => 3;
	}
}
