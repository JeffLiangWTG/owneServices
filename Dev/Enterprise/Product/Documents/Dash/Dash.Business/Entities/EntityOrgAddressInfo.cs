using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;

namespace Enterprise.Dash.Business.Entities
{
	// Consider moving this class into common location if a class implementing IOrgAddressInfo is needed by another team
	public sealed class EntityOrgAddressInfo : IOrgAddressInfo
	{
		public Guid OA_PK { get; set; }

		public string OA_Code { get; set; }

		public bool OA_IsActive { get; set; }

		public string OA_AdditionalAddressInformation { get; set; }

		public string OA_Address1 { get; set; }

		public string OA_Address2 { get; set; }

		public string OA_AuthorityToLeave { get; set; }

		public string OA_City { get; set; }

		public string OA_CompanyNameOverride { get; set; }

		public string OA_Email { get; set; }

		public string OA_Fax { get; set; }

		public string OA_GeofencePolygon { get; set; }

		public string OA_GeoLocation { get; set; }

		public int OA_JobLoadingDuration { get; set; }

		public string OA_Mobile { get; set; }

		public Guid OA_OH { get; set; }

		public string OA_Phone { get; set; }

		public string OA_PostCode { get; set; }

		public string OA_RL_NKRelatedPortCode { get; set; }

		public string OA_RN_NKCountryCode { get; set; }

		public string OA_State { get; set; }

		public IRefUNLOCOInfo RelatedPort { get; set; }

		public IRefCountryInfo Country { get; set; }

		public IOrgHeaderInfo OrgHeader { get; set; }

		public ICollection<IOrgAddressAdditionalInfoInfo> OrgAddressAdditionalInfos { get; set; }

		public ICollection<IOrgAddressCapabilityInfo> OrgAddressCapabilities { get; set; }
	}
}
