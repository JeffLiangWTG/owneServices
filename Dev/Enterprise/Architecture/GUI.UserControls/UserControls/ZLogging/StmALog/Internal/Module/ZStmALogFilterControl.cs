using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;
using ResString = Enterprise.ZArchitecture.GUI.UserControls.ResString;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZStmALogFilterControl : ZFilterStripControl
	{
		public bool AllowNew { get; set; } = true;
		public bool AllowCancel { get; set; } = true;
		public ZStmALogFilterControl(IStmALogParent master, IBusinessObjectCollection collection, FilterStripBusinessObject filterStripBusinessObject, ZStmALogModule module)
			: this(collection, filterStripBusinessObject)
		{
			if (module != null)
			{
				FilteredGrid.SetParentFilterGridModule(module);
			}

			this.master = master;
		}

		public ZStmALogFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
		: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				SetupGrid();
			}
		}

		protected IStmALogParent master;

		#region Grid

		void SetupGrid()
		{
			FilteredGrid.IsWholeRowSelectedOnClick = true;
			FilteredGrid.ForceShowExportToExcelMenuItem = true;
			FilteredGrid.ContextMenu.Popup += new EventHandler(ContextMenu_Popup);
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			if (!contextMenuItemsAdded)
			{
				FilteredGrid.ContextMenu.MenuItems.Add(SeparatorMenuItem);
				FilteredGrid.ContextMenu.MenuItems.Add(AddNewEventMenuItem);
				FilteredGrid.ContextMenu.MenuItems.Add(CancelMenuItem);
				contextMenuItemsAdded = true;
			}

			var stmALogFilterBusinessObject = FilterBusinessObject as ZStmALogFilterBusinessObject;
			CancelMenuItem.Visible = (stmALogFilterBusinessObject.LogsToShow & LogsToShow.All) != LogsToShow.ChangeLogs;
			AddNewEventMenuItem.Visible = stmALogFilterBusinessObject.LogsParentPK.IsValid && (stmALogFilterBusinessObject.LogsToShow & LogsToShow.All) != LogsToShow.ChangeLogs;
			SeparatorMenuItem.Visible = CancelMenuItem.Visible && AddNewEventMenuItem.Visible;
			AddNewEventMenuItem.Enabled = AllowNew;
			if (CancelMenuItem.Visible)
			{
				SetCancelEventMenuText();
			}
		}
		bool contextMenuItemsAdded;

		void SetCancelEventMenuText()
		{
			CancelMenuItem.Text = CancelSelectedEvent;
			CancelMenuItem.Enabled = AllowCancel;

			if (SelectedLog != null)
			{
				if (SelectedLog.SL_IsCancelled)
				{
					CancelMenuItem.Text = Res.GetString("ZStmALogGrid|CancelMenuItem|EventAlreadyCancelled", "Event already canceled");
					CancelMenuItem.Enabled = false;
				}
				else if (!SelectedLog.CanBeCancelledByUser)
				{
					CancelMenuItem.Text = Res.GetString("ZStmALogGrid|CancelMenuItem|CannotCancel", "Cannot cancel {0} event", SelectedLog.SL_SE_NKEvent);
					CancelMenuItem.Enabled = false;
				}
			}
		}

		protected virtual StmALog SelectedLog
		{
			get
			{
				return FilteredGrid.SelectedElements.Length > 0 ? FilteredGrid.SelectedElements[0] as StmALog : null;
			}
		}

		#region Menu items

		MenuItem separatorMenuItem;
		MenuItem SeparatorMenuItem
		{
			get
			{
				if (separatorMenuItem == null)
				{
					separatorMenuItem = new ZMenuItem("-");
				}
				return separatorMenuItem;
			}
		}

		MenuItem cancelMenuItem;
		MenuItem CancelMenuItem
		{
			get
			{
				if (cancelMenuItem == null)
				{
					cancelMenuItem = new ZMenuItem(CancelSelectedEvent, delegate
					{ CancelSelectedLogs(); });
				}
				return cancelMenuItem;
			}
		}

		MenuItem addNewEventMenuItem;
		MenuItem AddNewEventMenuItem
		{
			get
			{
				if (addNewEventMenuItem == null)
				{
					addNewEventMenuItem = new ZMenuItem(ResString.GetMultilingualString("82C43AED-29CD-4071-92F6-852215564CFA", "&Add New Event"), delegate
					{ ShowAddEventForm(); });
				}
				return addNewEventMenuItem;
			}
		}

		#endregion

		static string CancelSelectedEvent
		{
			get { return Res.GetString("85a969c6-5a01-48b1-ac2b-d9e9f1733092", "Cancel Selected Event"); }
		}

		#region Add Event

		void ShowAddEventForm()
		{
			if (master != null)
			{
				var newLog = new BusinessObjectFactory().New<StmALogAsAddedByUser>();
				newLog.Master = master;
				var zStmALogAddForm = new ZStmALogAddForm(newLog, master);
				zStmALogAddForm.EventAddedAction += (stmALog) => GridCollection.Insert(0, stmALog);
				ZFormModaliser.ShowDialogAndDispose(zStmALogAddForm);
			}
		}

		#endregion

		#region Cancel Selected Event

		void CancelSelectedLogs()
		{
			if (FilteredGrid.SelectedElements.Length == 0)
			{
				Globals.Message.Show(Res.GetString("2fb75c29-5140-4449-9bd3-1aed96e06074", "Please select an Event to cancel."), Res.GetString("94705d89-f2ae-4206-9a70-3fdcc64e3756", "Canceling an Event"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				var selectedLog = (StmALog)FilteredGrid.SelectedElements[0];
				if (!selectedLog.SL_IsCancelled)
				{
					selectedLog.Cancel();
					if (master != null)
					{
						if (master.Factory != selectedLog.Factory)
						{
							var query = new ZQuery(StmALogSchema.SL_Parent, selectedLog.SL_Parent);
							var logInMaster = master.Factory.Load<StmALog>(query).FirstOrDefault(x => x.PK == selectedLog.PK);
							if (logInMaster != null)
							{
								logInMaster.Cancel();
								((BusinessObject)master).HasChanges = true;
							}
						}
						else
						{
							((BusinessObject)master).HasChanges = true;
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			if (FilterBusinessObject != null && FilterBusinessObject.AreModuleFiltersLoaded)
			{
				try
				{
					FilterBusinessObject.RunPreSaveValidation();
					if (!FilterBusinessObject.HasErrors)
					{
						FilterBusinessObject.SaveLastUsedLayout();
					}
				}
				catch (Exception e) when (!(e is ZSaveConcurrencyException) && !e.IsCriticalException())
				{
					ErrorReporter.ReportOnce("DisposeFilterBusinessObject threw", e);
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
