using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LoginPhoneExtensionNumber))]
	sealed class LoginPhoneExtensionNumberTest : ValueProviderTest
	{
		public void TestMacroWorksWithNullEnvironment()
		{
			RunInNullEnvironment(() =>
				AssertEquals("Replaced Result when User is Null", "", ValueProviderToTest.GetReplacement("<LoginPhoneExtensionNumber>", Report))
			);
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertIsResponsibleForReplacing("<LoginPhoneExtensionNumber>");
		}

		public void TestReplacement()
		{
			bool oldUserPublishWorkExtension = GlbStaff.CurrentUser.GS_PublishWorkExtension;
			string oldUserWorkExtension = GlbStaff.CurrentUser.GS_WorkExtension;

			try
			{
				GlbStaff.CurrentUser.GS_PublishWorkExtension = false;
				GlbStaff.CurrentUser.GS_WorkExtension = "123";

				AssertIsReplacedWith("", "<Login phoneextension Number>");

				GlbStaff.CurrentUser.GS_PublishWorkExtension = true;
				AssertIsReplacedWith("123", "<Login PhoneExtensionNumber>");

				GlbStaff.CurrentUser.GS_WorkExtension = "";
				AssertIsReplacedWith("", "<Login Phone Extension Number>");
			}
			finally
			{
				GlbStaff.CurrentUser.GS_PublishWorkExtension = oldUserPublishWorkExtension;
				GlbStaff.CurrentUser.GS_WorkExtension = oldUserWorkExtension;
			}
		}

		#region Implementation

		protected override ValueProvider GetNewValueProvider()
		{
			return new LoginPhoneExtensionNumber();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbStaff.CurrentUser.GS_PublishWorkExtension = true;
			GlbStaff.CurrentUser.GS_WorkExtension = "123";
		}

		#endregion
	}
}
