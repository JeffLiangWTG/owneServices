using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Internal
{
	/// <summary>
	/// FilterStripBusinessObject or object that takes care of ZGrid layout configurations.
	/// 
	/// This interface is to reuse Manage and Save buttons that appear on a filter in grid layout configurations. 
	/// </summary>
	public interface IModifyModuleAndGridLayout
	{
		string ValidateAndGetErrorsForSavingLayout();

		string GetReasonLayoutNameNotAllowed(string layoutName, bool isPublished);

		/// <summary>
		/// Factory to save a new layout or change existing layouts.
		/// </summary>
		BusinessObjectFactory Factory { get; }

		/// <summary>
		/// When a layout is to be saved, the name is to be defaulted with a current layout name.
		/// </summary>
		IGridLayoutStorage LayoutToDefault { get; }

		/// <summary>
		/// When a new layout is to be saved
		/// </summary>
		StmModuleFilter AddNewLayoutStorage();
		IGridLayoutStorage FindLayout(string layoutName, bool isPublished, ZGuid? gcPk);

		IGridLayoutStorage FindLayout(string layoutName, bool isPublished);

		IGridLayoutStorage FindLayout(string layoutName);

		IGridLayoutStorage FindLayout(ZGuid layoutPK);

		/// <summary>
		/// It is matched against StmModuleFilter.S9_ModuleID. It is either module id or grid context key
		/// </summary>
		string LayoutSetIdentifierToSaveANewLayoutWith { get; }

		/// <summary>
		/// To retrieve all matching StmModuleFilter records including a superset
		/// 
		/// If LayoutContextPK is set in a grid, a set of layouts without LayoutContextPK should be available as well
		/// </summary>
		string[] LayoutSetIdentifiers { get; }

		void SerialiseLayoutAndWriteTo(StmModuleFilter layoutStorage);

		IEnumerable<StmModuleFilter> GetLayouts(bool isPublished);

		IGridLayoutStorage GetDefaultGridLayout();

		IEnumerable<ILayoutDetailTreeNode> GetLayoutDetailTree(StmModuleFilter layout);
	}
}
