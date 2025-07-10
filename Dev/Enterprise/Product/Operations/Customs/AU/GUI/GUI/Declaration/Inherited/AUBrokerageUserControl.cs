using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class AUBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public AUBrokerageUserControl()
		{
			InitializeComponent();
			InitializeCOLSTabPage();
			this.MainTabControl.SelectedIndexChanging += MainTabControl_SelectedIndexChanging;
		}

		#region Overriden for getting user controls

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new AUDeclarationUserControl();
		}

		protected override BaseCustomsCusContainersUserControl GetContainerUserControl()
		{
			return new AUContainerUserControl();
		}

		protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			if (IsImportCMR)
			{
				return new AUCMRSupplierHeaderUserControl();
			}
			else if (IsQuarantine)
			{
				return new AUQuarantineSupplierHeaderUserControl();
			}
			else if (IsDrawback)
			{
				return new AUDrawbackHeaderUserControl();
			}
			else
			{
				return new AUOtherSupplierHeaderUserControl();
			}
		}

		protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			BaseInvoiceLineUserControl result;

			if (IsImportCMR)
			{
				result = new AUCMRImportInvoiceLineUserControl();
			}
			else if (JobDeclaration.IsImport)
			{
				result = new AUEdificeImportInvoiceLinesUserControl();
			}
			else if (IsQuarantine)
			{
				result = new AUQuarantineInvoiceLineUserControl();
			}
			else if (IsDrawback)
			{
				result = new AUDrawbackInvoiceLineUserControl();
			}
			else
			{
				result = new AUExportInvoiceLineUserControl();
			}

			return result;
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			BaseCustomsEntryUserControl result;
			var declaration = (JobDeclaration)JobDeclaration;
			if (declaration.IsImport || (IsEXPDeclaration && declaration.DeclarationExportCusEntryNumber == null))
			{
				result = new AUImportDiscardedMessageUserControl();
			}
			else
			{
				result = new ExportMessageUserControl();
			}
			return result;
		}

		protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl()
		{
			BaseMiscOptionsUserControl result;
			if (IsImportCMR || IsDrawback)
			{
				result = new AUCMRMiscOptionsUserControl();
			}
			else if (IsQuarantine)
			{
				result = new AUQuarantineEdificeMiscOptionsUserControl();
			}
			else
			{
				result = new AUEdificeMiscOptionsUserControl();
			}
			return result;
		}

		protected override IBasePackingControl GetPackingUserControl()
		{
			return new PackingUserControl();
		}

		#endregion

		protected ZBool IsImportCMR
		{
			get
			{
				var declaration = (JobDeclaration)JobDeclaration;
				return declaration != null && declaration.IsImportCMR;
			}
		}

		protected ZBool IsQuarantine
		{
			get
			{
				var declaration = (JobDeclaration)JobDeclaration;
				return declaration != null && declaration.IsQuarantine;
			}
		}

		protected ZBool IsDrawback
		{
			get
			{
				var declaration = (JobDeclaration)JobDeclaration;
				return declaration != null && declaration.IsDrawback;
			}
		}

		protected ZBool IsEXPDeclaration
		{
			get
			{
				var declaration = (JobDeclaration)JobDeclaration;
				return declaration != null && declaration.IsEXPDeclaration;
			}
		}

		protected ZBool EnableCOLS
		{
			get
			{
				var declaration = (JobDeclaration)JobDeclaration;
				var entryNumber = declaration?.EntryHeader?.EntryNumber ?? ZString.Empty;
				return !entryNumber.IsEmpty && declaration.IsImport && QuarantineColsHeader.IsCOLSFunctionEnabled;
			}
		}

		protected override Customs.GUI.SendsMessagesToCustomsGUI NewSendsMessagesToCustomsGUI()
		{
			return new SendsMessagesToCustomsGUI();
		}

		public override Customs.Business.BaseJobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration; }
			set
			{
				UnhookJobDeclarationEvents();
				base.JobDeclaration = value;
				if (value != null)
				{
					var factory = value.Factory;
					factory.AddFetchHint(StmNoteSchema.ST_ParentID, value.PK);
					HookJobDeclarationEvents();
					ShowOrHideInvoiceGroupTabPage();
					ShowOrHidePackingTabPage();
					ShowOrHideCOLSTabPage();
				}
			}
		}

		void HookJobDeclarationEvents()
		{
			JobDeclaration.JE_MessageTypeInfo.ValueChanged += IsImportCMRChanged;
			JobDeclaration.JE_ApplicationCodeInfo.ValueChanged += IsImportCMRChanged;
			JobDeclaration.JE_MessageSubTypeInfo.ValueChanged += JE_MessageSubTypeInfo_ValueChanged;
			JobDeclaration.JE_TransportModeInfo.ValueChanged += JE_TransportModeInfo_ValueChanged;
		}

		void UnhookJobDeclarationEvents()
		{
			if (JobDeclaration != null)
			{
				JobDeclaration.JE_MessageTypeInfo.ValueChanged -= IsImportCMRChanged;
				JobDeclaration.JE_ApplicationCodeInfo.ValueChanged -= IsImportCMRChanged;
				JobDeclaration.JE_MessageSubTypeInfo.ValueChanged -= JE_MessageSubTypeInfo_ValueChanged;
				JobDeclaration.JE_TransportModeInfo.ValueChanged -= JE_TransportModeInfo_ValueChanged;
			}
		}

		void JE_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHidePackingTabPage();
		}

		void JE_MessageSubTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHidePackingTabPage();
		}

		#region COLS Tab Page

		public BaseDeclarationTabPage COLSTabPage;

		void LoadCOLSTabPage()
		{
			if (cOLSUserControl == null && cOLSMessageUserControl == null && COLSTabPage.Controls.Count == 0)
			{
				cOLSMessageUserControl = new AUCOLSEntryCreationMessageUserControl();
				cOLSUserControl = new AUCOLSUserControl();
				this.COLSTabPage.SuspendLayout();
				cOLSMessageUserControl.SuspendLayout();
				cOLSUserControl.SuspendLayout();
				cOLSMessageUserControl.Dock = DockStyle.Fill;
				cOLSMessageUserControl.Visible = false;
				cOLSUserControl.Dock = DockStyle.Fill;
				cOLSUserControl.Visible = false;
				COLSTabPage.Controls.Add(cOLSUserControl);
				COLSTabPage.Controls.Add(cOLSMessageUserControl);
				this.COLSTabPage.ResumeLayout(true);
				this.COLSTabPage.PerformLayout();
				cOLSMessageUserControl.ResumeLayout(true);
				cOLSMessageUserControl.PerformLayout();
				cOLSUserControl.ResumeLayout(true);
				cOLSUserControl.PerformLayout();
				cOLSUserControl.SetDataBinding(JobDeclaration, "");
			}
		}
		public AUCOLSUserControl cOLSUserControl;
		public AUCOLSEntryCreationMessageUserControl cOLSMessageUserControl;

		void InitializeCOLSTabPage()
		{
			this.COLSTabPage = new BaseDeclarationTabPage();
			this.MainTabControl.SuspendLayout();
			this.COLSTabPage.SuspendLayout();
			COLSTabPage.Dock = DockStyle.Fill;
			COLSTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23);
			COLSTabPage.Name = "COLSTabPage";
			COLSTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 629);
			COLSTabPage.TabIndex = 1;
			COLSTabPage.Text = Res.GetString("140f27f0-445b-4f72-b83b-fd6212eb9c54", "COLS");
			var indexOfMsg = this.MainTabControl.TabPages.IndexOf(this.MessagesTabPage);
			if (indexOfMsg > -1)
			{
				this.MainTabControl.TabPages.Insert(this.COLSTabPage, indexOfMsg + 1);
			}
			this.MainTabControl.ResumeLayout(true);
			this.MainTabControl.PerformLayout();
			this.COLSTabPage.ResumeLayout(true);
			this.COLSTabPage.PerformLayout();
		}

		void MainTabControl_SelectedIndexChanging(object sender, EventArgs e)
		{
			if (this.MainTabControl.SelectedTab == COLSTabPage)
			{
				LoadCOLSTabPage();
				var hasColsHeader = auJobDeclaration.QuarantineCOLSHeader != null;
				if (!hasColsHeader && QueryUserToCreateCOLSEntry())
				{
					hasColsHeader = auJobDeclaration.CreateCOLSHeaderIfRequired() != null;
					cOLSUserControl.SetDataBinding(JobDeclaration, "");
				}
				cOLSUserControl.Visible = hasColsHeader;
				cOLSMessageUserControl.Visible = !hasColsHeader;
			}
		}

		JobDeclaration auJobDeclaration => (JobDeclaration)JobDeclaration;

		bool QueryUserToCreateCOLSEntry()
		{
			return Globals.Message.Show(DoYouWantToCreateCOLSEntry, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
		}

		public const string DoYouWantToCreateCOLSEntry = "Do you want to create a COLS entry at this time?";

		#endregion

		#region Group Invoice Tab Visiblity

		void IsImportCMRChanged(object sender, EventArgs e)
		{
			ShowOrHideInvoiceGroupTabPage();
			ShowOrHidePackingTabPage();
			ShowOrHideCOLSTabPage();
		}

		void ShowOrHideInvoiceGroupTabPage()
		{
			InvoiceGroupingTabPage.TabRelevant = (IsImportCMR && !JobDeclaration.IsExWarehouse);
		}

		void ShowOrHideCOLSTabPage()
		{
			COLSTabPage.TabRelevant = EnableCOLS;
		}

		#endregion

		#region Dispose

		readonly System.ComponentModel.Container components;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookJobDeclarationEvents();

				components?.Dispose();
				InvoiceGroupingTabPage?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
