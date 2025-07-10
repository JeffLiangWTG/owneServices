using System;
using CargoWise.Types;
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
	public partial class B2AdjustmentsCustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public B2AdjustmentsCustomsBrokerageUserControl()
		{
			InitializeComponent();
			InitializeK84TabPage();
			DisposeRedundantTabs();
			this.MainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
			MessagesTabPage.GetExtension<ILabelCaptionRenderer>().Caption = Enterprise.Customs.CA.GUI.Res.GetString("53c7f3d0-19eb-4fe1-bf51-42e093b9dde5", "Messages");
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new B2AdjustmentsUserControl();
		}

		new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			this.b2LineAsAccountedForUserControl.SetDataBinding(JobDeclaration, "");
			this.b2LineAsClaimedForUserControl.SetDataBinding(JobDeclaration, "");
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			var declaration = JobDeclaration;
			var isB3X = declaration?.IsB3X ?? ZBool.False;
			this.MessagesTabPage.TabVisible = isB3X;
			this.K84TabPage.TabVisible = !isB3X;
		}

		void DisposeRedundantTabs()
		{
			InvoicesTabPage.Dispose();
			PackingTabPage.Dispose();
			InvoiceLinesTabPage.Dispose();
			InvoiceGroupingTabPage.Dispose();
			MiscOptionsTabPage.Dispose();
			ContainerTabPage.Dispose();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fK84UserControl != null)
				{
					fK84UserControl.Dispose();
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

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
			if (sender == K84TabPage)
			{
				LoadK84TabPage();
			}
			else
			{
				base.LazyCreateControlsFired(sender, e);
			}
		}

		void InitializeK84TabPage()
		{
			fK84TabPage = new BaseDeclarationTabPage();
			fK84TabPage.LazyCreateControls += LazyCreateControlsFired;
			fK84TabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			fK84TabPage.Name = "K84TabPage";
			fK84TabPage.Size = ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			fK84TabPage.TabIndex = 8;
			fK84TabPage.Text = Res.GetString("B601D84F-F60F-476A-A11D-F94CBCBC5385", "Status");
			this.MainTabControl.SuspendLayout();
			var indexOfDetails = this.MainTabControl.TabPages.IndexOf(this.B2LineAsClaimedForTabPage);
			if (indexOfDetails > -1)
			{
				this.MainTabControl.TabPages.Insert(this.fK84TabPage, indexOfDetails + 1);
			}
			this.MainTabControl.ResumeLayout(false);
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

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.MainTabControl.SelectedTab == K84TabPage)
			{
				LoadK84TabPage();
			}
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new B3XMessagesUserControl();
		}
	}
}
