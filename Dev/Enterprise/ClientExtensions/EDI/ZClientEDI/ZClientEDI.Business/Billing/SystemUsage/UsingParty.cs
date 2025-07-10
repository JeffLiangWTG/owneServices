using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface IUsingParty
	{
		ZGuid ClientCompanyPK { get; }
		ZGuid LicenceCompanyPK { get; }
		ZGuid DatabasePK { get; }
		ZGuid OrganisationPK { get; }
		string EnterpriseCode { get; }
		string CompanyCode { get; }
		string CompanyName { get; }
		string ServerCode { get; }

		LicenceHeader UsageOwnerLicence { get; }
		bool IsOrganisationActive { get; }
	}

	public static class UsingPartyComparer
	{
		public static bool IsEqual(IUsingParty a, IUsingParty b)
		{
			return a.OrganisationPK == b.OrganisationPK
				&& a.LicenceCompanyPK == b.LicenceCompanyPK
				&& a.EnterpriseCode == b.EnterpriseCode
				&& a.CompanyCode == b.CompanyCode
				&& a.ServerCode == b.ServerCode;
		}
	}

	public class UsingParty : IUsingParty
	{
		public UsingParty()
		{
			OrganisationPK = ZGuid.Empty;
			ClientCompanyPK = ZGuid.Empty;
			LicenceCompanyPK = ZGuid.Empty;
			DatabasePK = ZGuid.Empty;
		}

		public UsingParty(ClientChargeableUsage usage)
		{
			var client = usage.ClientCompany;
			if (client != null)
			{
				Init(client);
			}
			else
			{
				var db = usage.Database;
				if (db != null)
				{
					var lic = db.UsageOwnerOrFirstLicence;
					if (lic != null)
					{
						Init(lic);
					}
				}
				else
				{
					var licCompany = usage.LicenceCompany;
					if (licCompany != null)
					{
						Init(licCompany, "");
					}
				}
			}
		}

		public UsingParty(LicenceHeader lic)
		{
			Init(lic);
		}

		public UsingParty(LicenceHeader lic, ClientCompany clientCompany)
		{
			Init(lic);
			if (clientCompany != null)
			{
				CompanyCode = clientCompany.LCC_Code;
				CompanyName = clientCompany.LCC_Name;
				ClientCompanyPK = clientCompany.PK;
			}
		}

		public UsingParty(ClientCompany clientCompany)
		{
			Init(clientCompany);
		}

		public UsingParty(EDIOrgHeader org, string serverCode = "")
		{
			Init(org, serverCode);
		}

		void Init(LicenceHeader lic)
		{
			UsageOwnerLicence = lic;
			LicenceCompanyPK = lic.Company.PK;
			DatabasePK = lic.Database.PK;
			OrganisationPK = lic.Company.LC_OH;
			IsOrganisationActive = lic.Company.Header.OH_IsActive;
			EnterpriseCode = lic.Company.LicEnterprise.LE_EnterpriseCode;
			CompanyCode = lic.Company.LC_CompanyCode;
			ServerCode = lic.Database.LD_ServerCode;
		}

		void Init(ClientCompany clientCompany)
		{
			var owner = clientCompany.UsageOwnerLicence;
			UsageOwnerLicence = owner;
			ClientCompanyPK = clientCompany.PK;
			LicenceCompanyPK = owner != null ? owner.Company.PK : ZGuid.Empty;
			DatabasePK = clientCompany.Database.PK;
			if (!clientCompany.LCC_OH.IsEmpty)
			{
				OrganisationPK = clientCompany.LCC_OH;
				IsOrganisationActive = clientCompany.Org?.OH_IsActive ?? false;
			}
			else
			{
				OrganisationPK = owner != null ? owner.Company.LC_OH : ZGuid.Empty;
				IsOrganisationActive = owner?.Company.Header.OH_IsActive ?? false;
			}
			EnterpriseCode = clientCompany.Database.LicEnterprise.LE_EnterpriseCode;
			CompanyCode = clientCompany.LCC_Code;
			CompanyName = clientCompany.LCC_Name;
			ServerCode = clientCompany.Database.LD_ServerCode;
		}

		void Init(LicenceCompany licCompany, string serverCode)
		{
			LicenceCompanyPK = licCompany.PK;
			OrganisationPK = licCompany.LC_OH;
			IsOrganisationActive = licCompany.Header.OH_IsActive;
			EnterpriseCode = licCompany.LicEnterprise.LE_EnterpriseCode;
			CompanyCode = licCompany.LC_CompanyCode;
			ServerCode = serverCode;
		}

		void Init(EDIOrgHeader org, string serverCode)
		{
			var licCompany = org.LicCompany;
			if (licCompany != null)
			{
				Init(licCompany, serverCode);
			}
			else
			{
				LicenceCompanyPK = ZGuid.Empty;
				OrganisationPK = org.PK;
				IsOrganisationActive = org.OH_IsActive;
				ServerCode = serverCode;
			}
		}

		public ZGuid OrganisationPK { get; private set; }
		public ZGuid ClientCompanyPK { get; private set; }
		public ZGuid LicenceCompanyPK { get; private set; }
		public ZGuid DatabasePK { get; private set; }
		public string EnterpriseCode { get; private set; }
		public string CompanyCode { get; private set; }
		public string CompanyName { get; private set; }
		public string ServerCode { get; private set; }
		public LicenceHeader UsageOwnerLicence { get; private set; }
		public bool IsOrganisationActive { get; private set; }
	}
}

