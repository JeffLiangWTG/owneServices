using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class MiscRequestRelatedEntriesLayoutBuilder : ColumnLayoutBuilder<CusMiscRequestLine, MiscRequestRelatedEntriesControlBag>
	{
		protected override int MaxColumns => 1;
		public override MiscRequestRelatedEntriesControlBag CommonBag => MiscRequestRelatedEntriesControlBag.Instance;
	}
}
