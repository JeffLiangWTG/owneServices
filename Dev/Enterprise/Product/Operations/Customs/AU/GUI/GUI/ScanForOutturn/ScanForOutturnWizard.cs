using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class ScanForOutturnWizard : ZChildForm
	{
		public ScanForOutturnWizard()
		{
			InitializeComponent();
		}

		public ScanForOutturnWizard(ScanForOutturnManager scanManager)
		{
			this.scanManager = scanManager;
			InitializeComponent();
			if (scanManager.IsStandaloneShipment)
			{
				RemoveShipmentSelectionStep();
			}

			this.SetDataBinding(scanManager.ScanWizardDataSource, "");

			WizardTabControl.Selecting += new TabControlCancelEventHandler(WizardTabControlSelecting);
			WizardTabControl.SelectedIndexChanged += new EventHandler(WizardTabControlSelectedIndexChanged);
			ReportZeroLandedButton.Visible = false;
			SurplusConsignmentsTabPage.TabVisible = false;
			this.FormClosing += CancelConfirmation;
		}
		protected ScanForOutturnManager scanManager;

		void CancelConfirmation(object sender, FormClosingEventArgs e)
		{
			if ((scanManager.IsOutturnCollectionCreatedAndHasMembers)
				&& (Globals.Message.ShowConfirmation(Res.GetString("2AD66199-6417-40B5-9A83-1240D54C4B71", "Are you sure you want to cancel outturn scan?"), Res.GetString("7BD8B39A-EAFC-44B5-9647-285A0E4B3E42", "Cancel Outturn Scan"), Res.GetString("E7EAE6D1-F2B1-4E76-B4BC-827267EDD4DC", "To continue, type:") + " ", "Yes", MessageBoxIcon.Question) != DialogResult.OK))
			{
				e.Cancel = true;
			}
		}

		void DirectScanningButton_Click(object sender, EventArgs e)
		{
			var manualScanForm = new ManualScanning(scanManager);
			manualScanForm.FormClosed += new FormClosedEventHandler(ManualScanFormClosed);
			ZFormModaliser.Show(manualScanForm, this);
		}

		void ShowSurplusTab()
		{
			if (NeedToShowSurplusTab)
			{
				NextStepButton.Text = Res.GetString("e1690840-7fc3-4184-b48a-8216f75dd7d9", "Next Step");
				NextStepButton.Font = new Font(NextStepButton.Font.FontFamily, 8, NextStepButton.Font.Unit);
				SurplusConsignmentsTabPage.TabVisible = true;
			}
		}

		void ManualScanFormClosed(object sender, FormClosedEventArgs e)
		{
			ParcelsScannedLabel.Text = Res.GetString("eb90ccd8-fc8b-47f7-80b4-f8138ab3d620", "Parcels scanned so far: {0}", scanManager.NumberOfManualParcelScanned);
			ShowSurplusTab();
		}

		void WizardTabControlSelectedIndexChanged(object sender, EventArgs e)
		{
			if ((WizardTabControl.SelectedTab == ScanningManagementTabPage && !NeedToShowSurplusTab) || WizardTabControl.SelectedTab == SurplusConsignmentsTabPage)
			{
				SetControlToFinishStep();
			}
		}

		public bool NeedToShowSurplusTab
		{
			get
			{
				if (scanManager.ScanWizardDataSource.ShipmentSelectorLineCollection != null
					&& scanManager.ScanWizardDataSource.ShipmentSelectorLineCollection.GetHLSShipmentIncludeInScan().Count() > 1
					&& scanManager.ScanWizardDataSource.SurplusOutturnCollection.Count > 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		void SetControlToFinishStep()
		{
			NextStepButton.Text = Res.GetString("f6890aab-eac8-420e-83cd-d823ffd55df2", "Finish, Generate and Send Outturn");
			NextStepButton.Font = new Font(NextStepButton.Font.FontFamily, 7, NextStepButton.Font.Unit);
			if (UnderbondSelectionTabPage != null)
			{
				UnderbondSelectionTabPage.SetReadOnly(true);
			}

			if (ShipmentSelectionTapPage != null)
			{
				ShipmentSelectionTapPage.SetReadOnly(true);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			if (scanManager.ScanWizardDataSource.UnderbondSelectorLineCollection.Count > 0)
			{
				UnderbondSelectionGrid.Select(0);
			}
		}

		void WizardTabControlSelecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPage == ShipmentSelectionTapPage || e.TabPage == ScanningManagementTabPage)
			{
				if (!ValidateSelectedUnderbond())
				{
					e.Cancel = true;
					return;
				}
				OnAfterSelectingUnderbond();
			}
			else
			{
				ReportZeroLandedButton.Visible = false;
			}

			if (e.TabPage == ScanningManagementTabPage && !scanManager.IsStandaloneShipment)
			{
				if (!ValidateSelectedShipment())
				{
					e.Cancel = true;
					return;
				}

				ReportZeroLandedButton.Visible = false;
			}
		}

		protected virtual void OnAfterSelectingUnderbond()
		{
		}

		protected bool ValidateSelectedShipment()
		{
			bool isShipmentSelected = scanManager.ScanWizardDataSource.IsShipmentSelected;

			if (!isShipmentSelected)
			{
				Globals.Message.Show(
							   Res.GetString("22ec7290-87bd-48c9-89fe-ef7d4c83ee50", "Please select at least one shipment"),
							   Res.GetString("22ec7290-87bd-48c9-89fe-ef7d4c83ee51", "Shipment not selected error"),
							   MessageBoxButtons.OK,
							   MessageBoxIcon.Error);
				return false;
			}

			scanManager.SetSelectedShipment();
			string errorMessage = scanManager.ValidateStandAloneUnderbond();

			if (!string.IsNullOrEmpty(errorMessage))
			{
				Globals.Message.Show(
						errorMessage,
						Res.GetString("22ec7290-87bd-48c9-89fe-ef7d3c83ee55", "Underbond error"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);

				return false;
			}

			return TryLockScan();
		}

		bool TryLockScan()
		{
			string concurrencyErrorMessage = scanManager.TryLockScan();

			if (!string.IsNullOrEmpty(concurrencyErrorMessage))
			{
				Globals.Message.Show(
						concurrencyErrorMessage,
						Res.GetString("1fa71f44-1bb2-4be9-982e-07500088244a", "Access Error"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);

				return false;
			}

			return true;
		}

		protected bool ValidateSelectedUnderbond()
		{
			var selectedItems = UnderbondSelectionGrid.GetSelectedElements<UnderbondSelectorLine>();
			if (selectedItems.Length != 1)
			{
				Globals.Message.Show(
					Res.GetString("22ec7290-87bd-48c9-89fe-ef7d3c83ee57", "Please select one, and only one, underbond to process."),
					Res.GetString("22ec7290-87bd-48c9-89fe-ef7d3c83ee56", "Underbond select error"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);

				return false;
			}

			scanManager.SelectedUnderbond = selectedItems[0];
			string errorMessage = scanManager.ValidateSelectedUnderbond();

			if (!string.IsNullOrEmpty(errorMessage))
			{
				Globals.Message.Show(
						errorMessage,
						Res.GetString("22ec7290-87bd-48c9-89fe-ef7d3c83ee55", "Underbond error"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
				return false;
			}

			return !scanManager.IsStandaloneShipment || TryLockScan();
		}

		void RemoveShipmentSelectionStep()
		{
			WizardTabControl.SelectedTab = UnderbondSelectionTabPage;
			WizardTabControl.TabPages.Remove(ShipmentSelectionTapPage);
			ShipmentSelectionTapPage.Dispose();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void NextStepButton_Click(object sender, EventArgs e)
		{
			if (WizardTabControl.SelectedTab != ScanningManagementTabPage && WizardTabControl.SelectedTab != SurplusConsignmentsTabPage)
			{
				if (ValidateSelectedUnderbond())
				{
					ShowNextStep();
				}
			}
			else if (WizardTabControl.SelectedTab == ScanningManagementTabPage && !NeedToShowSurplusTab)
			{
				if (scanManager.HasScanHappened)
				{
					SetControlToFinishStep();
					SaveOutturn();
				}
				else
				{
					Globals.Message.Show(
							Res.GetString("12B70B4E-F342-4430-A8E8-AF939AFD59BB", "Please either load a scan result file back or complete manual scanning before proceeding to generate outturn message"),
							Res.GetString("2C846204-5D66-442B-9B74-CFEE562D061C", "Missing Load Error"),
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
				}
			}
			else
			{
				ProcessSurplusTab();
			}
		}

		protected void ProcessSurplusTab()
		{
			if (WizardTabControl.SelectedTab == ScanningManagementTabPage)
			{
				if (ValidateSelectedUnderbond())
				{
					ShowNextStep();
				}
			}
			else if (WizardTabControl.SelectedTab == SurplusConsignmentsTabPage)
			{
				if (!HasInvalidShipmentInSurplusConsignment())
				{
					SaveOutturn();
				}
				else
				{
					Globals.Message.Show(
							Res.GetString("4F7D8A8A-3509-4CC7-A267-7E69B42C702F", "Please select a valid shipment for each surplus consignment before proceeding to generate outturn message"),
							Res.GetString("89D8ACDC-2B7F-485B-B9CC-81370F72C434", "Invalid shipment"),
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
				}
			}
		}

		bool HasInvalidShipmentInSurplusConsignment()
		{
			foreach (SurplusOutturnLine surplusOutturnLine in scanManager.ScanWizardDataSource.SurplusOutturnCollection)
			{
				if (surplusOutturnLine.Shipment.IsEmpty || surplusOutturnLine.ShipmentInfo.Notifications.GetNotifications(CargoWise.EntityFramework.NotificationType.MessageError).Count() > 0)
				{
					return true;
				}
			}
			return false;
		}

		protected void SaveOutturn()
		{
			var errorMessage = scanManager.SaveOutturnResult(new SendsMessagesToCustomsGUI());

			if (!string.IsNullOrEmpty(errorMessage))
			{
				Globals.Message.Show(
						errorMessage,
						Res.GetString("22ec7290-87bd-48c9-89fe-ef7d3c83ee45", "Save results"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
			}
			this.FormClosing -= CancelConfirmation;
			this.Close();
		}

		protected void ShowNextStep()
		{
			WizardTabControl.SelectedIndex += 1;
		}

		void WriteFileToScanButton_Click(object sender, EventArgs e)
		{
			using (var saveFileToScanFolderDialog = new ZFolderBrowserDialog())
			{
				InitializeSaveToFileScanFolderDialog(saveFileToScanFolderDialog);

				if (saveFileToScanFolderDialog.ShowDialog() == DialogResult.OK)
				{
					AUCustomsDataRegistry.Instance.SaveToOutturnScanFileFolder.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, saveFileToScanFolderDialog.UnmappedSelectedPath);

					var exportHelper = new OutturnLineCollectionExportHelper(scanManager.ManifestCollection);
					string exportFilePath = Path.Combine(saveFileToScanFolderDialog.UnmappedSelectedPath, scanManager.ExportFileName);
					var errorMessage = exportHelper.ExportToScanner(exportFilePath);

					if (string.IsNullOrEmpty(errorMessage))
					{
						string showFilePath = Path.Combine(saveFileToScanFolderDialog.UnmappedSelectedPath, scanManager.ExportFileName);
						FileWritingLabel.Text = showFilePath;
						WriteFileToScanButton.Enabled = false;
					}
					else
					{
						Globals.Message.Show(
							   errorMessage,
							   Res.GetString("22ec7290-97bd-48c9-89fe-ef7d3c83ee51", "File writing error"),
							   MessageBoxButtons.OK,
							   MessageBoxIcon.Error);
					}
				}
			}
		}

		void InitializeSaveToFileScanFolderDialog(ZFolderBrowserDialog saveFileToScanFolderDialog)
		{
			saveFileToScanFolderDialog.CreateDirectory = false;
			saveFileToScanFolderDialog.RequireMappablePath = true;
			saveFileToScanFolderDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
			saveFileToScanFolderDialog.ShowNewFolderButton = true;

			var savedFolder = AUCustomsDataRegistry.Instance.SaveToOutturnScanFileFolder.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			if (!string.IsNullOrEmpty(savedFolder) && Directory.Exists(savedFolder))
			{
				saveFileToScanFolderDialog.SelectedPath = savedFolder;
			}
		}

		void LoadFileFromScanButton_Click(object sender, EventArgs e)
		{
			using (var loadScannedFileDialog = new ZOpenFileDialog())
			{
				InitializeLoadFromScannedFileDialog(loadScannedFileDialog);

				if (loadScannedFileDialog.ShowDialog() == DialogResult.OK)
				{
					var resultOutturnCollection = scanManager.GetNewOutturnLineCollection();
					var importHelper = new OutturnLineCollectionImportHelper(resultOutturnCollection);
					importHelper.ImportFromScanner(loadScannedFileDialog.UnmappedFileName);
					AUCustomsDataRegistry.Instance.LoadFromOutturnScanFilePath.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, loadScannedFileDialog.UnmappedFileName);
					scanManager.MergeAutomaticScanResults(resultOutturnCollection);
					FileLoadingLabel.Text = loadScannedFileDialog.UnmappedFileName;
					LoadFileFromScanButton.Enabled = false;
				}
			}
			ShowSurplusTab();
		}

		static void InitializeLoadFromScannedFileDialog(ZOpenFileDialog loadScannedFileDialog)
		{
			loadScannedFileDialog.AddExtension = true;
			loadScannedFileDialog.CheckFileExists = true;
			loadScannedFileDialog.CheckPathExists = true;
			loadScannedFileDialog.DefaultExt = "";
			loadScannedFileDialog.DereferenceLinks = true;
			loadScannedFileDialog.Filter = "eManifest file (*.csv)|*.csv";
			loadScannedFileDialog.FilterIndex = 1;
			loadScannedFileDialog.InitialDirectory = "";
			loadScannedFileDialog.Multiselect = false;
			loadScannedFileDialog.ReadOnlyChecked = false;
			loadScannedFileDialog.RestoreDirectory = true;
			loadScannedFileDialog.ShowHelp = false;
			loadScannedFileDialog.SupportMultiDottedExtensions = false;
			loadScannedFileDialog.Title = "";
			loadScannedFileDialog.ValidateNames = true;

			var loadFilePath = AUCustomsDataRegistry.Instance.LoadFromOutturnScanFilePath.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			if (!string.IsNullOrEmpty(loadFilePath))
			{
				loadScannedFileDialog.FileName = loadFilePath;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (scanManager != null)
				{
					scanManager.Dispose();
					scanManager = null;
				}
			}
			base.Dispose(disposing);
		}

		void SurplusConsignmentsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SurplusConsignmentsGrid = new ZArchitecture.ZGrid();
			this.SurplusConsignmentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SurplusConsignmentsGrid)).BeginInit();
			this.SurplusConsignmentsGrid.SuspendLayout();
			this.SurplusConsignmentsTabPage.Controls.Add(this.SurplusConsignmentsGrid);
			// 
			// SurplusConsignmentsGrid
			// 
			this.SurplusConsignmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SurplusConsignmentsGrid, "SurplusOutturnCollection");
			this.SurplusConsignmentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("03c8af2d-862f-422d-8010-77e84ca64289", "Consignment Ref");
			zTextBoxColumnStyleInfo18.ColumnName = "ConsignmentRef";
			zTextBoxColumnStyleInfo18.IsMandatory = true;
			zTextBoxColumnStyleInfo18.IsReadOnly = true;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.BindToList = "ShipmentList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("5f9762aa-0cbd-4266-80aa-af90bdd20caf", "Shipment");
			zDropEditColumnStyleInfo1.ColumnName = "Shipment";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("679fd8cd-7b9a-4749-bb4e-fe8dbdb8e0c8", "Description");
			zTextBoxColumnStyleInfo19.ColumnName = "Description";
			zTextBoxColumnStyleInfo19.IsMandatory = true;
			zTextBoxColumnStyleInfo19.IsReadOnly = true;
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(225);
			this.SurplusConsignmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.SurplusConsignmentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SurplusConsignmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.SurplusConsignmentsGrid.CopySelectedRowsAllowed = true;
			this.SurplusConsignmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SurplusConsignmentsGrid.GridId = "2c6eee39-ce6b-4abf-9429-c5c9260a15bc";
			this.SurplusConsignmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SurplusConsignmentsGrid.LayoutKey = "zGrid1";
			this.SurplusConsignmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SurplusConsignmentsGrid.Name = "SurplusConsignmentsGrid";
			this.SurplusConsignmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 265, true);
			this.SurplusConsignmentsGrid.TabIndex = 6;
			this.SurplusConsignmentsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SurplusConsignmentsGrid)).EndInit();
			this.SurplusConsignmentsGrid.ResumeLayout(false);
			this.SurplusConsignmentsGrid.PerformLayout();
			this.SurplusConsignmentsTabPage.ResumeLayout(true);
		}
	}
}
