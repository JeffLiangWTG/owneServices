using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public class DeclarationOrganizationsLayoutBuilder<T> : ColumnLayoutBuilder<T, DeclarationOrganizationsControlBag> where T : Business.EMCSJobDeclaration
	{
		public override DeclarationOrganizationsControlBag CommonBag => DeclarationOrganizationsControlBag.Instance;

		protected override int MaxColumns => 1;
	}
}
