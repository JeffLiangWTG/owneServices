using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class CommonEntryDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonEntryDetailsControlBag> where T : JobDeclaration
	{
		public override CommonEntryDetailsControlBag CommonBag => CommonEntryDetailsControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
