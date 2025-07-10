using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class IM2CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public IM2CustomsBrokerageUserControl()
		{
			InitializeDeclarationTabPage();
			InitializeK84TabPage();
			DisposeRedundantTabs();
			this.MainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
			SetCaptions();
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
			set { base.JobDeclaration = value; }
		}

		void DisposeRedundantTabs()
		{
			MessagesTabPage.Dispose();
		}

		void SetCaptions()
		{
			this.DeclarationTabPage.Text = Res.GetString("72DD8BBF-CE61-4C47-B1E3-439B306416CB", "B2");
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new IM2UserControl();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fJobDeclarationUserControl != null)
				{
					fJobDeclarationUserControl.Dispose();
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public BaseDeclarationTabPage OriginalDeclarationTabPage
		{
			get { return fDeclarationTabPage; }
		}
		BaseDeclarationTabPage fDeclarationTabPage;

		public CAJobDeclarationUserControl JobDeclarationUserControl
		{
			get { return fJobDeclarationUserControl; }
		}
		CAJobDeclarationUserControl fJobDeclarationUserControl;

		public BaseDeclarationTabPage K84TabPage
		{
			get { return fK84TabPage; }
		}
		BaseDeclarationTabPage fK84TabPage;

		public K84UserControl K84UserControl
		{
			get { return fK84UserControl; }
		}
		K84UserControl fK84UserControl;

		protected override void LazyCreateControlsFired(object sender, EventArgs e)
		{
			if (sender == OriginalDeclarationTabPage)
			{
				LoadOriginalDeclarationTabPage();
			}
			else
			{
				base.LazyCreateControlsFired(sender, e);
			}
		}

		void InitializeDeclarationTabPage()
		{
			fDeclarationTabPage = new BaseDeclarationTabPage();
			fDeclarationTabPage.LazyCreateControls += LazyCreateControlsFired;
			fDeclarationTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			fDeclarationTabPage.Name = "Declaration";
			fDeclarationTabPage.Size = ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			fDeclarationTabPage.TabIndex = 8;
			fDeclarationTabPage.Text = Res.GetString("4FC1D83A-B272-4D1B-A510-B88734A2804C", "Declaration");
			this.MainTabControl.SuspendLayout();
			var indexOfMisc = this.MainTabControl.TabPages.IndexOf(this.DeclarationTabPage);
			if (indexOfMisc > -1)
			{
				this.MainTabControl.TabPages.Insert(this.fDeclarationTabPage, indexOfMisc + 1);
			}
			this.MainTabControl.ResumeLayout(false);
		}

		void InitializeK84TabPage()
		{
			fK84TabPage = new BaseDeclarationTabPage();
			fK84TabPage.LazyCreateControls += LazyCreateControlsFired;
			fK84TabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			fK84TabPage.Name = "K84TabPage";
			fK84TabPage.Size = ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			fK84TabPage.TabIndex = 8;
			fK84TabPage.Text = Res.GetString("DFCAE9D2-97FC-45DB-AA71-31BC30C79731", "Status");
			this.MainTabControl.SuspendLayout();
			var indexOfDetails = this.MainTabControl.TabPages.IndexOf(this.MiscOptionsTabPage);
			if (indexOfDetails > -1)
			{
				this.MainTabControl.TabPages.Insert(this.fK84TabPage, indexOfDetails + 1);
			}
			this.MainTabControl.ResumeLayout(false);
		}

		public void LoadOriginalDeclarationTabPage()
		{
			if (OriginalDeclarationTabPage.Controls.Count == 0 && OriginalDeclarationTabPage.TabVisible)
			{
				fJobDeclarationUserControl = new CAJobDeclarationUserControl();
				fJobDeclarationUserControl.JobDeclaration = JobDeclaration;
				fJobDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				fJobDeclarationUserControl.Visible = false;
				OriginalDeclarationTabPage.Controls.Add(fJobDeclarationUserControl);
				fJobDeclarationUserControl.Visible = true;
				fJobDeclarationUserControl.SetDataBinding(JobDeclaration, "");
			}
		}

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.MainTabControl.SelectedTab == OriginalDeclarationTabPage)
			{
				LoadOriginalDeclarationTabPage();
			}
			else if (this.MainTabControl.SelectedTab == K84TabPage)
			{
				LoadK84TabPage();
			}
		}

		void LoadK84TabPage()
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

		protected override IBasePackingControl GetPackingUserControl()
		{
			return new CustomsPackingUserControl();
		}

		protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			return new CAImportSupplierHeaderUserControl();
		}

		protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			return new CAImportInvoiceLineUserControl();
		}
		protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl()
		{
			return new CAMiscOptionsUserControl();
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new CAImportMessagesUserControl();
		}
	}
}
