using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	sealed class LicenceConsumptionLogCreatorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCreateLogTakesLongerThanExpected()
		{
			var worker = new Mock<IIdleWorker>();
			worker.Setup(m => m.QueueWorkItemWithOptions(1000, UserIdleWorkItemOptions.DisableSlowRunningWarning,
				It.IsAny<Delegate>(), It.IsAny<object[]>()));

			ObjectFactory.Substitute(worker.Object);
			var logCreator = new LicenceConsumptionLogCreatorForTest();
			var checkpoint = new TestLicenceCheckpoint { LicenceType = ModuleLicenceType.CPT };
			logCreator.CreateLog(checkpoint);
			worker.VerifyAll();
		}

		public void TestCreateLog()
		{
			var logCreator = new LicenceConsumptionLogCreatorForTest();
			var checkpoint = new TestLicenceCheckpoint();
			checkpoint.LicenceType = ModuleLicenceType.ODM;
			logCreator.CreateLog(checkpoint);
			IdleWorker.Flush();

			AssertNotNull(logCreator.LastCreatedLog);
			AssertEquals(true, logCreator.LastCreatedLog.IsInDatabase);
			AssertEquals(true, logCreator.LastCreatedLog.S7_EnterpriseActivity);
			AssertEquals("ZUB", logCreator.LastCreatedLog.S7_FormCaption);
			AssertEquals(LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString(), logCreator.LastCreatedLog.S7_ControllerID);
			AssertZDatesWithin5Minutes("Time", ZDateTime.UtcNow, logCreator.LastCreatedLog.S7_OpenDateTimeUtc);
			AssertEquals(Env.CurrentBranch.PK, logCreator.LastCreatedLog.S7_ParentID);
			AssertEquals(GlbBranchSchema.Constants.Prefix, logCreator.LastCreatedLog.S7_ParentTableCode);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, logCreator.LastCreatedLog.S7_GS_NKUser);
			AssertEquals((int)ModuleLicenceType.ODM, logCreator.LastCreatedLog.S7_MouseClicks);
			AssertEquals(string.Empty, logCreator.LastCreatedLog.S7_DeviceID);
			AssertEquals(0, logCreator.LastCreatedLog.S7_KeyStrokes);

			checkpoint = new TestLicenceCheckpoint();
			checkpoint.LicenceType = ModuleLicenceType.CPT;
			logCreator.CreateLog(checkpoint);
			IdleWorker.Flush();
			AssertEquals((int)ModuleLicenceType.CPT, logCreator.LastCreatedLog.S7_MouseClicks);
		}

		public void TestCreateLog_WithDeviceIDAndKeystrokes()
		{
			var logCreator = new LicenceConsumptionLogCreatorForTest();
			var checkpoint = new TestLicenceCheckpoint();
			var deviceID = Guid.NewGuid().ToString();
			var deviceDetails = "MAN:Zebra|MDL:TC77|OS:AND";
			checkpoint.LicenceType = ModuleLicenceType.ODM;
			logCreator.CreateLog(checkpoint, deviceID, deviceDetails, keyStrokes: 2);
			IdleWorker.Flush();

			AssertNotNull(logCreator.LastCreatedLog);
			AssertEquals(true, logCreator.LastCreatedLog.IsInDatabase);
			AssertEquals(true, logCreator.LastCreatedLog.S7_EnterpriseActivity);
			AssertEquals("ZUB", logCreator.LastCreatedLog.S7_FormCaption);
			AssertEquals(LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString(), logCreator.LastCreatedLog.S7_ControllerID);
			AssertZDatesWithin5Minutes("Time", ZDateTime.UtcNow, logCreator.LastCreatedLog.S7_OpenDateTimeUtc);
			AssertEquals(Env.CurrentBranch.PK, logCreator.LastCreatedLog.S7_ParentID);
			AssertEquals(GlbBranchSchema.Constants.Prefix, logCreator.LastCreatedLog.S7_ParentTableCode);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, logCreator.LastCreatedLog.S7_GS_NKUser);
			AssertEquals((int)ModuleLicenceType.ODM, logCreator.LastCreatedLog.S7_MouseClicks);
			AssertEquals(deviceID, logCreator.LastCreatedLog.S7_DeviceID);
			AssertEquals(deviceDetails, logCreator.LastCreatedLog.S7_DeviceDetails);
			AssertEquals(2, logCreator.LastCreatedLog.S7_KeyStrokes);
		}

		public void TestCreateLog_WithDeviceID_Null()
		{
			TestCreateLog_WithDeviceID_NullOrWhiteSpaceCore(null);
		}

		public void TestCreateLog_WithDeviceID_WhiteSpace()
		{
			TestCreateLog_WithDeviceID_NullOrWhiteSpaceCore("    ");
		}

		public void TestCreateLog_WithDeviceID_WhiteSpaceCharacters()
		{
			TestCreateLog_WithDeviceID_NullOrWhiteSpaceCore("\r\n");
		}

		void TestCreateLog_WithDeviceID_NullOrWhiteSpaceCore(string deviceID)
		{
			var logCreator = new LicenceConsumptionLogCreatorForTest();
			var checkpoint = new TestLicenceCheckpoint();
			checkpoint.LicenceType = ModuleLicenceType.ODM;
			logCreator.CreateLog(checkpoint, deviceID, "MAN:Zebra|MDL:TC77|OS:AND", keyStrokes: 1);
			IdleWorker.Flush();

			AssertNotNull(logCreator.LastCreatedLog);
			AssertEquals(true, logCreator.LastCreatedLog.IsInDatabase);
			AssertEquals(true, logCreator.LastCreatedLog.S7_EnterpriseActivity);
			AssertEquals("ZUB", logCreator.LastCreatedLog.S7_FormCaption);
			AssertEquals(LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString(), logCreator.LastCreatedLog.S7_ControllerID);
			AssertZDatesWithin5Minutes("Time", ZDateTime.UtcNow, logCreator.LastCreatedLog.S7_OpenDateTimeUtc);
			AssertEquals(Env.CurrentBranch.PK, logCreator.LastCreatedLog.S7_ParentID);
			AssertEquals(GlbBranchSchema.Constants.Prefix, logCreator.LastCreatedLog.S7_ParentTableCode);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, logCreator.LastCreatedLog.S7_GS_NKUser);
			AssertEquals((int)ModuleLicenceType.ODM, logCreator.LastCreatedLog.S7_MouseClicks);
			AssertEquals(string.Empty, logCreator.LastCreatedLog.S7_DeviceID);
			AssertEquals("MAN:Zebra|MDL:TC77|OS:AND", logCreator.LastCreatedLog.S7_DeviceDetails);
			AssertEquals(1, logCreator.LastCreatedLog.S7_KeyStrokes);
		}

		public void TestCreateLog_WithDeviceDetails_Null()
		{
			TestCreateLog_WithDeviceDetails_NullOrWhiteSpaceCore(null);
		}

		public void TestCreateLog_WithDeviceDetails_WhiteSpace()
		{
			TestCreateLog_WithDeviceDetails_NullOrWhiteSpaceCore("    ");
		}

		public void TestCreateLog_WithDeviceDetails_WhiteSpaceCharacters()
		{
			TestCreateLog_WithDeviceDetails_NullOrWhiteSpaceCore("\r\n");
		}

		void TestCreateLog_WithDeviceDetails_NullOrWhiteSpaceCore(string deviceDetails)
		{
			var logCreator = new LicenceConsumptionLogCreatorForTest();
			var checkpoint = new TestLicenceCheckpoint();
			checkpoint.LicenceType = ModuleLicenceType.ODM;
			logCreator.CreateLog(checkpoint, "ABC", deviceDetails, keyStrokes: 1);
			IdleWorker.Flush();

			AssertNotNull(logCreator.LastCreatedLog);
			AssertEquals(true, logCreator.LastCreatedLog.IsInDatabase);
			AssertEquals(true, logCreator.LastCreatedLog.S7_EnterpriseActivity);
			AssertEquals("ZUB", logCreator.LastCreatedLog.S7_FormCaption);
			AssertEquals(LicenceCheckpoint.LicenceConsumptionActivityLogKey.ToString(), logCreator.LastCreatedLog.S7_ControllerID);
			AssertZDatesWithin5Minutes("Time", ZDateTime.UtcNow, logCreator.LastCreatedLog.S7_OpenDateTimeUtc);
			AssertEquals(Env.CurrentBranch.PK, logCreator.LastCreatedLog.S7_ParentID);
			AssertEquals(GlbBranchSchema.Constants.Prefix, logCreator.LastCreatedLog.S7_ParentTableCode);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, logCreator.LastCreatedLog.S7_GS_NKUser);
			AssertEquals((int)ModuleLicenceType.ODM, logCreator.LastCreatedLog.S7_MouseClicks);
			AssertEquals("ABC", logCreator.LastCreatedLog.S7_DeviceID);
			AssertEquals(string.Empty, logCreator.LastCreatedLog.S7_DeviceDetails);
			AssertEquals(1, logCreator.LastCreatedLog.S7_KeyStrokes);
		}

		public void TestCreateLog_BranchAndUserFromLicenceNotCurrentEnv()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "BZZ";
			branch.GB_RL_NKHomePort = "USERI";
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "MEH";
			staff.GS_FullName = "meh_hi";

			Factory.Save();

			LicenceConsumptionLogCreatorForTest logCreator = new LicenceConsumptionLogCreatorForTest();
			var core = Env.Licence.Core;
			using (var tempContext = Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				logCreator.CreateLog(core, Factory);
			}

			AssertEquals("Branch is from licence", Env.CurrentBranch.PK, logCreator.LastCreatedLog.S7_ParentID);
			AssertEquals("User is from licence", Env.CurrentUser.Initials, logCreator.LastCreatedLog.S7_GS_NKUser);
		}

		[TestDate(2013, 9, 11, 11, 22, 0)]
		public void TestCreateLog_UtcNow()
		{
			LicenceConsumptionLogCreatorForTest logCreator = new LicenceConsumptionLogCreatorForTest();
			var expectedDateTime = new ZDateTime(2012, 1, 1, 9, 5, 0);
			logCreator.CreateLog(Env.Licence.Core, expectedDateTime.ToDateTime());
			IdleWorker.Flush();
			AssertEquals(expectedDateTime, logCreator.LastCreatedLog.S7_OpenDateTimeUtc);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestCreateLogForWeb()
		{
			var cachedIsWeb = Globals.IsWeb;
			var cachedProvider = Env.GetCurrentProvider();
			LicenceConsumptionLogCreatorForTest logCreator = new LicenceConsumptionLogCreatorForTest();

			try
			{
				using (var environmentProvider = new WebEnvironmentProviderForTest())
				{
					var userContext = Env.Instance.CurrentUserContext;
					environmentProvider.Enable();
					Env.Instance.SetUserContext(userContext);
					Globals.IsWeb = true;
					var contact = (ContactForTest)((IWebEnvironment)Env.Instance).WebUser;

					contact.Name = "Zubin Appoo";
					contact.Email = "zubs@cargowise.com";
					logCreator.CreateLog(new TestLicenceCheckpoint());
				}
			}
			finally
			{
				Globals.IsWeb = cachedIsWeb;
				cachedProvider.Enable();
			}

			AssertNotNull(logCreator.LastCreatedLog);
			AssertEquals(true, logCreator.LastCreatedLog.IsInDatabase);
			AssertEquals(true, logCreator.LastCreatedLog.S7_EnterpriseActivity);
			AssertEquals("ZUB", logCreator.LastCreatedLog.S7_FormCaption);
			AssertEquals($"{LicenceCheckpoint.LicenceConsumptionActivityLogKey}|zubs@cargowise.com|Zubin Appoo", logCreator.LastCreatedLog.S7_ControllerID);
			AssertZDatesWithin5Minutes("Time", ZDateTime.UtcNow, logCreator.LastCreatedLog.S7_OpenDateTimeUtc);
			AssertEquals(Env.CurrentBranch.PK, logCreator.LastCreatedLog.S7_ParentID);
			AssertEquals(GlbBranchSchema.Constants.Prefix, logCreator.LastCreatedLog.S7_ParentTableCode);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, logCreator.LastCreatedLog.S7_GS_NKUser);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestCreateLogForWebWithLongUserName()
		{
			var cachedIsWeb = Globals.IsWeb;
			var cachedProvider = Env.GetCurrentProvider();
			var logCreator = new LicenceConsumptionLogCreatorForTest();
			var contactEmail = "test@test.com";
			var contactName = new string('C', StmActivityLogSchema.S7_ControllerID.MaxLength + 1);
			var expectedControllerID = $"{LicenceCheckpoint.LicenceConsumptionActivityLogKey}|{contactEmail}|{contactName}".Substring(0, StmActivityLogSchema.S7_ControllerID.MaxLength);

			try
			{
				using (var environmentProvider = new WebEnvironmentProviderForTest())
				{
					var userContext = Env.Instance.CurrentUserContext;
					environmentProvider.Enable();
					Env.Instance.SetUserContext(userContext);
					Globals.IsWeb = true;
					var contact = (ContactForTest)((IWebEnvironment)Env.Instance).WebUser;

					contact.Name = contactName;
					contact.Email = contactEmail;
					logCreator.CreateLog(new TestLicenceCheckpoint());
				}
			}
			finally
			{
				Globals.IsWeb = cachedIsWeb;
				cachedProvider.Enable();
			}

			AssertNotNull(logCreator.LastCreatedLog);
			AssertEquals(expectedControllerID, logCreator.LastCreatedLog.S7_ControllerID);
		}

		public void TestCreateLog_WithFactory()
		{
			var anotherFactory = new BusinessObjectFactory();
			LicenceConsumptionLogCreatorForTest logCreator = new LicenceConsumptionLogCreatorForTest();
			TestLicenceCheckpoint checkpoint = new TestLicenceCheckpoint();
			checkpoint.LicenceType = ModuleLicenceType.ODM;
			logCreator.CreateLog(checkpoint, anotherFactory);
			IdleWorker.Flush();
			CombineAssertions(() =>
			{
				AssertEquals("before save", false, logCreator.LastCreatedLog.IsInDatabase);
				anotherFactory.Save();
				AssertEquals("after save", true, logCreator.LastCreatedLog.IsInDatabase);
			});
		}

		class ContactForTest : IContactBase
		{
			public string Name { get; set; }

			public string Email { get; set; }
		}

		class WebEnvironmentForTest : BaseEnvironment, IWebEnvironment
		{
			public WebEnvironmentForTest() : base(new MultiThreadUserContextManager()) { }

			public IContactBase WebUser => webUser ?? (webUser = new ContactForTest());

			IContactBase webUser;

			public override string ApplicationStartupPath => throw new NotImplementedException();

			public override IUserLoginController LoginController => throw new NotImplementedException();

			protected override ISemaphoreProvider EnvironmentSpecificSemaphoreProvider => throw new NotImplementedException();

			public override void ExitApplication()
			{
				throw new NotImplementedException();
			}
		}

		class WebEnvironmentProviderForTest : EnvProvider
		{
			public override BaseEnvironment Instance => environment ?? (environment = new WebEnvironmentForTest());

			BaseEnvironment environment;

			protected override IDbEnvironment GetDbEnvironmentInstance() => new BaseDbEnvironment();

			protected override void Dispose(bool isDisposing)
			{
				base.Dispose(isDisposing);
				environment?.Dispose();
			}
		}

		public void TestCreateLog_SaveException()
		{
			LicenceConsumptionLogCreatorForTest logCreator = new LicenceConsumptionLogCreatorForTest();
			logCreator.ExceptionToThrow = new ZSaveException(new ZDataException(new InvalidOperationException("Error during save"), null, null), new BusinessObjectFactory());
			logCreator.CreateLog(new TestLicenceCheckpoint(), true);
			AssertNull("exception caught, log not created", logCreator.LastCreatedLog);
		}

		#region Implementation

		IIdleWorker IdleWorker
		{
			get
			{
				return ObjectFactory.Get<IIdleWorker>();
			}
		}

		public class LicenceConsumptionLogCreatorForTest : LicenceConsumptionLogCreator
		{
			public Exception ExceptionToThrow;

			public LicenceConsumptionLogCreatorForTest()
			{
				CreateLogOnIdleInTest = true;
			}

			protected override StmActivityLog CreateLogInternal(ILicenceCheckpoint checkpoint, DateTime utcNow, string deviceID, string deviceDetails, int keyStrokes, BusinessObjectFactory factory)
			{
				if (ExceptionToThrow != null)
				{
					throw ExceptionToThrow;
				}

				LastCreatedLog = base.CreateLogInternal(checkpoint, utcNow, deviceID, deviceDetails, keyStrokes, factory);
				return LastCreatedLog;
			}

			internal StmActivityLog CreateLog(ILicenceCheckpoint checkpoint, BusinessObjectFactory factory)
			{
				return CreateLogInternal(checkpoint, DateTime.MinValue, "", "", 0, factory);
			}

			public StmActivityLog LastCreatedLog;
		}

		class TestLicenceCheckpoint : ILicenceCheckpoint
		{
			public string DisplayName
			{
				get { return "Zubins Licence"; }
			}

			public string LastReasonForNotAllowing { get; set; }

			public ILicenceCheckpoint ParentCheckpoint { get; set; }

			public ModuleLicenceType LicenceType { get; set; }

			public LicenceLoginResponse Login(ILicensedComponent licensedComponent)
			{
				return LicenceLoginResponse.Granted;
			}

			public void Logout(ILicensedComponent licensedComponent)
			{
			}

			public string Name
			{
				get { return "ZUB"; }
			}

			public ILicenceCheckpointUserContext ParentUserContext
			{
				get { return parentUserContext ?? (parentUserContext = new TestParentUserContext()); }
			}
			ILicenceCheckpointUserContext parentUserContext;
		}

		class TestParentUserContext : ILicenceCheckpointUserContext
		{
			public Guid BranchPk
			{
				get { return Env.CurrentBranch.PK; }
			}

			public string UserInitials
			{
				get { return Env.CurrentUser.Initials; }
			}
		}

		#endregion
	}
}
