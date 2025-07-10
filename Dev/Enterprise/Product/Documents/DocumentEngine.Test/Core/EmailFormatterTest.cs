using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class EmailFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor_NoBranchAndUser()
		{
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNoExceptionThrown("Should not throw exception", () =>
				{
					new EmailFormatter(null);
				});
			}
		}

		public void TestUpdateDocumentName()
		{
			AssertEquals(ZString.Empty, formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.DocumentName]);
			formatter.UpdateDocumentName("blah");
			AssertEquals("blah", formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.DocumentName]);
		}

		public void TestGetEmailSignature()
		{
			formatter.EmailFormat.Disclaimer = ZString.Empty;
			EmailSignatureFieldCollection signatureFields = new EmailSignatureFieldCollection();
			AssertEquals(ZString.Empty, formatter.GetEmailSignature(signatureFields));

			EmailSignatureField signatureField = signatureFields.AddNew();
			signatureField.Code = "booboo";
			Assert(formatter.GetEmailSignature(signatureFields).IsEmpty);

			formatter.MappedEmailFields.Add("bob", "blah");
			signatureField = signatureFields.AddNew();
			signatureField.Code = "bob";
			AssertEquals("blah", formatter.GetEmailSignature(signatureFields));

			signatureField = signatureFields.AddNew();
			signatureField.Code = "boo";
			formatter.MappedEmailFields.Add("boo", "blah blah");
			AssertEquals("blah" + System.Environment.NewLine + "blah blah", formatter.GetEmailSignature(signatureFields));

			formatter.EmailFormat.Disclaimer = "disclamer here";
			AssertEquals("blah" + System.Environment.NewLine + "blah blah" + System.Environment.NewLine + System.Environment.NewLine + formatter.EmailFormat.Disclaimer, formatter.GetEmailSignature(signatureFields));
		}

		public void TestGetEmailSubjectLine()
		{
			formatter.EmailFormat.Separator = "#";
			EmailSubjectFieldCollection subjectFields = new EmailSubjectFieldCollection();
			AssertEquals(ZString.Empty, formatter.GetEmailSubjectLine(subjectFields));

			EmailSubjectField subjectField = subjectFields.AddNew();
			subjectField.Code = "booboo";
			AssertEquals(ZString.Empty, formatter.GetEmailSubjectLine(subjectFields));

			formatter.MappedEmailFields.Add("bob", "blah");
			subjectField = subjectFields.AddNew();
			subjectField.Code = "bob";
			AssertEquals("blah", formatter.GetEmailSubjectLine(subjectFields));

			subjectField = subjectFields.AddNew();
			subjectField.Code = "boo";
			formatter.MappedEmailFields.Add("boo", "blah blah");
			AssertEquals("blah#blah blah", formatter.GetEmailSubjectLine(subjectFields));
		}

		public void TestUpdateScheduledTaskDescription()
		{
			formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.ScheduledTaskDescription] = ZString.Empty;
			Assert("Precondition", formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.ScheduledTaskDescription] == ZString.Empty);

			formatter.UpdateScheduledTaskDescription("TEST");
			Assert("Should have updated MappedEmailFields[\"ScheduledTaskDescription\"]", formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.ScheduledTaskDescription] == "TEST");
		}

		public void TestUpdateUserDetails()
		{
			SetUserDetailsPublish(GlbStaff.CurrentUser, true);
			AssertUserDetails(GlbStaff.CurrentUser);

			GlbStaff anotherUser = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, SQLComparisonOperator.NotEqual, GlbStaff.CurrentUser.PK));
			SetUserDetailsPublish(anotherUser, true);
			formatter = new EmailFormatter(anotherUser);
			AssertUserDetails(anotherUser);

			SetUserDetailsPublish(anotherUser, false);
			string emailAddress = anotherUser.GS_EmailAddress;
			string mobilePhone = anotherUser.GS_MobilePhone;
			string faxNum = anotherUser.GS_FaxNum;
			string workPhone = anotherUser.GS_WorkPhone;

			anotherUser.GS_EmailAddress = "blah";
			anotherUser.GS_MobilePhone = "blah";
			anotherUser.GS_FaxNum = "blah";
			anotherUser.GS_WorkPhone = "blah";
			formatter = new EmailFormatter(anotherUser);

			AssertEqualsPhone(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserWorkPhone], workPhone);
			AssertEqualsFax(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserFax], faxNum);
			AssertEqualsMobile(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserMobile], mobilePhone);
			AssertEqualsEmail(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserEmail], emailAddress);
		}

		public void TestUpdateBranchDetails()
		{
			AssertBranchDetails(GlbBranch.CurrentBranch);

			GlbBranch anotherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK));
			formatter.UpdateBranchDetails(anotherBranch);
			AssertBranchDetails(anotherBranch);
		}

		public void TestUpdateCompanyNameToBrandName()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_Name, formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName]);
			formatter.UpdateCompanyNameToBrandName("bob");
			AssertEquals("bob", formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName]);
		}

		public void TestUpdateCompanyDetails()
		{
			AssertCompanyDetails(GlbCompany.CurrentCompany);

			var branchInAnotherCompany = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK));
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchInAnotherCompany.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				ResetFormatter();
				AssertCompanyDetails(branchInAnotherCompany.Company);
			}
		}

		public void TestEmailFormat_NoBranchAndNoUser()
		{
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNoExceptionThrown("Should not throw exception", () =>
				{
					var fomatter = new EmailFormatter(null);
					var emailFormat = fomatter.EmailFormat;
				});
			}
		}

		#region Implementation

		EmailFormatter formatter;
		protected override void SetUp()
		{
			formatter = new EmailFormatter(GlbStaff.CurrentUser);
			base.SetUp();
		}

		void ResetFormatter()
		{
			formatter = new EmailFormatter(GlbStaff.CurrentUser);
		}

		void SetUserDetailsPublish(GlbStaff user, ZBool value)
		{
			user.GS_PublishEmailAddress = value;
			user.GS_PublishMobilePhone = value;
			user.GS_PublishWorkPhone = value;
			user.GS_PublishFaxNum = value;
		}

		void AssertEqualsPhone(ZString value1, ZString value2)
		{
			AssertEquals(value1, formatter.GetStringWithHeading(EmailFormatter.PhoneHeading, value2));
		}
		void AssertEqualsFax(ZString value1, ZString value2)
		{
			AssertEquals(value1, formatter.GetStringWithHeading(EmailFormatter.FaxHeading, value2));
		}
		void AssertEqualsEmail(ZString value1, ZString value2)
		{
			AssertEquals(value1, formatter.GetStringWithHeading(EmailFormatter.EmailHeading, value2));
		}
		void AssertEqualsWeb(ZString value1, ZString value2)
		{
			AssertEquals(value1, formatter.GetStringWithHeading(EmailFormatter.WebHeading, value2));
		}
		void AssertEqualsMobile(ZString value1, ZString value2)
		{
			AssertEquals(value1, formatter.GetStringWithHeading(EmailFormatter.MobileHeading, value2));
		}

		void AssertCompanyDetails(GlbCompany company)
		{
			AssertEquals(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName], company.GC_Name);
			AssertEquals(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyAddress], formatter.GetCompanyAddress(company));
			AssertEqualsPhone(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyPhone], company.GC_Phone);
			AssertEqualsFax(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyFax], company.GC_Fax);
			AssertEqualsEmail(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyEmail], company.GC_Email);
			AssertEqualsWeb(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.CompanyWeb], company.GC_WebAddress);
		}

		void AssertBranchDetails(GlbBranch branch)
		{
			AssertEquals(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchName], branch.GB_BranchName);
			AssertEquals(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchCode], branch.GB_Code);
			AssertEquals(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchAddress], formatter.GetBranchAddress(branch));
			AssertEqualsPhone(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchPhone], branch.GB_Phone);
			AssertEqualsFax(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchFax], branch.GB_Fax);
			AssertEqualsEmail(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchEmail], branch.GB_Email);
			AssertEqualsWeb(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.BranchWeb], branch.GB_WebAddress);
		}

		void AssertUserDetails(GlbStaff user)
		{
			AssertEquals(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserName], user.GS_FullName);
			AssertEquals(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserTitle], user.GS_Title);
			AssertEqualsPhone(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserWorkPhone], user.GS_WorkPhone);
			AssertEqualsFax(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserFax], user.GS_FaxNum);
			AssertEqualsMobile(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserMobile], user.GS_MobilePhone);
			AssertEqualsEmail(formatter.MappedEmailFields[Core.Constants.EmailFormat.EmailFieldCodes.UserEmail], user.GS_EmailAddress);
		}
		#endregion
	}
}
