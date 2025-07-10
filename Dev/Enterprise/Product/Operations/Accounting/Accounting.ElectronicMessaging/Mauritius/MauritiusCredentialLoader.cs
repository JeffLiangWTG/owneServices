using System;
using System.Collections.Generic;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Mauritius
{
	sealed class MauritiusCredentialLoader : ICredentialsLoader
	{
		public IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> LoadForGEIRequest(GlbBranch branch, TransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory)
		{
			var username = AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingUsername.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
			if (!string.IsNullOrEmpty(username))
			{
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = CredentialKeys.Username,
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						Encrypted = false,
						EncryptedSpecified = false,
						Value = username,
					}
				};
			}

			var password = AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingPassword.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
			if (!string.IsNullOrEmpty(password))
			{
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = CredentialKeys.Password,
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						Encrypted = true,
						EncryptedSpecified = true,
						Value = CredentialSender.EncryptPasswordAsString(password),
					}
				};
			}

			var ebsMraId = AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingEbsMraID.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
			if (!string.IsNullOrEmpty(ebsMraId))
			{
				yield return new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
				{
					Key = "EbsMraId",
					Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue()
					{
						Encrypted = false,
						EncryptedSpecified = false,
						Value = ebsMraId,
					}
				};
			}
		}
	}
}
