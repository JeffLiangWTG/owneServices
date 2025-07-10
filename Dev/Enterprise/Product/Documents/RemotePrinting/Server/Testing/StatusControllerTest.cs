using System;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.RemotePrinting.Server.Controllers;
using Enterprise.RemotePrinting.Server.RPSCore;

namespace Enterprise.RemotePrinting.Server.Testing
{
	public class StatusControllerTest : TestCaseWithFactory
	{
		public void TestStatusResponseReturnsOKWhenDbConnectionIsGood()
		{
			var controller = new StatusController();
			var response = controller.CheckStatus() as ResponseMessageResult;
			var content = response.Response.Content.ReadAsStringAsync().Result;
			AssertEquals(@"INFO(Database): OK
INFO(ConnectedClients): 0", content);
		}

		public void TestCheckStatusResultIsCachedAndThreadSafe()
		{
			using var disposable = DbHelper.StartConnectionCounter();
			var controller = new StatusController();
			var response = Parallel.For(0, 10, _ => controller.CheckStatus());
			var content = MemoryCache.Default.Get(StatusController.StatusCacheKey);
			AssertEquals("There can only be one connection becuase the cache has not expired", 1, DbHelper.NumberOfConnectionsForTest);
			AssertNotNull("The cache has not expired", content);

			// Manually set the cache time to expire
			MemoryCache.Default.Set(StatusController.StatusCacheKey, content, DateTimeOffset.UtcNow);
			Thread.Sleep(TimeSpan.FromMilliseconds(10));
			AssertNull("The cache has expired", MemoryCache.Default.Get(StatusController.StatusCacheKey));

			controller.CheckStatus();
			AssertEquals("Should open a new connection to check status because the cache has expired", 2, DbHelper.NumberOfConnectionsForTest);
		}

		public void TestShouldNotReportSqlException()
		{
			var errorMessage = "Simulating an issue with DB";
			var sqlException = SqlExceptionBuilder.CreateSqlException(1, errorMessage);
			using var disposable = DbHelper.SetExceptionToThrowInTest(RemotePrintingDbConnectionException.New(sqlException));
			var controller = new StatusController();
			var response = controller.CheckStatus() as ResponseMessageResult;
			var content = response.Response.Content.ReadAsStringAsync().Result;

			Assert(content.Contains("INFO(Database): " + errorMessage));
			Assert(ErrorReporter.LastMessageReported.IsNullOrEmpty());
		}

		public void TestShouldNotReportDatabaseUpgradeInProgressException()
		{
			using var disposable = DbHelper.SetExceptionToThrowInTest(RemotePrintingDbConnectionException.New(new DatabaseUpgradeInProgressException()));
			var controller = new StatusController();
			var response = controller.CheckStatus() as ResponseMessageResult;
			var content = response.Response.Content.ReadAsStringAsync().Result;

			Assert(content.Contains("INFO(Database): The database is in the process of being upgraded, please try again later."));
			Assert(ErrorReporter.LastMessageReported.IsNullOrEmpty());
		}

		public void TestShouldReportNonDBRelatedException()
		{
			var errorMessage = "Simulating an non related DB issue";
			using var disposable = DbHelper.SetExceptionToThrowInTest(new ApplicationException(errorMessage));
			var controller = new StatusController();
			var response = controller.CheckStatus() as ResponseMessageResult;
			var content = response.Response.Content.ReadAsStringAsync().Result;

			Assert(content.Contains("INFO(Database): OK"));
			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
