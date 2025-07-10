using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.FR.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.Module
{
	public class UCC6TemporaryStorageModule : EU.TemporaryStorage.Module.UCC6TemporaryStorageModule
	{
		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = base.GetNewStandardMenuItems();

			if (NewMenuItem != null)
			{
				var istMenuitem = new ZMenuItem(ResString.GetMultilingualString("A5D85172-F97B-4C45-835F-6C538385585F", "New IST"), IstMenuitem_Click);
				var ladtMenuitem = new ZMenuItem(ResString.GetMultilingualString("B0453A74-A514-4781-A591-9ADFF085DB91", "New LADT"), LadtMenuitem_Click);

				NewMenuItem.MenuItems.Add(istMenuitem);
				NewMenuItem.MenuItems.Add(ladtMenuitem);

				istMenuitem.DefaultItem = true;
			}

			return result;
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			if (PntsInboundInterchangeImporter.IsMenuItemVisible())
			{
				result.Add(PntsInboundInterchangeImporter.GetNewMenuItem());
			}

			return result.ToArray();
		}

		protected override IFilterControl GetNewFilterControl() => new UCC6TemporaryStorageFilterControl(GridCollection, FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new UCC6TemporaryStorageFilterStripBusinessObject();

		protected virtual ZController GetNewController(string appCode)
		{
			return new UCC6TemporaryStorageController(appCode);
		}

		void IstMenuitem_Click(object sender, EventArgs e)
		{
			GetNewController(FRConstants.TemporaryStorage.AppCodeIST).ShowNewForm();
		}

		void LadtMenuitem_Click(object sender, EventArgs e)
		{
			GetNewController(FRConstants.TemporaryStorage.AppCodeLAD).ShowNewForm();
		}
		PntsInboundInterchangeImporter PntsInboundInterchangeImporter => pntsInboundInterchangeImporter ?? (pntsInboundInterchangeImporter = new PntsInboundInterchangeImporter(Factory));
		PntsInboundInterchangeImporter pntsInboundInterchangeImporter;
	}
}
