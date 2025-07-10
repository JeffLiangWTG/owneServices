using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class GmailOAuth2JsonFileUserControlTest : TestCase
	{
		public void TestJsonFile()
		{
			using (var control = new GmailOAuth2JsonFileUserControl())
			{
				control.JsonFile.JsonText = "test json text";
				control.JsonFile.FileName = "test.json";

				AssertEquals("test json text", control.JsonFile.JsonText);
				AssertEquals("test.json", control.JsonFile.FileName);
			}
		}

		public void TestClear()
		{
			using (var control = new GmailOAuth2JsonFileUserControl())
			{
				control.JsonFile.JsonText = "test json text";
				control.JsonFile.FileName = "test.json";
				AssertNotNull(control.JsonFile.JsonText);
				AssertNotNull(control.JsonFile.FileName);

				control.btnClear_Click(null, null);
				AssertNull(control.JsonFile.JsonText);
				AssertNull(control.JsonFile.FileName);
			}
		}
	}
}
