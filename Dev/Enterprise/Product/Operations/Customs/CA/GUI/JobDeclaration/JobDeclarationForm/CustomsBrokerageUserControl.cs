using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeK84TabPage();
			this.MainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
			SetCaptions();
		}

		void SetCaptions()
		{
			MessagesTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("B3BAE293-FD54-4484-9877-54A34D040F3E", "Messages");
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
			set { base.JobDeclaration = value; }
		}

		public new IInvoicesProvider CurrentDataItem
		{
			get { return (IInvoicesProvider)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			declarationValueChangedAnnouncer = CurrentDataItem?.GetValueChangedAnnouncer();
			declarationValueChangedAnnouncer_OnValueChanged(this, null);
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.OnValueChanged += declarationValueChangedAnnouncer_OnValueChanged;
			}
		}
		IInvoicesProviderValueChangedAnnouncer declarationValueChangedAnnouncer;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (declarationValueChangedAnnouncer != null)
			{
				declarationValueChangedAnnouncer.OnValueChanged -= declarationValueChangedAnnouncer_OnValueChanged;
				declarationValueChangedAnnouncer.Dispose();
			}
		}

		void declarationValueChangedAnnouncer_OnValueChanged(object sender, EventArgs e)
		{
			if (K84UserControl != null)
			{
				K84UserControl.ChangeControlsVisibility();
			}
		}

		#region Implementation

		protected override void OnJobDeclarationSet()
		{
			base.OnJobDeclarationSet();
			if (JobDeclaration != null)
			{
				JobDeclaration.OnGetReleaseStatusesToPrint += Declaration_OnGetReleaseStatusesToPrint;
				JobDeclaration.NeedValidateMessageType = true;
			}
			JobDeclaration_ControlVisibilityChanged(this, EventArgs.Empty);
		}

		protected override void ChangeControlsVisibility()
		{
			if (InvoicesTabPage != null)
			{
				InvoicesTabPage.TabRelevant = JobDeclaration != null && !JobDeclaration.IsLVS;
			}
			if (InvoiceGroupingTabPage != null)
			{
				InvoiceGroupingTabPage.TabRelevant = JobDeclaration != null && !JobDeclaration.IsExport && !JobDeclaration.IsLVS;
			}
			if (MessagesTabPage != null)
			{
				MessagesTabPage.TabRelevant = JobDeclaration != null && !JobDeclaration.IsMisc;
			}
			if (K84TabPage != null)
			{
				K84TabPage.TabRelevant = JobDeclaration != null && JobDeclaration.IsImport;
			}
		}

		void Declaration_OnGetReleaseStatusesToPrint(object sender, CancelEventArgs e)
		{
			using (var form = new ReleaseStatusPrintForm(JobDeclaration))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				e.Cancel = form.DialogResult != System.Windows.Forms.DialogResult.OK;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (JobDeclaration != null)
				{
					JobDeclaration.OnGetReleaseStatusesToPrint -= Declaration_OnGetReleaseStatusesToPrint;
				}

				if (fK84UserControl != null)
				{
					fK84UserControl.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void InitializeK84TabPage()
		{
			fK84TabPage = new BaseDeclarationTabPage();
			fK84TabPage.LazyCreateControls += LazyCreateControlsFired;
			fK84TabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			fK84TabPage.Name = "K84TabPage";
			fK84TabPage.Size = ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			fK84TabPage.TabIndex = 8;
			fK84TabPage.Text = Res.GetString("A1C781DC-634A-406F-B5CA-4F5C1BDB5C15", "Status");
			this.MainTabControl.SuspendLayout();
			var indexOfMisc = this.MainTabControl.TabPages.IndexOf(this.MiscOptionsTabPage);
			if (indexOfMisc > -1)
			{
				this.MainTabControl.TabPages.Insert(this.fK84TabPage, indexOfMisc + 1);
			}
			this.MainTabControl.ResumeLayout(false);
		}

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.MainTabControl.SelectedTab == K84TabPage)
			{
				LoadK84TabPage();
			}
		}

		#region Create New User Controls for each tab

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new CAJobDeclarationUserControl();
		}

		protected override BaseInvoiceGroupingUserControl GetInvoiceGroupingUserControl()
		{
			return new GroupInvoiceUserControl();
		}

		protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			BaseCustomsSupplierHeaderUserControl result;
			if (JobDeclaration.IsImport)
			{
				result = new CAImportSupplierHeaderUserControl();
			}
			else if (JobDeclaration.IsExport)
			{
				result = new CAExportSupplierHeaderUserControl();
			}
			else
			{
				result = new NonLayoutCustomsSupplierHeaderUserControl();
			}
			return result;
		}

		protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			BaseInvoiceLineUserControl result;
			if (JobDeclaration.IsImport)
			{
				result = new CAImportInvoiceLineUserControl();
			}
			else if (JobDeclaration.IsExport)
			{
				result = new CAExportInvoiceLineUserControl();
			}
			else
			{
				result = base.GetInvoiceLinesUserControl();
			}
			return result;
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			BaseCustomsEntryUserControl result;
			if (JobDeclaration.IsImport)
			{
				result = new CAImportMessagesUserControl();
			}
			else if (JobDeclaration.IsExport)
			{
				result = new ExportMessagesUserControl();
			}
			else
			{
				result = base.GetMessageUserControl();
			}
			return result;
		}

		protected override BaseCustomsCusContainersUserControl GetContainerUserControl()
		{
			return new CustomsContainersWithTrackingUserControl();
		}

		protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl()
		{
			return new CAMiscOptionsUserControl();
		}

		protected override IBasePackingControl GetPackingUserControl()
		{
			return new CustomsPackingUserControl();
		}

		public K84UserControl K84UserControl
		{
			get { return fK84UserControl; }
		}
		K84UserControl fK84UserControl;

		public BaseDeclarationTabPage K84TabPage
		{
			get { return fK84TabPage; }
		}
		BaseDeclarationTabPage fK84TabPage;

		protected override void LazyCreateControlsFired(object sender, EventArgs e)
		{
			if (sender == K84TabPage)
			{
				LoadK84TabPage();
			}
			else
			{
				base.LazyCreateControlsFired(sender, e);
			}
		}

		public void LoadK84TabPage()
		{
			if (K84TabPage.Controls.Count == 0 && K84TabPage.TabVisible)
			{
				fK84UserControl = new K84UserControl();
				fK84UserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				fK84UserControl.Visible = false;
				K84TabPage.Controls.Add(fK84UserControl);
				fK84UserControl.Visible = true;
				fK84UserControl.SetDataBinding(JobDeclaration, "");
			}
		}

		#endregion

		#endregion Implementation
	}
}
