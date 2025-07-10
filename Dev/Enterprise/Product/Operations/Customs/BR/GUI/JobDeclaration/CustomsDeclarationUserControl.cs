using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class CustomsDeclarationUserControl : Customs.GUI.BaseCustomsDeclarationUserControl, ISupportMultipleResourceStringDataSupporter
	{
		public CustomsDeclarationUserControl()
		{
			InitializeComponent();
			InitializeCustomsOfficesControlsLayout();
			defaultShipmentDetailsGroupBoxCaption = ShipmentDetailsGroupBox.CaptionResourceString;
			defaultShipmentDetailsGroupBoxLocation = ShipmentDetailsGroupBox.Location;
		}

		void InitializeCustomsOfficesControlsLayout()
		{
			if (!DesignMode)
			{
				var rightPanel = new ZArchitecture.GUI.ZPanel();

				rightPanel.SuspendLayout();
				RightTabControl.SuspendLayout();
				CustomsOfficesGroupBox.SuspendLayout();

				rightPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;

				ControlDpiScalingHelper.SetHeight(rightPanel, RightTabControl.Height + CustomsOfficesGroupBox.Height, false);
				ControlDpiScalingHelper.SetWidth(rightPanel, RightTabControl.Width, false);

				rightPanel.Location = RightTabControl.Location;

				Controls.Remove(RightTabControl);
				Controls.Remove(CustomsOfficesGroupBox);

				CustomsOfficesGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
				RightTabControl.Dock = System.Windows.Forms.DockStyle.Fill;

				rightPanel.Controls.Add(RightTabControl);
				rightPanel.Controls.Add(CustomsOfficesGroupBox);
				Controls.Add(rightPanel);

				CustomsOfficesGroupBox.ResumeLayout(true);
				CustomsOfficesGroupBox.PerformLayout();
				RightTabControl.ResumeLayout(true);
				RightTabControl.PerformLayout();
				rightPanel.ResumeLayout(true);
				rightPanel.PerformLayout();
			}
		}

		#region Override Implements

		JobDeclaration Declaration => (JobDeclaration)base.JobDeclaration;

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			var declaration = Declaration;
			if (declaration != null)
			{
				var isExport = declaration.IsExport;
				var isImportExcludingLicense = declaration.IsImportExcludingLicense;
				var isImportLicense = declaration.IsImportLicense;
				var isImportLicenseOrLPCO = isImportLicense || declaration.IsLPCO;
				var isImportSiscomex = declaration.IsImportSiscomex;
				var isImportOnly = declaration.IsImportOnly;

				DispatchInstructionTabPage.TabVisible = isImportExcludingLicense;
				ProcessRelatedTabPage.TabVisible = isImportExcludingLicense;
				LicensesTabPage.TabVisible = isImportSiscomex;

				ShipmentDetailsGroupBox.CaptionResourceString = isImportLicenseOrLPCO ? Res.GetData("84BE99DA-C41C-4C2D-9A35-7045B9F6B727", "Details") : defaultShipmentDetailsGroupBoxCaption;
				ShipmentDetailsGroupBox.Location = isImportLicenseOrLPCO ? ControlDpiScalingHelper.NewScaledPoint(264, 56, true) : defaultShipmentDetailsGroupBoxLocation;
				ImportLicenseOfficesUserControl.Visible = isImportLicense;
				TransportDetailsGroupBox.Visible = !isImportLicenseOrLPCO;
				NumbersTabPage.TabVisible = !isImportLicenseOrLPCO;
				OrganisationsTabPage.TabVisible = !isImportLicenseOrLPCO;
				DocsTabPage.TabVisible = !isImportLicenseOrLPCO;

				var customsOfficesGroupBoxHeight = isImportLicense || isImportOnly ? 160 : 201;
				CustomsOfficesGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(customsOfficesGroupBoxHeight);
				CustomsOfficesGroupBox.Visible = isExport || declaration.IsImport;
				CustomsOfficesTabControl.Visible = isExport;
				ImportOfficesUserControl.Visible = isImportOnly;
				ImportSiscomexOfficesUserControl.Visible = isImportSiscomex;

				using (processRelatedUserControl1.NumbersGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					processRelatedUserControl1.NumbersGrid.SetAllColumnsVisible(false);
					processRelatedUserControl1.NumbersGrid.SetColumnVisible(true, GetAvailableColumnsForProcessRelated(Declaration));
				}
			}
		}

		readonly ResourceStringData defaultShipmentDetailsGroupBoxCaption;
		System.Drawing.Point defaultShipmentDetailsGroupBoxLocation;

		protected override void SetRightTabControlSelectTab()
		{
			if (Declaration?.IsImportLicense ?? false)
			{
				RightTabControl.SelectedTab = OrdersTabPage;
			}
			else if (Declaration?.IsLPCO ?? false)
			{
				RightTabControl.SelectedTab = ShipmentCustomFieldsPage;
			}
			else
			{
				RightTabControl.SelectedTab = DocsTabPage;
			}
		}

		string[] GetAvailableColumnsForProcessRelated(JobDeclaration declaration)
		{
			return declaration.IsImportOnly ? availableColumnsForImport : availableColumnsForImportSiscomex;
		}

		readonly string[] availableColumnsForImportSiscomex = new string[]
		{
			CusEntryNumber.Schema.CE_EntryType,
			CusEntryNumber.Schema.CE_EntryNum,
			CusEntryNumber.Schema.CE_EntryLineReference,
			CusEntryNumber.Schema.CE_IssueDate,
		};

		readonly string[] availableColumnsForImport = new string[]
		{
			CusEntryNumber.Schema.CE_EntryType,
			CusEntryNumber.Schema.CE_EntryNum,
		};

		protected override bool IsJE_MasterBillForSeaBoundTextBoxVisible => base.IsJE_MasterBillForSeaBoundTextBoxVisible || (Declaration != null && (Declaration.IsRiver || Declaration.IsLake));

		protected override bool IsVoyageFlightNoVisible => base.IsVoyageFlightNoVisible || (Declaration != null && (Declaration.IsRiver || Declaration.IsLake));

		public ISupportMultipleResourceStringData SupportMultipleResourceStringData => JobDeclaration as JobDeclaration;

		#endregion
	}
}
