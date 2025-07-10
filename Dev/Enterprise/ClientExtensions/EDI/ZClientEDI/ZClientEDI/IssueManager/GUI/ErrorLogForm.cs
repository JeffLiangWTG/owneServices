using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IssueManager.GUI
{
	partial class ErrorLogForm : ZForm
	{
		public ErrorLogForm(EdiHelpErrorLog businessEntity)
			: base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			RelatedWorkItemGrid.Issue = businessEntity;
			ActionsMenuItem.MenuItems.AddRange(MenuItems.ToArray());
		}

		public override string FormCaption
		{
			get
			{
				var result = "Issue " + ErrorLog.HE_IssueNumber;

				if (!string.IsNullOrEmpty(ErrorLog.ExceptionMessageFirstLine))
				{
					result += " - ";
					if (ErrorLog.ExceptionMessageFirstLine.Length > 80)
					{
						result += ErrorLog.ExceptionMessageFirstLine.Substring(0, 80) + "...";
					}
					else
					{
						result += ErrorLog.ExceptionMessageFirstLine;
					}
				}

				return result;
			}
		}

		EdiHelpErrorLog ErrorLog
		{
			get { return (EdiHelpErrorLog)BusinessEntity; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		#region Related Work Item

		void RelatedIncidentsGrid_DoubleClick(object sender, EventArgs e)
		{
			SupportIncident incident = relatedIncidentsGrid.List[relatedIncidentsGrid.HitTest(relatedIncidentsGrid.PointToClient(Cursor.Position)).Row] as SupportIncident;
			if (incident != null)
			{
				ZController controller = ZControllerFactory.Create(ClientControllerRegistration.SupportIncident);
				controller.ShowViewForm(incident);
			}
		}

		void loadKeysButton_Click(object sender, EventArgs e)
		{
			ErrorLog.LoadFinalKeys = true;
		}

		internal void WorkItemButton_Click(object sender, EventArgs e)
		{
			TopLevelTabControl.SelectedTab = RelatedWorkItemsTabPage;
			RelatedWorkItemGrid.FireNewButtonClick();
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		void ViewLicence(object sender, EventArgs e)
		{
			OccurrencesControl.OpenLicence();
		}

		#region Actions

		List<MenuItem> MenuItems
		{
			get
			{
				if (menuItems == null)
				{
					menuItems = new List<MenuItem>();

					MenuItem viewLicenceMenuItem = new ZMenuItem("View Licence", ViewLicence);
					menuItems.Add(viewLicenceMenuItem);
				}
				return menuItems;
			}
		}

		List<MenuItem> menuItems;

#endregion
	}
}
