using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Schema;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Philippines.Credential
{
	internal sealed class PhilippinesCredentialLoader : PasswordTypeCredentialLoader
	{
		public override SchemaDateTimeColumn CertificateOrderByColumn => GlbExternalPasswordSchema.GP_SystemLastEditTimeUtc;

		public override IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> LoadForGEIRequest(GlbBranch branch, TransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory)
		{
			Argument.NotNull(branch, nameof(branch));
			Argument.NotNull(batch, nameof(batch));

			var glbExternalPassword_PHA = LoadGlbExternalPasswordForCompany(branch.Company, PasswordTypesList.Codes.PHA);
			if (glbExternalPassword_PHA != null)
			{
				yield return WrapCredential("ApplicationId", glbExternalPassword_PHA.GP_UserID);
				yield return WrapCredential("AccreditationId", glbExternalPassword_PHA.CurrentDecryptedPassword);
			}

			var glbExternalPassword_PHU = LoadGlbExternalPasswordForCompany(branch.Company, PasswordTypesList.Codes.PHU);
			if (glbExternalPassword_PHU != null)
			{
				yield return WrapCredential((NoResString)"Username", glbExternalPassword_PHU.GP_UserID);
				yield return WrapCredential((NoResString)"Password", glbExternalPassword_PHU.CurrentDecryptedPassword);
			}
		}
	}
}
