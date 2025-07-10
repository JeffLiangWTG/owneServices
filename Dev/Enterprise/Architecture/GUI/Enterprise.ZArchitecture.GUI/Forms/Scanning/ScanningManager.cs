using System;
using System.Media;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Scanning
{
	public class ScanningManager
	{
		#region Static

		public static void PlayScanErrorSound()
		{
			SystemSounds.Hand.Play();
		}

		public static bool IsStartOrStopScan(Keys key)
		{
			return key == ScannerPreAndPostamble || key == ScannerPreAndPostamble_Legacy;
		}

		const Keys ScannerPreAndPostamble = Keys.Control | Keys.L; // [FF - Form Feed] ASCII 12, Hex 0C.
		const Keys ScannerPreAndPostamble_Legacy = Keys.Control | Keys.OemCloseBrackets; // [GS - Group Separator] ASCII 29, Hex 1D. -- only works on US/UK keyboards

		#region Test
#if DEBUG
		internal const Keys ScannerPreAndPostamble_ForTesting = ScannerPreAndPostamble;
#endif
		#endregion

		#endregion

		#region Construction

		public ScanningManager(ZForm form, BarcodeManager barCodes, Func<bool> isOkToScan)
		{
			Argument.NotNull(form, "form");
			Argument.NotNull(barCodes, "barCodes");
			Argument.NotNull(isOkToScan, "isOkToScan");

			Barcodes = barCodes;
			IsOkToScan = isOkToScan;
			form.KeyDown += new KeyEventHandler(Form_KeyDown);
			form.KeyPreview = true;
		}

		readonly BarcodeManager Barcodes;
		readonly Func<bool> IsOkToScan;

		#endregion

		#region Capturing KeyDown

		void Form_KeyDown(object sender, KeyEventArgs e)
		{
			HandleKey((ZForm)sender, e);
		}

		internal void HandleKey(ZForm form, KeyEventArgs e)
		{
			if (IsStartOrStopScan(e.KeyData))
			{
				var isScanStart = !IsScanningBarcode;
				if (!isScanStart || IsOkToScan())
				{
					e.Handled = true;
					e.SuppressKeyPress = true;

					if (isScanStart)
					{
						StartBarcodeScan();
					}
					else if (LastPackingBarcode.ToString().Trim().Length != 0) // Since the scan is empty, ignore the end scan and treat it as still in SCAN.
					{
						EndBarcodeScan(form);
					}
				}
			}
			// store the next character of the barcode
			else if (IsScanningBarcode)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;

				var character = KeyToAscii.ToAscii(e.KeyValue, e.Shift, e.Control, e.Alt);
				if (character != '\0') // keys such as Control etc. cannot be converted to a char.
				{
					LastPackingBarcode.Append(character);
				}
			}
		}

		#endregion

		#region Start / End Scan

		void StartBarcodeScan()
		{
			IsScanningBarcode = true;
		}

		void EndBarcodeScan(ZForm form)
		{
			IsScanningBarcode = false;

			if (IsLicenseValid(form))
			{
				OnBarcodeScan();

				var barcode = LastPackingBarcode.ToString();
				if (!Barcodes.ExecuteBarcode(barcode))
				{
					SendBarcodeToFocusedTextControl(form, barcode);
				}
			}
			else
			{
				ShowNoScanningLicenseError();
			}

			LastPackingBarcode.Clear();
		}

		void OnBarcodeScan()
		{
			if (BarcodeScan != null)
			{
				BarcodeScan(this, EventArgs.Empty);
			}
		}

		bool IsLicenseValid(ILicensedComponent licensedComponent)
		{
			if (!isLicenseValid.HasValue)
			{
				isLicenseValid = EnvProxy.Instance.Licence.Packing.Login(licensedComponent) != LicenceLoginResponse.Denied;
			}

			return isLicenseValid.Value;
		}

		void ShowNoScanningLicenseError()
		{
			EnvProxy.Instance.Licence.Packing.ShowLastError();
		}

		public event EventHandler BarcodeScan;
		bool? isLicenseValid;

		#endregion

		#region Send Barcode to Focused Text Control

		void SendBarcodeToFocusedTextControl(Form form, ZString barcode)
		{
			// if the focused control is a text control, send the non-system barcode to it
			var focusedTextBox = FindFocusedControl(form) as TextBoxBase;
			if (focusedTextBox != null && !focusedTextBox.ReadOnly)
			{
				focusedTextBox.Text = barcode.Left(focusedTextBox.MaxLength);
			}
		}

		Control FindFocusedControl(Control control)
		{
			var container = control as ContainerControl;
			while (container != null)
			{
				control = container.ActiveControl;
				container = control as ContainerControl;
			}
			return control;
		}

		#endregion

		#region LastPackingBarcode

		StringBuilder LastPackingBarcode
		{
			get { return lastPackingBarcode ?? (lastPackingBarcode = new StringBuilder()); }
		}

		StringBuilder lastPackingBarcode;
		bool IsScanningBarcode;

		#endregion
	}
}
