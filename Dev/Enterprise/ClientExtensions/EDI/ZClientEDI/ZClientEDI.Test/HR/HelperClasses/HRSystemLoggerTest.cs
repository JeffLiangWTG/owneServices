using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.HR.Testing
{
	public class HRSystemLoggerTest : TestCaseWithFactory
	{
		[TestDate(2019, 11, 13)]
		public void TestLogger()
		{
			var staff1 = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff1.GS_Code = "SO1";
			staff1.GS_FullName = "Staff One";
			staff1.GS_EmailAddress = "newemail1@wisetechglobal.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff1);
			Factory.Save();
			EDIDataRegistry.Instance.HRNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			var initialLogger = new TestServiceLogger();
			var logger1 = new HRSystemLogger(initialLogger, "SYS1");
			logger1.Log(LogType.Information, "INFO1");
			logger1.SendNotificationEmail();
			AssertEquals(false, Env.OutgoingMailManager.EmailsCreated.Any());
			logger1.Log(LogType.Error, "ERROR2", new Exception("Exception3"));
			logger1.SendNotificationEmail();
			AssertEquals(@"[13-Nov-19 00:00] Information INFO1
[13-Nov-19 00:00] Error ERROR2 System.Exception: Exception3
", logger1.ToString());
			var email = Env.OutgoingMailManager.EmailsCreated.Single();
			AssertEquals("ediProd SYS1 sync failed", email.Subject);
			AssertEquals(@"[13-Nov-19 00:00] Information INFO1
[13-Nov-19 00:00] Error ERROR2 System.Exception: Exception3


You are receiving this email because you are a member of the group in registry WiseTech Global Client Extensions/HR > HR Notification Group", email.Body);
			AssertEquals(@"Information|INFO1
Error|ERROR2|System.Exception: Exception3
", initialLogger.ToString());
		}
	}
}
