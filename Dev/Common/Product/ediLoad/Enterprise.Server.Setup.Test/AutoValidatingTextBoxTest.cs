using NUnit.Framework;

namespace Enterprise.Server.Setup.Testing
{
	class AutoValidatingTextBoxTest : TestCase
	{
		public void TestTextChangedValidates()
		{
			using (AutoValidatingTextBox textBox = new AutoValidatingTextBox())
			{
				bool validated = false;
				textBox.Validating += delegate
				{
					validated = true;
				};
				textBox.Text = "moo";
				AssertEquals("Validated", true, validated);
			}
		}
	}
}