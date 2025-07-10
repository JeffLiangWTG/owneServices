using System;
using System.Collections.Generic;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.D365
{
	public class D365CredentialsLoader : ICredentialsLoader
	{
		public IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> LoadForGEIRequest(GlbBranch branch, TransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory)
		{
			var webServiceURL = AccountingMasterFilesRegistry.Instance.D365WebserviceURL.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);

			if (!string.IsNullOrEmpty(webServiceURL))
			{
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = "ServiceEndpoint",
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						EncryptedSpecified = false,
						Value = webServiceURL,
					}
				};
			}

			var credential = AccountingMasterFilesRegistry.Instance.D365Credentials.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
			if (credential == null)
			{
				yield break;
			}

			if (!string.IsNullOrEmpty(credential.TenantID)) {
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = "TenantId",
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						EncryptedSpecified = false,
						Value = credential.TenantID,
					}
				};
			}

			if (!string.IsNullOrEmpty(credential.ClientID))
			{
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = "ClientId",
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						EncryptedSpecified = false,
						Value = credential.ClientID,
					}
				};
			}

			if (!string.IsNullOrEmpty(credential.ClientSecret))
			{
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = "ClientSecret",
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						Encrypted = true,
						EncryptedSpecified = true,
						Value = CredentialSender.EncryptPasswordAsString(credential.ClientSecret),
					}
				};
			}
		}
	}
}
