using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module
{
	public class UPEPrintBatchModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.BatchPrinting; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return null;
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new UPEPrintBatchFilterControl(this, (UPEPrintBatchCollection)GridCollection, (UPEPrintBatchFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new UPEPrintBatchCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new UPEPrintBatchFilterBusinessObject();
		}

		#region Disallow New/Edit/View/Delete

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			return null;
		}

		#endregion

		#region Batch Print

		public void PrintBatch(UPEPrintBatch printBatch)
		{
			Cursor oldCursor = EmbeddedControl.Cursor;
			EmbeddedControl.Cursor = Cursors.WaitCursor;
			try
			{
				printBatch.Print();
				printBatch.Factory.Save();
				PerformSearch();
			}
			finally
			{
				EmbeddedControl.Cursor = oldCursor;
			}
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> collection = new List<MenuItem>(base.GetNewStandardMenuItems());
			collection.Remove(ViewMenuItem);
			return collection.ToArray();
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> collection = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			collection.Add(new ZMenuItem("Print", new EventHandler(OnPrintToolBarButton_Click)));
			return collection.ToArray();
		}

		protected virtual IReadOnlyList<BusinessObject> SelectedGridElements
		{
			get { return this.Grid.SelectedElements; }
		}

		void OnPrintToolBarButton_Click(object sender, EventArgs e)
		{
			if (SelectedGridElements.Count == 0)
			{
				Globals.Message.ShowWarning("You must select a print batch from the grid.");
			}
			else
			{
				foreach (UPEPrintBatch selectedBatch in SelectedGridElements)
				{
					PrintBatch(selectedBatch);
				}
			}
		}

		#endregion
	}
}
