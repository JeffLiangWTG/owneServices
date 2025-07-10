using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// Set of LicenceCompany records representing owners of usage and payers of usage.
	/// Also loads the ClientInvoiceDelivery records of owner companies to determine the paying org.
	/// Efficiently loads records in bulk.
	/// </summary>
	public class LicenceCompanyDeliverySet
	{
		public LicenceCompanyDeliverySet(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;
		readonly Dictionary<Guid, LicenceCompany> allCompanyPkMap = new Dictionary<Guid, LicenceCompany>();
		readonly Dictionary<Guid, LicenceCompany> orgPkToLicenceCompany = new Dictionary<Guid, LicenceCompany>();

		readonly HashSet<Guid> ownerCompanyPksToLoad = new HashSet<Guid>();
		readonly HashSet<Guid> allCompanyPksToLoad = new HashSet<Guid>();
		readonly HashSet<Guid> orgPksToLoad = new HashSet<Guid>();

		readonly Dictionary<Guid, IEnumerable<ClientInvoiceDelivery>> companyPkToDeliveries = new Dictionary<Guid, IEnumerable<ClientInvoiceDelivery>>();

		/// <summary>
		/// Add the LicenceCompany PKs that own usage of interest.
		/// </summary>
		public void AddOwnerCompanyPks(IEnumerable<Guid> companyPks)
		{
			foreach (var pk in companyPks)
			{
				if (!companyPkToDeliveries.ContainsKey(pk))
				{
					ownerCompanyPksToLoad.Add(pk);
					allCompanyPksToLoad.Add(pk);
				}
			}
		}

		/// <summary>
		/// Add OrgHeader PKs of orgs that own usage of interest (typically ClientCompany LCC_OH owners).
		/// </summary>
		public void AddOwnerOrgPks(IEnumerable<Guid> orgPks)
		{
			foreach (var pk in orgPks)
			{
				if (!orgPkToLicenceCompany.ContainsKey(pk))
				{
					orgPksToLoad.Add(pk);
				}
			}
		}

		/// <summary>
		/// Add the paying orgs from the given set of ClientInvoiceDelivery
		/// </summary>
		public void AddPayingOrgs(IEnumerable<ClientInvoiceDelivery> deliveries)
		{
			foreach (var orgPk in deliveries
				.Where(x => x.L9_IsBilled && !x.L9_OH_InvoiceTo.IsEmpty)
				.Select(x => x.L9_OH_InvoiceTo.ToGuid())
				.Distinct())
			{
				if (!orgPkToLicenceCompany.ContainsKey(orgPk))
				{
					orgPksToLoad.Add(orgPk);
				}
			}
		}

		public void LoadOrgs()
		{
			factory.Load<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgPkToLicenceCompany.Keys.Concat(orgPksToLoad)));
		}

		public IEnumerable<ClientInvoiceDelivery> GetDeliveries(Guid companyPk)
		{
			IEnumerable<ClientInvoiceDelivery> result;
			if (!companyPkToDeliveries.TryGetValue(companyPk, out result))
			{
				ownerCompanyPksToLoad.Add(companyPk);
				if (!allCompanyPkMap.ContainsKey(companyPk))
				{
					allCompanyPksToLoad.Add(companyPk);
				}
				LoadDeliveries();
				companyPkToDeliveries.TryGetValue(companyPk, out result);
			}

			return result;
		}

		public LicenceCompany GetCompany(Guid companyPk)
		{
			LicenceCompany result;
			if (!allCompanyPkMap.TryGetValue(companyPk, out result))
			{
				allCompanyPksToLoad.Add(companyPk);
				LoadAllCompanies();
				allCompanyPkMap.TryGetValue(companyPk, out result);
			}
			return result;
		}

		public LicenceCompany GetCompanyByOrgPk(Guid orgPk)
		{
			LicenceCompany result;
			if (!orgPkToLicenceCompany.TryGetValue(orgPk, out result))
			{
				orgPksToLoad.Add(orgPk);
				LoadAllCompanies();
				orgPkToLicenceCompany.TryGetValue(orgPk, out result);
			}
			return result;
		}

		public LicenceCompany GetInvoicedCompany(ClientInvoiceDelivery delivery)
		{
			LoadAllCompanies();

			LicenceCompany result = null;
			if (delivery != null && delivery.L9_IsBilled)
			{
				if (!delivery.L9_OH_InvoiceTo.IsEmpty)
				{
					orgPkToLicenceCompany.TryGetValue(delivery.L9_OH_InvoiceTo.ToGuid(), out result);
				}
				else
				{
					allCompanyPkMap.TryGetValue(delivery.L9_LC.ToGuid(), out result);
				}
			}
			return result;
		}

		void LoadAllCompanies()
		{
			if (allCompanyPksToLoad.Any() || orgPksToLoad.Any())
			{
				var licCompanyQuery = new ZQuery();
				if (allCompanyPksToLoad.Any())
				{
					licCompanyQuery.AddToFilter(LicenceCompanySchema.PK, allCompanyPksToLoad);
				}
				if (orgPksToLoad.Any())
				{
					licCompanyQuery.AddToFilter(JoinCondition.Or, LicenceCompanySchema.LC_OH, orgPksToLoad);
				}
				foreach (var company in factory.Load<LicenceCompany>(licCompanyQuery))
				{
					allCompanyPkMap[company.PK.ToGuid()] = company;
					orgPkToLicenceCompany[company.LC_OH.ToGuid()] = company;
					orgPksToLoad.Remove(company.LC_OH.ToGuid());
				}

				foreach (var orgPk in orgPksToLoad)
				{
					orgPkToLicenceCompany[orgPk] = null;
				}

				orgPksToLoad.Clear();
				allCompanyPksToLoad.Clear();
			}
		}

		void LoadDeliveries()
		{
			foreach (var deliveriesByCompany in factory.Load<ClientInvoiceDelivery>(new ZQuery(ClientInvoiceDeliverySchema.L9_LC, ownerCompanyPksToLoad))
				.GroupBy(x => x.L9_LC.ToGuid()))
			{
				companyPkToDeliveries[deliveriesByCompany.Key] = deliveriesByCompany;
				ownerCompanyPksToLoad.Remove(deliveriesByCompany.Key);
			}

			foreach (var pk in ownerCompanyPksToLoad)
			{
				companyPkToDeliveries[pk] = null;
			}
		}
	}
}

