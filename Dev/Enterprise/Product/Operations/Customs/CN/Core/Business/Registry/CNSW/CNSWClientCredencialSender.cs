using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;

namespace Enterprise.Customs.CN.Business
{
	class CNSWClientCredencialSender
	{
		public static void SendSettingCreateOrUpdate(BusinessObjectFactory factory, Guid companyPK, Guid branchPk, ZString userName)
		{
			SendSetting(factory, companyPK, branchPk, userName);
		}

		public static void SendSettingDelete(BusinessObjectFactory factory, Guid companyPK, Guid branchPk)
		{
			SendSetting(factory, companyPK, branchPk, ZString.Empty);
		}

		const string Company = nameof(Company);
		const string Branch = nameof(Branch);
		const string Current = nameof(Current);

		static void SendSetting(BusinessObjectFactory factory, Guid companyPk, Guid branchPk, ZString userName)
		{
			var credentialSender = new CredentialSender(Constants.CNSWClient.ConfigNameForeService);

			var branch = factory.Load<GlbBranch>(branchPk);
			var company = factory.Load<GlbCompany>(companyPk) ?? branch.Company;
			var groupCompany = new Group() { Type = Company, Reference = company.GC_Code };
			var credential = CredentialSender.CreateCredential(Current, userName, ZString.Empty);

			if (branchPk == Guid.Empty)
			{
				groupCompany.Items = new[] { credential };
			}
			else
			{
				var groupBranch = new Group() { Type = Branch, Reference = branch.GB_Code };
				groupCompany.Items = new[] { groupBranch };
				groupBranch.Items = new[] { credential };
			}

			credentialSender.AddItems(groupCompany);
			credentialSender.SendCredential(factory);
		}
	}
}
