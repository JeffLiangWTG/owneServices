using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.GUI.Testing
{
	[TestedType(typeof(EdiUserAgreementForm))]
	public class EdiUserAgreementFormTest : ZFormBasherTest
	{
		public void TestShouldShowFallbackWarningIfFallbackDoesNotExistForType()
		{
			var firstAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			firstAgreement.ERA_Title = "First Agreement";
			firstAgreement.ERA_Type = "MYA";
			firstAgreement.ERA_Content = "Agreement content";
			firstAgreement.ERA_RN_NKCountryCode = string.Empty;
			firstAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddHours(1);
			UnitTestUserNotification.Instance.ClearMessages();
			using (var testForm = new EdiUserAgreementForm(firstAgreement))
			{
				testForm.FireSaveButton();
			}

			AssertEquals("No warning since this is the fallback (no country code)", null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Should have been saved", true, firstAgreement.IsInDatabase);
			firstAgreement.ERA_RN_NKCountryCode = "NZ";
			Factory.Save();
			var secondAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			secondAgreement.ERA_Title = "Second Agreement";
			secondAgreement.ERA_Type = "MYA";
			secondAgreement.ERA_Content = "Agreement content";
			secondAgreement.ERA_RN_NKCountryCode = "AU";
			secondAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddHours(1);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			using (var testForm = new EdiUserAgreementForm(secondAgreement))
			{
				testForm.FireSaveButton();
				AssertEquals("Should have a warning since this type has no fallback", "There is no Country/Region Code fallback for this Agreement Type. You can create a fallback by specifying an Agreement with an empty Country/Region Code. The fallback version will be used by the Web API when the requested country-specific Agreement does not exist.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have been saved", false, secondAgreement.IsInDatabase);
				UnitTestUserNotification.Instance.ClearMessages();
				AssertEquals("Precondition", null, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.AddOKAnswer();
				testForm.FireSaveButton();
				AssertEquals("Should have a warning since this type has no fallback", "There is no Country/Region Code fallback for this Agreement Type. You can create a fallback by specifying an Agreement with an empty Country/Region Code. The fallback version will be used by the Web API when the requested country-specific Agreement does not exist.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have been saved", true, secondAgreement.IsInDatabase);
			}

			firstAgreement.ERA_RN_NKCountryCode = "";
			Factory.Save();
			var thirdAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			thirdAgreement.ERA_Title = "Third Agreement";
			thirdAgreement.ERA_Type = "MYA";
			thirdAgreement.ERA_Content = "Agreement content";
			thirdAgreement.ERA_RN_NKCountryCode = "US";
			thirdAgreement.ERA_EffectiveTimeUtc = secondAgreement.ERA_EffectiveTimeUtc;
			UnitTestUserNotification.Instance.ClearMessages();
			AssertEquals("Precondition", null, UnitTestUserNotification.Instance.LastMessage.Text);
			using (var testForm = new EdiUserAgreementForm(thirdAgreement))
			{
				testForm.FireSaveButton();
			}

			AssertEquals("No warning since this has a fallback", null, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestEdiUserAgreementFormHasPlugIns()
		{
			var userAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			using (var form = new EdiUserAgreementForm(userAgreement))
			{
				AssertNotNull("The form should contain the Documents PlugIn", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			return new EdiUserAgreementForm(Factory.New<EdiUserAgreement>());
		}
		#endregion
	}
}
