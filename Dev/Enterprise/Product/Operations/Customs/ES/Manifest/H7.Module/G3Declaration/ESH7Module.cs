using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ES.Manifest.H7.Module
{
	public class ESH7Module : EU.H7.Module.EUH7Module
	{
		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem(ZMenuItem.Separator));
			result.Add(new ZMenuItem(ViewG3DeclarationDescription, ViewG3DeclarationClick));

			return result.ToArray();
		}

		MultilingualString ViewG3DeclarationDescription => ResString.GetMultilingualString("653201d1-6c95-4503-955d-25a56127cf28", "View G3 Declarations");

		void ViewG3DeclarationClick(object sender, EventArgs e)
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.EU.ES.G3Declaration))
			using (var popup = new EmbeddedModulePopup(module))
			{
				popup.RequireAtLeastOneItemToBeSelected = false;
				ZFormModaliser.ShowDialogAndDispose(popup);
			}
		}
	}
}
