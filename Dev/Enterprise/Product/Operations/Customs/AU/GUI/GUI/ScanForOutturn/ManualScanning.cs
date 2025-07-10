using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Scanning;
using Res = Enterprise.Customs.AU.Declaration.GUI.Res;
using ScanStatus = Enterprise.Customs.AU.Declaration.Business.ScanForOutturnManager.ManualScanStatuses;

namespace Enterprise.Customs.AU.GUI
{
	public partial class ManualScanning : ZChildForm
	{
		static ManualScanning()
		{
			ZFormStrategy.AddFormTypeThatCanBeCreatedDuringDbTransaction(typeof(ManualScanning));
		}

		readonly IScanForOutturnManager manager;
		ManualScanTarget currentScanTarget;

		public ManualScanning(IScanForOutturnManager manager)
			: base()
		{
			this.manager = Argument.NotNull(manager, "manager");
			InitializeComponent();
			this.SetDataBinding(manager.ManualScanHistory, "");

			this.FormClosing += (a, b) =>
			{
				if (manager.ManualScanHistory.Count != 0)
				{
					if (Globals.Message.ShowConfirmation(Res.GetString("2AD66199-6417-40B5-9A83-1240D54C4B71", "Are you sure you want to cancel outturn scan?"), Res.GetString("7BD8B39A-EAFC-44B5-9647-285A0E4B3E42", "Cancel Outturn Scan"), Res.GetString("E7EAE6D1-F2B1-4E76-B4BC-827267EDD4DC", "To continue, type:") + " ", "Yes", MessageBoxIcon.Question) == DialogResult.OK)
					{
						manager.ManualScanHistory.RemoveAndDeleteAll();
						this.Close();
					}
					else
					{
						b.Cancel = true;
					}
				}
			};
		}

		protected override void OnShown(EventArgs e)
		{
			var barcodes = new BarcodeManager();
			barcodes.NonSystemBarcodeScanned += new EventHandler<BarcodeScanEventArgs>(BarcodeScanned);
			EnableScanning(barcodes, () => Visible, null);
			WaitForScanInput();
		}

		void BarcodeScanned(object sender, BarcodeScanEventArgs e)
		{
			currentScanTarget = manager.CreateManualScanTarget(e.Barcode);

			if (currentScanTarget.IsSurplusPackage)
			{ PromptSurplusPackage(); return; }
			if (currentScanTarget.IsSurplusConsignment)
			{ PromptSurplusConsignment(); return; }
			if (currentScanTarget.IsInvalidConsignment)
			{ PromptInvalidConsignment(); return; }

			ReleaseOrHoldScanTarget();
		}

		#region Button Click

		void SurplusYesButton_Click(object sender, EventArgs e)
		{
			ReleaseOrHoldScanTarget();
		}

