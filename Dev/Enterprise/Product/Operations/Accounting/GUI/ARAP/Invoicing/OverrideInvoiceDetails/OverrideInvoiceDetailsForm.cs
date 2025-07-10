using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public abstract partial class OverrideInvoiceDetailsForm : ZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		protected OverrideInvoiceDetailsForm()
		{
		}

		protected OverrideInvoiceDetailsForm(BusinessObject bo)
			: base(bo)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			var helper = BizO as OverrideInvoiceDetailsHelper;
			if (helper != null)
			{
				DisplayMode = helper.CanBizOBeSaved ? ODisplayMode.NewSaved : ODisplayMode.Undefined;
			}
			PostingButtonsUserControl.AllowOverlap(ContinueButton);
			PostingButtonsUserControl.AllowOverlap(CloseButton);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			HideUnrelatedButtons();
			HideSecurityRightsMessage();
		}

		public override string FormVerb => String.Empty;

		protected override bool AllowNew => false;

#if DEBUG
		public ZGrid InvoicesGridForTest => InvoicesGrid;

		public Core.Forms.ZPostingButtonsUserControl PostingButtonsForTest => PostingButtonsUserControl;
#endif

		protected BusinessObject BizO => (BusinessObject)BusinessEntity;

		void ContinueButton_Click(object sender, EventArgs e)
		{
			var helper = BizO as OverrideInvoiceDetailsHelper;
			if (helper != null)
			{
				helper.UpdateRelatedParentBizO();
				var parentForm = (ZForm)ZFormModaliser.GetParentFormForModalForm(this);
				var plugIns = parentForm?.PlugIns;
				if (plugIns != null)
				{
					var plugin = (InvoicingPluginToFreight)plugIns.GetPlugIn(ControllerIDs.JobInvoicing);
					if (plugin != null)
					{
						plugin.UpdateJobChargeGrid();
					}
				}
				this.Close();
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		protected virtual void HideSecurityRightsMessage()
		{
			//For designer it is possible that form does not have business object
			if (BizO != null)
			{
				UpperPanel.Visible = false;
			}
		}

		void HideUnrelatedButtons()
		{
			if (BizO == null)
			{
				return; //For designer it is possible that form does not have business object
			}

			var overInvDetHelper = BizO as OverrideInvoiceDetailsHelper;
			if (overInvDetHelper != null)
			{
				bool canBeSaved = overInvDetHelper.CanBizOBeSaved;

				ContinueButton.Visible = !canBeSaved;
				CloseButton.Visible = !canBeSaved;

				PostingButtonsUserControl.Visible = canBeSaved;
			}
		}
	}
}
