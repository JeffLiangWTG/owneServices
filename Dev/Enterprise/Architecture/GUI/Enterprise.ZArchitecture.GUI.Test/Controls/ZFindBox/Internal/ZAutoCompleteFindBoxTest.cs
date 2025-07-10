namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class ZAutoCompleteFindBoxTest : BaseFindBoxTest
	{
		public void TestAutoCompleteWithReplace()
		{
			CreateDummies();

			using (var testForm = new ZChildForm())
			{
				CreateControls(testForm, "");
				testForm.Show();

				FindBox.CodeBox.Text = "Z";
				FindBox.CodeBox.SelectionStart = 1;
				FindBox.CodeBox.SelectionLength = 0;

				SendKeyPressToCodeBox('=');
				AssertEquals("SelectionStart when AutoCompleting Z with replace", 0, FindBox.CodeBox.SelectionStart);
				AssertEquals("SelectionLength when AutoCompleting Z with replace", FindBox.CodeBox.Text.Length, FindBox.CodeBox.SelectionLength);

				var autoCompleteFindBox = FindBox as ZAutoCompleteFindBoxTester;
				AssertEquals(autoCompleteFindBox.FindBoxExposed.Code, autoCompleteFindBox.CodeForFindingExposed);
			}
		}
	}
}
