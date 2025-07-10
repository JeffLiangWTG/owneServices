using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.GUI.EInvoicing.HardwareTokenSigning;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.EInvoicing
{
	public partial class ESigningForm : ZChildForm
	{
		ResourceString HeadingLabelSignInvoicePleaseWait => ResString.GetMultilingualString("b22ac238 -b3d3-435c-955b-841e2018d1fb", "You are about to sign {0} invoices. Please wait...", eSigningBusinessObject.NumOfTransactions.ToString());
		ResourceString HeadingLabelSignInvoice => ResString.GetMultilingualString("abc9d9a4-7842-4d84-a8f8-90438eae62c8", "Sign {0} invoices.", eSigningBusinessObject.NumOfTransactions.ToString());

		public ESigningForm(ESigningBusinessObject eSigningBusinessObject) : base(eSigningBusinessObject)
		{
			this.eSigningBusinessObject = eSigningBusinessObject;
			InitializeComponent();
			HeadingLabel.Text = HeadingLabelSignInvoicePleaseWait;

			// These drop edits are either enum or coming from harware token/user system so should not be localised
			TypeDescriptor.AddAttributes(this.ChipsetDropEdit, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(this.CertificateDropEdit, new SuppressFormsLocalizedTestAttribute());
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);
			eSigningBusinessObject.ChipsetTypeInfo.ValueChanged += ChipsetType_ValueChanged;
			ChipsetType_ValueChanged(this, null);
		}

		protected override void OnShown(System.EventArgs e)
		{
			base.OnShown(e);
			Refresh();
			using (new CursorSwitcher(Cursors.WaitCursor))
			{
				eSigningBusinessObject.LazyInitialize();
			}
			HeadingLabel.Text = HeadingLabelSignInvoice;
		}

		readonly ESigningBusinessObject eSigningBusinessObject;

		void ChipsetType_ValueChanged(object sender, System.EventArgs e)
		{
			PinTextBox.Enabled = eSigningBusinessObject.IsPinRequired;
		}

		void SignButton_Click(object sender, System.EventArgs e)
		{
			eSigningBusinessObject.RunPreSaveValidation();

			if (!eSigningBusinessObject.HasErrors)
			{
				using (new CursorSwitcher(Cursors.WaitCursor))
				{
					DialogResult = DialogResult.OK;
					eSigningBusinessObject.MapAndSignAndQueueElectronicInvoices();
					Close();
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("64883ad6-68e7-497a-8d75-f645a5909734", "Please resolve all errors before signing."));
			}
		}

		void CancelSignButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnsubscribeHandlers();
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		void UnsubscribeHandlers()
		{
			eSigningBusinessObject.ChipsetTypeInfo.ValueChanged -= ChipsetType_ValueChanged;
		}

		#endregion

	}
}
