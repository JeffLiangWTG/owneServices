using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	class OrganizationAddressMatchToOrgAddress : OrganizationAddressMatchBase
	{
		/// <summary>
		/// Defines an Organisation Match where the target is a guid reference to OrgAddress.
		/// </summary>
		/// <param name="organisationAddressType">Organisation Address Type code from the Universal Shipment.</param>
		/// <param name="dbField">Field containing an Address PK.</param>
		public OrganizationAddressMatchToOrgAddress(MatchableOrganizationType organisationAddressType, SchemaGuidColumn dbField, OrganizationAddressMatchPool matchPool, OrganisationTypes unmatchedOrgNoteType, string unmatchedOrgNoteSubType)
			: base(organisationAddressType, matchPool, unmatchedOrgNoteType, unmatchedOrgNoteSubType)
		{
			this.dbField = Argument.NotNull(dbField, "SchemaGuidColumn dbField");
		}
		readonly SchemaGuidColumn dbField;

		protected override ZGuid[] GetTargetOrgHeaderPKsToMatchTo(BusinessObject matchingBO)
		{
			var orgAddressPK = (ZGuid)matchingBO[dbField];
			if (orgAddressPK.IsValid)
			{
				var factory = matchingBO.Factory;
				var orgAddress = factory.Load<IOrgAddress>(orgAddressPK);
				if (orgAddress != null)
				{
					return new[] { orgAddress.OrganisationPK };
				}
			}

			return null;
		}
	}
}
