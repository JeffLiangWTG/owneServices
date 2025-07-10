using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.UniversalDataBuss.DataObjects;
using static Enterprise.Core.Constants;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Mauritius.Testing
{
	sealed class MauritiusCredentialLoaderTest : TestCaseWithFactory
	{
		public void TestLoadForGEIRequest_ReturnsEmptyCollection_WhenNoCredentialsSaved()
		{
			// Arrange
			var objectFactory = GetEInvoicingObjectFactory();
			var branch = new TestObjectCreator(Factory).CreateBranchWithCompany("MUPLU");
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			// Act
			var result = GetCredentialsLoader().LoadForGEIRequest(branch, batch, objectFactory).ToArray();

			// Assert
			Assert(!result.Any());
		}

		public void TestLoadForGEIRequest_ReturnsThreeCredentials_WhenAllCredentialsSaved_CompanyLevel()
		{
			// Arrange
			var objectFactory = GetEInvoicingObjectFactory();
			var branch = new TestObjectCreator(Factory).CreateBranchWithCompany("MUPLU");
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			using (AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingUsername.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "MRA Username for test"))
			using (AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingPassword.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "MRA Password for test"))
			using (AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingEbsMraID.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "EBS MRA ID for test"))
			{
				// Act
				var result = GetCredentialsLoader().LoadForGEIRequest(branch, batch, objectFactory).ToArray();

				// Assert
				AssertEquals("Three credentials should be returned", 3, result.Length);
				AssertContainsExactElementsInAnyOrder("Username, Password and EbsMraId should be returned", new[] { CredentialKeys.Username, CredentialKeys.Password, "EbsMraId" }, result.Select(x => x.Key));
				Assert("All credentials should have a value", result.All(x => !x.Value.Value.IsEmpty));

				var userNameElement = result.Single(x => x.Key == CredentialKeys.Username).Value;
				Assert("Username should not be encrypted", !userNameElement.Encrypted);
				var userNameContent = userNameElement.Value;
				AssertEquals("Username content", "MRA Username for test", userNameContent);

				var passwordElement = result.Single(x => x.Key == CredentialKeys.Password).Value;
				Assert(message: "Password should be encrypted", passwordElement.Encrypted);
				var passwordContent = passwordElement.Value;
				AssertNoExceptionThrown("Password should be Base64 encrypted for eHub", () => Convert.FromBase64String(passwordContent));

				var ebsMraIdElement = result.Single(x => x.Key == "EbsMraId").Value;
				Assert(message: "EBS MRA ID should not be encrypted", !ebsMraIdElement.Encrypted);
				var ebsMraIdContent = ebsMraIdElement.Value;
				AssertEquals("EBS MRA ID content", ebsMraIdContent, "EBS MRA ID for test");
			}
		}

		public void TestLoadForGEIRequest_ReturnsThreeCredentials_WhenAllCredentialsSaved_CompanyAndBranchLevel()
		{
			// Arrange
			var objectFactory = GetEInvoicingObjectFactory();
			var branch = new TestObjectCreator(Factory).CreateBranchWithCompany("MUPLU");
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

			using (AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingUsername.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "MRA Username for company"))
			using (AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingPassword.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "MRA Password for company"))
			using (AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingEbsMraID.SetTemporaryValue(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "EBS MRA ID for company"))
			using (AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingUsername.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, "MRA Username for branch"))
			using (AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingPassword.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, "MRA Password for branch"))
			using (AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingEbsMraID.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, "EBS MRA ID for branch"))
			{
				// Act
				var result = GetCredentialsLoader().LoadForGEIRequest(branch, batch, objectFactory).ToArray();

				// Assert
				AssertEquals("Three credentials should be returned", 3, result.Length);
				AssertContainsExactElementsInAnyOrder("Username, Password and EbsMraId should be returned", new[] { CredentialKeys.Username, CredentialKeys.Password, "EbsMraId" }, result.Select(x => x.Key));
				Assert("All credentials should have a value", result.All(x => !x.Value.Value.IsEmpty));

				var userNameElement = result.Single(x => x.Key == CredentialKeys.Username).Value;
				Assert("Username should not be encrypted", !userNameElement.Encrypted);
				var userNameContent = userNameElement.Value;
				AssertEquals("Username content", "MRA Username for branch", userNameContent);

				var passwordElement = result.Single(x => x.Key == CredentialKeys.Password).Value;
				Assert(message: "Password should be encrypted", passwordElement.Encrypted);
				var passwordContent = passwordElement.Value;
				AssertNoExceptionThrown("Password should be Base64 encrypted for eHub", () => Convert.FromBase64String(passwordContent));

				var ebsMraIdElement = result.Single(x => x.Key == "EbsMraId").Value;
				Assert(message: "EBS MRA ID should not be encrypted", !ebsMraIdElement.Encrypted);
				var ebsMraIdContent = ebsMraIdElement.Value;
				AssertEquals("EBS MRA ID content", ebsMraIdContent, "EBS MRA ID for branch");
			}
		}

		#region Implementation

		ICountryEInvoicingObjectFactory GetEInvoicingObjectFactory()
			=> GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCodes.Mauritius);

		ICredentialsLoader GetCredentialsLoader()
			=> GetEInvoicingObjectFactory().GetCredentialsLoader();

		#endregion
	}
}
