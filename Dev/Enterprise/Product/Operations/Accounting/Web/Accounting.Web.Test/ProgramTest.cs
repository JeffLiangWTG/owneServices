#if NET
using System.Collections.Immutable;
using System.IO;
using System.Net;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Web.Testing
{
	class ProgramTest : TestCase
	{
		readonly ImmutableArray<string> serviceUrls =
		[
			"/Accounting/AccountingTransactionExportService.asmx",
			"/Accounting/CreditLimitService.asmx",
			"/Accounting/UpdateInvoicePaymentDetailsService.asmx"
		];

		public void TestProgramStart()
		{
			using (Db.DisposableActionForDbConnection())
			{
				using var factory = new WebApplicationFactory<Program>();

				var client = factory.WithWebHostBuilder(builder =>
				{
					builder.UseUrls("http://localhost");
					// Critical to make integration tests work on DAT
					builder.UseContentRoot(Directory.GetCurrentDirectory());
				}).CreateClient();

				AssertEquals("IsUserInteractive is false", expected: false, actual: Globals.IsUserInteractive);
				AssertEquals($"Env.GetCurrentProvider() is {nameof(ServiceManagerEnvProvider)}", expected: typeof(ServiceManagerEnvProvider), actual: Env.GetCurrentProvider().GetType());
				AssertEquals($"DbEnv.Instance is {nameof(ServiceManagerDbEnvironmentPooled)}", expected: typeof(ServiceManagerDbEnvironmentPooled), actual: DbEnv.Instance.GetType());

				CombineAssertions(delegate
				{
					foreach (var url in serviceUrls)
					{
						var response = client.GetAsync(url).Result;
						AssertEquals(expected: HttpStatusCode.OK, actual: response.StatusCode);
					}
				});
			}
		}
	}
}
#endif
