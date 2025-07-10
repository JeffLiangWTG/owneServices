using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Sensitive;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Jordan
{
	class JordanCredentialLoader : ICredentialsLoader
	{
		public IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> LoadForGEIRequest(GlbBranch branch, TransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory)
		{
			var registryLoader = new JordanRegistryCredentialLoader(AccountingElectronicMessagingRegistry.Instance.JordanEInvoicingCredentials);
			yield return registryLoader.GenerateSecretCredential();

			foreach (var credential in registryLoader.LoadForGEIRequest(branch, batch, countryFactory))
			{
				yield return credential;
			}
		}
	}

	class JordanRegistryCredentialLoader : RegistryCredentialLoader
	{
		readonly byte[] Secret;

		public JordanRegistryCredentialLoader(EInvoicingCredentialsRegistryItem registryItem) : base(registryItem)
		{
			Secret = GenerateSecret();
		}

		byte[] GenerateSecret()
		{
			var secret = new byte[64];
			RandomNumberGenerator.Create().GetBytes(secret);
			return secret;
		}

		public GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential GenerateSecretCredential()
		{
			return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
			{
				Key = CredentialKeys.Secret,
				Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
				{
					Encrypted = true,
					EncryptedSpecified = true,
					Value = Convert.ToBase64String(CredentialSender.EncryptPasswordAsBytes(Secret)),
				}
			};
		}

		#region override

		protected override string GetEncryptedPassphrase(EInvoicingCredentials credential)
		{
			var sensitiveDataEncryption = new SensitiveDataEncryption(RandomNumberGenerator.Create());
			return sensitiveDataEncryption.EncryptStringAndSerialize(credential.ClientSecret, Secret);
		}

		#endregion
	}
}
