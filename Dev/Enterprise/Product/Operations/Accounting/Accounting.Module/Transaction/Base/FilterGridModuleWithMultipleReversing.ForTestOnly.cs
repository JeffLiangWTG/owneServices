#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class FilterGridModuleWithMultipleReversing
	{
		public void DeleteMultiple_ForTestOnly(BusinessObject[] selectedBusinessObjects)
		{
			DeleteMultiple(selectedBusinessObjects);
		}

		public IZForm LastFormShownForTest_ForTestOnly
		{
			get { return LastFormShownForTest; }
			set { LastFormShownForTest = value; }
		}

		public bool IsUserAllowedForMultipleReversing_ForTestOnly()
		{
			return IsUserAllowedForMultipleReversing();
		}

		public ResourceStringData GetDeleteMenuItemText_ForTestOnly()
		{
			return GetDeleteMenuItemText();
		}

		public IBusinessObjectCollection GetNewGridCollection_ForTestOnly()
		{
			return GetNewGridCollection();
		}

		public BusinessObject CurrentBusinessObjectInGrid_ForTestOnly => CurrentBusinessObjectInGrid;

		public BusinessObjectFactory Factory_ForTesTonly => Factory;

		public MenuItem[] GetNewActionMenuItems_ForTestOnly()
		{
			return GetNewActionMenuItems();
		}

		public MenuItem[] GetNewAdditionalMenuItems_ForTestOnly()
		{
			return GetNewAdditionalMenuItems();
		}

		public ZController GetNewController_ForTestOnly(BusinessObject selectedBusinessObject)
		{
			return GetNewController(selectedBusinessObject);
		}

		public BusinessObject[] SelectedBusinessObjects_ForTestOnly => SelectedBusinessObjects;

		public IZForm ShowDeleteForm_ForTestOnly(BusinessObject selectedBusinessObject)
		{
			return ShowDeleteForm(selectedBusinessObject);
		}

		public void HandleEditClick_ForTestOnly(object sender, EventArgs e)
		{
			HandleEditClick(sender, e);
		}

		public void HandleTemplateCopyClick_ForTestOnly(object sender, EventArgs e)
		{
			HandleTemplateCopyClick(sender, e);
		}

		public void HandleDeleteClick_ForTestOnly(object sender, EventArgs e)
		{
			HandleDeleteClick(sender, e);
		}

		public FilteredGridLoader SearchManager_ForTestOnly => SearchManager;

		public ZQuery GetDisplayResultsQuery_ForTestOnly() => GetDisplayResultsQuery();

		public void HandleRegenerateJournalEntries_ForTestOnly(object sender, EventArgs e)
		{
			HandleRegenerateJournalEntries(sender, e);
		}

		public MenuItem FindMenuItemByText_ForTestOnly(string text)
		{
			var menuItem = GetNewActionMenuItems();
			return menuItem.FindByText(text);
		}
	}
}

#endif
