using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.GUI;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class EDIWorkTaskRelatedItemUserControl : WorkTaskRelatedItemUserControl
	{
		public EDIWorkTaskRelatedItemUserControl(bool isViewOrDeleteMode)
			: base(isViewOrDeleteMode)
		{
			InitializeComponent();
			AddCreateIncidentsButton();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			CreateIncidentsButton.Visible = CurrentDataItem is NewWorkItem;
		}

		#region Menu Strip Items

		protected ZToolStripMenuItem NewSupportIncidentMenuItem;

		protected override void AddNewMenuItem(KContextMenuStrip contextMenuStrip, string caption, ControllerID controllerID, EventHandler onClick)
		{
			if (controllerID == ClientControllerRegistration.SupportIncident)
			{
				NewSupportIncidentMenuItem = new ZToolStripMenuItem();
				NewSupportIncidentMenuItem.Name = "newSupportIncidentButton";
				NewSupportIncidentMenuItem.Text = Res.GetString("82336E36-C223-44BB-B7C3-2933EB5AD15F", "Incident");
				NewSupportIncidentMenuItem.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("55F09C78-6E6A-44C0-8F1F-0E1773CB50D6", "New"), onClick));
				NewSupportIncidentMenuItem.DropDownOpening += NewSupportIncidentButton_DropDownOpening;

				contextMenuStrip.Items.Add(NewSupportIncidentMenuItem);
			}
			else
			{
				base.AddNewMenuItem(contextMenuStrip, caption, controllerID, onClick);
			}
		}

		protected override void ShowNewItemForm(ControllerID controllerID, string type, object sender)
		{
			var currentSupportIncident = CurrentDataItem as SupportIncident;
			if (currentSupportIncident != null && !currentSupportIncident.CanCreateWorkItem && controllerID == ControllerIDs.WorkItem)
			{
				Globals.Message.Show(ModuleSelectionControl.YouCannotCreateWorkItem);
			}
			else if (CurrentDataItem != null && CurrentDataItem is IncidentManagementGroup && controllerID == ClientControllerRegistration.SupportIncident)
			{
				string message = Res.GetString("be356bda-807c-455f-b97d-ea6c37661766", "An Incident added as a Related Item will not be subject to group control. If this Incident should be controlled by the group, it must be attached as a Linked Incident from the Incident Control Center tab. Attach Incident as a Related Item?");
				string caption = Res.GetString("d6963bb5-bd7b-4d12-b3c7-7a1d9ffa9471", "Confirm New Incident...");
				DialogResult dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dialogResult == DialogResult.Yes)
				{
					base.ShowNewItemForm(controllerID, type, sender);
				}
			}
			else
			{
				base.ShowNewItemForm(controllerID, type, sender);
			}
		}

		void NewSupportIncidentButton_DropDownOpening(object sender, EventArgs e)
		{
			AddUniversalCopyButton();
		}

		#endregion

		#region Adding Items

		public void AddNewWorkItem()
		{
			LastController = ZControllerFactory.Create(ControllerIDs.WorkItem);
			NewItemHelper.AddNewItem(LastController, RelatedItemSource, RelatedItemSource.RelatedItems, WorkTaskRelatedItemTypes.WorkItem, false);
		}

		#endregion

		#region New Incident Button

		bool universalCopyLoaded;

		protected void AddUniversalCopyButton()
		{
			if (!universalCopyLoaded && Parent?.FindForm() is ZForm form)
			{
				var universalCopyManager = ObjectFactory.New<IFormUniversalCopyManager>(form);
				if (universalCopyManager.AllowsUniversalCopy)
				{
					var universalCopyMenuItem = new ZMenuItem(ResString.GetMultilingualString("CAC124A0-9CE6-43B8-AB98-85A59C3C72E9", "Universal Copy"));
					universalCopyManager.AddMenuItems(universalCopyMenuItem, includeEditMenuItems: true, includeCopySchedulesItem: false, lazyPopulate: false);
					universalCopyManager.FormShowingForNewElement += UniversalCopyManager_FormShowingForNewElement;
					NewSupportIncidentMenuItem.DropDownItems.Add(MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(universalCopyMenuItem));
					universalCopyLoaded = true;
				}
				else
				{
					universalCopyManager.Dispose();
				}
			}
		}

		void UniversalCopyManager_FormShowingForNewElement(object sender, FormShowingForElementArgs e)
		{
			if (e.Element is SupportIncident bizo)
			{
				using (bizo.SuspendSettingHasChanges())
				{
					RelatedItemSource.PopulateNewRelatedItem(null, bizo);
				}

				_ = new NewItemTracker(bizo, RelatedItemSource.RelatedItems);
			}
		}

		#endregion

		#region Detach

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			if (RelatedItemGrid.ListManager.Position > -1)
			{
				var relatedItems = RelatedItemGrid.SelectedElements;

				foreach (var relatedItem in relatedItems)
				{
					if (relatedItem is SupportIncident && RelatedItemSource is IncidentManagementGroup)
					{
						var group = (IncidentManagementGroup)RelatedItemSource;
						if (group.LinkedIncidents != null && group.LinkedIncidents.Any(link => ((IncidentManagementLink)link).INL_IM_Incident == relatedItem.PK))
						{
							Globals.Message.ShowError("Links between Incidents and Incident Management Groups can only be modified from the Incident Control Center tab.");
							return;
						}
					}
					if (relatedItem is IncidentManagementGroup && RelatedItemSource is SupportIncident)
					{
						Globals.Message.ShowError("Links between Incidents and Incident Management Groups can only be modified in the Incident Group.");
						return;
					}
				}
				DialogResult dialogResult = Globals.Message.Show("Are you sure you want to detach the selected related item?", "Confirm Detach...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dialogResult == DialogResult.Yes)
				{
					foreach (var relatedItem in relatedItems)
					{
						RelatedItemSource.RelatedItems.Remove(relatedItem);
					}
				}
			}
		}

		#endregion

		#region Open Items

		void RelatedItemGrid_DoubleClick(object sender, EventArgs e)
		{
			OpenRelatedItem();
		}

		void EditButton_Click(object sender, EventArgs e)
		{
			OpenRelatedItem();
		}

		void OpenRelatedItem()
		{
			if (RelatedItemGrid.ListManager.Position > -1)
			{
				var relatedItem = (IWorkTaskRelatedItem)RelatedItemGrid.ListManager.GetCurrent();
				LastController = ZControllerFactory.Create(relatedItem.ControllerID);
				LastController.ShowEditForm((BusinessObject)relatedItem);
			}
		}

		#endregion

		#region Create Incidents

		protected ZButton CreateIncidentsButton;

		void AddCreateIncidentsButton()
		{
			CreateIncidentsButton = new ZButton();
			CreateIncidentsButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			CreateIncidentsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			CreateIncidentsButton.Name = "CreateIncidentsButton";
			CreateIncidentsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 23, true);
			CreateIncidentsButton.TabIndex = 0;
			CreateIncidentsButton.Text = "Auto Create Incidents";
			CreateIncidentsButton.UseVisualStyleBackColor = true;
			CreateIncidentsButton.Click += new EventHandler(CreateIncidentsButton_Click);
			PlaceAdditionalControlForRelatedItems(CreateIncidentsButton);
		}

		void CreateIncidentsButton_Click(object sender, EventArgs e)
		{
			List<SupportIncident> incidents = null;

			var workItem = ((ZForm)FindForm()).BusinessEntity as NewWorkItem;
			if (workItem != null)
			{
				if (Globals.Message.Show("This operation may create lots of incidents, do you want to continue?", "Confirmation", MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
				{
					incidents = workItem.CreateIncidentsFromRelatedIssues();
				}
				else
				{
					return;
				}
			}

			string message;

			if (incidents != null && incidents.Count > 0)
			{
				message = EdiHelpErrorLog.GetIncidentCreatedMessage(incidents);
			}
			else
			{
				message = "No new incidents were created. Either there are no attached issues or all the client databases that the issues belong to already have a related work item attached to the issues.";
			}

			Globals.Message.ShowInformation(message);
		}

		#endregion

		#region For Testing
#if DEBUG

		public ZToolStripMenuItem NewSupportIncidentMenuItem_Exposed => NewSupportIncidentMenuItem;

		public void AddUniversalCopyButton_Exposed() => AddUniversalCopyButton();

#endif
		#endregion

		#region Implementation

		public new IWorkTaskRelatedItem CurrentDataItem
		{
			get { return base.CurrentDataItem; }
		}

		IWorkTaskRelatedItemSource RelatedItemSource
		{
			get { return (IWorkTaskRelatedItemSource)BindingSource.Current; }
		}

		protected override bool ShouldShowRecordAttacher(ModuleIdentifier moduleID)
		{
			if (moduleID == ModuleIDs.WorkItem && CurrentDataItem is SupportIncident)
			{
				SupportIncident currentSupportIncident = (SupportIncident)CurrentDataItem;
				if (!currentSupportIncident.CanCreateWorkItem)
				{
					Globals.Message.Show(Enterprise.Client.EDI.IncidentManager.GUI.ModuleSelectionControl.YouCannotCreateWorkItem);
					return false;
				}
			}

			if (CurrentDataItem != null && CurrentDataItem is IncidentManagementGroup && moduleID == ClientModuleRegistration.SupportIncident)
			{
				string message = Res.GetString("be356bda-807c-455f-b97d-ea6c37661766", "An Incident added as a Related Item will not be subject to group control. If this Incident should be controlled by the group, it must be attached as a Linked Incident from the Incident Control Center tab. Attach Incident as a Related Item?");
				string caption = Res.GetString("fa530d52-cae5-4d39-9e36-a5cd95d56581", "Confirm Attach Incident...");
				DialogResult dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (dialogResult != DialogResult.Yes)
				{
					return false;
				}
			}

			return base.ShouldShowRecordAttacher(moduleID);
		}

		protected override void ShowRecordAttacherCore(IBusinessObjectCollection lookupsCollection, ModuleIdentifier moduleID)
		{
			if (moduleID == ClientModuleRegistration.IssueManager && CurrentDataItem is NewWorkItem)
			{
				LastAttacher = new WorkTaskAttacher(RelatedItemSource.RelatedItems, ((NewWorkItem)CurrentDataItem).Lookups.ErrorLogList, ClientModuleRegistration.IssueManager);
			}
			else if (moduleID == ClientModuleRegistration.SupportIncident && CurrentDataItem is NewWorkItem)
			{
				LastAttacher = new SupportIncidentAttacher(RelatedItemSource.RelatedItems, lookupsCollection, moduleID, true);
			}
			else
			{
				base.ShowRecordAttacherCore(lookupsCollection, moduleID);
			}
		}

		#endregion
	}
}
