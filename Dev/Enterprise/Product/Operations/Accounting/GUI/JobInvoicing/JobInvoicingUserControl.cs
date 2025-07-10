using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobInvoicingUserControl : ZUserControl
	{
		public JobInvoicingUserControl()
		{
			InitializeComponent();
			SetupControlParentsAndBinding();
			APInvoicePrintingSplitContainer.AllowOverlap(APJobInvoicingPrintingSecurityPanel);
			APInvoicePrintingSplitContainer.Panel2Collapsed = !AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal.Value;

			jobChargeUserControl1.AllowOutsideOfParent();
		}

		public JobInvoicingUserControl(SecurityCheckpoint plugInSecurity)
			: this()
		{
			this.PlugInSecurity = plugInSecurity;
			SetupControlSecurity();
		}

		#region Controls and Binding

		void SetupControlParentsAndBinding()
		{
			InvoicingTabPage.Controls.Add(jobChargeUserControl1);
			jobChargeUserControl1.Dock = DockStyle.Fill;
			ARInvoicesTabPage.Controls.Add(jobInvoicePrintingControl1);
			jobInvoicePrintingControl1.Dock = DockStyle.Fill;
			CreditStatusTabPage.Controls.Add(creditStatusControl1);
			creditStatusControl1.Dock = DockStyle.Fill;
			CashAdvanceRequestsTabPage.Controls.Add(cashAdvanceRequestUserControl1);
			cashAdvanceRequestUserControl1.Dock = DockStyle.Fill;
		}

		void SetupControlSecurity()
		{
			jobInvoicePrintingControl1.PluginSecurity = PlugInSecurity;
			apInvoicePrintingUserControl1.PluginSecurity = PlugInSecurity;
			jobProfitLossControl1.PluginSecurity = PlugInSecurity;
			creditStatusControl1.PluginSecurity = PlugInSecurity;
			cashAdvanceRequestUserControl1.PluginSecurity = PlugInSecurity;
		}

		public JobChargeUserControl JobChargeUserControl
		{
			get { return jobChargeUserControl1; }
		}

		public JobInvoicePrintingControl JobInvoicePrintingControl
		{
			get { return jobInvoicePrintingControl1; }
		}

		public APInvoicePrintingUserControl APInvoicePrintingUserControl
		{
			get { return apInvoicePrintingUserControl1; }
		}

		public APDraftInvoicePrintingUserControl APDraftInvoicePrintingUserControl
		{
			get { return draftInvoiceListUserControl1; }
		}

		public JobProfitLossControl JobProfitLossControl
		{
			get { return jobProfitLossControl1; }
		}

		public CreditStatusControl JobCreditStatusControl
		{
			get { return creditStatusControl1; }
		}

		public CashAdvanceRequestUserControl CashAdvanceRequestUserControl
		{
			get { return cashAdvanceRequestUserControl1; }
		}

		#endregion

		#region Resize

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			ControlDpiScalingHelper.SetHeight(ref JobInvoicingTabControl, ClientSize.Height - JobInvoicingTabControl.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(2), false);
			ControlDpiScalingHelper.SetWidth(ref JobInvoicingTabControl, ClientSize.Width - JobInvoicingTabControl.Left - ControlDpiScalingHelper.ScaleToCurrentDpiX(2), false);
		}

		#endregion

		#region Refresh Profit / Loss

		void JobInvoicingTabControl_Selected(object sender, EventArgs e)
		{
			var tabcontrol = sender as ZTabControl;
			if (tabcontrol != null && tabcontrol.SelectedTab != null)
			{
				if (tabcontrol.SelectedTab == ProfitLossTabPage)
				{
					foreach (Control ctrl in ProfitLossTabPage.Controls)
					{
						var profitLossControl = ctrl as JobProfitLossControl;
						if (profitLossControl != null)
						{
							profitLossControl.RefreshSelectedTab();
							break;
						}
					}
				}
			}
		}

		#endregion

		void creditStatusControl1_VisibleChanged(object sender, EventArgs e)
		{
			if (creditStatusControl1.Visible)
			{
				var job = JobChargeUserControl.Job;
				if (job != null && job.CreditStatusBizObject.OrganisationPK != job.LocalChargesPK)
				{
					job.CreditStatusBizObject.OrganisationPK = job.LocalChargesPK;
				}
			}
		}

		#region Security

		readonly SecurityCheckpoint PlugInSecurity;

		public ZPanel ARInvoicingPrintingSecurityPanel
		{
			get { return ARJobInvoicingPrintingSecurityPanel; }
		}

		public ZLabel ARInvoicingPrintingSecurityLabel
		{
			get { return ARInvoicePrintingSecurityLabel; }
		}

		public ZPanel APInvoicingPrintingSecurityPanel
		{
			get { return APJobInvoicingPrintingSecurityPanel; }
		}

		public ZLabel APInvoicingPrintingSecurityLabel
		{
			get { return APInvoicePrintingSecurityLabel; }
		}

		public ZPanel ProfitLossSecurityPanel
		{
			get
			{
				return JobProfitLossSecurityPanel;
			}
		}

		public ZLabel ProfitLossSecurityLabel
		{
			get
			{
				return JobProfitLossSecurityLabel;
			}
		}

		public ZPanel InvoicingSecurityPanel
		{
			get
			{
				return JobInvoicingSecurityPanel;
			}
		}

		public ZLabel InvoicingSecurityLabel
		{
			get
			{
				return JobInvoicingSecurityLabel;
			}
		}

		public ZPanel CreditStatusSecurityPanel
		{
			get
			{
				return jobCreditStatusSecurityPanel;
			}
		}

		public ZLabel CreditStatusSecurityLabel
		{
			get
			{
				return jobCreditStatusSecurityLabel;
			}
		}

		#endregion

#if DEBUG
		public ZTemplateTabControl JobInvoicingTabControl_ForTest => JobInvoicingTabControl;
#endif

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
			base.Dispose(disposing);
		}

		#endregion
	}
}

