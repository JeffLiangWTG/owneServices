using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.SWL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.SWL.GUI
{
	public partial class ShipnetSetupUserControl : ZUserControl
	{
		public ShipnetSetupUserControl()
		{
			InitializeComponent();
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual ShipnetSetupBusinessObject ShipnetObject
		{
			get { return (ShipnetSetupBusinessObject)CurrentDataItem; }
		}

		protected virtual void IsShipnetCarrierInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ShipnetObject != null)
			{
				ChargeGroupsGroupBox.Visible = ShipnetObject.IsShipnetCarrier;
				ChargeCodesGroupBox.Visible = ShipnetObject.IsShipnetCarrier;
				DebtorControlCodeLabel.Visible = ShipnetObject.IsShipnetCarrier;
				CreditorControlCode.Visible = ShipnetObject.IsShipnetCarrier;
				CreditorControlCodeLabel.Visible = ShipnetObject.IsShipnetCarrier;
				DebtorControlCodeTextBox.Visible = ShipnetObject.IsShipnetCarrier;
				EDITypeLabel.Visible = ShipnetObject.IsShipnetCarrier;
				EDITypeDropEdit.Visible = ShipnetObject.IsShipnetCarrier;
				if (ShipnetObject.IsShipnetCarrier)
				{
					ShipnetObject.CommunicationMode.EK_CommunicationsTransportInfo.ValueChanged -= new EventHandler(ShipnetObject_ControlVisibilityChanged);
					ShipnetObject.CommunicationMode.EK_CommunicationsTransportInfo.ValueChanged += new EventHandler(ShipnetObject_ControlVisibilityChanged);
				}
				else
				{
					ShipnetObject.CommunicationMode.EK_CommunicationsTransportInfo.ValueChanged -= new EventHandler(ShipnetObject_ControlVisibilityChanged);
				}
				EDISettingsGroupBox.Visible = ShipnetObject.IsShipnetCarrier;
				if (EDISettingsGroupBox.Visible)
				{
					ShipnetObject_ControlVisibilityChanged(sender, e);
				}
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (ShipnetObject != null)
			{
				ShipnetObject.IsShipnetCarrierInfo.ValueChanged -= new EventHandler(IsShipnetCarrierInfo_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (ShipnetObject != null)
			{
				ShipnetObject.IsShipnetCarrierInfo.ValueChanged += new EventHandler(IsShipnetCarrierInfo_ValueChanged);
				IsShipnetCarrierInfo_ValueChanged(this, EventArgs.Empty);
			}
		}

		protected virtual void ShipnetObject_ControlVisibilityChanged(object sender, EventArgs e)
		{
			ChangeEDISettingsVisibility();
		}

		protected void ChangeEDISettingsVisibility()
		{
			if (ShipnetObject != null)
			{
				switch (ShipnetObject.CommunicationMode.EK_CommunicationsTransport.ToString())
				{
					case ShipnetExportCommunicationsTransportMappingList.Codes.Email:
						SetEmailVisibility();
						break;
					case ShipnetExportCommunicationsTransportMappingList.Codes.FTP:
						SetFTPVisibility();
						break;
					case ShipnetExportCommunicationsTransportMappingList.Codes.File:
						SetFileVisibility();
						break;
					default:
						SetDefaulVisibility();
						break;
				}
			}
		}

		protected virtual void SetDefaulVisibility()
		{
			EDISettingsGroupBox.Visible = false;
		}

		protected virtual void SetFileVisibility()
		{
			EDISettingsGroupBox.Visible = true;
			DestinationLabel.Text = Res.GetString("d95ac9c4-3525-4496-b7e8-70c5fe72d091", "Destination Directory:");
			ControlDpiScalingHelper.SetWidth(ref DestinationTextBox, ExportFileNameTextBox.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(GapWidth) - BrowseButton.Width, false);
			ContainerControlToolTip.SetToolTip(DestinationTextBox, Res.GetString("060467ca-e590-404e-8955-f26fb2a17509", "Enter the Directory where the data will be saved"));
			BrowseButton.Visible = true;
			ServerAddressSubjectLabel.Visible = false;
			ServerAddressSubjectTextBox.Visible = false;
			PortNumberLabel.Visible = false;
			PortNumberCalcEdit.Visible = false;
			UsernameLabel.Visible = false;
			UsernameTextBox.Visible = false;
			PasswordLabel.Visible = false;
			PasswordTextBox.Visible = false;
		}

		internal protected const int GapWidth = 6;

		protected virtual void SetFTPVisibility()
		{
			EDISettingsGroupBox.Visible = true;
			DestinationLabel.Text = Res.GetString("d95ac9c4-3525-4496-b7e8-70c5fe72d091", "Destination Directory:");
			ControlDpiScalingHelper.SetWidth(ref DestinationTextBox, ExportFileNameTextBox.Width, false);
			ContainerControlToolTip.SetToolTip(DestinationTextBox, Res.GetString("0b0a3e72-2798-461f-a46a-8d096a26b051", "Enter the Directory on the FTP where the data will be stored"));
			BrowseButton.Visible = false;

			ServerAddressSubjectLabel.Visible = true;
			ServerAddressSubjectLabel.Text = Res.GetString("9a02be2f-bed3-46fb-8e63-ff34de11e490", "Server Address:");
			ContainerControlToolTip.SetToolTip(ServerAddressSubjectTextBox, Res.GetString("6fc4b949-07ad-4c82-8426-c5937ab808a3", "Enter the Name or IP Address of the FTP Server"));
			ServerAddressSubjectTextBox.Visible = true;

			PortNumberLabel.Visible = true;
			PortNumberCalcEdit.Visible = true;
			UsernameLabel.Visible = true;
			UsernameTextBox.Visible = true;
			PasswordLabel.Visible = true;
			PasswordTextBox.Visible = true;
		}

		protected virtual void SetEmailVisibility()
		{
			EDISettingsGroupBox.Visible = true;
			DestinationLabel.Text = Res.GetString("5e08441a-5e72-4e06-be41-f84c8cf8c46d", "Email Address:");
			ControlDpiScalingHelper.SetWidth(ref DestinationTextBox, ExportFileNameTextBox.Width, false);
			ContainerControlToolTip.SetToolTip(DestinationTextBox, Res.GetString("6a3002c4-d7f2-49bf-b9a6-87416f0b5c52", "Enter the Email Address to send the data to"));
			BrowseButton.Visible = false;

			ServerAddressSubjectLabel.Visible = true;
			ServerAddressSubjectLabel.Text = Res.GetString("8df9a4db-cf4b-47b2-8427-347c37f2ccf4", "Email Subject:");
			ContainerControlToolTip.SetToolTip(ServerAddressSubjectTextBox, Res.GetString("e992fcd1-507d-4bc7-9d79-637f74152a5e", "Enter the Email Subject"));
			ServerAddressSubjectTextBox.Visible = true;

			PortNumberLabel.Visible = false;
			PortNumberCalcEdit.Visible = false;
			UsernameLabel.Visible = false;
			UsernameTextBox.Visible = false;
			PasswordLabel.Visible = false;
			PasswordTextBox.Visible = false;
		}

		void BrowseButton_Click(object sender, EventArgs e)
		{
			BrowserDialog.RequireMappablePath = true;
			DialogResult result = BrowserDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				ShipnetObject.CommunicationMode.EK_Destination = BrowserDialog.UnmappedSelectedPath;
				ShipnetObject.RefreshBinding();
			}
		}

		void IsShipnetCarrierCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!IsShipnetCarrierCheckBox.Checked && ShipnetObject.IsShipnetCarrier)
			{
				DialogResult answer = Globals.Message.ShowConfirmation(Res.GetString("a6ea8328-5502-4658-a219-df9ea3ea4b79", "Unchecking this field will cause all Shipnet settings to be deleted for this carrier.\r\nDo you want to continue?"), Res.GetString("ba30de98-ae5b-4499-8fa7-f1b897012412", "WARNING REMOVING SHIPNET SETTINGS"), Res.GetString("9b7205d6-323a-43a2-8ce6-28b320fa728d", "Yes"), MessageBoxIcon.Warning);
				IsShipnetCarrierCheckBox.Checked = (answer != DialogResult.OK);
			}
		}
		#region internal variables
		internal ZGroupBox internalEDISettingsGroupBoxTest => EDISettingsGroupBox;
		internal ZLabel internalDestinationLabelTest => DestinationLabel;
		internal ZTextBox internalExportFileNameTextBoxTest => ExportFileNameTextBox;
		internal ZButton internalBrowseButtonTest => BrowseButton;
		internal ZLabel internalServerAddressSubjectLabelTest => ServerAddressSubjectLabel;
		internal ZTextBox internalServerAddressSubjectTextBoxTest => ServerAddressSubjectTextBox;
		internal ZLabel internalPortNumberLabelTest => PortNumberLabel;
		internal ZCalcEdit internalPortNumberCalcEditTest => PortNumberCalcEdit;
		internal ZLabel internalUsernameLabelTest => UsernameLabel;
		internal ZTextBox internalUsernameTextBoxTest => UsernameTextBox;
		internal ZLabel internalPasswordLabelTest => PasswordLabel;
		internal ZTextBox internalPasswordTextBoxTest => PasswordTextBox;
		internal ZTextBox internalDestinationTextBoxTest => DestinationTextBox;
		internal ZGroupBox internalChargeGroupsGroupBoxTest => ChargeGroupsGroupBox;
		internal ZGroupBox internalChargeCodesGroupBoxTest => ChargeCodesGroupBox;
		internal ZLabel internalDebtorControlCodeLabelTest => DebtorControlCodeLabel;
		internal ZTextBox internalCreditorControlCodeTest => CreditorControlCode;
		internal ZLabel internalCreditorControlCodeLabelTest => CreditorControlCodeLabel;
		internal ZTextBox internalDebtorControlCodeTextBoxTest => DebtorControlCodeTextBox;
		internal ZLabel internalEDITypeLabelTest => EDITypeLabel;
		internal ZDropEdit internalEDITypeDropEditTest => EDITypeDropEdit;
		#endregion
	}
}
