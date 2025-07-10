using System;
using System.ServiceModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.CreditLimitService;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.Integration.Testing
{
	public class CreditLimitServiceSoapClientProviderTest : TestCaseWithFactory
	{
		public void TestWebServiceTimeoutValuesAreTakenFromRegistry()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			OrgHeader org = creator.AALSHI;
			org.CompanyData.OB_IsCreditor = org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.OB_ARCreditLimit = 100m;
			org.CompanyData.OB_APCreditLimit = 150m;

			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://SYD-WDPA-1/AccountingWebService/CreditLimitService.asmx");

			var details = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
			var client = details.CreateNewServiceClient_ForTest() as ClientBase<CreditLimitServiceSoap>;
			AssertNotNull(client);
			AssertEquals("OperationTimeout: InnerChannel OperationTimeout", 5, client.InnerChannel.OperationTimeout.Seconds);
			AssertEquals("SendTimeout: default value is 5 seconds", 5, client.Endpoint.Binding.SendTimeout.Seconds);
			AssertEquals("OpenTimeout: default value is 5 seconds", 5, client.Endpoint.Binding.OpenTimeout.Seconds);
			AssertEquals("CloseTimeout: default value is 5 seconds", 5, client.Endpoint.Binding.CloseTimeout.Seconds);
			AssertEquals("ReceiveTimeout: default value is 5 seconds", 5, client.Endpoint.Binding.ReceiveTimeout.Seconds);
			AssertEquals("Endpoint address", "http://syd-wdpa-1/AccountingWebService/CreditLimitService.asmx", client.Endpoint.Address.ToString());

			AccountingConfigurationRegistry.Instance.UseWebServiceTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			details = new OrgCreditLimitAndBalanceDetails(org.OH_Code);
			client = details.CreateNewServiceClient_ForTest() as ClientBase<CreditLimitServiceSoap>;
			AssertNotNull(client);
			AssertEquals("OperationTimeout: InnerChannel OperationTimeout", 10, client.InnerChannel.OperationTimeout.Seconds);
			AssertEquals("SendTimeout: overriden value is 10 seconds", 10, client.Endpoint.Binding.SendTimeout.Seconds);
			AssertEquals("OpenTimeout: overriden value is 10 seconds", 10, client.Endpoint.Binding.OpenTimeout.Seconds);
			AssertEquals("CloseTimeout: overriden value is 10 seconds", 10, client.Endpoint.Binding.CloseTimeout.Seconds);
			AssertEquals("ReceiveTimeout: overriden value is 10 seconds", 10, client.Endpoint.Binding.ReceiveTimeout.Seconds);
			AssertEquals("Endpoint address", "http://syd-wdpa-1/AccountingWebService/CreditLimitService.asmx", client.Endpoint.Address.ToString());
		}

		public void TestWebServiceSecurityModeIsTransportForHttpsURLs()
		{
			var creator = new TestObjectCreator(Factory);
			var org = creator.AALSHI;
			var details = new OrgCreditLimitAndBalanceDetails(org.OH_Code);

			AssertSecurityModeAndUrl(details, "http://web-service-url/", BasicHttpSecurityMode.None);
			AssertSecurityModeAndUrl(details, "HTTP://web-service-url/", BasicHttpSecurityMode.None);
			AssertSecurityModeAndUrl(details, "https://web-service-url/", BasicHttpSecurityMode.Transport);
			AssertSecurityModeAndUrl(details, "HTTPS://web-service-url/", BasicHttpSecurityMode.Transport);
		}

		void AssertSecurityModeAndUrl(OrgCreditLimitAndBalanceDetails details, string url, BasicHttpSecurityMode securityMode)
		{
			AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, url);
			var client = details.CreateNewServiceClient_ForTest() as ClientBase<CreditLimitServiceSoap>;
			var binding = client.Endpoint.Binding as BasicHttpBinding;
			AssertEquals($"Binding Security Mode for {url}", securityMode, binding.Security.Mode);
			AssertEquals($"Endpoint address for {url}", url.ToLower(), client.Endpoint.Address.ToString());
		}
	}
}
