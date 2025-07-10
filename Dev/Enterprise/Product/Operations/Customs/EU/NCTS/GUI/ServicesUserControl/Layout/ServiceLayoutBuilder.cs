using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class ServiceLayoutBuilder<T> : ColumnLayoutBuilder<T, ServiceControlBag> where T : NctsHeader
	{
		public override ServiceControlBag CommonBag => ServiceControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
