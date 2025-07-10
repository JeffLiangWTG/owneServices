using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	class CombinationCustomisationMenuItemMenusMaker : DocumentCustomisationMenuItemMenusMaker
	{
		internal CombinationCustomisationMenuItemMenusMaker(Form parentForm, IDocumentSupportable documentSupportable, UserControlProviderList userFieldList, ZDocumentsMenuItemMenuHelper helper)
			: base(parentForm, documentSupportable, userFieldList, helper)
		{
		}

		protected override void AddCustomiseMenuItem(List<MenuItem> menuItems)
		{
			base.AddCustomiseMenuItem(menuItems);
			menuItems.Add(MenuItemHelper.GetNewMenuItem("CustomizeFroms", CustomizeFromsMenuText, new EventHandler(OnCustomiseFromsMenuClick))); // Menu item name, not menu item text, should not be localized
		}

		void OnCustomiseFromsMenuClick(object sender, EventArgs e)
		{
			var bizObj = documentSupportable.DocumentSupporter?.BusinessObject;
			if (bizObj == null)
			{
				return;
			}

			var formCreator = ObjectFactory.Get<IVisualizerMenuCustomisationFormCreator>();
			var checkPoint = formCreator.GetCustomizeFormCheckpoint(bizObj);

			ShowCustomisationForm(checkPoint, () => formCreator.CreateVisualizerMenuCustomisationForm(bizObj));
		}

		protected override MultilingualString CustomizeMenuText => ResString.GetMultilingualString("316df62f-1420-482b-ac63-f05fa91f05e6", "Customize (Documents)");
		MultilingualString CustomizeFromsMenuText => ResString.GetMultilingualString("2f14009b-f785-45a7-ac06-af616ada5399", "Customize (Forms)");
	}
}
