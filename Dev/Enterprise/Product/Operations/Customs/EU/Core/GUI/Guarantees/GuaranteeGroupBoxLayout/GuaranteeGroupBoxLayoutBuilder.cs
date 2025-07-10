using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class GuaranteeGroupBoxLayoutBuilder<T> : ColumnLayoutBuilder<T, GuaranteeGroupBoxControlBag> where T : CommonGuarantee
	{
		public override GuaranteeGroupBoxControlBag CommonBag => GuaranteeGroupBoxControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
