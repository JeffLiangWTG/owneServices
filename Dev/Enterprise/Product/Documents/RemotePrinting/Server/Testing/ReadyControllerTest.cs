using System;
using System.Net;
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
	public class ReadyControllerTest : TestCaseWithFactory
	{
		public void TestCheckReadyReturnsOKWhenDbConnectionIsGood()
		{
			var controller = new ReadyController();
			var response = (StatusCodeResult)controller.CheckReady();
			AssertEquals(response.StatusCode, HttpStatusCode.OK);
		}

		public void TestCheckReadyResultIsCachedAndThreadSafe()
		{
			using var disposable = DbHelper.StartConnectionCounter();
			var controller = new ReadyController();
			var response = Parallel.For(0, 10, _ => controller.CheckReady());
			var content = MemoryCache.Default.Get(ReadyController.ReadyCacheKey);
			AssertEquals("There can only be one connection becuase the cache has not expired", 1, DbHelper.NumberOfConnectionsForTest);
			AssertNotNull("The cache has not expired", content);

			// Manually set the cache time to expire
			MemoryCache.Default.Set(ReadyController.ReadyCacheKey, content, DateTimeOffset.UtcNow);
			Thread.Sleep(TimeSpan.FromMilliseconds(10));
			AssertNull("The cache has expired", MemoryCache.Default.Get(ReadyController.ReadyCacheKey));

			controller.CheckReady();
			AssertEquals("Should open a new connection to check ready because the cache has expired", 2, DbHelper.NumberOfConnectionsForTest);
		}

		public void TestShouldNotReportSqlException()
		{
			var sqlException = SqlExceptionBuilder.CreateSqlException(1, "Simulating an issue with DB");
			using var disposable = DbHelper.SetExceptionToThrowInTest(RemotePrintingDbConnectionException.New(sqlException));
			var controller = new ReadyController();
			var response = (StatusCodeResult)controller.CheckReady();

			AssertEquals(response.StatusCode, HttpStatusCode.ServiceUnavailable);
			Assert(ErrorReporter.LastMessageReported.IsNullOrEmpty());
		}

		public void TestShouldNotReportDatabaseUpgradeInProgressException()
		{
			using var disposable = DbHelper.SetExceptionToThrowInTest(RemotePrintingDbConnectionException.New(new DatabaseUpgradeInProgressException()));
			var controller = new ReadyController();
			var response = (StatusCodeResult)controller.CheckReady();

			AssertEquals(response.StatusCode, HttpStatusCode.ServiceUnavailable);
			Assert(ErrorReporter.LastMessageReported.IsNullOrEmpty());
		}

		public void TestShouldReportNonDBRelatedException()
		{
			var errorMessage = "Simulating an non related DB issue";
			using var disposable = DbHelper.SetExceptionToThrowInTest(new ApplicationException(errorMessage));
			var controller = new ReadyController();
			var response = (StatusCodeResult)controller.CheckReady();

			AssertEquals(response.StatusCode, HttpStatusCode.ServiceUnavailable);
			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
