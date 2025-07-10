using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
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

namespace Enterprise.Accounting.Module
{
	public partial class AccCollectionBatchModule : ZFilterGridModule
	{
		public AccCollectionBatchModule()
		{
		}

		public override ModuleIdentifier ID => ModuleIDs.AccCollectionBatch;

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("83d28bac-de13-4d48-a812-acf9cb31df29", "Cancel Batch", "Deletes the selected item after viewing its details read-only (shortcut Del)");
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccCollectionBatch);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccCollectionBatchFilterControl(GridCollection, (AccCollectionBatchFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccCollectionBatchCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccCollectionBatchFilterBusinessObject();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("9b4a7352-2fbb-4cff-a781-38c99471f614", "Create Receipts and Deposit Batch"), new EventHandler(HandleCreateReceiptsAndOneDepositBatch)));
			menuItems.Add(new ZMenuItem(AccountingConstants.AccCollectionOrderMenuNames.CreateReceiptMenuItemName, new EventHandler(HandleCreateReceiptsAndIndividualDepositBatch)));
			return menuItems.ToArray();
		}

		AccCollectionBatch LoadSelectedBatchInNewFactory()
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length != 1)
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("f8205e52-0c91-458a-8a45-d7157830258e", "Please select a batch first"));
				return null;
			}
			else
			{
				var factory = new BusinessObjectFactory();
				var selectedBatch = factory.Load<AccCollectionBatch>(SelectedBusinessObjects.First().PK);
				return selectedBatch;
			}
		}

		protected void HandleCreateReceiptsAndOneDepositBatch(object sender, EventArgs e)
		{
			var batch = LoadSelectedBatchInNewFactory();
			if (batch != null)
			{
				CollectionOrderBatchHelper.CreateReceiptsAndOneDepositBatch(batch, Env.Security.CollectionBatchCreateReceipt);
			}
		}

		protected void HandleCreateReceiptsAndIndividualDepositBatch(object sender, EventArgs e)
		{
			var batch = LoadSelectedBatchInNewFactory();
			if (batch != null)
			{
				CollectionOrderBatchHelper.CreateReceiptsAndIndividualDepositBatch(batch, Env.Security.CollectionBatchCreateReceipt);
			}
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CollectionBatch;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CollectionBatchCode;
	}
}

