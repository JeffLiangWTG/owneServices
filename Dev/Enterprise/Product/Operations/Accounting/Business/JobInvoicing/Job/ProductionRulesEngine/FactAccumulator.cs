using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class FactAccumulator
	{
		public FactAccumulator()
		{
			OrgFacts = new Dictionary<ZGuid, IOrganisationWithMainAddressFact>();
			UnlocoFacts = new Dictionary<ZGuid, UNLOCOFact>();
			CountryFacts = new Dictionary<ZGuid, CountryFact>();
			AddressFacts = new Dictionary<ZGuid, AddressFact>();
			StaffFacts = new Dictionary<ZGuid, StaffFact>();
			DepartmentFacts = new Dictionary<Guid, DepartmentFact>();
			BranchFacts = new Dictionary<Guid, BranchFact>();
		}

		public UNLOCOFact CreateUniqueUNLOCOFact(RefUNLOCO unloco)
		{
			var unlocoCountry = unloco.Country;

			if (unlocoCountry == null)
			{
				return null;
			}

			if (!CountryFacts.TryGetValue(unloco.Country.PK, out var originCountryFact))
			{
				CountryFacts[unloco.Country.PK] = originCountryFact = new CountryFact(unloco.Country);
			}
			if (!UnlocoFacts.TryGetValue(unloco.PK, out var unlocoFact))
			{
				UnlocoFacts[unloco.PK] = unlocoFact = new UNLOCOFact(unloco, originCountryFact);
			}

			return unlocoFact;
		}

		public IOrganisationWithMainAddressFact CreateUniqueOrganisationFact(OrgHeader org)
		{
			var address = org.MainAddress;
			var addressCountry = address.Country;

			if (addressCountry == null)
			{
				return null;
			}

			if (!CountryFacts.TryGetValue(addressCountry.PK, out var countryFact))
			{
				CountryFacts[addressCountry.PK] = countryFact = new CountryFact(addressCountry);
			}
			if (!AddressFacts.TryGetValue(address.PK, out var addressFact))
			{
				AddressFacts[address.PK] = addressFact = new AddressFact(address, countryFact);
			}
			if (!OrgFacts.TryGetValue(org.PK, out var orgFact))
			{
				OrgFacts[org.PK] = orgFact = new OrganisationWithMainAddressFact(org, addressFact);
			}

			return orgFact;
		}

		public StaffFact CreateUniqueStaffFact(GlbStaff staff)
		{
			var homeDepartmentFact = CreateUniqueDepartmentFact(staff.HomeDepartment);

			if (!StaffFacts.TryGetValue(staff.PK, out var staffFact))
			{
				StaffFacts[staff.PK] = staffFact = new StaffFact(staff, homeDepartmentFact);
			}

			return staffFact;
		}

		public DepartmentFact CreateUniqueDepartmentFact(IDepartment department)
		{
			DepartmentFact departmentFact = null;
			if (department != null)
			{
				if (!DepartmentFacts.TryGetValue(department.PK, out departmentFact))
				{
					DepartmentFacts[department.PK] = departmentFact = new DepartmentFact(department.PK, department.Code);
				}
			}

			return departmentFact;
		}

		public BranchFact CreateUniqueBranchFact(IBranch branch)
		{
			BranchFact branchFact = null;
			if (branch != null)
			{
				if (!BranchFacts.TryGetValue(branch.PK, out branchFact))
				{
					BranchFacts[branch.PK] = branchFact = new BranchFact(branch.PK, branch.Code);
				}
			}

			return branchFact;
		}

		Dictionary<ZGuid, IOrganisationWithMainAddressFact> OrgFacts { get; }
		Dictionary<ZGuid, UNLOCOFact> UnlocoFacts { get; }
		Dictionary<ZGuid, CountryFact> CountryFacts { get; }
		Dictionary<ZGuid, AddressFact> AddressFacts { get; }
		Dictionary<ZGuid, StaffFact> StaffFacts { get; }
		Dictionary<Guid, DepartmentFact> DepartmentFacts { get; }
		Dictionary<Guid, BranchFact> BranchFacts { get; }
	}
}
