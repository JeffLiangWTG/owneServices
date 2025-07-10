using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	public sealed class CustomsProcessingUserControlTest : TestCaseWithFactory
	{
		public void TestUserControl()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				var declaration = JobDeclaration.New(Factory);
				declaration.ActiveEntryHeaders.AddNew();
				declaration.JE_EDITransmitDate = ZDate.Today.AddDays(1);

				using (var testForm = new ZForm(declaration))
				using (var userControl = new CustomsProcessingUserControl())
				{
					testForm.Controls.Add(userControl);
					testForm.Show();

					AssertEquals("customsProcessingGroupBox Caption", "Customs Processing", userControl.FindSingle<ZGroupBox>("customsProcessingGroupBox").Text);

					var transmitDateDateEdit = userControl.FindSingle<ZDateEdit>("transmitDateDateEdit");
					AssertEquals("transmitDateDateEdit Caption", "EDI Transmit Date", transmitDateDateEdit.CaptionResourceString.Caption);
					AssertEquals("transmitDateDateEdit DateTimeValue", declaration.JE_EDITransmitDate, transmitDateDateEdit.DateTimeValue);
				}
			}
		}
	}
}
