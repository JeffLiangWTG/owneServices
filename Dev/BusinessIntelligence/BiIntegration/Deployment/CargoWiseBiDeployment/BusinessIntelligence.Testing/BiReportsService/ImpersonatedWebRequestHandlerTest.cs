using System;
using System.Net;
using System.Net.Http;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Bi.Deployment.ReportingServices;
using CargoWise.Bi.Registration.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace CargoWise.Bi.BusinessIntelligence.Testing.BiReportsService
{
	public class ImpersonatedWebRequestHandlerTest : TransactionedTestCase
	{
		public void TestUsesDefaultCredentials()
		{
			var webHandler = new ImpersonatedWebRequestHandler(new BiReportUser());
			AssertEquals(true, webHandler.UseDefaultCredentials);
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestSendAsyncImpersonatesAUser()
		{
			string vmName = DATHelper.GetVMWithPowerBi();
			var domain = "SAND";
			var userName = TestConstants.ADTestUserAccount.Name;
			var password = TestConstants.ADTestUserAccount.Password;
			var credentialRegItem = new BiReportCredentialRegistryItem(string.Empty, null, null, null,
				RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				new BiReportCredential() { Domain = domain, UserName = userName, Password = password });

			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, credentialRegItem.Value))
			using (SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			{
				using (var webHandler = new ImpersonatedWebRequestHandler(new BiReportUser()))
				{
					var powerBiBrowser = @"http://" + vmName + @"/pbirs/api/v2.0/Folders(path='/')/CatalogItems";
					var request = new HttpRequestMessage(HttpMethod.Get, new Uri(powerBiBrowser));
					using (var response = webHandler.SendAsync(request))
					{
						var result = response.Result;
						AssertEquals(@"Impersonated response should have a status of Forbidden for non admin user", HttpStatusCode.Forbidden, result.StatusCode);
					}
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestSendAsyncImpersonatesNothing()
		{
			string vmName = DATHelper.GetVMWithPowerBi();
			var cred = SystemDataRegistry.Instance.BiReportUserCredential.DefaultValue;
			var credentialRegItem = new BiReportCredentialRegistryItem(string.Empty, null, null, null,
				RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				cred);

			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName))
			using (SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, credentialRegItem.Value))
			{
				using (var webHandler = new ImpersonatedWebRequestHandler(new BiReportUser()))
				{
					var powerBiBrowser = @"http://" + vmName + @"/pbirs/api/v2.0/Folders(path='/')/CatalogItems";
					var request = new HttpRequestMessage(HttpMethod.Get, new Uri(powerBiBrowser));
					using (var response = webHandler.SendAsync(request))
					{
						var result = response.Result;
						AssertEquals(@"Impersonated response should have a status of OK on DAT as default executing user is a member of builtin administrators", HttpStatusCode.OK, result.StatusCode);
					}
				}
			}
		}
	}
}
