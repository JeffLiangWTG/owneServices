using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class InwardProcessingUserControlTest : TestCaseWithFactory
	{
		public void TestChangeControlsVisibility()
		{
			using (var form = new ZForm(declaration))
			using (var control = new InwardProcessingUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				CombineAssertions(() =>
				{
					control.ChangeControlsVisibility(entryInstruction);
					var mainAccountingDocAddressControl = control.FindSingle<ZDocAddressControl>("MainAccountingDocAddressControl");
					AssertEquals("mainAccountingDocAddressControl is NOT visible", false, mainAccountingDocAddressControl.Visible);

					entryInstruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.J;
					control.ChangeControlsVisibility(entryInstruction);
					AssertEquals("mainAccountingDocAddressControl is visible", true, mainAccountingDocAddressControl.Visible);

					entryInstruction.CEI_SimplifiedGrantAuthorization = SimplifiedGrantAuthorizationList.Codes.N;
					control.ChangeControlsVisibility(entryInstruction);
					AssertEquals("mainAccountingDocAddressControl is NOT visible", false, mainAccountingDocAddressControl.Visible);
				});
			}
		}

		public void TestCEI_InwardProcessingAdditionalInformation_CharacterCasing()
		{
			entryInstruction.CEI_InwardProcessingAdditionalInformation = UpperLowerTestString;
			AssertUpperLowerCasing("InwardProcessingAdditionalInformationTextBox");
		}

		public void TestCEI_InwardProcessingDescription_CharacterCasing()
		{
			entryInstruction.CEI_InwardProcessingDescription = UpperLowerTestString;
			AssertUpperLowerCasing("InwardProcessingDescriptionTextBox");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;

		void AssertUpperLowerCasing(string zTextBoxControlName)
		{
			using (var form = new ZForm(declaration))
			using (var control = new InwardProcessingUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.ChangeControlsVisibility(entryInstruction);

				var controlToCheck = control.FindSingle<ZTextBox>(zTextBoxControlName);
				CombineAssertions(() =>
				{
					AssertEquals("CharacterCasing", CharacterCasing.Normal, controlToCheck.CharacterCasing);
					AssertEquals("TestString", UpperLowerTestString, controlToCheck.Text);
				});
			}
		}

		const string UpperLowerTestString = "UPPERlower";
	}
}
