using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.GUI.Riba;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class AccCollectionOrderModule : ZFilterGridModule
	{
		public AccCollectionOrderModule()
		{
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> menus = new List<MenuItem>(base.GetNewAdditionalMenuItems());

			menus.Insert(0, new ZMenuItem(ResString.GetMultilingualString("C31DF4BE-010A-42A3-9160-30E5C310CC5A", "Print"), new EventHandler(HandlePrint)));

			return menus.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItems.Add(new ZMenuItem(AccountingConstants.AccCollectionOrderMenuNames.RejectOrderMenuItemName, new EventHandler(HandleRejectOrder)));
			menuItems.Add(new ZMenuItem(AccountingConstants.AccCollectionOrderMenuNames.CreateReceiptMenuItemName, new EventHandler(HandleCreateReceiptsAndIndividualDepositBatch)));
			return menuItems.ToArray();
		}

		public override bool AllowDelete => false;

		protected void HandlePrint(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects.Length == 0)
			{
				ShowNoSelectedMessage();
				return;
			}

			var collectionOrders = SelectedBusinessObjects.Cast<AccCollectionOrder>();
			CollectionOrderBatchHelper.PrintCollectionOrders(collectionOrders.ToArray(), Env.Security.CollectionOrderPrint);
		}

		protected void HandleRejectOrder(object sender, EventArgs e)
		{
			var selectedOrders = Grid.SelectedElements;
			if (selectedOrders == null || selectedOrders.Length != 1)
			{
				Globals.Message.ShowError(CollectionOrderBatchHelper.SelectOrderFirstErrorMessage);
				return;
			}
			var order = LoadSelectedOrderInNewFactory(selectedOrders);
			if (order != null)
			{
				CollectionOrderBatchHelper.RejectOrder(order.First(), Env.Security.CollectionOrderReject, doSave: true);
			}
		}

		protected void HandleCreateReceiptsAndIndividualDepositBatch(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects.Length == 0)
			{
				ShowNoSelectedMessage();
				return;
			}

			var selectedOrders = Grid.SelectedElements;
			var orders = LoadSelectedOrderInNewFactory(selectedOrders);
			if (orders != null)
			{
				CollectionOrderBatchHelper.CreateReceipts(orders, Env.Security.CollectionOrderCreateReceipt);
			}
		}

		AccCollectionOrder[] LoadSelectedOrderInNewFactory(BusinessObject[] selectedOrders)
		{
			var factory = new BusinessObjectFactory();
			var selectedOrder = factory.Load<AccCollectionOrder>(new ZQuery(AccCollectionOrderSchema.PK, selectedOrders.Select(x => x.PK)));
			return selectedOrder;
		}

		public override ModuleIdentifier ID => ModuleIDs.AccCollectionOrder;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CollectionOrder;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CollectionOrderCode;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccCollectionOrder);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccCollectionOrderFilterControl(GridCollection, (AccCollectionOrderFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccCollectionOrderCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccCollectionOrderFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override SortInfo DefaultSortOrder => new SortInfo(AccCollectionOrder.Schema.ACO_OrderNumber, ListSortDirection.Ascending);
	}
}

