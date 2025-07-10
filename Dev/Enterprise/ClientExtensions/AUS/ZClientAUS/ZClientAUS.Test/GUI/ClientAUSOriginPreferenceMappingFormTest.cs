using System.Windows.Forms;
using Enterprise.Client.AUS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.AUS.GUI
{
	[TestedType(typeof(ClientAUSOriginPreferenceMappingForm))]
	public class ClientAUSOriginPreferenceMappingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ClientAUSOriginPreferenceMappingForm(Factory.New<ClientAUSOriginPreferenceMapping>());
		}

		protected override string CountryCode
		{
			get
			{
				return "ER";
			}
		}

		[ExpectNoExceptions]
		public void TestLoadForm()
		{
			using (ClientAUSOriginPreferenceMappingForm testForm = new ClientAUSOriginPreferenceMappingForm(Factory.New<ClientAUSOriginPreferenceMapping>()))
			{
				testForm.Show();
				Application.DoEvents();
			}
		}

		public void TestFormHeading()
		{
			using (ClientAUSOriginPreferenceMappingForm testForm = new ClientAUSOriginPreferenceMappingForm(Factory.New<ClientAUSOriginPreferenceMapping>()))
			{
				testForm.Show();
				AssertEquals("Form text", "New Origin-Preference Mapping", testForm.Text);
			}
		}
	}
}
