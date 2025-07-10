using System.Windows.Forms;
using Enterprise.Client.AUS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.AUS.GUI
{
	[TestedType(typeof(ClientAusProductImportRegistryForm))]
	public class ClientAusProductImportRegistryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ClientAusProductImportRegistryForm(Factory.New<ClientAUSProductImportRegistry>());
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
			using (ClientAusProductImportRegistryForm testForm = new ClientAusProductImportRegistryForm(Factory.New<ClientAUSProductImportRegistry>()))
			{
				testForm.Show();
				Application.DoEvents();
			}
		}

		public void TestFormHeading()
		{
			using (ClientAusProductImportRegistryForm testForm = new ClientAusProductImportRegistryForm(Factory.New<ClientAUSProductImportRegistry>()))
			{
				testForm.Show();
				AssertEquals("Form text", "New Product Import/Export Registry", testForm.Text);
			}
		}
	}
}
