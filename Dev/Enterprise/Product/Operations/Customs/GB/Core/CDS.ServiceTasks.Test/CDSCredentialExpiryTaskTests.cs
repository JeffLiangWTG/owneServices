using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.CDS.ServiceTasks.Testing
{
	[TestedType(typeof(CDSCredentialExpiryTask))]
	class CDSCredentialExpiryTaskTests : ServiceTaskTestCase<CDSCredentialExpiryTask>
	{
		public void TestExpiredProcess()
		{
			var refreshMonths = CdsGlbExternalPasswordChecker.ExpiredMonths;

			var issueDate1 = ZDateTime.Now.AddMonths(-(refreshMonths - 1)).AddDays(-1); // Expiring this month
			var expiryDate1 = issueDate1.AddMonths(refreshMonths);

			var issueDate2 = ZDateTime.Now.AddDays(-1); // Not Expiring
			var expiryDate2 = issueDate2.AddMonths(refreshMonths);

			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUser.GS_EmailAddress = "testing@budwise.com";
			currentUser.Groups.RemoveAll();

			currentUser.Groups.AddFromDatabase(Core.Constants.Groups.PostMastersGroupPK);

			var comp1 = CreateCompanyAndCreds("1", issueDate1, expiryDate1, issueDate2, expiryDate2);
			var comp2 = CreateCompanyAndCreds("2", issueDate1, expiryDate1, issueDate2, expiryDate2);

			Factory.Save();

			InitialiseAndRunTaskSchedule(new CDSCredentialExpiryTask());

			// Assert 2 emails created with 2 entries each
			var emails = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Direction, MailDirection.Transmit)
												.AddToFilter(MailDBItemsSchema.MI_Subject, CDSExpiringTokenNotificationProcessor.EmailSubject));

			AssertEquals(2, emails.Length);

			var issueDateStr = issueDate1.ToString(CDSExpiringTokenNotificationProcessor.DateTimeFormat, CultureInfo.InvariantCulture);
			var expiryDateStr = expiryDate1.ToString(CDSExpiringTokenNotificationProcessor.DateTimeFormat, CultureInfo.InvariantCulture);

			var email1 = emails.Where(x => x.MI_Body.Contains(comp1.PK.ToGuid().ToString())).ToArray();
			var email2 = emails.Where(x => x.MI_Body.Contains(comp2.PK.ToGuid().ToString())).ToArray();

			AssertNotNull(email1);
			AssertEquals(1, email1.Length);
			AssertNotNull(email2);
			AssertEquals(1, email2.Length);

			AssertContains(comp1.CompanyName, email1[0].MI_Body);
			AssertContains("GB00001.ABC", email1[0].MI_Body);
			AssertContains("GB00001.ABC", email1[0].MI_Body);
			AssertContains("GB00001.XYZ", email1[0].MI_Body);
			AssertNotContains("GB00001.DEF", email1[0].MI_Body);

			AssertContains(comp2.CompanyName, email2[0].MI_Body);
			AssertContains("GB00002.ABC", email2[0].MI_Body);
			AssertContains("GB00002.ABC", email2[0].MI_Body);
			AssertContains("GB00002.XYZ", email2[0].MI_Body);
			AssertNotContains("GB00002.DEF", email2[0].MI_Body);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		GlbCompany CreateCompanyAndCreds(string identifier, ZDateTime issueDate1, ZDateTime expiryDate1, ZDateTime issueDate2, ZDateTime expiryDate2)
		{
			var comp = Factory.New<GlbCompany>();
			comp.GC_Code = "GC" + identifier;
			comp.CompanyName = "Test Company " + identifier;
			comp.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var gbBranch = Factory.New<GlbBranch>();
			gbBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			gbBranch.GB_Code = "BR" + identifier;
			gbBranch.GB_GC = comp.PK;

			CreateCDSCreds(comp.PK, "GB0000" + identifier + ".ABC", issueDate1, expiryDate1, PasswordStatusList.Codes.Valid);
			CreateCDSCreds(comp.PK, "GB0000" + identifier + ".DEF", issueDate2, expiryDate2, PasswordStatusList.Codes.Valid);
			CreateCDSCreds(comp.PK, "GB0000" + identifier + ".XYZ", issueDate1, expiryDate1, PasswordStatusList.Codes.Valid);
			CreateCDSCreds(comp.PK, "GB0000" + identifier + ".QWE", issueDate1, expiryDate1, PasswordStatusList.Codes.Invalid);

			return comp;
		}

		void CreateCDSCreds(ZGuid comp, string userId, ZDateTime issueDate, ZDateTime expiryDate, ZString passwordStatus)
		{
			var extPassword = Factory.New<GlbExternalPassword_GB>();

			extPassword.GP_PasswordType = PasswordTypesList.Codes.CDS;
			extPassword.GP_UserID = userId;
			extPassword.GP_PasswordStatus = passwordStatus;
			extPassword.GP_IssueDate = issueDate;
			extPassword.GP_ExpiryDate = expiryDate;
			extPassword.GP_GC = comp;
		}
	}
}
