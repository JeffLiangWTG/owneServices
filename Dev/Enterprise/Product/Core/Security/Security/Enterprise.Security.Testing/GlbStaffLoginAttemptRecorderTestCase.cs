using System;
using System.Security.Cryptography;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.Testing
{
	sealed class GlbStaffLoginAttemptRecorderTestCase : TestCaseWithFactory
	{
		public void TestRecordLoginAttempt()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "TestLogin";
			Factory.Save();

			var recorder = new GlbStaffLoginAttemptRecorder();

			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

			var hash = GetRandomBytes(32);

			var utcStart = GetDatabaseDate();
			recorder.RecordLoginAttempt(staff.GS_LoginName, null);
			Assert("Not locked out on first attempt", !recorder.IsLockedOut(staff.GS_LoginName, null));

			recorder.RecordLoginAttempt(staff.GS_LoginName, null);
			Assert("Locked out on second attempt", recorder.IsLockedOut(staff.GS_LoginName, null));
			Assert("Not locked out with loginHash", !recorder.IsLockedOut(staff.GS_LoginName, hash));

			var utcEnd = GetDatabaseDate();
			var loginDisabledUntilUtc = recorder.LockoutDateTimeLocal(staff.GS_LoginName).ToUniversalBranchTime();
			CombineAssertions("Lockout time is set", () =>
			{
				Assert($"{utcStart.AddMinutes(5).SqlFormat} <= {loginDisabledUntilUtc.SqlFormat}", utcStart.AddMinutes(5) <= loginDisabledUntilUtc);
				Assert($"{loginDisabledUntilUtc.SqlFormat} <= {utcEnd.AddMinutes(5).SqlFormat}", loginDisabledUntilUtc <= utcEnd.AddMinutes(5));
			});

			ZDateTime GetDatabaseDate()
			{
				var datetimeType = StmLoginFailureLogSchema.SFL_SystemCreateTimeUtc.SqlDbTypeDeclaration;
				using (var command = Db.Connection.Command($"SELECT CAST(GetUtcDate() AS {datetimeType})"))
				{
					return new ZDateTime((DateTime)command.ExecuteScalar());
				}
			}
		}

		public void TestRecordLoginAttempt_AfterLockoutAndUnlock()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "TestLogin";
			Factory.Save();

			var recorder = new GlbStaffLoginAttemptRecorder();

			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

			recorder.RecordLoginAttempt(staff.GS_LoginName, null);
			Assert("Not locked out on first attempt", !recorder.IsLockedOut(staff.GS_LoginName, null));

			recorder.RecordLoginAttempt(staff.GS_LoginName, null);
			Assert("Locked out on second attempt", recorder.IsLockedOut(staff.GS_LoginName, null));

			recorder.RecordLoginAttempt(staff.GS_LoginName, null);
			Assert("Still locked out", recorder.IsLockedOut(staff.GS_LoginName, null));

			recorder.Unlock(staff.GS_LoginName);

			recorder.RecordLoginAttempt(staff.GS_LoginName, null);
			Assert("Not locked out on first attempt after unlock", !recorder.IsLockedOut(staff.GS_LoginName, null));
		}

		public void TestUnlock()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "TestLogin";
			Factory.Save();

			var recorder = new GlbStaffLoginAttemptRecorder();

			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

			recorder.RecordLoginAttempt(staff.GS_LoginName, null);
			recorder.RecordLoginAttempt(staff.GS_LoginName, null);
			Assert("Should be locked out", recorder.IsLockedOut(staff.GS_LoginName, null));

			recorder.Unlock(staff.GS_LoginName);

			Assert("Should not be locked out", !recorder.IsLockedOut(staff.GS_LoginName, null));
		}

		public void TestIsAnonymousUserLockedOut()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "TestLogin";
			Factory.Save();

			var recorder = new GlbStaffLoginAttemptRecorder();

			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

			var hash = GetRandomBytes(32);

			recorder.RecordLoginAttempt(staff.GS_LoginName, hash);
			recorder.RecordLoginAttempt(staff.GS_LoginName, hash);

			Assert("Should be locked out", recorder.IsAnonymousUserLockedOut(staff.GS_LoginName));

			recorder.Unlock(staff.GS_LoginName);

			Assert("Should not be locked out", !recorder.IsAnonymousUserLockedOut(staff.GS_LoginName));
		}

		public void TestLockoutDateTimeLocal()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "TestLogin";
			Factory.Save();

			var recorder = new GlbStaffLoginAttemptRecorder();
			const int lockoutMinutes = 30;

			Env.Registry.RawRegistry.LoginAttempts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			Env.Registry.RawRegistry.LoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lockoutMinutes);

			var utcNow = ZDateTime.UtcNow;

			var loginFailureLog = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog.SFL_IsLockOut = true;
			loginFailureLog.SFL_LoginName = staff.GS_LoginName;
			loginFailureLog.SFL_TableCode = GlbStaffSchema.Constants.Prefix;
			loginFailureLog.SFL_SystemCreateTimeUtc = utcNow.AddMinutes(-20);
			Factory.Save();

			var lockedOutUntil = recorder.LockoutDateTimeLocal(staff.GS_LoginName);
			Assert("Should return date plus lockout minutes from the record", lockedOutUntil == utcNow.AddMinutes(-20).AddMinutes(lockoutMinutes).ToLocalBranchTime());

			var loginFailureLog2 = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog2.SFL_IsLockOut = true;
			loginFailureLog2.SFL_LoginName = staff.GS_LoginName;
			loginFailureLog2.SFL_TableCode = GlbStaffSchema.Constants.Prefix;
			loginFailureLog2.SFL_SystemCreateTimeUtc = utcNow.AddMinutes(-15);
			Factory.Save();

			var lockedOutUntil2 = recorder.LockoutDateTimeLocal(staff.GS_LoginName);
			Assert("Should return date plus lockout minutes from the second record because it's the latest", lockedOutUntil2 == utcNow.AddMinutes(-15).AddMinutes(lockoutMinutes).ToLocalBranchTime());

			loginFailureLog.SFL_SystemCreateTimeUtc = utcNow.AddMinutes(-10);
			Factory.Save();

			var lockedOutUntil3 = recorder.LockoutDateTimeLocal(staff.GS_LoginName);
			Assert("Should return date plus lockout minutes from the first record again because it's the latest", lockedOutUntil3 == utcNow.AddMinutes(-10).AddMinutes(lockoutMinutes).ToLocalBranchTime());
		}

		static byte[] GetRandomBytes(int size)
		{
			using (var generator = RandomNumberGenerator.Create())
			{
				var buf = new byte[size];
				generator.GetNonZeroBytes(buf);
				return buf;
			}
		}
	}
}
