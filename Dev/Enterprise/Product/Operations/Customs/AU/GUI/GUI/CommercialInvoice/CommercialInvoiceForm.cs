using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI.CommercialInvoice
{
	public class CommercialInvoiceForm : Customs.GUI.CommercialInvoiceForm
	{
		#region Designer Stuff

		public CommercialInvoiceForm()
		{
			InitializeQuarantineTabPage();
		}

		#endregion

		public CommercialInvoiceForm(JobComInvoiceHeader header)
			: base(header)
		{
			InitializeQuarantineTabPage();
		}

		public new JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.Invoice; }
		}

		protected override CommonInvoiceHeaderUserControl GetHeaderUserControl()
		{
			return new InvoiceHeaderUserControl();
		}

		protected override BaseInvoiceLineUserControl GetNewInvoiceLineUserControl()
		{
			if (Invoice.IsImport)
			{
				return new AUCMRImportInvoiceLineUserControl();
			}
			else if (Invoice.JobDeclaration.IsQuarantine)
			{
				return new AUQuarantineInvoiceLineUserControl();
			}
			else
			{
				return new AUExportInvoiceLineUserControl();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			HookEvent();
			MessageTypeInfo_ValueChanged(null, EventArgs.Empty);
		}

		protected override void Dispose(bool disposing)
		{
			UnHookEvent();
			base.Dispose(disposing);
		}

		#region Event

		void QuarantineTabPage_LazyCreateControls(object sender, EventArgs e)
		{
			if (quarantineTabPage.Controls.Count == 0)
			{
				var tabControl = new ZTabControl() { Dock = System.Windows.Forms.DockStyle.Fill };
				quarantineTabPage.Controls.Add(tabControl);

				var manager = new QuarantineControlsManager();
				manager.Initialize(tabControl, BindingSource, Invoice);

				var tabPage = tabControl.SelectedTab as BaseDeclarationTabPage;

				if (tabPage != null)
				{
					tabPage.SetDataBinding(DataSource, string.Empty);
				}
			}
		}

		void HookEvent()
		{
			var invoice = Invoice;

			if (invoice != null)
			{
				invoice.JZ_MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
			}
		}

		void UnHookEvent()
		{
			var invoice = Invoice;

			if (invoice != null)
			{
				invoice.JZ_MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
			}
		}

		void MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var page = quarantineTabPage;

			if (page != null)
			{
				page.TabVisible = Invoice.IsQuarantine;
			}
		}

		#endregion

		#region Designer Generated

		void InitializeQuarantineTabPage()
		{
			this.quarantineTabPage = new BaseDeclarationTabPage();
			// 
			// QuarantineTabPage
			// 
			this.quarantineTabPage.CaptionResourceString = Res.GetData("f9c397ee-13b8-4753-a767-1e8a3b87157f", "Quarantine");
			this.quarantineTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.quarantineTabPage.Name = "QuarantineTabPage";
			this.quarantineTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.quarantineTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1101, 561, true);
			this.quarantineTabPage.TabIndex = 1;
			this.quarantineTabPage.Text = "Quarantine";
			this.quarantineTabPage.LazyCreateControls += QuarantineTabPage_LazyCreateControls;
			this.quarantineTabPage.UseVisualStyleBackColor = true;
			this.quarantineTabPage.CheckForChildrenControlsVisibilityChange = false;
			this.quarantineTabPage.CheckForNotifications = false;
			// 
			// MainTabControl
			// 
			this.MainTabControl.TabPages.Insert(quarantineTabPage, 1);
		}

		BaseDeclarationTabPage quarantineTabPage;

		#endregion
	}
}
