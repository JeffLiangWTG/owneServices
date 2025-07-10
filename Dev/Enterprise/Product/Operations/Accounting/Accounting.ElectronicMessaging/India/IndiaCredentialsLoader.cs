using System;
using System.Collections.Generic;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.India
{
	public class IndiaCredentialsLoader : ICredentialsLoader
	{
		public IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> LoadForGEIRequest(GlbBranch branch, TransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory)
		{
			var registryValue = AccountingMasterFilesRegistry.Instance.IndiaEInvoicingCredentials.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var serviceProviderClientId = registryValue.ClientId;
			var serviceProviderClientSecret = registryValue.ClientSecret;

			if (!branch.BranchCredentialsIndia.ClientSecret.IsEmpty)
			{
				serviceProviderClientId = branch.BranchCredentialsIndia.ClientId;
				serviceProviderClientSecret = branch.BranchCredentialsIndia.ClientSecret;
			}
			if (!serviceProviderClientId.IsEmpty && !serviceProviderClientSecret.IsEmpty)
			{
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = ServiceProviderClientId,
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						Encrypted = false,
						EncryptedSpecified = false,
						Value = serviceProviderClientId,
					}
				};
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = ServiceProviderClientSecret,
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						Encrypted = true,
						EncryptedSpecified = true,
						Value = CredentialSender.EncryptPasswordAsString(serviceProviderClientSecret),
					}
				};
			}

			if (!branch.BranchCredentialsIndia.Username.IsEmpty && !branch.BranchCredentialsIndia.Password.IsEmpty)
			{
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = CredentialKeys.Username,
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						Encrypted = false,
						EncryptedSpecified = false,
						Value = branch.BranchCredentialsIndia.Username,
					}
				};
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = CredentialKeys.Password,
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						Encrypted = true,
						EncryptedSpecified = true,
						Value = CredentialSender.EncryptPasswordAsString(branch.BranchCredentialsIndia.Password),
					}
				};
			}
		}

#pragma warning disable CW1161 // Res.GetString Analyzer. Hard coded data transfer constants.
		const string ServiceProviderClientId = "ClientId";
		const string ServiceProviderClientSecret = "ClientSecret";
#pragma warning restore CW1161 // Res.GetString Analyzer
	}
}