		void SurplusNoButton_Click(object sender, EventArgs e)
		{
			WaitForScanInput();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void CompleteButton_Click(object sender, EventArgs e)
		{
			var result = manager.MergeManualScanResults();
			if (!string.IsNullOrEmpty(result))
			{
				Globals.Message.ShowWarning(result);
			}
			this.Close();
		}

		#endregion

		#region Implementation

		void WaitForScanInput()
		{
			currentScanTarget = manager.CreateEmptyManualScanTarget();
			ShowScanStatus(currentScanTarget);
			scanLogGrid.Focus();
		}

		void PromptSurplusPackage()
		{
			currentScanTarget.Status = ScanStatus.SurplusPackage;
			ShowScanStatus(currentScanTarget);
		}

		void PromptSurplusConsignment()
		{
			currentScanTarget.Status = ScanStatus.Unknow;
			ShowScanStatus(currentScanTarget);
		}

		void PromptInvalidConsignment()
		{
			currentScanTarget.Status = ScanStatus.Invalid;
			ShowScanStatus(currentScanTarget);
		}

		void ReleaseOrHoldScanTarget()
		{
			if (currentScanTarget.IsMatchedConsignmentCleared)
			{
				ReleaseScanTarget();
			}
			else
			{
				HoldScanTarget();
			}

			WaitForScanInput();
		}

		void ReleaseScanTarget()
		{
			currentScanTarget.Status = ScanStatus.Release;
			AddScanHistory();
			ShowScanStatus(currentScanTarget);
		}

		void HoldScanTarget()
		{
			currentScanTarget.Status = ScanStatus.Hold;
			AddScanHistory();
			ShowScanStatus(currentScanTarget);
		}

		void AddScanHistory()
		{
			var scanResult = new ManualScanLine() { Time = ZDateTime.Now, Barcode = currentScanTarget.Barcode, Instruction = currentScanTarget.Status.ToString() };
			manager.ManualScanHistory.Add(scanResult);
			scanLogGrid.ListManager.Position = scanLogGrid.ListManager.Count - 1;
			scanLogGrid.Select(scanLogGrid.ListManager.Count - 1);
			RefreshScanLogGrid();
		}

		void RefreshScanLogGrid()
		{
			scanLogGrid.Update();
			scanLogGrid.Invalidate();
		}

		void ShowScanStatus(ManualScanTarget scanTarget)
		{
			string scanResultLabel = string.Empty;
			string scanNoteLabel = string.Empty;

			switch (scanTarget.Status)
			{
				case ScanStatus.AwaitingScanInput:
					scanResultLabel = Res.GetString("01d70c16-d7f5-44fd-8aa2-1c4fc6d0ce4h", "Awaiting Scan Input");
					ShowScanResultPanel(Color.LightGray, scanResultLabel, scanNoteLabel, false);
					break;
				case ScanStatus.Release:
					scanResultLabel = Res.GetString("01d70c16-d7f5-44fd-8aa2-1c4fc6d0ce4e", "Release Package");
					ShowScanResultPanel(Color.Green, scanResultLabel, scanNoteLabel, false, 2000);
					break;
				case ScanStatus.Hold:
					scanResultLabel = Res.GetString("01d70c16-d7f5-44fd-8aa2-1c4fc6d0ce4f", "Hold Package");
					ShowScanResultPanel(Color.Red, scanResultLabel, scanNoteLabel, false, 2000);
					break;
				case ScanStatus.Unknow:
					scanResultLabel = Res.GetString("01d70c16-d7f5-44fd-8aa2-1c4fc6d0ce4g", "Surplus Consignment?");
					scanNoteLabel = Res.GetString("AEC0F9B5-0880-401A-9F65-EF5F8B572111", "Barcode: {0}\r\n\r\nThis consignment has not been registered.", scanTarget.Barcode);
					ShowScanResultPanel(Color.Yellow, scanResultLabel, scanNoteLabel, true);
					break;
				case ScanStatus.SurplusPackage:
					scanResultLabel = Res.GetString("726e4094-291c-4d60-aadf-aec42013144d", "Surplus Package?");
					scanNoteLabel = Res.GetString("e3b68c38-c813-49c2-bb0b-aa2b20918aad", "No. of packages already scanned with this barcode: {0}\r\nExpect: {1}", scanTarget.NumberOfScannedPackage, scanTarget.NumberOfExpectedPackage);
					ShowScanResultPanel(Color.Yellow, scanResultLabel, scanNoteLabel, true);
					break;
				case ScanStatus.Invalid:
					scanResultLabel = Res.GetString("38232F74-32B5-47EC-BF8B-7FBA9B8E123C", "Invalid Consignment");
					scanNoteLabel = Res.GetString("6E0BD42B-24D5-492D-B39F-47845CCA182C", "Barcode: {0}\r\n\r\nThis consignment has either:\r\nnot been registered\r\nor not out-turned.", scanTarget.Barcode);
					ShowScanResultPanel(Color.Yellow, scanResultLabel, scanNoteLabel, showSurplusButton: false, showRescanOnly: true);
					break;
			}
		}

		void ShowScanResultPanel(Color color, string scanResultLabel, string scanNoteLabel, bool showSurplusButton, int milliSecondsToFlash = 0, bool showRescanOnly = false)
		{
			scanResultPanel.BackColor = color;
			this.scanResultLabel.BackColor = color;
			this.scanResultLabel.Text = scanResultLabel;
			RefreshScanResultLabel();
			this.scanNoteLabel.BackColor = color;
			this.scanNoteLabel.Text = scanNoteLabel;
			RefreshScanNoteLabel();
			surplusYesButton.Visible = showSurplusButton;
			surplusNoButton.Visible = showSurplusButton || showRescanOnly;
			if (milliSecondsToFlash > 0)
			{
				Thread.Sleep(milliSecondsToFlash);
			}
		}

		void RefreshScanResultLabel()
		{
			scanResultLabel.Update();
			scanResultLabel.Invalidate();
		}

		void RefreshScanNoteLabel()
		{
			scanNoteLabel.Update();
			scanNoteLabel.Invalidate();
		}

		#endregion
	}
}
