using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.DeviceManagement.GUI
{
	class ClientTelRimRegistrationOpenRegistrationForm : ClientTelRimRegistrationChangeRegistrationForm
	{
		public ClientTelRimRegistrationOpenRegistrationForm(ClientDeviceHeader clientDeviceHeader)
			: base(clientDeviceHeader)
		{
			this.okButton.Click += ClientTelRimRegistrationChangeRegistrationForm_OkButtonPress;
			this.cancelButton.Click += ClientTelRimRegistrationChangeRegistrationForm_CancelButtonPress;
		}

		void ClientTelRimRegistrationChangeRegistrationForm_OkButtonPress(object sender, System.EventArgs e)
		{
			if (MissingRequiredFields())
			{
				Globals.Message.ShowError(Res.GetString("f017e937-dea7-4c53-a4a6-1f0a7e9f4513", "All fields required Values"));
				return;
			}

			var result = rimEnrolmentRequestProcessor.TryProcessEnrolmentRequest(
				clientDeviceHeader,
				enrolmentSchemeDropEdit.CodeBox.Text,
				VehicleIdentificationNumberTextBox.Text,
				vehicleRegistrationTextBox.Text,
				vehicleRegistrationStateDropEdit.CodeBox.Text,
				deviceLocationTextBox.Text,
				installationDateTimeOffsetEdit.DateTimeOffsetValue.ToDateTimeOffset(),
				commencementDateTimeOffsetEdit.DateTimeOffsetValue.ToDateTimeOffset(),
				approvalDateTimeOffsetEdit.DateTimeOffsetValue.ToDateTimeOffset(),
				out var errorMessage);

			if (!result)
			{
				Globals.Message.ShowError(Res.GetString("0062dd32-9b0f-48c4-8155-ea747ac033a7", "Failed to Register with TCA: {0}", errorMessage));
				return;
			}

			this.Close();
		}

		void ClientTelRimRegistrationChangeRegistrationForm_CancelButtonPress(object sender, System.EventArgs e)
		{
			this.Close();
		}

		bool MissingRequiredFields()
		{
			if (enrolmentSchemeDropEdit.CodeBox.Text.Length == 0 ||
				VehicleIdentificationNumberTextBox.Text.Length == 0 ||
				vehicleRegistrationTextBox.Text.Length == 0 ||
				vehicleRegistrationStateDropEdit.CodeBox.Text.Length == 0 ||
				deviceLocationTextBox.Text.Length == 0 ||
				installationDateTimeOffsetEdit.DateTimeOffsetValue.IsEmpty ||
				commencementDateTimeOffsetEdit.DateTimeOffsetValue.IsEmpty ||
				approvalDateTimeOffsetEdit.DateTimeOffsetValue.IsEmpty)
			{
				return true;
			}

			return false;
		}
	}
}
