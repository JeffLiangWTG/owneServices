using System;
using Enterprise.Accounting.Business.CreditStatus;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class CreditStatusControl : ZUserControl
	{
		public CreditStatusControl()
		{
			InitializeComponent();

			MissingResourceStringChecker.ExcludeFromTest(this.SettlementGroupInfoLabel);
			AsStandardUnderLabel.AllowOutsideOfParent();
			TreatDisbursementsLabel.AllowOutsideOfParent();
		}

		#region Security

		public virtual SecurityCheckpoint PluginSecurity
		{
			get { return pluginSecurity; }
			set
			{
				pluginSecurity = value;
				ResetSecurityHelper();
			}
		}
		SecurityCheckpoint pluginSecurity;

		protected JobInvoicingSecurityHelper SecurityHelper
		{
			get { return securityHelper ?? (securityHelper = new JobInvoicingSecurityHelper(PluginSecurity, false)); }
		}
		JobInvoicingSecurityHelper securityHelper;

		void ResetSecurityHelper()
		{
			securityHelper = null;
		}

		#endregion

		#region CreditCheckReport

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				ShowCreditReportPanel();
			}
		}

		CreditStatusBusinessObject CreditObject => CurrentDataItem as CreditStatusBusinessObject;

		void ShowCreditReportPanel()
		{
			if (!DesignModeFinder.IsDesigning && CreditReportHelper.CreditReportEnabled && OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.Value.Count > 0)
			{
				if (creditReportControl == null)
				{
					CreditStatusSplitContainer.Panel1Collapsed = false;
					creditReportControl = new CreditReportUserControl();
					creditReportControl.SetDataBinding(CreditObject.Organisation, string.Empty);
					CreditStatusSplitContainer.Panel1.Controls.Add(creditReportControl);
				}
				else
				{
					creditReportControl.SetDataBinding(CreditObject.Organisation, string.Empty);
				}
			}
			else
			{
				CreditStatusSplitContainer.Panel1Collapsed = true;
			}
		}

		CreditReportUserControl creditReportControl;

		#endregion
	}
}
