using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class OrderSelectionPrompter
	{
		public OrderSelectionPrompter(NctsOrderInventorySelectionHeader bizObj)
		{
			BusinessEntity = Argument.NotNull(bizObj,  nameof(bizObj));
		}

		public NctsOrderInventorySelectionHeader BusinessEntity { get; }

		public void Prompt()
		{
			var factory = BusinessEntity.Factory;
			var order = PromptForOrderSelection(factory);

			if (order != null)
			{
				BusinessEntity.ImportInventories(order);

				if (!BusinessEntity.ImportInventoriesResult.IsEmpty)
				{
					Globals.Message.ShowError(BusinessEntity.ImportInventoriesResult);
				}
			}
		}

		IWhsOrder PromptForOrderSelection(BusinessObjectFactory factory)
		{
			var collection = BusinessEntity.GetCollectionForWhsOrderSelection(factory);

			var emptyFilter = new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(StmModuleFilter) }.New<StmModuleFilter>();
			var order = (IWhsOrder)BusinessObjectModulePicker.PickOneRecordFromModuleScreen<BusinessObject>(collection, ModuleIDs.WhsOrder, emptyFilter, shouldLoadLayoutEvenWhenUnsaved: true, okButtonCaption: Res.GetData("5bec79ab-d627-4282-8861-4ac5ee313cc6", "Select").Caption);

			return order;
		}
	}
}
