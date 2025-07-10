using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LoginFaxNumber))]
	sealed class LoginFaxNumberTest : ValueProviderTest
	{
		public void TestMacroWorksWithNullEnvironment()
		{
			RunInNullEnvironment(() => AssertEquals("Replaced Result when User is Null", "", ValueProviderToTest.GetReplacement("<LoginFaxNumber>", Report)));
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");
			AssertIsResponsibleForReplacing("<LoginFaxNumber>");
		}

		public void TestReplacement()
		{
			var oldUserPublishFaxNumber = GlbStaff.CurrentUser.GS_PublishFaxNum;
			var oldUserFaxNumber = GlbStaff.CurrentUser.GS_FaxNum;
			var oldBranchFaxNumber = GlbBranch.CurrentBranch.GB_Fax;
			var oldCompanyFaxNumber = GlbCompany.CurrentCompany.GC_Fax;

			try
			{
				GlbStaff.CurrentUser.GS_PublishFaxNum = false;
				GlbStaff.CurrentUser.GS_FaxNum = "user";
				GlbBranch.CurrentBranch.GB_Fax = "";
				GlbCompany.CurrentCompany.GC_Fax = "";

				AssertIsReplacedWith("", "<LoginFaxNumber>");

				GlbCompany.CurrentCompany.GC_Fax = "company";
				AssertIsReplacedWith("company", "<LoginFaxNumber>");

				GlbBranch.CurrentBranch.GB_Fax = "branch";
				AssertIsReplacedWith("branch", "<LoginFaxNumber>");

				GlbStaff.CurrentUser.GS_PublishFaxNum = true;
				AssertIsReplacedWith("user", "<LoginFaxNumber>");

				GlbStaff.CurrentUser.GS_FaxNum = "";
				AssertIsReplacedWith("branch", "<LoginFaxNumber>");
			}
			finally
			{
				GlbStaff.CurrentUser.GS_PublishFaxNum = oldUserPublishFaxNumber;
				GlbStaff.CurrentUser.GS_FaxNum = oldUserFaxNumber;
				GlbBranch.CurrentBranch.GB_Fax = oldBranchFaxNumber;
				GlbCompany.CurrentCompany.GC_Fax = oldCompanyFaxNumber;
			}
		}

		#region Implementation

		protected override ValueProvider GetNewValueProvider()
		{
			return new LoginFaxNumber();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbStaff.CurrentUser.GS_PublishFaxNum = true;
			GlbStaff.CurrentUser.GS_FaxNum = "07 3868 1274";
		}

		#endregion
	}
}
