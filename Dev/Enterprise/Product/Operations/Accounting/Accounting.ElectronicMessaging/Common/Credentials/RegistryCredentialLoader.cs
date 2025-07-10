using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Credentials
{
	/// <summary>
	/// Credential loader which loads from a registry item.
	/// Note that this is not best practice for storing credentials - please use ObjectFactoryCredentialLoader whenever possible.
	/// </summary>
	public class RegistryCredentialLoader : ICredentialsLoader
	{
		public RegistryCredentialLoader(EInvoicingCredentialsRegistryItem registryItem)
		{
			RegistryItem = Argument.NotNull(registryItem, nameof(registryItem));
		}

		public EInvoicingCredentialsRegistryItem RegistryItem { get; }

		public IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> LoadForGEIRequest(GlbBranch branch, TransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory)
		{
			Argument.NotNull(branch, nameof(branch));
			Argument.NotNull(batch, nameof(batch));
			Argument.NotNull(countryFactory, nameof(countryFactory));

			var credential = RegistryItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
			if (credential == null)
			{
				yield break;
			}

			if (!credential.ClientId.IsEmpty)
			{
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = CredentialKeys.Username,
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						EncryptedSpecified = false,
						Value = credential.ClientId,
					}
				};
			}

			if (!credential.ClientSecret.IsEmpty)
			{
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = CredentialKeys.Password,
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						Encrypted = true,
						EncryptedSpecified = true,
						Value = GetEncryptedPassphrase(credential),
					}
				};
			}
		}

		protected virtual string GetEncryptedPassphrase(EInvoicingCredentials credential) =>
				credential.GetEncryptedPassphraseForEHub();
	}
}
