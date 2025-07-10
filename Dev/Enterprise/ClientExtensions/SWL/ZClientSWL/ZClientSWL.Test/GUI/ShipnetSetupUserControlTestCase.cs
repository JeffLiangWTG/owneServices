using CargoWise.Windows.UI;
using Enterprise.Client.SWL.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.SWL.GUI
{
	public class ShipnetSetupUserControlTestCase : ShipnetTestCase
	{
		public void TestIsShipnetCarrierChanges()
		{
			using (ZForm form = new ZForm(ShipnetCarrierSettings))
			{
				using (ShipnetSetupUserControl control = new ShipnetSetupUserControl())
				{
					control.SetDataBinding(ShipnetCarrierSettings, "");
					form.Controls.Add(control);
					form.Show();
					ShipnetCarrierSettings.IsShipnetCarrier = false;
					AssertIsShipnetCarrierChanges(control);
				}
			}
		}

		protected void AssertIsShipnetCarrierChanges(ShipnetSetupUserControl control)
		{
			AssertEquals("ChargeGroupsGroupBox.Visible", control.ShipnetObject.IsShipnetCarrier, control.internalChargeGroupsGroupBoxTest.Visible);
			AssertEquals("ChargeCodesGroupBox.Visible", control.ShipnetObject.IsShipnetCarrier, control.internalChargeCodesGroupBoxTest.Visible);
			AssertEquals("EDISettingsGroupBox.Visible", control.ShipnetObject.IsShipnetCarrier, control.internalEDISettingsGroupBoxTest.Visible);
			AssertEquals("DebtorControlCodeLabel.Visible", control.ShipnetObject.IsShipnetCarrier, control.internalDebtorControlCodeLabelTest.Visible);
			AssertEquals("CreditorControlCode.Visible", control.ShipnetObject.IsShipnetCarrier, control.internalCreditorControlCodeTest.Visible);
			AssertEquals("CreditorControlCodeLabel.Visible", control.ShipnetObject.IsShipnetCarrier, control.internalCreditorControlCodeLabelTest.Visible);
			AssertEquals("DebtorControlCodeTextBox.Visible", control.ShipnetObject.IsShipnetCarrier, control.internalDebtorControlCodeTextBoxTest.Visible);
			AssertEquals("EDITypeLabel.Visible", control.ShipnetObject.IsShipnetCarrier, control.internalEDITypeLabelTest.Visible);
			AssertEquals("EDITypeDropEdit.Visible", control.ShipnetObject.IsShipnetCarrier, control.internalEDITypeDropEditTest.Visible);
		}

		public void TestChangeEDISettingsVisibility()
		{
			using (ZForm form = new ZForm(ShipnetCarrierSettings))
			{
				using (ShipnetSetupUserControl control = new ShipnetSetupUserControl())
				{
					control.SetDataBinding(ShipnetCarrierSettings, "");
					form.Controls.Add(control);
					form.Show();
					ShipnetCarrierSettings.CommunicationMode.EK_CommunicationsTransport = "";
					AssertDefaulVisibility(control);
					ShipnetCarrierSettings.CommunicationMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.Email;
					AssertEmailVisibility(control);
					ShipnetCarrierSettings.CommunicationMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.FTP;
					AssertFTPVisibility(control);
					ShipnetCarrierSettings.CommunicationMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.File;
					AssertFileVisibility(control);
				}
			}
		}

		protected virtual void AssertDefaulVisibility(ShipnetSetupUserControl control)
		{
			AssertEquals("EDISettingsGroupBox.Visible", false, control.internalEDISettingsGroupBoxTest.Visible);
		}

		protected virtual void AssertFileVisibility(ShipnetSetupUserControl control)
		{
			AssertEquals("EDISettingsGroupBox.Visible", true, control.internalEDISettingsGroupBoxTest.Visible);
			AssertEquals("DestinationLabel.Text", "Destination Directory:", control.internalDestinationLabelTest.Text);
			AssertEquals("DestinationTextBox.Width", control.internalExportFileNameTextBoxTest.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(ShipnetSetupUserControl.GapWidth) - control.internalBrowseButtonTest.Width, control.internalDestinationTextBoxTest.Width);
			AssertEquals("BrowseButton.Visible", true, control.internalBrowseButtonTest.Visible);
			AssertEquals("ServerAddressSubjectLabel.Visible", false, control.internalServerAddressSubjectLabelTest.Visible);
			AssertEquals("ServerAddressSubjectTextBox.Visible", false, control.internalServerAddressSubjectTextBoxTest.Visible);
			AssertEquals("PortNumberLabel.Visible", false, control.internalPortNumberLabelTest.Visible);
			AssertEquals("PortNumberCalcEdit.Visible", false, control.internalPortNumberCalcEditTest.Visible);
			AssertEquals("UsernameLabel.Visible", false, control.internalUsernameLabelTest.Visible);
			AssertEquals("UsernameTextBox.Visible", false, control.internalUsernameTextBoxTest.Visible);
			AssertEquals("PasswordLabel.Visible", false, control.internalPasswordLabelTest.Visible);
			AssertEquals("PasswordTextBox.Visible", false, control.internalPasswordTextBoxTest.Visible);
		}

		protected virtual void AssertFTPVisibility(ShipnetSetupUserControl control)
		{
			AssertEquals("EDISettingsGroupBox.Visible", true, control.internalEDISettingsGroupBoxTest.Visible);
			AssertEquals("DestinationLabel.Text", "Destination Directory:", control.internalDestinationLabelTest.Text);
			AssertEquals("DestinationTextBox.Width", control.internalExportFileNameTextBoxTest.Width, control.internalDestinationTextBoxTest.Width);
			AssertEquals("BrowseButton.Visible", false, control.internalBrowseButtonTest.Visible);
			AssertEquals("ServerAddressSubjectLabel.Visible", true, control.internalServerAddressSubjectLabelTest.Visible);
			AssertEquals("ServerAddressSubjectLabel.Text", "Server Address:", control.internalServerAddressSubjectLabelTest.Text);
			AssertEquals("ServerAddressSubjectTextBox.Visible", true, control.internalServerAddressSubjectTextBoxTest.Visible);
			AssertEquals("PortNumberLabel.Visible", true, control.internalPortNumberLabelTest.Visible);
			AssertEquals("PortNumberCalcEdit.Visible", true, control.internalPortNumberCalcEditTest.Visible);
			AssertEquals("UsernameLabel.Visible", true, control.internalUsernameLabelTest.Visible);
			AssertEquals("UsernameTextBox.Visible", true, control.internalUsernameTextBoxTest.Visible);
			AssertEquals("PasswordLabel.Visible", true, control.internalPasswordLabelTest.Visible);
			AssertEquals("PasswordTextBox.Visible", true, control.internalPasswordTextBoxTest.Visible);
		}

		protected virtual void AssertEmailVisibility(ShipnetSetupUserControl control)
		{
			AssertEquals("EDISettingsGroupBox.Visible", true, control.internalEDISettingsGroupBoxTest.Visible);
			AssertEquals("DestinationLabel.Text", "Email Address:", control.internalDestinationLabelTest.Text);
			AssertEquals("DestinationTextBox.Width", control.internalExportFileNameTextBoxTest.Width, control.internalDestinationTextBoxTest.Width);
			AssertEquals("BrowseButton.Visible", false, control.internalBrowseButtonTest.Visible);
			AssertEquals("ServerAddressSubjectLabel.Visible", true, control.internalServerAddressSubjectLabelTest.Visible);
			AssertEquals("ServerAddressSubjectLabel.Text", "Email Subject:", control.internalServerAddressSubjectLabelTest.Text);
			AssertEquals("ServerAddressSubjectTextBox.Visible", true, control.internalServerAddressSubjectTextBoxTest.Visible);
			AssertEquals("PortNumberLabel.Visible", false, control.internalPortNumberLabelTest.Visible);
			AssertEquals("PortNumberCalcEdit.Visible", false, control.internalPortNumberCalcEditTest.Visible);
			AssertEquals("UsernameLabel.Visible", false, control.internalUsernameLabelTest.Visible);
			AssertEquals("UsernameTextBox.Visible", false, control.internalUsernameTextBoxTest.Visible);
			AssertEquals("PasswordLabel.Visible", false, control.internalPasswordLabelTest.Visible);
			AssertEquals("PasswordTextBox.Visible", false, control.internalPasswordTextBoxTest.Visible);
		}
	}
}
