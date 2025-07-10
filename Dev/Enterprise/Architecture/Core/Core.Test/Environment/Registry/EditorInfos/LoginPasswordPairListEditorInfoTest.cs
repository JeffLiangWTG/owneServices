using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class LoginPasswordPairListEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			var editorInfo = new LoginPasswordPairListEditorInfo();

			AssertEquals("ShowCodeColumn", true, editorInfo.ShowCodeColumn);
			AssertEquals("ShowDescriptionColumn", true, editorInfo.ShowDescriptionColumn);
			AssertEquals("CodeFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editorInfo.CodeFieldCasing);
			AssertEquals("DescriptionFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editorInfo.DescriptionFieldCasing);
			AssertEquals("CodeColumnCaption", "User Login", editorInfo.CodeColumnCaption);
			AssertEquals("DescriptionColumnCaption", "User Password", editorInfo.DescriptionColumnCaption);
		}
	}
}
