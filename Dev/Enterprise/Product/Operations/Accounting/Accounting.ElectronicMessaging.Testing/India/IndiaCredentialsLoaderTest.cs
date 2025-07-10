using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.India.Testing
{
	public sealed class IndiaCredentialsLoaderTest : TestCaseWithFactory
	{
		public void TestNoCredentials()
		{
			var branch = Factory.NewCompanyAndBranchWith(countryCode: "IN");
			var credentials = new IndiaCredentialsLoader().LoadForGEIRequest(branch, null, new IndiaEInvoicingObjectFactory());
			AssertEquals(0, credentials.Count());
		}

		public void TestCredentials_ForRegistry()
		{
			var branch = Factory.NewCompanyAndBranchWith(countryCode: "IN");
			using (SetRegistryCredentials("TheUser", "SecretPassword"))
			{
				var credentials = new IndiaCredentialsLoader().LoadForGEIRequest(branch, null, new IndiaEInvoicingObjectFactory());
				AssertEquals(2, credentials.Count());

				var providerUsername = credentials.Single(x => x.Key == "ClientId").Value;
				AssertEquals(false, providerUsername.Encrypted);
				AssertEquals("TheUser", providerUsername.Value);

				var providerPassword = credentials.Single(x => x.Key == "ClientSecret").Value;
				AssertEquals(true, providerPassword.Encrypted);
				AssertNoExceptionThrown("Encrypted credential is base64 encoded", () => providerPassword.Value.ToUTF8FromBase64());
			}
		}

		public void TestCredentials_ForBranchTaxpayer()
		{
			var branch = Factory.NewCompanyAndBranchWith(countryCode: "IN");
			SetBranchProviderCredentials(branch, "AnotherUser", "MoreSecretPassword");

			var credentials = new IndiaCredentialsLoader().LoadForGEIRequest(branch, null, new IndiaEInvoicingObjectFactory());
			AssertEquals(2, credentials.Count());

			var providerUsername = credentials.Single(x => x.Key == "ClientId").Value;
			AssertEquals(false, providerUsername.Encrypted);
			AssertEquals("AnotherUser", providerUsername.Value);

			var providerPassword = credentials.Single(x => x.Key == "ClientSecret").Value;
			AssertEquals(true, providerPassword.Encrypted);
			AssertNoExceptionThrown("Encrypted credential is base64 encoded", () => providerPassword.Value.ToUTF8FromBase64());
		}

		public void TestCredentials_ForRegistryAndBranchProvider()
		{
			var branch = Factory.NewCompanyAndBranchWith(countryCode: "IN");
			SetBranchProviderCredentials(branch, "AnotherUser", "MoreSecretPassword");
			using (SetRegistryCredentials("TheUser", "SecretPassword"))
			{
				var credentials = new IndiaCredentialsLoader().LoadForGEIRequest(branch, null, new IndiaEInvoicingObjectFactory());
				AssertEquals(2, credentials.Count());

				var providerUsername = credentials.Single(x => x.Key == "ClientId").Value;
				AssertEquals(false, providerUsername.Encrypted);
				AssertEquals("AnotherUser", providerUsername.Value);

				var providerPassword = credentials.Single(x => x.Key == "ClientSecret").Value;
				AssertEquals(true, providerPassword.Encrypted);
				AssertNoExceptionThrown("Encrypted credential is base64 encoded", () => providerPassword.Value.ToUTF8FromBase64());
			}
		}

		public void TestCredentials_ForRegistryAndBranchTaxpayer()
		{
			var branch = Factory.NewCompanyAndBranchWith(countryCode: "IN");
			SetBranchTaxPayerCredentials(branch, "CargoWiseUser", "ClientPassword");
			using (SetRegistryCredentials("TheUser", "SecretPassword"))
			{
				var credentials = new IndiaCredentialsLoader().LoadForGEIRequest(branch, null, new IndiaEInvoicingObjectFactory());
				AssertEquals(4, credentials.Count());

				var providerUsername = credentials.Single(x => x.Key == "ClientId").Value;
				AssertEquals(false, providerUsername.Encrypted);
				AssertEquals("TheUser", providerUsername.Value);

				var providerPassword = credentials.Single(x => x.Key == "ClientSecret").Value;
				AssertEquals(true, providerPassword.Encrypted);
				AssertNoExceptionThrown("Encrypted credential is base64 encoded", () => providerPassword.Value.ToUTF8FromBase64());

				var taxpayerUsername = credentials.Single(x => x.Key == "Username").Value;
				AssertEquals(false, taxpayerUsername.Encrypted);
				AssertEquals("CargoWiseUser", taxpayerUsername.Value);

				var taxpayerPassword = credentials.Single(x => x.Key == "Password").Value;
				AssertEquals(true, taxpayerPassword.Encrypted);
				AssertNoExceptionThrown("Encrypted credential is base64 encoded", () => taxpayerPassword.Value.ToUTF8FromBase64());
			}
		}

		public void TestCredentials_ForBranchProviderAndBranchTaxpayer()
		{
			var branch = Factory.NewCompanyAndBranchWith(countryCode: "IN");
			SetBranchProviderCredentials(branch, "AnotherUser", "MoreSecretPassword");
			SetBranchTaxPayerCredentials(branch, "CargoWiseUser", "ClientPassword");

			var credentials = new IndiaCredentialsLoader().LoadForGEIRequest(branch, null, new IndiaEInvoicingObjectFactory());
			AssertEquals(4, credentials.Count());

			var providerUsername = credentials.Single(x => x.Key == "ClientId").Value;
			AssertEquals(false, providerUsername.Encrypted);
			AssertEquals("AnotherUser", providerUsername.Value);

			var providerPassword = credentials.Single(x => x.Key == "ClientSecret").Value;
			AssertEquals(true, providerPassword.Encrypted);
			AssertNoExceptionThrown("Encrypted credential is base64 encoded", () => providerPassword.Value.ToUTF8FromBase64());

			var taxpayerUsername = credentials.Single(x => x.Key == "Username").Value;
			AssertEquals(false, taxpayerUsername.Encrypted);
			AssertEquals("CargoWiseUser", taxpayerUsername.Value);

			var taxpayerPassword = credentials.Single(x => x.Key == "Password").Value;
			AssertEquals(true, taxpayerPassword.Encrypted);
			AssertNoExceptionThrown("Encrypted credential is base64 encoded", () => taxpayerPassword.Value.ToUTF8FromBase64());
		}

		public void TestCredentials_ForRegistryAndBranchProviderAndBranchTaxpayer()
		{
			var branch = Factory.NewCompanyAndBranchWith(countryCode: "IN");
			SetBranchProviderCredentials(branch, "AnotherUser", "MoreSecretPassword");
			SetBranchTaxPayerCredentials(branch, "CargoWiseUser", "ClientPassword");
			using (SetRegistryCredentials("TheUser", "SecretPassword"))
			{
				var credentials = new IndiaCredentialsLoader().LoadForGEIRequest(branch, null, new IndiaEInvoicingObjectFactory());
				AssertEquals(4, credentials.Count());

				var providerUsername = credentials.Single(x => x.Key == "ClientId").Value;
				AssertEquals(false, providerUsername.Encrypted);
				AssertEquals("AnotherUser", providerUsername.Value);

				var providerPassword = credentials.Single(x => x.Key == "ClientSecret").Value;
				AssertEquals(true, providerPassword.Encrypted);
				AssertNoExceptionThrown("Encrypted credential is base64 encoded", () => providerPassword.Value.ToUTF8FromBase64());

				var taxpayerUsername = credentials.Single(x => x.Key == "Username").Value;
				AssertEquals(false, taxpayerUsername.Encrypted);
				AssertEquals("CargoWiseUser", taxpayerUsername.Value);

				var taxpayerPassword = credentials.Single(x => x.Key == "Password").Value;
				AssertEquals(true, taxpayerPassword.Encrypted);
				AssertNoExceptionThrown("Encrypted credential is base64 encoded", () => taxpayerPassword.Value.ToUTF8FromBase64());
			}
		}

		#region Implementation

		IDisposable SetRegistryCredentials(string user, string password)
		{
			return AccountingMasterFilesRegistry.Instance.IndiaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new EInvoicingCredentials() { ClientId = user, ClientSecret = password });
		}

		void SetBranchProviderCredentials(GlbBranch branch, string user, string password)
		{
			branch.BranchCredentialsIndia.ClientId = user;
			branch.BranchCredentialsIndia.ClientSecret = password;
		}

		void SetBranchTaxPayerCredentials(GlbBranch branch, string user, string password)
		{
			branch.BranchCredentialsIndia.Username = user;
			branch.BranchCredentialsIndia.Password = password;
			branch.BranchCredentialsIndia.PasswordConfirmation = password;
		}

		#endregion
	}
}
