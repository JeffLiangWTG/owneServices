using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using static Enterprise.Core.Constants;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.D365.Testing
{
	sealed class D365CredentialLoaderTest : TestCaseWithFactory
	{
		public void TestLoadForGEIRequest_ReturnsEmpty_WhenNoCredentials()
		{
			var objectFactory = GetEInvoicingObjectFactory();
			var branch = TestObjectCreator.CreateBranchWithCompany("GB");
			var credentials = GetCredentialsLoader().LoadForGEIRequest(branch, null, objectFactory);
			AssertEquals(0, credentials.Count());
		}

		public void TestLoadForGEIRequest_ReturnsAllCredentials_WhenValuesExist()
		{
			var branch = TestObjectCreator.CreateBranchWithCompany("GB");
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			var expectedClientID = "cd41ae9a-fcdd-46d0-8ec3-aaa26c93b03b";
			var expectedClientSecret = "1d8b2aca-480d-4509-b7ce-c12e84c86800";
			var expectedTenantID = "dfe40c9f-2a22-45ac-b964-fff142292141";	

			using (SetD365WebServiceURLRegistry(branch.GB_GC.ToGuid(), "D365WebServiceURL"))
			using (AccountingMasterFilesRegistry.Instance.D365Credentials.DataType.SuspendValidation())
			using (SetD365CredentialsRegistry(branch.GB_GC.ToGuid(), expectedClientID, expectedClientSecret, expectedTenantID))
			{
				var credentials = GetCredentialsLoader().LoadForGEIRequest(branch, batch, GetEInvoicingObjectFactory()).ToArray();
				AssertEquals(4, credentials.Length);

				AssertContainsExactElementsInAnyOrder("ServiceEndpoint, TenantID, ClientId, ClientSecret",
					new[] { "ServiceEndpoint", "TenantId", "ClientId", "ClientSecret" },
					credentials.Select(x => x.Key));

				AssertEquals("All credentials should have a value", expected: true, credentials.All(x => !x.Value.Value.IsEmpty));

				AssertD365WebserviceURL(credentials, "D365WebServiceURL");

				AssertD365Credentials(credentials, expectedClientID, expectedTenantID);
			}
		}

		public void TestLoadForGEIRequest_ReturnsPartialCredentials_WhenOnlyServiceEndpointExists()
		{
			var branch = TestObjectCreator.CreateBranchWithCompany("GB");
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			using (SetD365WebServiceURLRegistry(branch.GB_GC.ToGuid(),"D365WebServiceURL"))
			{
				var credentials = GetCredentialsLoader().LoadForGEIRequest(branch, batch, GetEInvoicingObjectFactory());
				AssertD365WebserviceURL(credentials, "D365WebServiceURL");
			}
		}

		public void TestLoadForGEIRequest_ReturnsPartialCredentials_WhenOnlyD365CredentialsExist()
		{
			var branch = TestObjectCreator.CreateBranchWithCompany("GB");
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			var expectedClientID = "cd41ae9a-fcdd-46d0-8ec3-aaa26c93b03b";
			var expectedClientSecret = "1d8b2aca-480d-4509-b7ce-c12e84c86800";
			var expectedTenantID = "dfe40c9f-2a22-45ac-b964-fff142292141";

			using (AccountingMasterFilesRegistry.Instance.D365Credentials.DataType.SuspendValidation())
			using (SetD365CredentialsRegistry(branch.GB_GC.ToGuid(), expectedClientID, expectedClientSecret, expectedTenantID))
			{
				var credentials = GetCredentialsLoader().LoadForGEIRequest(branch, batch, GetEInvoicingObjectFactory());
				AssertEquals(3, credentials.Count());
				AssertD365Credentials(credentials, expectedClientID, expectedTenantID);
			}
		}

		void AssertD365WebserviceURL(IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> credentials, string expectedServiceEndpoint)
		{
			var serviceEndpoint = credentials.Single(x => x.Key == "ServiceEndpoint").Value;
			AssertEquals(false, serviceEndpoint.Encrypted);
			AssertEquals(expectedServiceEndpoint, serviceEndpoint.Value);
		}

		void AssertD365Credentials(IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> credentials, string expectedClientID, string expectedTenantID)
		{
			var clientId = credentials.Single(x => x.Key == "ClientId").Value;
			AssertEquals(false, clientId.Encrypted);
			AssertEquals(expectedClientID, clientId.Value);

			var clientSecret = credentials.Single(x => x.Key == "ClientSecret").Value;

			AssertEquals(true, clientSecret.Encrypted);
			AssertNoExceptionThrown("Encrypted credential is base64 encoded", () => Convert.FromBase64String(clientSecret.Value));

			var tenantId = credentials.Single(x => x.Key == "TenantId").Value;
			AssertEquals(false, tenantId.Encrypted);
			AssertEquals(expectedTenantID, tenantId.Value);
		}

		IDisposable SetD365CredentialsRegistry(Guid companyPK, string clientID, string clientSecret, string tenantID)
		{
			var credential = AccountingMasterFilesRegistry.Instance.D365Credentials.Value;
			credential.ClientID = clientID;
			credential.ClientSecret = clientSecret;
			credential.TenantID = tenantID;

			return AccountingMasterFilesRegistry.Instance.D365Credentials.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, credential);
		}

		IDisposable SetD365WebServiceURLRegistry(Guid companyPK, string serviceEndpoint)
		{
			return AccountingMasterFilesRegistry.Instance.D365WebserviceURL.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, serviceEndpoint);
		}

		ICountryEInvoicingObjectFactory GetEInvoicingObjectFactory()
		=> GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCodes.UnitedKingdom);

		ICredentialsLoader GetCredentialsLoader()
			=> GetEInvoicingObjectFactory().GetCredentialsLoader();

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
