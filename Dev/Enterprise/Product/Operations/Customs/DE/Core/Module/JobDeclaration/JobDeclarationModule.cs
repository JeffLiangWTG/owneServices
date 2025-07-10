using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI;
using Enterprise.Customs.DE.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.Module
{
	[UniversalCopyInstanceType(InstanceType = typeof(JobDeclaration))]
	public class JobDeclarationModule : EU.Module.JobDeclarationModule
	{
		protected override Customs.Module.JobDeclarationController GetControllerForStandAlone() => new JobDeclarationController();

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = base.GetNewActionMenuItems().ToList();
			var indexToAdd = menuItems.IndexOf(createFromShipmentMenuItem) + 1;
			if (DECustomsDataRegistry.Instance.BondedWarehouseCreateDeclarationFromInventory.Value)
			{
				menuItems.Insert(indexToAdd, new ZMenuItem(Res.GetString("78F09902-5187-4930-AB15-12FDA11EDF0D", "Create Declaration from Inventory"), CreateDeclarationFromInventory_Click));
			}
			if (CustomsDataRegistry.Instance.EnableInwardProcessing.Value)
			{
				menuItems.Insert(++indexToAdd, new ZMenuItem(Res.GetString("CE8200C0-77EF-42E4-9D09-07C0CDA8141A", "Create Declaration from Inventory - IPR"), CreateDeclarationFromInventoryIPR_Click));
			}
			if (DECustomsDataRegistry.Instance.BondedWarehouseCreateDeclarationFromWarehouseOrder.Value)
			{
				menuItems.Insert(++indexToAdd, new ZMenuItem(Res.GetString("6E069B4D-3655-497D-9C91-213406177D79", "Create Declaration from Warehouse Order"), CreateDeclarationFromWarehouseOrder_Click));
			}
			return menuItems.ToArray();
		}

		void CreateDeclarationFromInventory_Click(object sender, EventArgs e)
		{
			var createDeclarationBizObj = new CreateDeclarationBizObj(createFromWarehouseOrder: false);
			using (var createDeclarationForm = new CreateDeclarationForm(createDeclarationBizObj))
			{
				ZFormModaliser.ShowDialogWithoutDispose(createDeclarationForm);
			}
		}

		void CreateDeclarationFromInventoryIPR_Click(object sender, EventArgs e)
		{
			var createDeclarationIPR = new CreateDeclarationIPR();
			using (var createDeclarationForm = new CreateDeclarationForm(createDeclarationIPR))
			{
				ZFormModaliser.ShowDialogWithoutDispose(createDeclarationForm);
			}
		}

		void CreateDeclarationFromWarehouseOrder_Click(object sender, EventArgs e)
		{
			var createDeclarationBizObj = new CreateDeclarationBizObj(createFromWarehouseOrder: true);
			using (var createDeclarationForm = new CreateDeclarationForm(createDeclarationBizObj))
			{
				ZFormModaliser.ShowDialogWithoutDispose(createDeclarationForm);
			}
		}
	}
}
