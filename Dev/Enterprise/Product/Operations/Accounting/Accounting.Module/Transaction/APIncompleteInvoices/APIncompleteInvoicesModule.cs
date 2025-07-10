using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Transaction
{
	public partial class APIncompleteInvoicesModule : ZFilterGridModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var invoice = selectedBusinessObject as InvoicingBase;
			if (invoice != null)
			{
				return AccountingControllerCreator.GetNewController(invoice);
			}
			else
			{
				return AccountingControllerCreator.GetNewController(TransactionTypes.IncompleteInvoice, LedgerTypes.IncompleteTransactions);
			}
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.APInvoiceCode; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new APIncompleteInvoicesFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new APIncompleteInvoicesFilterStripControl(GridCollection, (APIncompleteInvoicesFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var aPInvoices = new InvoicingBaseCollectionForModule(Factory);
			return new FilteredAPInvoicingBaseCollectionViewForModule(aPInvoices);
		}

		protected override void PushItemsIntoCollectionCore(IBusinessObjectCollection collection, PerformSearchResult searchResult, SortInfo sort)
		{
			var filteredCollection = collection as FilteredAPInvoicingBaseCollectionViewForModule;

			IBusinessObjectCollection originalCollection;
			if (filteredCollection == null)
			{
				originalCollection = collection;
			}
			else
			{
				originalCollection = filteredCollection.CollectionToFilter;
			}
			base.PushItemsIntoCollectionCore(originalCollection, searchResult, sort);
		}

		protected override void OnAfterPerformSearchCore()
		{
			if (GridCollection is FilteredAPInvoicingBaseCollectionViewForModule && ((FilteredAPInvoicingBaseCollectionViewForModule)GridCollection).CollectionToFilter.Count > GridCollection.Count)
			{
				Globals.Message.ShowWarning(ViewingRestrictionOutsideLoginWarningMessage, Caption);
			}
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			ZMenuItem cancelMenuItem = null;
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			menuItems.Add(cancelMenuItem = new ZMenuItem(CancelIncompleteInvoiceMenuText, IconTypes.DeleteButtonActive, IconTypes.DeleteButtonRest));
			cancelMenuItem.Click += new EventHandler(HandleCancel);
			return menuItems.ToArray();
		}

		protected ResourceStringData CancelIncompleteInvoiceMenuText
		{
			get { return Res.GetData("9b5e3b94-efc3-4a9e-9234-1c9ff0304695", "&Cancel", "Cancels the selected item after viewing its details read-only"); }
		}

		protected MultilingualString ViewingRestrictionOutsideLoginWarningMessage
		{
			get { return ResString.GetMultilingualString("54B86A5C-0152-4AD1-BD0C-EBAF6B162F36", "Transaction created in branch / dept outside your login permission are not listed."); }
		}

		protected MultilingualString Caption
		{
			get { return ResString.GetMultilingualString("cfb5ab45-8612-4d5f-a9da-541208539999", "Search Results"); }
		}

		protected void HandleCancel(object sender, EventArgs e)
		{
			var currentBusinessObjectInGrid = CurrentBusinessObjectInGrid;
			if (currentBusinessObjectInGrid == null)
			{
				ShowNoSelectedMessage();
				return;
			}

			if (HasTypeErrorForSelectedBusinessObjects(currentBusinessObjectInGrid))
			{
				ShowIncorrectTypeErrorMessage();
				return;
			}

			var newController = GetNewController(currentBusinessObjectInGrid) as APIncompleteTransactionsController;
			if (newController != null)
			{
				newController.ShowCancelForm(currentBusinessObjectInGrid);
			}
			else
			{
				ShowIncorrectTypeErrorMessage();
			}
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.APIncompleteInvoices; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.APIncompleteInvoices; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		protected override bool CanBeCopied()
		{
			return false;
		}

		protected override bool HasTypeErrorForSelectedBusinessObjects(BusinessObject selectedBusinessObject)
		{
			var query = new ZQuery(FilterBusinessObject.Filter);
			query.ReLoadExistingRows = true;

			var factory = new BusinessObjectFactory();
			var obj = selectedBusinessObject == null ? null : factory.Load<TransactionPendingAllocation>(selectedBusinessObject.PK);
			return !(obj?.MatchesFilter(query) ?? false);
		}

		protected override ZString IncorrectTypeErrorMessage
		{
			get { return Res.GetString("b66e31a0-f646-4798-94f9-a23c457bf926", "The selected transaction is no longer valid. Please refresh the grid and try again."); }
		}

		protected override ZQuery ExportQuery
		{
			get
			{
				return TransactionModuleStrip.CreateCollectionExportQuery(GridCollection, FilterBusinessObject);
			}
		}
	}
}
