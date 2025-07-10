using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class DeclarationAgreementUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalCommentTextBox()
		{
			using (var control = new DeclarationAgreementUserControl())
			{
				AssertEquals("Max length", 1000, control.AdditionalCommentTextBox.MaxLength);
			}
		}

		public void TestCharacterCountTextBox()
		{
			using (var control = new DeclarationAgreementUserControl())
			{
				AssertEquals("Character count", "0", control.CharacterCountTextBox.Text);

				control.AdditionalCommentTextBox.Text = "Test text.";
				AssertEquals("Character count", "10", control.CharacterCountTextBox.Text);
			}
		}
	}
}
