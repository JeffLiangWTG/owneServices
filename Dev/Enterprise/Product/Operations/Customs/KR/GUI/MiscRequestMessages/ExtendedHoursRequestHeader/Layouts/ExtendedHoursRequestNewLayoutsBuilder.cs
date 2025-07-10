using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ExtendedHoursRequestNewLayoutsBuilder : ColumnLayoutBuilder<ExtendedHoursRequestHeader, ExtendedHoursRequestNewControlBag>
	{
		public override ExtendedHoursRequestNewControlBag CommonBag => ExtendedHoursRequestNewControlBag.Instance;

		protected override int MaxColumns => 2;
	}
}
