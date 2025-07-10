#if DEBUG

using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class MatchingModule
	{
		public void LoadCollection_ForTestOnly(IBusinessObjectCollection collection, ZQuery query)
		{
			var result = LoadCollection(new BusinessObjectFactory(), null, query);
			PushItemsIntoCollectionCore(collection, result, DefaultSortOrder);
		}

		public ResourceStringData GetDeleteMenuItemText_ForTestOnly()
		{
			return GetDeleteMenuItemText();
		}

		public void UpdateMatchedTransactionGridInFilterControl_ForTestOnly()
		{
			UpdateMatchedTransactionGridInFilterControl();
		}

		public ZController GetNewController_ForTestOnly(BusinessObject selectedBusinessObject) => GetNewController(selectedBusinessObject);

		public void HandleEditClick_ForTestOnly(object sender, EventArgs e)
		{
			HandleEditClick(sender, e);
		}
	}
}

#endif
