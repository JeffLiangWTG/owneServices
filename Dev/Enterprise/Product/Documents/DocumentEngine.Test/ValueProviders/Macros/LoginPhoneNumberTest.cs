using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LoginPhoneNumber))]
	sealed class LoginPhoneNumberTest : ValueProviderTest
	{
		public void TestMacroWorksWithNullEnvironment()
		{
			RunInNullEnvironment(
				() => AssertEquals("Replaced Result when User is Null", "", ValueProviderToTest.GetReplacement("<LoginPhoneNumber>", Report))
			);
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertIsResponsibleForReplacing("<LoginPhoneNumber>");
		}

		public void TestReplacement()
		{
			bool oldUserPublishPhoneNumber = GlbStaff.CurrentUser.GS_PublishWorkPhone;
			string oldUserPhoneNumber = GlbStaff.CurrentUser.GS_WorkPhone;
			string oldBranchPhoneNumber = GlbBranch.CurrentBranch.GB_Phone;
			string oldCompanyPhoneNumber = GlbCompany.CurrentCompany.GC_Phone;

			try
			{
				GlbStaff.CurrentUser.GS_PublishWorkPhone = false;
				GlbStaff.CurrentUser.GS_WorkPhone = "user";
				GlbBranch.CurrentBranch.GB_Phone = "";
				GlbCompany.CurrentCompany.GC_Phone = "";

				AssertIsReplacedWith("", "<LoginPhoneNumber>");

				GlbCompany.CurrentCompany.GC_Phone = "company";
				AssertIsReplacedWith("company", "<LoginPhoneNumber>");

				GlbBranch.CurrentBranch.GB_Phone = "branch";
				AssertIsReplacedWith("branch", "<LoginPhoneNumber>");

				GlbStaff.CurrentUser.GS_PublishWorkPhone = true;
				AssertIsReplacedWith("user", "<LoginPhoneNumber>");

				GlbStaff.CurrentUser.GS_WorkPhone = "";
				AssertIsReplacedWith("branch", "<LoginPhoneNumber>");
			}
			finally
			{
				GlbStaff.CurrentUser.GS_PublishWorkPhone = oldUserPublishPhoneNumber;
				GlbStaff.CurrentUser.GS_WorkPhone = oldUserPhoneNumber;
				GlbBranch.CurrentBranch.GB_Phone = oldBranchPhoneNumber;
				GlbCompany.CurrentCompany.GC_Phone = oldCompanyPhoneNumber;
			}
		}

		#region Implementation

		protected override ValueProvider GetNewValueProvider()
		{
			return new LoginPhoneNumber();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbStaff.CurrentUser.GS_PublishWorkPhone = true;
			GlbStaff.CurrentUser.GS_WorkPhone = "+61 2 9025 1100";
		}

		#endregion
	}
}
