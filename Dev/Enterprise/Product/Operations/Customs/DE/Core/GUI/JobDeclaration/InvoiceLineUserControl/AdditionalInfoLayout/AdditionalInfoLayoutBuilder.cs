using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public class AdditionalInfoLayoutBuilder<T> : ColumnLayoutBuilder<T, AdditionalInfoControlBag> where T : JobDeclaration
	{
		public override AdditionalInfoControlBag CommonBag => AdditionalInfoControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
