using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Moq;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class RegistryCredentialLoaderTest : TestCaseWithFactory
	{
		public void TestConstructor_SetsRegistryProperty()
		{
			AssertExceptionThrown<ArgumentNullException>(() => CreateObjectForTest(null));

			var loader = CreateObjectForTest(AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials);
			AssertEquals(nameof(AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials), loader.RegistryItem.Name);

			var loaderForIndia = CreateObjectForTest(AccountingMasterFilesRegistry.Instance.IndiaEInvoicingCredentials);
			AssertEquals(nameof(AccountingMasterFilesRegistry.Instance.IndiaEInvoicingCredentials), loaderForIndia.RegistryItem.Name);
		}

		public void TestLoadForGEIRequest_Throws_WhenNullArguments()
		{
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = ZGuid.NewZGuid();

			AssertExceptionThrown<ArgumentNullException>(() =>
				CreateEgyptObjectForTest().LoadForGEIRequest(null, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray()
			);
			AssertExceptionThrown<ArgumentNullException>(() =>
				CreateEgyptObjectForTest().LoadForGEIRequest(branch, null, countryFactoryMock.Object).ToArray()
			);
			AssertExceptionThrown<ArgumentNullException>(() =>
				CreateEgyptObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), null).ToArray()
			);
			AssertNoExceptionThrown(() =>
				CreateEgyptObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray()
			);
		}

		public void TestLoadForGEIRequest_ReturnsEmpty_WhenNoCredentials()
		{
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = ZGuid.NewZGuid();

			AssertEquals("Precondition ClientId", "", AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials.Value.ClientId);
			AssertEquals("Precondition ClientSecret", "", AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials.Value.ClientSecret);

			var result = CreateEgyptObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object);
			Assert(!result.Any());
		}

		public void TestLoadForGEIRequest_ReturnsEmpty_WhenCredentialsSetForDifferentCompany()
		{
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = ZGuid.NewZGuid();

			var credential = AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials.Value;
			credential.ClientId = "the client id";
			credential.ClientSecret = "super secret!!";
			using (AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, credential))
			{
				var credentialFromRegistry = AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				AssertEquals("Precondition ClientId", "the client id", credentialFromRegistry.ClientId);
				AssertEquals("Precondition ClientSecret", "super secret!!", credentialFromRegistry.ClientSecret);

				var result = CreateEgyptObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object);
				Assert(!result.Any());
			}
		}

		public void TestLoadForGEIRequest_ReturnsCredentialsObjects_WhenSetForCompany()
		{
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = ZGuid.NewZGuid();

			var credential = AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials.Value;
			credential.ClientId = "cd41ae9a-fcdd-46d0-8ec3-aaa26c93b03b";
			credential.ClientSecret = "1d8b2aca-480d-4509-b7ce-c12e84c86800";
			using (AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, credential))
			{
				var credentialFromRegistry = AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
				AssertEquals("Precondition ClientId", "cd41ae9a-fcdd-46d0-8ec3-aaa26c93b03b", credentialFromRegistry.ClientId);
				AssertEquals("Precondition ClientSecret", "1d8b2aca-480d-4509-b7ce-c12e84c86800", credentialFromRegistry.ClientSecret);

				var results = CreateEgyptObjectForTest().LoadForGEIRequest(branch, new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), countryFactoryMock.Object).ToArray();

				AssertEquals("Two credentials should be returned", 2, results.Length);
				AssertContainsExactElementsInAnyOrder("Username and Password should be returned", new[] { CredentialKeys.Username, CredentialKeys.Password }, results.Select(x => x.Key));
				Assert("All credentials should have a value", results.All(x => !x.Value.Value.IsEmpty));

				var usernameElement = results.First(x => x.Key == CredentialKeys.Username).Value;
				Assert("Username should not be encrypted", !usernameElement.Encrypted);
				var usernameContent = usernameElement.Value;
				AssertEquals("Username content", "cd41ae9a-fcdd-46d0-8ec3-aaa26c93b03b", usernameContent);

				var passwordElement = results.First(x => x.Key == CredentialKeys.Password).Value;
				Assert("Password should be encrypted", passwordElement.Encrypted);
				var passwordContent = passwordElement.Value;
				AssertNoExceptionThrown("Password as Base64 encrypted for eHub", () => Convert.FromBase64String(passwordContent));
			}
		}

		#region Implementation

		RegistryCredentialLoader CreateObjectForTest(EInvoicingCredentialsRegistryItem registryItem)
			=> new RegistryCredentialLoader(registryItem);

		RegistryCredentialLoader CreateEgyptObjectForTest()
			=> new RegistryCredentialLoader(AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials);

		#endregion
	}
}
