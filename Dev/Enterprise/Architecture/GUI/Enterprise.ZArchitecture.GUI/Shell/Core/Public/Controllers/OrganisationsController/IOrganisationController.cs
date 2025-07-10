
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IOrganisationController
	{
		IZForm ShowForm(BusinessObject bizObj, OrganisationTabPageType tabPageNameToShow, FormAction formDisplayMode);
	}
}
