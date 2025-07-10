using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;

namespace Enterprise.Dash.Business.Entities
{
	// Consider moving this class into common location if a class implementing IOrgHeaderInfo is needed by another team
	public sealed class EntityOrgHeaderInfo : IOrgHeaderInfo
	{
		public Guid OH_PK { get; set; }

		public string OH_Code { get; set; }

		public string OH_FullName { get; set; }

		public bool OH_IsActive { get; set; }

		public bool OH_IsAirCTO { get; set; }

		public bool OH_IsAirLine { get; set; }

		public bool OH_IsAirWholesaler { get; set; }

		public bool OH_IsConsignee { get; set; }

		public bool OH_IsConsignor { get; set; }

		public bool OH_IsContainerLeasingCompany { get; set; }

		public bool OH_IsControllingCustomer { get; set; }

		public bool OH_IsFerryWaterTerminal { get; set; }

		public bool OH_IsForwarder { get; set; }

		public bool OH_IsInlandWaterwayProvider { get; set; }

		public bool OH_IsLineHaulProvider { get; set; }

		public bool OH_IsLocalTransport { get; set; }

		public bool OH_IsMiscFreightServices { get; set; }

		public bool OH_IsRailHead { get; set; }

		public bool OH_IsRailProvider { get; set; }

		public bool OH_IsSeaCTO { get; set; }

		public bool OH_IsSeaWholesaler { get; set; }

		public bool OH_IsShippingConsortium { get; set; }

		public bool OH_IsShippingLine { get; set; }

		public bool OH_IsShippingProvider { get; set; }

		public bool OH_IsVGMContractor { get; set; }

		public string OH_RL_NKClosestPort { get; set; }

		public Guid? OH_RSL_ShippingLine { get; set; }

		public IRefUNLOCOInfo ClosestPort { get; set; }

		public IRefShippingLineInfo ShippingLine { get; set; }

		public ICollection<INettingOrganisationInfo> NettingOrganisations { get; set; }

		public ICollection<IOrgAddressInfo> OrgAddresses { get; set; }

		public ICollection<IOrgBrandOrRelatedNameInfo> OrgBrandOrRelatedNames { get; set; }

		public ICollection<IOrgCompanyDataInfo> OrgCompanyData { get; set; }

		public ICollection<IOrgContactInfo> OrgContacts { get; set; }

		public ICollection<IOrgCusCodeInfo> CusCodes { get; set; }

		public ICollection<IOrgMiscServInfo> OrgMiscServs { get; set; }

		public ICollection<IOrgRelatedPartyInfo> RelatedParties { get; set; }

		public ICollection<IOrgRelatedPartyInfo> PartiesRelatedToThisOrg { get; set; }

		public ICollection<ICrmOpportunityInfo> CrmOpportunities { get; set; }
	}
}
