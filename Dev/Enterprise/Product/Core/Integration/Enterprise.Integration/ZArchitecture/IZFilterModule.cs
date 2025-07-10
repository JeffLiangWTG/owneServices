using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public interface IZFilterModule
	{
		bool HasImportMenuItems { get; }
		bool HasExportMenuItems { get; }
		bool DoNotCheckOrSaveChanges { get; set; }
		void CommitAllFilters();
		IBusinessObjectCollection GetNewBusinessObjectCollection();
		IBusinessObjectCollection GridCollection { get; }
	}
}
