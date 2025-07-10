using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkQueueUserControl : ZUserControl
	{
		public WorkQueueUserControl()
		{
			InitializeComponent();
			InitializeGrid();
		}

		void InitializeGrid()
		{
			MembersGrid.AfterBind += MembersGrid_AfterBind;
			AddResequenceMenuItem();
		}

		void MembersGrid_AfterBind(object sender, EventArgs e)
		{
			(MembersGrid.Columns["Parent+ProviderJobNumber"].ColumnStyle as ZGridColumnStyle).Hotkeys.RegisterHotKey(Keys.F3, OpenFormByHotKey);

			EnableSequenceValidationForAllLinks();
		}

		void EnableSequenceValidationForAllLinks()
		{
			foreach (var link in DataSource.Members)
			{
				link.ShouldValidateSequenceUniqueness = true;
			}
		}

		void AddResequenceMenuItem()
		{
			reSequenceMenuItem = new ZMenuItem(ResString.GetMultilingualString("{59DC6695-9F8E-4B23-99B7-F868FBAA435F}", "Re-Sequence"), ReSequenceMenuItem_Click);
			MembersGrid.ContextMenu.MenuItems.Add(reSequenceMenuItem);
			MembersGrid.ContextMenu.Popup += ContextMenu_Popup;
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			reSequenceMenuItem.Visible = !(DataSource?.Members.ReadOnly ?? true) && WorkQueueSecurity.CheckResequenceQueueSecurity(DataSource);
		}

		ZMenuItem reSequenceMenuItem;

		void ReSequenceMenuItem_Click(object sender, EventArgs e)
		{
			var selectedElements = MembersGrid.SelectedElements;
			if ((selectedElements?.Length ?? 0) > 1)
			{
				var reSequencer = new ReSequencer(selectedElements.OfType<WorkQueueMembershipLink>(), DataSource.Factory);
				using (var form = new ReSequencerForm(reSequencer))
				{
					if (ZFormModaliser.ShowDialogAndDispose(form) == DialogResult.OK)
					{
						reSequencer.ReSequence();
					}
				}
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("{D4E9CCED-483C-4840-BC04-5DF4C27655D5}", "Please select two or more rows to re-sequence."));
			}
		}

		public new WorkQueue DataSource
		{
			get { return (WorkQueue)base.DataSource; }
		}

#if DEBUG
		public
#endif
		IReadOnlyCollection<WorkQueueMembershipLink> SelectedMembers
		{
			get { return MembersGrid.SelectedElements.Cast<WorkQueueMembershipLink>().ToArray(); }
		}

		#region Event Handlers

		void RemoveMemberButton_Click(object sender, EventArgs e)
		{
			var selectedProcessHeaders = SelectedMembers.Select(m => m.Parent).ToArray();
			if (selectedProcessHeaders.Length > 0)
			{
				var message = Res.GetString("1a1c03f3-d2fe-4bfb-9e09-00724b40584a", "Remove the selected items from this queue?");
				var caption = Res.GetString("8772b919-49c9-4377-a3f1-4c2d2fd373ed", "Remove items");

				var userWantsDelete = Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, DialogResult.OK) == DialogResult.OK;
				if (userWantsDelete)
				{
					string failureMessage;
					if (!BatchTagOperator.TryRemoveTag(DataSource, selectedProcessHeaders, out failureMessage, showSecurityDialog: false))
					{
						Globals.Message.Show(failureMessage);
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("7593d43e-2983-4c10-8b5d-0bbf9d53eca8", "Please select items from the grid."));
			}
		}

		void AddMemberButton_Click(object sender, EventArgs e)
		{
			var queue = DataSource;
			if (queue != null)
			{
				var collection = new ProcessHeaderCollection(queue.Factory);
				var items = BusinessObjectModulePicker.PickFromModuleScreen<ProcessHeader>(collection, ModuleIDs.ProcessHeader);

				if (items.Length > 0)
				{
					var query = new ZQuery(ProcessHeaderSchema.PK, items.Select(i => i.PK).ToArray());
					query.OrderBy = ProcessHeaderSchema.FH_FH_ParentHeader.Name;
					items = queue.Factory.Load<ProcessHeader>(query);

					string failureMessage;
					if (!BatchTagOperator.TryAddTag(queue, items, out failureMessage, showSecurityDialog: false))
					{
						Globals.Message.Show(failureMessage);
					}
				}
			}
		}

#if DEBUG
		public
#endif
		void MembersGrid_DoubleClick(object sender, MouseEventArgs e)
		{
			var selectedItem = SelectedMembers.FirstOrDefault();
			if (selectedItem != null)
			{
				GridEntityFormOpener.OpenForm(MembersGrid, e, () => selectedItem.Parent, ControllerIDs.ProcessHeader);
			}
		}

		void OpenFormByHotKey()
		{
			var workQueueMembershipLink = MembersGrid.ListManager.GetCurrent() as WorkQueueMembershipLink;
			if (workQueueMembershipLink != null)
			{
				GridEntityFormOpener.OpenForm(0, () => workQueueMembershipLink.Parent, ControllerIDs.ProcessHeader);
			}
		}

		#endregion

		public void NavigateToTagMagnitude(BusinessObject selectedWorkQueue)
		{
			MembersGrid.SelectSingleElement(selectedWorkQueue);
		}

		#region For test
#if DEBUG

		public void AddMembers_ForTest()
		{
			var addButton = (ZButton)Controls.Find("AddMemberButton", true)[0];
			addButton.PerformClick();
		}

		public void RemoveSelectedMembers_ForTest()
		{
			var removeButton = (ZButton)Controls.Find("RemoveMemberButton", true)[0];
			removeButton.PerformClick();
		}

#endif
		#endregion
	}
}
