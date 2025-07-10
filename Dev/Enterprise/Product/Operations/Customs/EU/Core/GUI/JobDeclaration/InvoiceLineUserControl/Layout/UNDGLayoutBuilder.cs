using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class UNDGLayoutBuilder<TDeclaration> : ColumnLayoutBuilder<TDeclaration, UNDGUserControlBag> where TDeclaration : Business.Declaration.JobDeclaration
	{
		public override UNDGUserControlBag CommonBag => UNDGUserControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
