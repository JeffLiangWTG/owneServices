using Enterprise.Customs.GUI;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public class OrganizationsLayoutBuilder<T> : ColumnLayoutBuilder<T, CommonOrganisationsControlBag> where T : JobDeclaration
	{
		public override CommonOrganisationsControlBag CommonBag => CommonOrganisationsControlBag.Instance;
	}
}
