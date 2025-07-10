using System;
using System.Linq;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Jordan;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.UniversalDataBuss.DataObjects;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Jordan
{
	class JordanRegistryCredentialLoaderTest : RegistryCredentialLoaderTest
	{
		public void TestGetEncryptedPassphrase()
		{
			var jordanObjectFactory = new JordanEInvoicingObjectFactory();
			var branch = new TestObjectCreator(Factory).CreateBranchWithCompany("JOWRZ");
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			var credentialClientId = "Client ID";
			var credentialClientSecret = "8vX9eJ2kR7pY4tH1wB6qF3dS0gZ5nM9rL2cV8jP7oI4uY6aE1sW3fK9hX5mQ2pL8gB3vN7jH4wU1zR6dF9cS2kP5lO8iY3eA7rT0yU2fD6mG1xJ9vC4pN8hK2bW5qR7eH3tY6iU1oP9aL2sD8fG4kM7jQ3wE0zX5cV9hB6pN2lO5iY8eA3rT7yU9fD4mG6xJ1vC8pN3hK5bW2qR9eH7tY4iU3oP6aL9sD2fG8kM1jQ4wE7zX3cV6hB9pN5lO2iY7eA4rT1yU6fD3mG9xJ2vC5pN8";

			var credential = AccountingElectronicMessagingRegistry.Instance.JordanEInvoicingCredentials.Value;
			credential.ClientId = credentialClientId;
			credential.ClientSecret = credentialClientSecret;
			using (AccountingElectronicMessagingRegistry.Instance.JordanEInvoicingCredentials.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, credential))
			{
				var credentialLoader = new JordanRegistryCredentialLoader(AccountingElectronicMessagingRegistry.Instance.JordanEInvoicingCredentials);
				AssertNoExceptionThrown(() =>
				{
					var result = credentialLoader.LoadForGEIRequest(branch, batch, jordanObjectFactory).ToArray();
					AssertEquals(2, result.Length);
				});
			}
		}
	}
}
