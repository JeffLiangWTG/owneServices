using System.Collections.Generic;
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
	public abstract class PasswordTypeCredentialLoader : ICredentialsLoader
	{
		public abstract IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> LoadForGEIRequest(GlbBranch branch, TransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory);

		public abstract SchemaDateTimeColumn CertificateOrderByColumn { get; }

		protected GlbExternalPassword LoadGlbExternalPassword(GlbBranch branch, string passwordType, IEInvoicingCredentialSettings credentialSettings)
		{
			Argument.NotNull(credentialSettings, nameof(credentialSettings));

			GlbExternalPassword result = null;

			if (credentialSettings.IsBranchCredentialsRequired)
			{
				result = LoadGlbExternalPasswordForBranch(branch, passwordType);
			}

			if (result == null && credentialSettings.IsCompanyCredentialsRequired)
			{
				result = LoadGlbExternalPasswordForCompany(branch.Company, passwordType);
			}

			return result;
		}

		protected virtual GlbExternalPassword LoadGlbExternalPasswordForBranch(GlbBranch branch, string passwordType)
		{
			Argument.NotNull(branch, nameof(branch));

			var query = new ZQuery(GlbExternalPasswordSchema.GP_GB, branch.PK);
			AddCommonCritera(query, passwordType, CertificateOrderByColumn.Name);

			return branch.Factory.LoadTop1<GlbExternalPassword>(query);
		}

		protected virtual GlbExternalPassword LoadGlbExternalPasswordForCompany(GlbCompany company, string passwordType)
		{
			Argument.NotNull(company, nameof(company));

			ZQuery query = new ZQuery(GlbExternalPasswordSchema.GP_GC, company.PK);
			AddCommonCritera(query, passwordType, CertificateOrderByColumn.Name);

			return company.Factory.LoadTop1<GlbExternalPassword>(query);
		}

		protected GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential WrapCredential(ZString key, ZString value, bool valueIsEncrypted = false)
		{
			var credentialValue = valueIsEncrypted
				? new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue() { Value = value, Encrypted = true, EncryptedSpecified = true }
				: new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue() { Value = value };
			var credential = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential() { Key = key, Value = credentialValue };

			return credential;
		}

		static void AddCommonCritera(ZQuery query, string passwordType, string orderByColumn)
		{
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, SQLComparisonOperator.Equal, passwordType);
			query.OrderBy = orderByColumn + OrderByClause.Descending + ", " + GlbExternalPassword.Schema.GP_SystemCreateTimeUtc;
		}
	}
}