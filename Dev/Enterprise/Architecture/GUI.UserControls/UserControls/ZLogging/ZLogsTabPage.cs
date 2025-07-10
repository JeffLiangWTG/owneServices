using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressResourceStringContainerControlNameKeyPrefix]
	public class ZLogsTabPage : ZBindingTabPage
	{
		public ZLogsTabPage()
		{
			ImageIndex = Icons.GetImageIndex(IconTypes.Events);
			CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ABB2A318-0AA2-4f25-A827-1724719E23A7", "Logs", "The Logs tab.");
			this.ShouldBeReadOnlyInViewMode = false;
		}

		ZForm Form
		{
			get { return form ?? (form = FindForm() as ZForm); }
		}
		ZForm form;

		public override bool ExcludeFromBindingOnSave
		{
			get { return true; }
		}

		internal ZLogsUserControl LogsUserControl;

		protected sealed override bool IsAutoSized
		{
			get { return true; }
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			if (Form != null)
			{
				Form.Shown -= ZLogsTabPage_Shown;
				Form.Shown += ZLogsTabPage_Shown;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Tab Page Name")]
		void ZLogsTabPage_Shown(object sender, EventArgs e)
		{
			if (Form != null && !CaptionRenderingSupport.IsCaptionRenderingEnabled(this))
			{
				Text = "Logs";
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed && disposing)
			{
				if (Form != null)
				{
					Form.Shown -= ZLogsTabPage_Shown;
					form = null;
				}
			}
			base.Dispose(disposing);
		}

		public void HideLogReferenceAndEventDetail()
		{
			var grid = LogsUserControl?.ChangeLogsTabPage?.EventUserControl?.LogsControl?.Grid;
			grid?.SetAvailability(false, "SL_ReferenceForBinding");
			grid?.SetAvailability(false, "DisplayEventReference");
		}

		#region Determining log types to show

		public override void ClearNotificationImage()
		{
			base.ClearNotificationImage();
			ImageIndex = Icons.GetImageIndex(IconTypes.Events);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (!DesignModeFinder.IsDesigning && TabVisible)
			{
				if (!ContainsZLogsUserControl())
				{
					LogsUserControl = new ZLogsUserControl(GetStmALogFilterStripBusinessObject());
					LogsUserControl.Dock = DockStyle.Fill;
					LogsUserControl.LogsToShow = HasWorkflowTabWithEvents ? LogsToShow.ChangeLogs : LogsToShow.All;
					SetWorkflowTabPageInitialized();
					Controls.Add(LogsUserControl);
				}
				else if (!WorkflowTabPageInitialized)
				{
					SetWorkflowTabPageInitialized();
					LogsUserControl.LogsToShow = HasWorkflowTabWithEvents ? LogsToShow.ChangeLogs : LogsToShow.All;
				}
				else if (LogsUserControl.LogsToShow == LogsToShow.All && HasWorkflowTabWithEvents)
				{
					LogsUserControl.LogsToShow = LogsToShow.ChangeLogs;
				}
			}
		}

		bool ContainsZLogsUserControl()
		{
			foreach (Control c in Controls)
			{
				if (c.GetType() == typeof(ZLogsUserControl))
				{
					return true;
				}
			}
			return false;
		}

		bool HasWorkflowTabWithEvents
		{
			get
			{
				IWorkflowTabPage tabPage = GetWorkflowTabPage();
				return tabPage != null && tabPage.SupportsEventTracking;
			}
		}

		IWorkflowTabPage GetWorkflowTabPage()
		{
			foreach (ZTabPage tabPage in TabControl.TabPages)
			{
				IWorkflowTabPage workflowTabPage = tabPage as IWorkflowTabPage;
				if (workflowTabPage != null)
				{
					return workflowTabPage;
				}
			}

			return null;
		}

		void SetWorkflowTabPageInitialized()
		{
			bool res = false;
			IWorkflowTabPage tabPage = GetWorkflowTabPage();
			if (tabPage == null)
			{
				res = true;
			}
			else if (tabPage.SupportsEventTracking && tabPage.Initialized)
			{
				res = true;
			}
			WorkflowTabPageInitialized = res;
		}

		bool WorkflowTabPageInitialized { get; set; }

		ZTabControl TabControl
		{
			get { return (ZTabControl)Parent; }
		}

		#endregion

		#region Additional Tabs

		/// <summary>
		/// Adds an additional sub tab to the "Logs" tab with the specified text and the specified control as a docked control
		/// </summary>
		public void AddAdditionalTab(string text, UserControl dockedControl)
		{
			AddAdditionalTab(text, dockedControl, null);
		}

		/// <summary>
		/// Adds an additional sub tab to the "Logs" tab with the specified text and the specified control as a docked control with an option to exclude from binding on save
		/// </summary>
		public void AddAdditionalTab(string text, UserControl dockedControl, bool? excludeFromBindingOnSave)
		{
			var tabPage = new ZTabPage();
			tabPage.Text = text;
			dockedControl.Dock = DockStyle.Fill;
			tabPage.Controls.Add(dockedControl);

			if (excludeFromBindingOnSave != null)
			{
				tabPage.ExcludeFromBindingOnSave = excludeFromBindingOnSave.Value;
			}

			LogsUserControl.MainTabControl.TabPages.Add(tabPage);
		}

		public void SetSelectedTab(string text)
		{
			var tabPage = LogsUserControl.MainTabControl.TabPages.OfType<ZTabPage>().FirstOrDefault(u => u.Text == text);

			if (tabPage != null && LogsUserControl.MainTabControl.SelectedTab != tabPage)
			{
				LogsUserControl.MainTabControl.SelectedTab = tabPage;
			}
		}

		#endregion

		#region Hiding Properties

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int ImageIndex
		{
			get { return base.ImageIndex; }
			set { base.ImageIndex = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override ResourceStringData CaptionResourceString
		{
			get { return base.CaptionResourceString; }
			set { base.CaptionResourceString = value; }
		}

		#endregion

		protected virtual GetStmALogFilterStripBusinessObject GetStmALogFilterStripBusinessObject()
		{
			return null;
		}
	}
}
