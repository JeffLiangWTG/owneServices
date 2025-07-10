using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	class CreateDocManagerDatabaseExceptionTest : TestCaseWithFactory
	{
		public void TestHandleCreateDocManagerDatabaseException()
		{
			ExceptionReporter.Instance.ReportException("TestHandleBatchProcessorExceptionReport", new CreateDocManagerDatabaseException("Reason"));
			Assertion.AssertEquals("Error creating database.\r\n\r\nReason : Reason\r\n\r\nTo fix this, please ask your system administrator to go into the system registry and update the paths for 'System -> DocManager -> DocManager Databases Data File Path' and 'System -> DocManager -> DocManager Databases Log File Path'. These need to be paths that the SQL Server can access from the machine that is running the SQL Server service itself. The path is relative to the machine that is running the SQL Server service.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestHandleCreateDocManagerDatabaseExceptionExemptFromSaveAlerting()
		{
			GlbGroup postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff staff = postMastersGroup.Staff.AddNew();
			staff.GS_EmailAddress = "staff@group.com";
			staff.GS_Code = "ZAC";
			Factory.Save();

			using (new FactorySaveAlerter(() => "test", "is bad"))
			{
				ExceptionReporter.Instance.ReportException("TestHandleBatchProcessorExceptionReport", new CreateDocManagerDatabaseException("Reason"));
			}

			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}
	}
}
