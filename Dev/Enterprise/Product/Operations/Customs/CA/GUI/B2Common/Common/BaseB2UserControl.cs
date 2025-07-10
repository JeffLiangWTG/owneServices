using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class BaseB2UserControl : Customs.GUI.FrontPageUserControl
	{
		protected BaseB2UserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			JobDeclaration.JE_OH_ImporterInfo.ValueChanged += JE_OH_ImporterInfo_ValueChanged;
			if (!JobDeclaration.IsInDatabase && !JobDeclaration.JE_OH_Importer.IsEmpty)
			{
				JE_OH_ImporterInfo_ValueChanged(this, null);
				if (JobDeclaration.DisplaySequentialOfTransactionNumberSeparately)
				{
					JobDeclaration.TransactionNumber.SetAccountSecurityNo();
				}
			}

			if (JobDeclaration.IsB3X)
			{
				TransportModeDropEdit.Visible = true;
				CarrierCodeFindBox.Visible = true;
				MailToOrganisationControlWithMiscellaneous.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("3A13753B-D8C9-4725-84FA-F85D25F58799", "Vendor");
			}
			else
			{
				TransportModeDropEdit.Visible = false;
				CarrierCodeFindBox.Visible = false;
			}

			ChangeTransactionNumberControlVisibility();
		}

		void ChangeTransactionNumberControlVisibility()
		{
			if (SupportTransactionNumberControl())
			{
				var isFormattedControlVisible = !JobDeclaration.DisplaySequentialOfTransactionNumberSeparately;
				FormattedTransactionNumberTextBox.Visible = isFormattedControlVisible;
				AccountSecurityCodeTextBox.Visible = !isFormattedControlVisible;
				SequentialNumberTextBox.Visible = !isFormattedControlVisible;
				CheckDigitTextBox.Visible = !isFormattedControlVisible;
			}
		}

		protected virtual ZBool SupportTransactionNumberControl() => ZBool.False;

		void JE_OH_ImporterInfo_ValueChanged(object sender, EventArgs e)
		{
			if (JobDeclaration.TransactionNumber.CanChange)
			{
				var importerAddInfo = JobDeclaration.ImporterAddInfo;
				if (importerAddInfo != null && importerAddInfo.HasAccountSecurityNumber)
				{
					JobDeclaration.CA_UseImporterAccountSecurityNumber = CACustomsDataRegistry.Instance.AlwaysUseImporterAccountSecurity.Value ||
						Globals.Message.Show(
						Res.GetString("89185DC5-F471-4C0B-9503-B01EE0700669", "Do you want to use the Importer's Account Security Number instead of yours?"),
						Res.GetString("892FE048-2AC0-4FDD-9947-160E81FCDA18", "Importer Account Security Number"),
						MessageBoxButtons.YesNo,
						DialogResult.Yes) == DialogResult.Yes;
				}
			}
		}

		protected new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
	}
}
