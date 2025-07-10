using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class AdditionalInfoDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, AdditionalInfoDetailsControlBag> where T : AdditionalInfo
	{
		public override AdditionalInfoDetailsControlBag CommonBag => AdditionalInfoDetailsControlBag.Instance;
	}
}
