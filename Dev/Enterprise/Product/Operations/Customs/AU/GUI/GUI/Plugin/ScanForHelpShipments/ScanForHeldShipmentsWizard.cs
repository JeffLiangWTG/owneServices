using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class ScanForHeldShipmentsWizard : ZChildForm
	{
		public ScanForHeldShipmentsWizard()
			: base()
		{
			InitializeComponent();
		}

		public ScanForHeldShipmentsWizard(ScanForOutturnHeldShipmentManager heldScanManager)
			: base(heldScanManager)
		{
			InitializeComponent();
		}

		ScanForOutturnHeldShipmentManager Manager
		{
			get { return (ScanForOutturnHeldShipmentManager)BusinessEntity; }
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void FinnishButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void DirectScanningButton_Click(object sender, EventArgs e)
		{
			if (IsOKToProceed())
			{
				Manager.UpdateManifestCollectionIfNeeded();
				var manualScanForm = new ManualScanning(Manager);
				manualScanForm.FormClosed += new FormClosedEventHandler(ManualScanFormClosed);
				ZFormModaliser.Show(manualScanForm, this);
			}
		}

		bool IsOKToProceed()
		{
			var result = true;
			if (Manager.HasErrors)
			{
				result = false;
				Globals.Message.ShowError("Cannot proceed until all errors are fixed.");
			}
			return result;
		}

		void ManualScanFormClosed(object sender, FormClosedEventArgs e)
		{
			parcelsScannedLabel.Text = Declaration.GUI.Res.GetString("eb90ccd8-fc8b-47f7-80b4-f8138ab3d620", "Parcels scanned so far: {0}", Manager.NumberOfManualParcelScanned);
			if (ValidateAndSave() == ContinueWithSave.Yes)
			{
				Manager.UpdateManifestCollectionIfNeeded();
			}
		}

		void WriteFileToScanButton_Click(object sender, EventArgs e)
		{
			if (IsOKToProceed())
			{
				using (var saveFileToScanFolderDialog = new ZFolderBrowserDialog())
				{
					InitializeSaveToFileScanFolderDialog(saveFileToScanFolderDialog);

					if (saveFileToScanFolderDialog.ShowDialog() == DialogResult.OK)
					{
						if (string.IsNullOrEmpty(AUCustomsDataRegistry.Instance.SaveToOutturnScanFileFolder.Value))
						{
							AUCustomsDataRegistry.Instance.SaveToOutturnScanFileFolder.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, saveFileToScanFolderDialog.UnmappedSelectedPath);
						}

						var exportHelper = new OutturnLineCollectionExportHelper(Manager.ManifestCollection);
						string exportFilePath = Path.Combine(saveFileToScanFolderDialog.UnmappedSelectedPath, Manager.ExportFileName);
						var errorMessage = exportHelper.ExportToScanner(exportFilePath);

						if (string.IsNullOrEmpty(errorMessage))
						{
							string showFilePath = Path.Combine(saveFileToScanFolderDialog.UnmappedSelectedPath, Manager.ExportFileName);
							fileWritingLabel.Text = showFilePath;
							writeFileToScanButton.Enabled = false;
						}
						else
						{
							Globals.Message.Show(
								   errorMessage,
								   Declaration.GUI.Res.GetString("22ec7290-97bd-48c9-89fe-ef7d3c83ee51", "File writing error"),
								   MessageBoxButtons.OK,
								   MessageBoxIcon.Error);
						}
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
			if (IsOKToProceed())
			{
				using (var loadScannedFileDialog = new ZOpenFileDialog())
				{
					InitializeLoadFromScannedFileDialog(loadScannedFileDialog);

					if (loadScannedFileDialog.ShowDialog() == DialogResult.OK)
					{
						var resultOutturnCollection = Manager.GetNewOutturnLineCollection();
						var importHelper = new OutturnLineCollectionImportHelper(resultOutturnCollection);
						importHelper.ImportFromScanner(loadScannedFileDialog.UnmappedFileName);
						var result = Manager.MergeAutomaticScanResults(resultOutturnCollection);
						fileLoadingLabel.Text = loadScannedFileDialog.UnmappedFileName;
						loadFileFromScanButton.Enabled = false;

						if (string.IsNullOrEmpty(AUCustomsDataRegistry.Instance.LoadFromOutturnScanFilePath.Value))
						{
							AUCustomsDataRegistry.Instance.LoadFromOutturnScanFilePath.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, loadScannedFileDialog.UnmappedFileName);
						}

						if (!string.IsNullOrEmpty(result))
						{
							Globals.Message.ShowWarning(result, "UNMATCHED CONSIGNMENTS");
						}
						if (ValidateAndSave() == ContinueWithSave.Yes)
						{
							Manager.UpdateManifestCollectionIfNeeded();
						}
					}
				}
			}
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

		static ScanForHeldShipmentsWizard()
		{
			ZFormStrategy.AddFormTypeThatCanBeCreatedDuringDbTransaction(typeof(ScanForHeldShipmentsWizard));
		}
	}
}
