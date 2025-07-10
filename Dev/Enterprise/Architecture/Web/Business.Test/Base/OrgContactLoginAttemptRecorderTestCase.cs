using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class OrgContactLoginAttemptRecorderTestCase : TestCaseWithFactory
	{
		public void TestRecordLoginAttempt()
		{
			var contactEmail = "contact@email.com";
			CreateTestContact(contactEmail);
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var recorder = new OrgContactLoginAttemptRecorder();

			using (WebDataRegistry.Instance.WebLoginAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				var utcStart = ZDateTime.UtcNow.AddSeconds(-1); // Minus one second because the test runs too fast

				recorder.RecordLoginAttempt(org.OH_Code, contactEmail);
				Assert("Not locked out on first attempt WITH company code", !recorder.IsLockedOut(org.OH_Code, contactEmail, null));

				recorder.RecordLoginAttempt(string.Empty, contactEmail);
				Assert("Not locked out on attempt WITHOUT company code", !recorder.IsLockedOut(org.OH_Code, contactEmail, null));

				recorder.RecordLoginAttempt(org.OH_Code, contactEmail);
				Assert("Locked out on second attempt WITH company code", recorder.IsLockedOut(org.OH_Code, contactEmail, null));

				var utcEnd = ZDateTime.UtcNow.AddSeconds(1); // Add one second because the test runs too fast

				var loginDisabledUntilUtc = recorder.LockoutDateTimeLocal(org.OH_Code, contactEmail).ToUniversalBranchTime();
				Assert("Lockout time is set", utcStart.AddMinutes(5) <= loginDisabledUntilUtc && loginDisabledUntilUtc <= utcEnd.AddMinutes(5));
			}
		}

		public void TestRecordLoginAttempt_MultipleContacts()
		{
			var contactEmail = "contact@email.com";
			var contact = CreateTestContact(contactEmail);
			var contact2 = CreateTestContact(contactEmail);

			var filter = new ZQuery(OrgContactSchema.OC_Email, contactEmail);
			var contacts = new OrgContactCollection(Factory, filter);
			contacts.Load();

			var recorder = new OrgContactLoginAttemptRecorder();

			using (WebDataRegistry.Instance.WebLoginAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				var utcStart = ZDateTime.UtcNow.AddSeconds(-1); // Minus one second because the test runs too fast

				recorder.RecordLoginAttempt(string.Empty, contactEmail);
				Assert("Not locked out on first attempt", contacts.OfType<OrgContact>().All(x => !recorder.IsLockedOut(string.Empty, x.OC_Email, null)));

				Thread.Sleep(20); // utcStart and utcEnd may be the same value if too fast.

				recorder.RecordLoginAttempt(string.Empty, contactEmail);
				Assert("Locked out on second attempt", contacts.OfType<OrgContact>().All(x => recorder.IsLockedOut(string.Empty, x.OC_Email, null)));

				var utcEnd = ZDateTime.UtcNow.AddSeconds(1); // Add one second because the test runs too fast

				var loginDisabledUntilUtc = contact.LockoutDateTimeLocal.ToUniversalBranchTime();
				var loginDisabledUntilUtc2 = contact2.LockoutDateTimeLocal.ToUniversalBranchTime();
				Assert("Lockout time is set on first contact", utcStart.AddMinutes(5) <= loginDisabledUntilUtc && loginDisabledUntilUtc <= utcEnd.AddMinutes(5));
				Assert("Lockout time is set on second contact", utcStart.AddMinutes(5) <= loginDisabledUntilUtc2 && loginDisabledUntilUtc2 <= utcEnd.AddMinutes(5));
			}
		}

		public void TestRecordLoginAttempt_AfterLockoutAndUnlock()
		{
			var contactEmail = "contact@email.com";
			var contact = CreateTestContact(contactEmail);

			var recorder = new OrgContactLoginAttemptRecorder();

			using (WebDataRegistry.Instance.WebLoginAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				recorder.RecordLoginAttempt(string.Empty, contactEmail);
				Assert("Not locked out on first attempt", !recorder.IsLockedOut("", contact.OC_Email, null));

				recorder.RecordLoginAttempt(string.Empty, contactEmail);
				Assert("Locked out on second attempt", recorder.IsLockedOut("", contact.OC_Email, null));

				recorder.RecordLoginAttempt(string.Empty, contactEmail);
				Assert("Still locked out", recorder.IsLockedOut("", contact.OC_Email, null));

				recorder.Unlock(string.Empty, contactEmail, false);

				recorder.RecordLoginAttempt(string.Empty, contactEmail);
				Assert("Not locked out on first attempt after unlock", !recorder.IsLockedOut("", contact.OC_Email, null));
			}
		}

		public void TestClearLoginAttempts()
		{
			var contactEmail = "contact@email.com";
			var contact = CreateTestContact(contactEmail);

			var recorder = new OrgContactLoginAttemptRecorder();

			using (WebDataRegistry.Instance.WebLoginAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				recorder.RecordLoginAttempt(string.Empty, contactEmail);
				Assert("Not locked out on first attempt", !recorder.IsLockedOut("", contact.OC_Email, null));

				ClearLoginAttempts(contactEmail);
				recorder.RecordLoginAttempt(string.Empty, contactEmail);
				Assert("Not locked out on first attempt after clear", !recorder.IsLockedOut("", contact.OC_Email, null));
			}
		}

		public void TestClearLoginAttempts_MultipleContacts()
		{
			var contact = CreateTestContact("contact@email.com");
			var contact2 = CreateTestContact("contact2@email.com");

			var recorder = new OrgContactLoginAttemptRecorder();

			using (WebDataRegistry.Instance.WebLoginAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				var utcStart = ZDateTime.UtcNow.AddSeconds(-1); // Minus one second because the test runs too fast

				recorder.RecordLoginAttempt(string.Empty, "contact@email.com");
				recorder.RecordLoginAttempt(string.Empty, "contact2@email.com");
				Assert("Not locked out on first attempt of all contacts", !recorder.IsLockedOut("", contact.OC_Email, null) && !recorder.IsLockedOut("", contact2.OC_Email, null));

				ClearLoginAttempts("contact@email.com");
				recorder.RecordLoginAttempt(string.Empty, "contact@email.com");
				recorder.RecordLoginAttempt(string.Empty, "contact2@email.com");

				AssertEquals("Lockout time is not set on contact that had attempt cleared", ZDateTime.Empty, contact.LockoutDateTimeLocal);

				var utcEnd = ZDateTime.UtcNow.AddSeconds(1); // Add one second because the test runs too fast

				var loginDisabledUntilUtc2 = contact2.LockoutDateTimeLocal.ToUniversalBranchTime();
				Assert("Lockout time is set on contact with max attempts", utcStart.AddMinutes(5) <= loginDisabledUntilUtc2 && loginDisabledUntilUtc2 <= utcEnd.AddMinutes(5));
			}
		}

		public void TestUnlock()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contactEmail = "contact@email.com";
			CreateTestContact(contactEmail);

			var recorder = new OrgContactLoginAttemptRecorder();

			using (WebDataRegistry.Instance.WebLoginAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				// Without company code
				recorder.RecordLoginAttempt(string.Empty, contactEmail);
				recorder.RecordLoginAttempt(string.Empty, contactEmail);
				Assert("Should be locked out", recorder.IsLockedOut(string.Empty, contactEmail, null));
				Assert("Should not be locked out", !recorder.IsLockedOut(org.OH_Code, contactEmail, null));

				recorder.Unlock(string.Empty, contactEmail, false);

				Assert("Should not be locked out", !recorder.IsLockedOut(string.Empty, contactEmail, null));
				Assert("Should not be locked out", !recorder.IsLockedOut(org.OH_Code, contactEmail, null));

				// With company code
				recorder.RecordLoginAttempt(org.OH_Code, contactEmail);
				recorder.RecordLoginAttempt(org.OH_Code, contactEmail);
				Assert("Should be locked out", recorder.IsLockedOut(org.OH_Code, contactEmail, null));
				Assert("Should not be locked out", !recorder.IsLockedOut(string.Empty, contactEmail, null));

				recorder.Unlock(string.Empty, contactEmail, false);

				Assert("Should not be locked out", !recorder.IsLockedOut(string.Empty, contactEmail, null));
				Assert("Should be locked out", recorder.IsLockedOut(org.OH_Code, contactEmail, null));

				recorder.Unlock(org.OH_Code, contactEmail, false);
				Assert("Should not be locked out", !recorder.IsLockedOut(org.OH_Code, contactEmail, null));
			}
		}

		public void TestIsAnonymousUserLockedOut()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contactEmail = "contact@email.com";
			CreateTestContact(contactEmail);

			var recorder = new OrgContactLoginAttemptRecorder();
			var hash = GetRandomBytes(32);

			using (WebDataRegistry.Instance.WebLoginAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 15))
			{
				recorder.RecordLoginAttempt(org.OH_Code, contactEmail, hash);
				recorder.RecordLoginAttempt(org.OH_Code, contactEmail, hash);

				Assert("Should be locked out", recorder.IsAnonymousUserLockedOut(org.OH_Code, contactEmail));
				Assert("Should not be locked out when company code is empty and just a record with company code is locked out", !recorder.IsAnonymousUserLockedOut(string.Empty, contactEmail));

				recorder.Unlock(org.OH_Code, contactEmail, true);

				Assert("Should not be locked out", !recorder.IsAnonymousUserLockedOut(org.OH_Code, contactEmail));
				Assert("Should not be locked out", !recorder.IsAnonymousUserLockedOut(string.Empty, contactEmail));

				recorder.RecordLoginAttempt(string.Empty, contactEmail, hash);
				recorder.RecordLoginAttempt(string.Empty, contactEmail, hash);

				Assert("Should be locked out because even with company code we verify witout it as well", recorder.IsAnonymousUserLockedOut(org.OH_Code, contactEmail));
				Assert("Should be locked out", recorder.IsAnonymousUserLockedOut(string.Empty, contactEmail));
			}
		}

		public void TestLockoutDateTimeLocal()
		{
			var contactEmail = "contact@email.com";
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var recorder = new OrgContactLoginAttemptRecorder();

			using (WebDataRegistry.Instance.WebLoginAttempts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (WebDataRegistry.Instance.WebLoginLockoutMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				var loginFailureLogWithCompany = Factory.NewWithValidTestData<StmLoginFailureLog>();
				loginFailureLogWithCompany.SFL_IsLockOut = true;
				loginFailureLogWithCompany.SFL_LoginName = contactEmail + " " + org.OH_Code;
				loginFailureLogWithCompany.SFL_TableCode = OrgContactSchema.Constants.Prefix;
				loginFailureLogWithCompany.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-20);
				Factory.Save();

				var lockedOutUntilWithCompany = recorder.LockoutDateTimeLocal(org.OH_Code, contactEmail);
				Assert("Should return date from the record with company code", lockedOutUntilWithCompany > ZDateTime.UtcNow.AddMinutes(7));

				var loginFailureLogNoCompany = Factory.NewWithValidTestData<StmLoginFailureLog>();
				loginFailureLogNoCompany.SFL_IsLockOut = true;
				loginFailureLogNoCompany.SFL_LoginName = contactEmail;
				loginFailureLogNoCompany.SFL_TableCode = OrgContactSchema.Constants.Prefix;
				loginFailureLogNoCompany.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-15);
				Factory.Save();

				var lockedOutUntilNoCompany = recorder.LockoutDateTimeLocal(org.OH_Code, contactEmail);
				Assert("Should return date from the record without company because it's the latest", lockedOutUntilNoCompany > ZDateTime.UtcNow.AddMinutes(12) && lockedOutUntilNoCompany > lockedOutUntilWithCompany);

				loginFailureLogWithCompany.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-10);
				Factory.Save();

				var lockedOutUntilWithCompany2 = recorder.LockoutDateTimeLocal(org.OH_Code, contactEmail);
				Assert("Should return date from the record with company again because it's the latest", lockedOutUntilWithCompany2 > ZDateTime.UtcNow.AddMinutes(17) && lockedOutUntilWithCompany2 > lockedOutUntilNoCompany);
			}
		}

		void ClearLoginAttempts(string username)
		{
			var logs = Factory.Load<StmLoginFailureLog>(new ZQuery(StmLoginFailureLogSchema.SFL_LoginName, username));
			logs.DeleteAll();
			Factory.Save();
		}

		OrgContact CreateTestContact(string email)
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = email;
			contact.OC_PER = person.PK;

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
