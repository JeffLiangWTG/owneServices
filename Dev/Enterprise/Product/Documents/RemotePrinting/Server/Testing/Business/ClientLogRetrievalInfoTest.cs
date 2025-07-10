using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.RemotePrinting.Server.Business;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing.Business
{
	[TestedType(typeof(ClientLogRetrievalInfo))]
	public class ClientLogRetrievalInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ClientLogRetrievalInfo();
		}

		public void TestValidateFromDate()
		{
			var logRequestInfo = new ClientLogRetrievalInfo();

			logRequestInfo.FromDate = ZDateTime.Empty;
			AssertHasError(logRequestInfo.FromDateInfo, "Enter From Date.");

			logRequestInfo.FromDate = ZDateTime.Today;
			AssertNoErrors(logRequestInfo.FromDateInfo);

			logRequestInfo.ToDate = ZDateTime.Today.AddDays(-1);
			AssertNoErrors("Validation is not called yet", logRequestInfo.FromDateInfo);

			logRequestInfo.FromDate = ZDateTime.Today;
			AssertHasError(logRequestInfo.FromDateInfo, "From Date should not be after To Date.");

			logRequestInfo.FromDate = ZDateTime.Today.AddDays(-2);
			AssertNoErrors(logRequestInfo.FromDateInfo);
		}

		public void TestValidateToDate()
		{
			var logRequestInfo = new ClientLogRetrievalInfo();

			logRequestInfo.ToDate = ZDateTime.Empty;
			AssertHasError(logRequestInfo.ToDateInfo, "Enter To Date.");

			logRequestInfo.ToDate = ZDateTime.Today;
			AssertNoErrors(logRequestInfo.ToDateInfo);

			logRequestInfo.FromDate = ZDateTime.Today.AddDays(1);
			AssertNoErrors("Validation is not called yet", logRequestInfo.ToDateInfo);

			logRequestInfo.ToDate = ZDateTime.Today;
			AssertHasError(logRequestInfo.ToDateInfo, "To Date should not be before From Date.");

			logRequestInfo.ToDate = ZDateTime.Today.AddDays(2);
			AssertNoErrors(logRequestInfo.ToDateInfo);
		}

		public void TestValidateLogs()
		{
			var logRequestInfo = new ClientLogRetrievalInfo();

			logRequestInfo.Logs = false;
			logRequestInfo.ServiceLogs = false;
			logRequestInfo.InstallLogs = false;
			AssertHasError(logRequestInfo.LogsInfo, "Select log types to request.");
			AssertNoErrors(logRequestInfo.ServiceLogsInfo);
			AssertNoErrors(logRequestInfo.InstallLogsInfo);

			logRequestInfo.Logs = true;
			AssertNoErrors(logRequestInfo.LogsInfo);
			AssertNoErrors(logRequestInfo.ServiceLogsInfo);
			AssertNoErrors(logRequestInfo.InstallLogsInfo);

			logRequestInfo.Logs = false;
			logRequestInfo.ServiceLogs = true;
			AssertNoErrors(logRequestInfo.LogsInfo);
			AssertNoErrors(logRequestInfo.ServiceLogsInfo);
			AssertNoErrors(logRequestInfo.InstallLogsInfo);

			logRequestInfo.ServiceLogs = false;
			logRequestInfo.InstallLogs = true;
			AssertNoErrors(logRequestInfo.LogsInfo);
			AssertNoErrors(logRequestInfo.ServiceLogsInfo);
			AssertNoErrors(logRequestInfo.InstallLogsInfo);

			logRequestInfo.InstallLogs = false;
			AssertHasError(logRequestInfo.LogsInfo, "Select log types to request.");
			AssertNoErrors(logRequestInfo.ServiceLogsInfo);
			AssertNoErrors(logRequestInfo.InstallLogsInfo);
		}

		public void TestValidateEmailAddress()
		{
			var logRequestInfo = new ClientLogRetrievalInfo();

			logRequestInfo.EmailAddress = "";
			AssertHasError(logRequestInfo.EmailAddressInfo, "Enter Email Address.");

			logRequestInfo.EmailAddress = "abc";
			AssertNoErrors(logRequestInfo.EmailAddressInfo);
		}

		public void TestRunPreSaveValidation()
		{
			var logRequestInfo = new ClientLogRetrievalInfo();
			logRequestInfo.RunPreSaveValidation();

			AssertHasError(logRequestInfo.FromDateInfo, "Enter From Date.");
			AssertHasError(logRequestInfo.ToDateInfo, "Enter To Date.");
			AssertHasError(logRequestInfo.LogsInfo, "Select log types to request.");
			AssertNoErrors(logRequestInfo.ServiceLogsInfo);
			AssertNoErrors(logRequestInfo.InstallLogsInfo);
			AssertHasError(logRequestInfo.EmailAddressInfo, "Enter Email Address.");

			logRequestInfo.FromDate = ZDateTime.Today;
			logRequestInfo.ToDate = ZDateTime.Today;
			logRequestInfo.Logs = true;
			logRequestInfo.EmailAddress = "abc";
			logRequestInfo.RunPreSaveValidation();

			AssertNoErrors(logRequestInfo.FromDateInfo);
			AssertNoErrors(logRequestInfo.ToDateInfo);
			AssertNoErrors(logRequestInfo.LogsInfo);
			AssertNoErrors(logRequestInfo.ServiceLogsInfo);
			AssertNoErrors(logRequestInfo.InstallLogsInfo);
			AssertNoErrors(logRequestInfo.EmailAddressInfo);
		}
	}
}
