using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class LVSSubHeaderDetailsUserControl : ZUserControl
	{
		public LVSSubHeaderDetailsUserControl()
		{
			InitializeComponent();
			Resize += LVSSubHeaderUserControl_Resize;
			ImporterOrganisationControl.SizeChanged += ImporterOrganisationControl_SizeChanged;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var declaration = (JobDeclaration)DataSource;
			if (!(declaration?.IsLVX ?? ZBool.False))
			{
				this.IsReadyForConsolidationCheckBox.Visible = false;
			}
		}

		#region SplitContainer Layout Bug Fix

		//TODO: Replace this code with new ZSplitContainer when ready

		void LVSSubHeaderUserControl_Resize(object sender, EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (workItem1 != null)
				{
					workItem1.Dispose();
				}

				workItem1 = UserIdleWorker.QueueWorkItem(this, 0, new Action<SplitContainer>(RefreshSplitter), SplitContainer1);

				if (workItem2 != null)
				{
					workItem2.Dispose();
				}

				workItem2 = UserIdleWorker.QueueWorkItem(this, 100, new Action<SplitContainer>(RefreshSplitter), SplitContainer2);
			}
		}

		void RefreshSplitter(SplitContainer splitContainer)
		{
			splitContainer.Panel1.SuspendLayout();
			splitContainer.Panel2.SuspendLayout();
			if (shouldLeft)
			{
				splitContainer.SplitterDistance--;
				shouldLeft = false;
			}
			else
			{
				splitContainer.SplitterDistance++;
				shouldLeft = true;
			}
			splitContainer.Panel1.ResumeLayout();
			splitContainer.Panel2.ResumeLayout();
		}

		IDisposable workItem1;
		IDisposable workItem2;
		bool shouldLeft;

		void ImporterOrganisationControl_SizeChanged(object sender, EventArgs e)
		{
			SupplierDocAddressControl.SingleLineNoGroupBoxPanelWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiX(ImporterOrganisationControl.Width);
		}

		#endregion

		#region INCOTERMS Explanation

		protected void IncoTermExplainButton_Click(object sender, EventArgs e)
		{
			var form = new IncoTermDescriptionForm(JZ_IncoTermBoundDropDownEdit.Text);
			ZFormModaliser.Show(form, ParentForm as ZForm);
		}

		#endregion

		public void IsReadyForConsolidationCheckBox_Click(object sender, EventArgs e)
		{
			if (this.IsReadyForConsolidationCheckBox.Checked && CACustomsDataRegistry.Instance.EnableCreditCheckForCLVS.Value)
			{
				var result = true;
				var declaration = (JobDeclaration)DataSource;
				var declarationSaved = true;

				if (!declaration.IsInDatabase)
				{
					declarationSaved = new MessageInstructionUserNotification().ShowConfirmation(Res.GetString("FB4977C5-0953-41BA-A249-5B2B260BCBFF", "The Job has not yet been saved. Do you want to save and proceed?"), Res.GetString("4173C4BF-876E-41A4-B419-82F1537E8C05", "Save Job")) && ((ZForm)ParentForm).FireSaveButton() == ContinueWithSave.Yes;
				}

				if (declarationSaved)
				{
					var helper = new MessageManagerCreditCheckWithSecurityHelper(declaration);
					result = helper.IsCreditCheckOKToSend;
				}
				else
				{
					result = false;
				}

				if (!result)
				{
					this.IsReadyForConsolidationCheckBox.Checked = false;
				}
			}
		}
	}
}
