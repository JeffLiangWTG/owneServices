using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ClearanceDocAddressControlTest : TestCaseWithFactory
	{
		public void TestGenerateTemporaryOrganization()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.ClearanceLocalInvolvedParty.E2_AddressOverride = true;
			declaration.ClearanceLocalInvolvedParty.E2_CompanyName = "BRAZIL COMPANY";
			declaration.ClearanceLocalInvolvedParty.E2_Longitude = 15m;
			declaration.ClearanceLocalInvolvedParty.E2_Latitude = 10m;
			declaration.ClearanceLocalInvolvedParty.E2_GovRegNumType = "CPF";
			declaration.ClearanceLocalInvolvedParty.E2_GovRegNum = "03142520000127";

			using (var form = new ZForm(declaration))
			using (var control = new ClearanceDocAddressControlForTest())
			{
				form.Controls.Add(control);
				control.SetDataBinding(declaration, "ClearanceLocalInvolvedParty");
				AssertTemporaryOrganization(control.GenerateTemporaryOrganization_Exposed(null));

				var newOrg = Factory.New<OrgHeader>();
				var tempOrg = control.GenerateTemporaryOrganization_Exposed(newOrg);
				AssertTemporaryOrganization(tempOrg);
				AssertSame(newOrg, tempOrg);
			}

			void AssertTemporaryOrganization(OrgHeader orgHeader)
			{
				AssertEquals("BRAZIL COMPANY", orgHeader.OH_FullName);
				AssertEquals(15m, orgHeader.MainAddress.OA_Longitude);
				AssertEquals(10m, orgHeader.MainAddress.OA_Latitude);
				AssertEquals("CPF", orgHeader.PrimaryRegistrationNumber.NumberType);
				AssertEquals("03142520000127", orgHeader.PrimaryRegistrationNumber.Number);
			}
		}

		public void TestLongitudeAndLatitude()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			using (var form = new ZForm(declaration))
			using (var control = new ClearanceDocAddressControlForTest())
			{
				form.Controls.Add(control);
				control.SetDataBinding(declaration, "ClearanceLocalInvolvedParty");

				var longitudeTextBox = control.GovernmentRegistrationTabPage_Exposed.Controls[4] as ZCalcEdit;
				var latitudeTextBox = control.GovernmentRegistrationTabPage_Exposed.Controls[5] as ZCalcEdit;

				CombineAssertions(() =>
				{
					AssertEquals("Longitude Caption", "Longitude", longitudeTextBox.CaptionResourceString.Caption);
					AssertEquals("Longitude DecimalPlaces", 6, longitudeTextBox.DecimalPlaces);
					AssertEquals("Longitude Decimals", 6, longitudeTextBox.Decimals);
					AssertEquals("Latitude Caption", "Latitude", latitudeTextBox.CaptionResourceString.Caption);
					AssertEquals("Latitude DecimalPlaces", 6, latitudeTextBox.DecimalPlaces);
					AssertEquals("Latitude Decimals", 6, latitudeTextBox.Decimals);
				});
			}
		}

		class ClearanceDocAddressControlForTest : ClearanceDocAddressControl
		{
			public OrgHeader GenerateTemporaryOrganization_Exposed(OrgHeader inputOrganization) => base.GenerateTemporaryOrganization(inputOrganization);

			public ZTabPage GovernmentRegistrationTabPage_Exposed => GovernmentRegistrationTabPage;
		}
	}
}
