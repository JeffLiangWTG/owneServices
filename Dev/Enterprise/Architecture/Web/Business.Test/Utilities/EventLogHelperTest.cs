using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
#if NET
using Microsoft.AspNetCore.Http;
using Moq;
#endif

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class EventLogHelperTest : TestCaseWithFactory
	{
		#region Implementation

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected override void SetUp()
		{
			EnableLogging(true);
			base.SetUp();
#if NET
			var mockAccessor = new Mock<IHttpContextAccessor>();
			var context = new DefaultHttpContext();
			context.Session = new DummySession();
			mockAccessor.Setup(a => a.HttpContext).Returns(context);
			WebEnv.HttpContextAccessor = mockAccessor.Object;
#endif
		}

#if NET
		protected override void TearDown()
		{
			WebEnv.HttpContextAccessor = null;
		}
#endif

		EventLogHelper Helper
		{
			get
			{
				if (logHelper == null)
				{
					logHelper = new EventLogHelper();
				}
				return logHelper;
			}
		}

		EventLogHelper logHelper;

		WebUser TestSiteUser
		{
			get
			{
				if (siteUser == null)
				{
					siteUser = GetNewSiteUser(true);
				}
				return siteUser;
			}
		}

		WebUser siteUser;

		WebUser TestSiteUserNotLoggedIn
		{
			get
			{
				if (siteUserNotLoggedIn == null)
				{
					siteUserNotLoggedIn = GetNewSiteUser(false);
				}
				return siteUserNotLoggedIn;
			}
		}

		WebUser siteUserNotLoggedIn;

		WebUser GetNewSiteUser(bool performLogin)
		{
			OrgContactWebUser user = new OrgContactWebUser();
			if (performLogin)
			{
				user.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			}
			return user;
		}

		ZString CurrentUserCode
		{
			get { return GlbStaff.CurrentUser.GS_Code; }
		}

		void EnableLogging(bool enable)
		{
			WebDataRegistry.Instance.WebActivityLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enable);
		}

		#endregion

		#region TestGetReference

		public void TestGetReference()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.HumanReadableNameForTest = "Dummy Job DJ000123";
			ZString reference = Helper.GetReference(dummyBO);

			AssertEquals("Dummy Job DJ000123", reference);
		}

		#endregion

		#region TestLoggingEnabled

		public void TestLoggingEnabled()
		{
			EnableLogging(false);
			AssertNull(Helper.CreateLogForModuleChanged(TestSiteUser, "Some module name"));
		}

		#endregion

		#region TestCreateLogForUserLoggedIn

		public void TestCreateLogForUserLoggedIn()
		{
			var log = Helper.CreateLogForUserLoggedIn(TestSiteUser);

			AssertEquals(Events.Login.Code, log.SL_SE_NKEvent);
			AssertEquals(CurrentUserCode, log.SL_GS_NKUser);
			AssertEquals(OrgContact.Schema.TableName, log.SL_Table);
			AssertEquals(TestSiteUser.LoggedInUser.PK, log.SL_Parent);
			AssertEquals(string.Empty, log.SL_Reference);
		}

		#endregion

		#region TestCreateLogForDocumentPrinted

		public void TestCreateLogForDocumentPrinted()
		{
			var log = Helper.CreateLogForDocumentPrinted(TestSiteUser, "Some document name");

			AssertEquals(Events.WebDocumentPrintedDocName.Code, log.SL_SE_NKEvent);
			AssertEquals(CurrentUserCode, log.SL_GS_NKUser);
			AssertEquals(OrgContact.Schema.TableName, log.SL_Table);
			AssertEquals(TestSiteUser.LoggedInUser.PK, log.SL_Parent);
			AssertEquals("Some document name", log.SL_Reference);
		}

		#endregion

		#region TestCreateLogForReportPrinted

		public void TestCreateLogForReportPrinted()
		{
			var log = Helper.CreateLogForReportPrinted(TestSiteUser, "Some report name");

			AssertEquals(Events.WebReportPrintedReportName.Code, log.SL_SE_NKEvent);
			AssertEquals(CurrentUserCode, log.SL_GS_NKUser);
			AssertEquals(OrgContact.Schema.TableName, log.SL_Table);
			AssertEquals(TestSiteUser.LoggedInUser.PK, log.SL_Parent);
			AssertEquals("Some report name", log.SL_Reference);
		}

		#endregion

		#region TestCreateLogForModuleChanged

		public void TestCreateLogForModuleChanged()
		{
			var log = Helper.CreateLogForModuleChanged(TestSiteUser, "Some module name");

			AssertEquals(Events.WebModuleAccessedModuleName.Code, log.SL_SE_NKEvent);
			AssertEquals(CurrentUserCode, log.SL_GS_NKUser);
			AssertEquals(OrgContact.Schema.TableName, log.SL_Table);
			AssertEquals(TestSiteUser.LoggedInUser.PK, log.SL_Parent);
			AssertEquals("Some module name", log.SL_Reference);
		}

		#endregion

		#region TestCreateLogForAdd

		public void TestCreateLogForAdd()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.HumanReadableNameForTest = "Dummy Job DJ000123";
			var log = Helper.CreateLogForAdd(TestSiteUser, dummyBO);

			AssertEquals(Events.AddedARecordToTheSystem.Code, log.SL_SE_NKEvent);
			AssertEquals(CurrentUserCode, log.SL_GS_NKUser);
			AssertEquals(OrgContact.Schema.TableName, log.SL_Table);
			AssertEquals(TestSiteUser.LoggedInUser.PK, log.SL_Parent);
			AssertEquals("Dummy Job DJ000123", log.SL_Reference);
		}

		#endregion

		#region TestCreateLogForEdit

		public void TestCreateLogForEdit()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.HumanReadableNameForTest = "Dummy Job DJ000123";
			var log = Helper.CreateLogForEdit(TestSiteUser, dummyBO);

			AssertEquals(Events.EditedARecord.Code, log.SL_SE_NKEvent);
			AssertEquals(CurrentUserCode, log.SL_GS_NKUser);
			AssertEquals(OrgContact.Schema.TableName, log.SL_Table);
			AssertEquals(TestSiteUser.LoggedInUser.PK, log.SL_Parent);
			AssertEquals("Dummy Job DJ000123", log.SL_Reference);
		}

		#endregion

		#region TestCreateLogForCancel

		public void TestCreateLogForCancel()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.HumanReadableNameForTest = "Dummy Job DJ000123";
			var log = Helper.CreateLogForCancel(TestSiteUser, dummyBO);

			AssertEquals(Events.SetToInactive.Code, log.SL_SE_NKEvent);
			AssertEquals(CurrentUserCode, log.SL_GS_NKUser);
			AssertEquals(OrgContact.Schema.TableName, log.SL_Table);
			AssertEquals(TestSiteUser.LoggedInUser.PK, log.SL_Parent);
			AssertEquals("Dummy Job DJ000123", log.SL_Reference);
		}

		#endregion

		#region TestCreateWMREvent

		public void TestCreateWMREvent()
		{
			string refStr = "RAG, CNT, BKP";
			var log = Helper.CreateWMREvent(TestSiteUser, refStr);

			AssertEquals(Events.ModifiedOnWeb.Code, log.SL_SE_NKEvent);
			AssertEquals(CurrentUserCode, log.SL_GS_NKUser);
			AssertEquals(OrgContact.Schema.TableName, log.SL_Table);
			AssertEquals(TestSiteUser.LoggedInUser.PK, log.SL_Parent);
			AssertEquals("RAG, CNT, BKP", log.SL_Reference);
		}

		#endregion

		#region TestCreateLogForModuleChangedWhenUserNotLoggedIn

		[ExpectNoExceptions]
		public void TestCreateLogForModuleChangedWhenUserNotLoggedIn()
		{
			AssertNull("We don't create logs if user is not logged in", Helper.CreateLogForModuleChanged(TestSiteUserNotLoggedIn, "Some module name"));
		}

		#endregion
	}
}
