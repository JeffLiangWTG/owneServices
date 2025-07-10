using System.Security.Cryptography;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.Testing
{
	sealed class LoginAttemptRecorderTest : TestCaseWithFactory
	{
		public void TestRecordLoginAttempt()
		{
			var contact = CreateContact();

			var isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, null, 3, 15, User.ServiceUserCode);
			Assert(!isLocked);

			var loginFailureLog = new BusinessObjectFactory().LoadTop1<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			AssertNotNull(loginFailureLog);
		}

		public void TestRecordLoginAttempt_LockoutUser()
		{
			var contact = CreateContact();

			var loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			AssertEquals("Should create a login failure log", 0, loginFailureLog.Length);

			var isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, null, 3, 15, User.ServiceUserCode);
			loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			Assert("User should not be locked out", !isLocked);
			AssertEquals("Should create a login failure log", 1, loginFailureLog.Length);

			isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, null, 3, 15, User.ServiceUserCode);
			loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			Assert("User should not be locked out", !isLocked);
			AssertEquals("Should create another login failure log", 2, loginFailureLog.Length);

			isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, null, 3, 15, User.ServiceUserCode);
			loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			Assert("User should be locked out", isLocked);
			AssertEquals("Should have 3 records, shouldn't add a new one", 3, loginFailureLog.Length);

			isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, null, 3, 15, User.ServiceUserCode);
			loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			Assert("User should be locked out", isLocked);
			AssertEquals("Should have 3 records, shouldn't add a new one", 3, loginFailureLog.Length);
		}

		public void TestRecordLoginAttempt_LockoutUser_WithHash()
		{
			var contact = CreateContact();

			var hash = GetRandomBytes(32);

			var loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			AssertEquals("Should create a login failure log", 0, loginFailureLog.Length);

			var isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, hash, 3, 15, User.ServiceUserCode);
			loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			Assert("User should not be locked out", !isLocked);
			AssertEquals("Should create a login failure log", 1, loginFailureLog.Length);
			AssertEquals("Should store the hash", hash, loginFailureLog[0].SFL_LoginHash);

			isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, hash, 3, 15, User.ServiceUserCode);
			loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			Assert("User should not be locked out", !isLocked);
			AssertEquals("Should create another login failure log", 2, loginFailureLog.Length);

			isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, hash, 3, 15, User.ServiceUserCode);
			loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			Assert("User should be locked out", isLocked);
			AssertEquals("Should have 3 records, shouldn't add a new one", 3, loginFailureLog.Length);

			isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, hash, 3, 15, User.ServiceUserCode);
			loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			Assert("User should be locked out", isLocked);
			AssertEquals("Should have 3 records, shouldn't add a new one", 3, loginFailureLog.Length);
		}

		public void TestRecordLoginAttempt_LockoutUser_LockUserWithoutHash_LoginWithHash()
		{
			var contact = CreateContact();

			var loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			AssertEquals("Should create a login failure log", 0, loginFailureLog.Length);

			// Try to login without hash
			var isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, null, 3, 15, User.ServiceUserCode);
			loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			Assert("User should not be locked out", !isLocked);
			AssertEquals("Should create a login failure log", 1, loginFailureLog.Length);

			isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, null, 3, 15, User.ServiceUserCode);
			loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			Assert("User should not be locked out", !isLocked);
			AssertEquals("Should create another login failure log", 2, loginFailureLog.Length);

			isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, null, 3, 15, User.ServiceUserCode);
			loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			Assert("User should be locked out", isLocked);
			AssertEquals("Should have 3 records, shouldn't add a new one", 3, loginFailureLog.Length);

			// Try to login with Hash
			var hash = GetRandomBytes(32);
			isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, hash, 3, 15, User.ServiceUserCode);
			Assert("User should not be locked out", !isLocked);
		}

		public void TestIsLockedOut()
		{
			var contact = CreateContact();

			LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, null, 2, 30, User.ServiceUserCode);
			var isLocked = LoginAttemptRecorder.IsLockedOut(contact.OC_Email, contact.TablePrefix, null, 30);
			Assert("User should not be locked out", !isLocked);

			LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, null, 2, 30, User.ServiceUserCode);
			isLocked = LoginAttemptRecorder.IsLockedOut(contact.OC_Email, contact.TablePrefix, null, 30);
			Assert("User should be locked out", isLocked);

			isLocked = LoginAttemptRecorder.IsLockedOut(contact.OC_Email, "GS", null, 30);
			Assert("User should not be locked out, wrong table code", !isLocked);
		}

		public void TestIsLockedOut_WithHash()
		{
			var contact = CreateContact();

			var hash = GetRandomBytes(32);

			LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, hash, 2, 30, User.ServiceUserCode);
			var isLocked = LoginAttemptRecorder.IsLockedOut(contact.OC_Email, contact.TablePrefix, hash, 30);
			Assert("User should not be locked out", !isLocked);

			LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, hash, 2, 30, User.ServiceUserCode);
			isLocked = LoginAttemptRecorder.IsLockedOut(contact.OC_Email, contact.TablePrefix, hash, 30);
			Assert("User should be locked out", isLocked);

			var isLockedNoHash = LoginAttemptRecorder.IsLockedOut(contact.OC_Email, contact.TablePrefix, null, 30);
			Assert("User should not be locked out without hash", !isLockedNoHash);
		}

		public void TestUnlock()
		{
			var lockoutMinutes = 15;
			var contact = CreateContact();

			var isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, null, 2, lockoutMinutes, User.ServiceUserCode);
			Assert(!isLocked);
			isLocked = LoginAttemptRecorder.RecordFailedLoginAttempts(contact.OC_Email, contact.TablePrefix, null, 2, lockoutMinutes, User.ServiceUserCode);
			Assert(isLocked);

			var loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			AssertEquals(2, loginFailureLog.Length);

			LoginAttemptRecorder.Unlock(contact.OC_Email, contact.TablePrefix);

			loginFailureLog = new BusinessObjectFactory().Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, contact.OC_Email));
			AssertEquals(0, loginFailureLog.Length);
			isLocked = LoginAttemptRecorder.IsLockedOut(contact.OC_Email, contact.TablePrefix, null, lockoutMinutes);
			Assert(!isLocked);
		}

		public void TestLockoutDateTimeLocal()
		{
			var contactEmail = "contact@wisetech.com";
			var loginFailureLog = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog.SFL_IsLockOut = true;
			loginFailureLog.SFL_LoginName = contactEmail;
			loginFailureLog.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			loginFailureLog.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow;

			var loginFailureLogNotLockedOut = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLogNotLockedOut.SFL_IsLockOut = false;
			loginFailureLogNotLockedOut.SFL_LoginName = "NotLockedOut@wisetech.com";
			loginFailureLogNotLockedOut.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			loginFailureLogNotLockedOut.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			var lockoutDateTime = LoginAttemptRecorder.LockoutDateTimeLocal(contactEmail, OrgContactSchema.Constants.Prefix, 10);
			Assert("Should return datetime plus 10 min", lockoutDateTime > ZDateTime.UtcNow.AddMinutes(8).ToLocalBranchTime() && lockoutDateTime < ZDateTime.UtcNow.AddMinutes(12).ToLocalBranchTime());

			var lockoutDateTime2 = LoginAttemptRecorder.LockoutDateTimeLocal(contactEmail, OrgContactSchema.Constants.Prefix, 20);
			Assert("Should return datetime plus 20 min", lockoutDateTime2 > ZDateTime.UtcNow.AddMinutes(18).ToLocalBranchTime() && lockoutDateTime2 < ZDateTime.UtcNow.AddMinutes(22).ToLocalBranchTime());

			var lockoutDateTime3 = LoginAttemptRecorder.LockoutDateTimeLocal(contactEmail, OrgContactSchema.Constants.Prefix, 0);
			Assert("Should be locked out indefinitely", lockoutDateTime3 > ZDateTime.MaxSmallDateTime.AddMinutes(-2));

			var lockoutDateTime4 = LoginAttemptRecorder.LockoutDateTimeLocal("NotLockedOut@wisetech.com", OrgContactSchema.Constants.Prefix, 0);
			Assert("Should not find any locked out log", lockoutDateTime4.IsEmpty);
		}

		OrgContact CreateContact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test@email.com";
			Factory.Save();
			return contact;
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
