using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IFilterModuleInternalsForTesting
	{
#if DEBUG
		IZForm ShowNewForm();
		IZForm ShowViewForm(BusinessObject selectedBusinessObject);
		IZForm ShowEditForm(BusinessObject selectedBusinessObject);
		void PerformSearch();
		ZController LastController { get; set; }
		FilterBusinessObject FilterBusinessObject { get; }
		IBusinessObjectCollection GridCollection { get; }
		FilterModuleMenuItemDescriptorCollection ImportMenuItems { get; }
#endif
	}

	public interface IFilterGridModuleInternalsForTesting : IFilterModuleInternalsForTesting
	{
#if DEBUG
		ZDisplayGrid Grid { get; }
		ZQuery GetDisplayResultsQuery();
		void ClearGridBlobFields();
#endif
	}
}
