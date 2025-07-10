using System.Windows.Forms;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(EMCSValidationSettingsForm))]
	public class EMCSValidationSettingsFormTest : ZFormBasherTest
	{
		public void TestControls()
		{
			using (var form = (ZForm)GetFormToBashCore())
			{
				AssertNoExceptionThrown("ExplanationOnReasonForShortageCheckBox", () => form.FindSingle<ZCheckBox>(c => c.Name == "ExplanationOnReasonForShortageCheckBox"));
				AssertNoExceptionThrown("ConfirmButton", () => form.FindSingle<ZButton>(c => c.Name == "ConfirmButton"));
			}
		}

		public void TestFormHeading()
		{
			using (var form = (ZForm)GetFormToBashCore())
			{
				AssertEquals("Validation Settings", form.FormHeading);
			}
		}

		EMCSValidationSettingsForm GetEMCSValidationSettingsForm()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			return new EMCSValidationSettingsForm(declaration);
		}

		protected override Form GetFormToBashCore() => GetEMCSValidationSettingsForm();
	}
}
