using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ClearanceDocAddressControl : ZDocAddressControl, Integration.Customs.BR.IClearanceDocAddressControl
	{
		public ClearanceDocAddressControl()
		{
			SetupGeographicCoordinatesFields();
		}

		void SetupGeographicCoordinatesFields()
		{
			GovernmentRegistrationTabPage.SuspendLayout();

			var longitudeTextBox = new ZArchitecture.ZCalcEdit();
			var latitudeTextBox = new ZArchitecture.ZCalcEdit();

			BindingSource.SetBindingMember(longitudeTextBox, "E2_Longitude");
			longitudeTextBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("fccce309-b909-4a37-949c-c84e9be64b36", "Longitude");
			longitudeTextBox.DecimalPlaces = 6;
			longitudeTextBox.Decimals = 6;
			longitudeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 35);
			longitudeTextBox.Name = "LongitudeTextBox";
			longitudeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 20);
			longitudeTextBox.TabIndex = 3;
			longitudeTextBox.Text = "0.000000";
			longitudeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

			BindingSource.SetBindingMember(latitudeTextBox, "E2_Latitude");
			latitudeTextBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("318559b5-c0a1-4e97-9bb1-835363b183ae", "Latitude");
			latitudeTextBox.DecimalPlaces = 6;
			latitudeTextBox.Decimals = 6;
			latitudeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 62);
			latitudeTextBox.Name = "LatitudeTextBox";
			latitudeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 20);
			latitudeTextBox.TabIndex = 4;
			latitudeTextBox.Text = "0.000000";
			latitudeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

			GovernmentRegistrationTabPage.Controls.Add(longitudeTextBox);
			GovernmentRegistrationTabPage.Controls.Add(latitudeTextBox);

			GovernmentRegistrationTabPage.ResumeLayout(false);
			GovernmentRegistrationTabPage.PerformLayout();
		}

		protected override OrgHeader GenerateTemporaryOrganization(OrgHeader inputOrganization = null)
		{
			inputOrganization = base.GenerateTemporaryOrganization(inputOrganization);

			var mainAddress = inputOrganization?.MainAddress;
			if (mainAddress != null)
			{
				mainAddress.OA_Longitude = DocAddress.E2_Longitude;
				mainAddress.OA_Latitude = DocAddress.E2_Latitude;

				var numberType = DocAddress.E2_GovRegNumType;
				var regNumber = DocAddress.E2_GovRegNum;
				var countryCode = Core.Constants.CountryCodes.Brazil;

				if (!numberType.IsEmpty && !regNumber.IsEmpty)
				{
					var cusCode = inputOrganization.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(numberType, countryCode) ?? inputOrganization.CustomsCodes.AddNew();
					cusCode.OK_RN_NKCodeCountry = countryCode;
					cusCode.OK_CodeType = numberType;
					cusCode.OK_CustomsRegNo = regNumber;

					inputOrganization.RefreshBinding();
				}
			}

			return inputOrganization;
		}

		protected override Control ValidationReferenceControl => this;
	}
}
