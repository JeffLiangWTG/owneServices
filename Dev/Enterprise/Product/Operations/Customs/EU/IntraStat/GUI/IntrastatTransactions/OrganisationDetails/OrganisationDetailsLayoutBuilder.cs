using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public class OrganisationDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, OrganisationDetailsControlBag>
		where T : CusIntrastatHeader
	{
		public override OrganisationDetailsControlBag CommonBag => OrganisationDetailsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
