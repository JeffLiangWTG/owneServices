using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class LowValueShipmentsUserControl : BaseCustomsBrokerageUserControl
	{
		public LowValueShipmentsUserControl()
		{
			InitializeComponent();
			InitializeK84TabPage();
			DisposeRedundantTabs();
			SetCaptions();
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

		void SetCaptions()
		{
			DeclarationTabPage.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("5161559e-cb73-45f4-8ae1-bde96f9540ff", "Low Value Shipments");
			MessagesTabPage.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("2f8cf163-9d08-45b9-9fd4-c5704a479dbe", "Messages");
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new LVSHeaderUserControl();
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new CAImportMessagesUserControl();
		}

		void InitializeK84TabPage()
		{
			fK84TabPage = new BaseDeclarationTabPage();
			fK84TabPage.LazyCreateControls += LazyCreateControlsFired;
			fK84TabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			fK84TabPage.Name = "K84TabPage";
			fK84TabPage.Size = ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			fK84TabPage.TabIndex = 8;
			fK84TabPage.Text = Res.GetString("511067A2-CD1E-4026-9E11-E9EE20EA08A2", "Status");
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
			if (K84TabPage.Controls.Count == 0)
			{
				fK84UserControl = new K84UserControl();
				fK84UserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				fK84UserControl.Visible = false;
				K84TabPage.Controls.Add(fK84UserControl);
				fK84UserControl.Visible = true;
				fK84UserControl.SetDataBinding(JobDeclaration, "");
			}
		}

		new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		protected override void OnJobDeclarationSet()
		{
			base.OnJobDeclarationSet();
			JobDeclaration.RunMergeForLVSIfRequired();
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
	}
}
