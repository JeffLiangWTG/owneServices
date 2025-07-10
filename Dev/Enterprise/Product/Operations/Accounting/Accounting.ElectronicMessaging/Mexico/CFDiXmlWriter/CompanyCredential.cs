using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface ICompanyCredential
	{
		EInvoicingCertificateCredential GetCompanyCredential(TransactionInfo transactionInfo);
	}

	class CompanyCredential : ICompanyCredential
	{
		EInvoicingCertificateCredential ICompanyCredential.GetCompanyCredential(TransactionInfo transactionInfo)
		{
			if (transactionInfo != null)
			{
				var branchCode = transactionInfo.Branch?.Code ?? ZString.Empty;
				if (branchCode.IsEmpty)
				{
					return null;
				}

				var branch = Factory.LoadFromUniqueKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
				if (branch == null
					|| branch.Company == null
					|| transactionInfo.TransactionDate == null)
				{
					return null;
				}

				var credentials = EInvoicingCertificateCredential.LoadBestCertificate(branch.Company,
									currentTime: transactionInfo.TransactionDate.Value.ToDateTime(),
									passwordType: PasswordTypesList.Codes.EIM,
									orderByColumn: GlbExternalPasswordSchema.GP_ExpiryDate.Name
								);
				return credentials;
			}

			return null;
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
