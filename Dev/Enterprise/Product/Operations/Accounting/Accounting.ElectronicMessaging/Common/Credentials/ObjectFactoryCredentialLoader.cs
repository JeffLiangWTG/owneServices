using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Credentials
{
	/// <summary>
	/// Credential loader which loads credentials as defined by the ICountryEInvoicingObjectFactory.Credentials object.
	/// </summary>
	public sealed class ObjectFactoryCredentialLoader : PasswordTypeCredentialLoader
	{
		public ObjectFactoryCredentialLoader(SchemaDateTimeColumn certificateOrderByColumn = null, bool supportCertificateBase64EncodedTwice = false)
		{
			CertificateOrderByColumn = certificateOrderByColumn ?? GlbExternalPasswordSchema.GP_IssueDate;
			SupportCertificateBase64EncodedTwice = supportCertificateBase64EncodedTwice;
		}

		public override SchemaDateTimeColumn CertificateOrderByColumn { get; }

		readonly bool SupportCertificateBase64EncodedTwice;

		public override IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> LoadForGEIRequest(GlbBranch branch, TransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory)
		{
			Argument.NotNull(branch, nameof(branch));
			Argument.NotNull(batch, nameof(batch));
			Argument.NotNull(countryFactory, nameof(countryFactory));

			if (countryFactory.Credentials.IsNoCredential())
			{
				yield break;
			}

			if (countryFactory.Credentials is IEInvoicingCertificateCredentialSettings certificateSettings)
			{
				foreach (var credential in LoadMostRecentCertificate(branch, certificateSettings))
				{
					yield return credential;
				}
			}

			if (countryFactory.Credentials is IEInvoicingPasswordCredentialSettings passwordSettings
				&& passwordSettings.PasswordDefinitions != null)
			{
				foreach (var credential in LoadAllPasswords(branch, passwordSettings))
				{
					yield return credential;
				}
			}
		}

		#region Implementation

		GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential[] LoadMostRecentCertificate(GlbBranch branch, IEInvoicingCertificateCredentialSettings certificateSettings)
		{
			var credential = LoadGlbExternalPassword(branch, certificateSettings.PasswordType, certificateSettings);
			if (credential == null)
			{
				return Array.Empty<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>();
			}

			var certificate = WrapCredential(CredentialKeys.Certificate, credential.GetCertificateAsBase64String(SupportCertificateBase64EncodedTwice));
			var password = WrapCredential(CredentialKeys.CertificatePassword, credential.GetEncryptedPassphraseForEHub(), true);
			return new[] { certificate, password };
		}

		protected override GlbExternalPassword LoadGlbExternalPasswordForBranch(GlbBranch branch, string passwordType)
			=> EInvoicingCertificateCredential.LoadBestCertificate(branch, ZDateTime.Now, passwordType, CertificateOrderByColumn.Name);

		protected override GlbExternalPassword LoadGlbExternalPasswordForCompany(GlbCompany company, string passwordType)
			=> EInvoicingCertificateCredential.LoadBestCertificate(company, ZDateTime.Now, passwordType, CertificateOrderByColumn.Name);

		IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> LoadAllPasswords(GlbBranch branch, IEInvoicingPasswordCredentialSettings passwordSettings)
		{
			var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, branch.GB_GC);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, SQLComparisonOperator.Equal, passwordSettings.PasswordType);
			query.AddToFilter(GlbExternalPasswordSchema.GP_Certificate, SQLComparisonOperator.Equal, null);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordStatus, SQLComparisonOperator.Equal, PasswordStatusList.Codes.Valid);
			var records = branch.Factory.Load<GlbExternalPassword>(query);

			foreach (var definition in passwordSettings.PasswordDefinitions)
			{
				GlbExternalPassword credential = null;
				if (passwordSettings.IsBranchCredentialsRequired)
				{
					credential = records.FirstOrDefault(x => x.GP_GB == branch.PK && x.GP_MailBoxID == definition.UniqueKey);
				}
				if (credential == null && passwordSettings.IsCompanyCredentialsRequired)
				{
					credential = records.FirstOrDefault(x => x.GP_GB.IsEmpty && x.GP_MailBoxID == definition.UniqueKey);
				}
				if (credential != null)
				{
					yield return WrapCredential(definition.UniqueKey + "." + CredentialKeys.Username, credential.GP_UserID, valueIsEncrypted: false);
					yield return WrapCredential(definition.UniqueKey + "." + CredentialKeys.Password, credential.CurrentDecryptedPassword, valueIsEncrypted: true);
				}
			}
		}

		#endregion
	}
}
