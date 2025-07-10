using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Internal
{
	/// <summary>
	/// Column layout settings are stored either in StmData.SD_BinaryValue or in StmModuleFilter.S9_ColumnLayout
	/// 
	/// For module grids, 
	///		if StmModule.S9_SaveColumnData is true, then it is stored in StmModuleFilter.S9_ColumnLayout.
	///		else no module filter layout is selected or StmModule.S9_SaveColumnData is false, then it is stored in StmData.SD_BinaryValue
	///		
	///	For normal grids,
	///		if users have selected a pre-configured column layout, then it is stored in StmModuleFilter.S9_ColumnLayout
	///		else no pre-configured column layout is selected, then it is stored in StmData.SD_BinaryValue
	/// </summary>
	public interface IGridLayoutStorage
	{
		ZGuid PK { get; }
		string GridLayoutKey { get; }
		string ColumnLayoutName { get; set; }
		string ColumnLayoutDisplayName { get; }
		byte[] ColumnLayoutData { get; }
		bool IsRenameAllowed { get; }
		bool SaveColumnLayout { get; set; }
		bool SaveGridColourLayout { get; set; }
		ZGuid GridColourLayoutID { get; set; }
		bool IsSystemDefined { get; }
		bool IsPublished { get; }
		bool IsPublishedAcrossAllCompanies { get; }
		bool IsDeleted { get; }
		bool IsDeleteAllowed { get; }
		void Delete();
	}

	public enum SaveColumnLayout
	{
		Yes,
		No,
		Ignore
	}

	public enum SaveGridColourLayout
	{
		Yes,
		No,
		Ignore
	}
}
