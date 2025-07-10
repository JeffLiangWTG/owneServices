using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	public class UserContextTest : TestCaseWithFactory
	{
		public void TestThreadSafeClone()
		{
			var user = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff"));
			var company = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			var branch = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch["GB_GC"] = company.PK;
			var department = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbDepartment"));

			Factory.Save();
			var token = new Mock<ILoginToken>();
			((IUser)user).LoginToken = token.Object;

			var original = new UserContext(user.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid(), token.Object);
			var clone = original.ThreadSafeClone();

			AssertSameObjectOnDifferentFactories("Company", original.Company, clone.Company);
			AssertSameObjectOnDifferentFactories("Branch", original.Branch, clone.Branch);
			AssertSameObjectOnDifferentFactories("Department", original.Department, clone.Department);
			AssertSameObjectOnDifferentFactories("User", original.User, clone.User);

			AssertEquals("LoginToken should be transferred", original.User.LoginToken, clone.User.LoginToken);
		}

		[ExpectNoExceptions]
		public void TestThreadSafeCloneInNewThread()
		{
			var user = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff"));
			var company = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			var branch = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch["GB_GC"] = company.PK;
			var department = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbDepartment"));

			Factory.Save();

			var original = new UserContext(user.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid());
			var thread = new Thread(() => original.ThreadSafeClone());
			thread.Start();
			thread.Join();
		}

		public void TestFactoryRefreshEnabled()
		{
			var user = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff"));
			var company = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			var branch = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch["GB_GC"] = company.PK;
			var department = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbDepartment"));

			Factory.Save();

			Globals.IsUserInteractive = true;
			var context = new UserContext(user.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid());
			AssertEquals(true, ((BusinessObject)context.Branch).Factory.RefreshEnabled);
			Globals.IsUserInteractive = false;
			context = new UserContext(user.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid());
			AssertEquals(false, ((BusinessObject)context.Branch).Factory.RefreshEnabled);
		}

		void AssertSameObjectOnDifferentFactories(string name, object a, object b)
		{
			var idA = ((IFactoryProvider)a).Factory._Instance;
			var idB = ((IFactoryProvider)b).Factory._Instance;

			Assert(name + " should be different references", !ReferenceEquals(a, b));
			AssertEquals(name + " should be the same bizo", ((BusinessObject)a).PK, ((BusinessObject)b).PK);
			AssertNotEquals(name + " should use different factory instances", idA, idB);
		}

		void SetUpPostMaster()
		{
			var pm = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff"));
			pm[GlbStaffSchema.GS_EmailAddress] = "pm@pm.pm";
			var pmg = Factory.Load(ObjectFactory.GetType("IGlbGroup"), EnvProxy.Instance.Registry.PostMasterGroup);
			var groupLink = Factory.New(ObjectFactory.GetType("IGlbGroupLink"));
			groupLink[GlbGroupLinkSchema.GK_GS] = pm.PK;
			groupLink[GlbGroupLinkSchema.GK_GG] = pmg.PK;
			Factory.Save();
		}

		public void TestEmailWillBeSentAndErrorWillBeReportedWhenUserIsNullAndNotUserInteractive()
		{
			bool isUserInteractive = Globals.IsUserInteractive;
			const string userName = "UserIsNull";
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				UserContext.ForceToNotSkipForTest = true;
				// Must be OK to send an error Email.
				Assert(UserContext.CheckErrorEmailInterval(userName));
				var demoDepartment = (IDepartment)Factory.LoadTop1(ObjectFactory.GetType("IGlbDepartment"), new ZQuery());
				SetUpPostMaster();
				AssertNotNull(new UserContext(userName, Guid.Empty, demoDepartment.PK));

				// The error Email must have been created.
				Assert(Env.OutgoingMailManager.EmailsCreated.Any(ed => ed.Body.Contains("There is no user found", StringComparison.OrdinalIgnoreCase)));
				AssertEquals("No file attachment", 0, Env.OutgoingMailManager.EmailsCreated[0].Attachments.Count);
				// Must not be OK to send an error Email now to prevent too many Emails.
				Assert(!UserContext.CheckErrorEmailInterval(userName));

				// Must report an error
				string expectedErrorMessage = $@"There is no User found for login ""{userName}"".
And it in fact, does not appear to be in the database. Check callstack for how this username could have been attempted in non-interactive context.
";
				AssertEquals(expectedErrorMessage, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestDemoCompanyLicence()
		{
			var factory = new BusinessObjectFactory();
			var demoCompany = (ICompany)factory.LoadTop1(ObjectFactory.GetType("IGlbCompany"), new ZQuery(GlbCompanySchema.GC_Code, "DEM"));

			UserForTest user = new UserForTest();

			user.LoggedInWithMasterPassword = false;
			UserContext context = new UserContextForTest(user, demoCompany);
			AssertEquals("no master password", LicenceLoginResponse.Granted, context.Licence.Core.Login(new LicensedComponentForTest()));

			foreach (LicenceCheckpoint checkpoint in context.Licence.GetAllCheckpoints())
			{
				AssertEquals("Login", LicenceLoginResponse.Granted, checkpoint.Login(new LicensedComponentForTest()));
			}

			context.Licence.Core.ForceLogout();

			user.LoggedInWithMasterPassword = true;
			context = new UserContextForTest(user, demoCompany);
			var component = new LicensedComponentForTest();
			AssertEquals("master password", LicenceLoginResponse.Granted, context.Licence.Core.Login(component));

			foreach (LicenceCheckpoint checkpoint in context.Licence.GetAllCheckpoints())
			{
				AssertEquals("Login", LicenceLoginResponse.Granted, checkpoint.Login(new LicensedComponentForTest()));
			}

			context.Licence.Core.Logout(component);
		}

		public void TestInstances()
		{
			var userContext = new UserContext();
			var x = userContext.GetInstance<X>(() => new X());
			AssertEquals(x, userContext.GetInstance<X>(() => new X()));
			AssertNotEquals(x, new UserContext().GetInstance<X>(() => new X()));
		}

		public void TestDefaultValueOfStaticForTestProperties()
		{
			AssertEquals(false, UserContext.ForceToNotSkipForTest);
		}

		public void TestCtorSetsLoginAuthenticationInfo()
		{
			var currentUser = Env.CurrentUser;
			var currentBranch = Env.CurrentBranchPK;
			var currentDepartment = Env.CurrentDepartmentPK;

			CombineAssertions(() =>
			{
				Test(LoginAuthenticationInfo.NewSuccessfulLogin(currentUser));
				Test(LoginAuthenticationInfo.NewTwoFactorAuthenticationLogin(currentUser));
				Test(LoginAuthenticationInfo.NewTwoFactorAuthenticationFailedLogin(currentUser));
				Test(LoginAuthenticationInfo.NewTwoFactorAuthenticationFieldMissing(currentUser));
				Test(LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.ADRecordNotFound, ":("));
				Test(LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.SecurityFailure, ":("));
				Test(LoginAuthenticationInfo.NewFailedLogin(":("));
			});

			void Test(LoginAuthenticationInfo loginAuthenticationInfo)
			{
				var result = new UserContext(currentUser, currentBranch, currentDepartment, loginAuthenticationInfo: loginAuthenticationInfo).LoginAuthenticationInfo;

				AssertEquals(loginAuthenticationInfo, result);
			}
		}

		public void TestLoadFromQueryWithFactoryLoadDoesntWork()
		{
			var company = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			var branch = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch["GB_GC"] = company.PK;
			var department = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbDepartment"));
			Factory.Save();

			var factory = new Mock<BusinessObjectFactory>();
			//fails on load by pk
			factory.Setup(f => f.Load(ObjectFactory.GetType("IGlbBranch"), It.IsAny<ZGuid>())).Returns(() => null);
			factory.Setup(f => f.Load(ObjectFactory.GetType("IGlbCompany"), It.IsAny<ZGuid>())).Returns(() => null);
			//succeeds on load by query
			factory.Setup(f => f.Load(ObjectFactory.GetType("IGlbBranch"), It.IsAny<ZQuery>())).Returns((Type bizO, ZQuery sqlQ) => Factory.Load(bizO, sqlQ));
			factory.Setup(f => f.Load(ObjectFactory.GetType("IGlbCompany"), It.IsAny<ZQuery>())).Returns((Type bizO, ZQuery sqlQ) => Factory.Load(bizO, sqlQ));

			AssertNoExceptionThrown(() => UserContext.SetupAndReturnUserContextForTemporarySwitch(User.ServiceUserName, branch.PK.ToGuid(), department.PK.ToGuid(), factory.Object));
		}

		class X
		{
		}

		public class UserContextForTest : UserContext
		{
			public UserContextForTest(IUser user, ICompany company)
			{
				this.User = user;
				this.Company = company;
			}
		}

		class LicensedComponentForTest : ILicensedComponent
		{
			#region ILicensedComponent Members

			public LicensedComponentManager LicensedComponentManager
			{
				get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
			}

			IDisposable ILicensedComponent.LicensedComponentManager
			{
				get { return new LicensedComponentManager(this); }
			}

			#endregion
		}
	}
}
