using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.Module
{
	public class TemporaryStorageModule : EU.TemporaryStorage.Module.TemporaryStorageModule
	{
		protected override IBusinessObjectCollection GetNewGridCollection() => new CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, GlbBranch.CurrentBranch);

		protected override IFilterControl GetNewFilterControl() => new TemporaryStorageFilterControl(GridCollection, FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TemporaryStorageFilterStripBusinessObject();

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = base.GetNewStandardMenuItems();

			if (NewMenuItem != null)
			{
				var istMenuitem = new ZMenuItem(ResString.GetMultilingualString("EACEEE7B-5C67-4FE9-9A21-DB58B9D16447", "New IST"), IstMenuitem_Click);
				var frcMenuitem = new ZMenuItem(ResString.GetMultilingualString("7C7A4677-C7AF-4E81-BFCD-15A47FFB0AC7", "New CIN/IST"), FrcMenuitem_Click);
				var ladtMenuitem = new ZMenuItem(ResString.GetMultilingualString("76956BFF-A957-4840-8E8A-34A7BD4F5786", "New LADT"), LadtMenuitem_Click);
				var newFromOtherMenuitem = new ZMenuItem(ResString.GetMultilingualString("752732EB-FDF3-441A-B010-39563E7B8ADF", "New IST From Other Job"), NewFromOtherMenuitem_Click);

				NewMenuItem.MenuItems.Add(istMenuitem);
				NewMenuItem.MenuItems.Add(frcMenuitem);
				NewMenuItem.MenuItems.Add(ladtMenuitem);
				NewMenuItem.MenuItems.Add(newFromOtherMenuitem);

				istMenuitem.DefaultItem = true;
			}

			return result;
		}

		void FrcMenuitem_Click(object sender, EventArgs e)
		{
			GetNewController(FRConstants.TemporaryStorage.AppCodeFRC).ShowNewForm();
		}

		void IstMenuitem_Click(object sender, EventArgs e)
		{
			GetNewController(FRConstants.TemporaryStorage.AppCodeIST).ShowNewForm();
		}

		void LadtMenuitem_Click(object sender, EventArgs e)
		{
			GetNewController(FRConstants.TemporaryStorage.AppCodeLAD).ShowNewForm();
		}

		void NewFromOtherMenuitem_Click(object sender, EventArgs e)
		{
			GetNewFromOthersTemporaryStorageController().ShowNewForm();
		}

		protected virtual ZController GetNewFromOthersTemporaryStorageController() => new NewFromOthersTemporaryStorageController();

		protected virtual ZController GetNewController(string appCode)
		{
			return new TemporaryStorageController(appCode);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var temporaryStorage = selectedBusinessObject as Business.CusTempStorage.CusTempStorageJobHeader;
			if (temporaryStorage != null && temporaryStorage.RelatedBusinessObject != null && temporaryStorage.RelatedBusinessObject is ForwardingConsol)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.FR.CINTemporaryStorageConsolController);
			}
			else
			{
				return base.GetNewController(selectedBusinessObject);
			}
		}
	}
}
