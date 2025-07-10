using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	class OrganizationAddressMatchToOrgHeader : OrganizationAddressMatchBase
	{
		/// <summary>
		/// Defines an Organisation Match where the target is a guid reference to OrgHeader.
		/// </summary>
		/// <param name="organisationAddressType">Organisation Address Type code from the Universal Shipment.</param>
		/// <param name="dbField">Field containing an Organisation PK.</param>
		public OrganizationAddressMatchToOrgHeader(MatchableOrganizationType organisationAddressType, SchemaGuidColumn dbField, OrganizationAddressMatchPool matchPool, OrganisationTypes unmatchedOrgNoteType, string unmatchedOrgNoteSubType)
			: base(organisationAddressType, matchPool, unmatchedOrgNoteType, unmatchedOrgNoteSubType)
		{
			this.dbField = Argument.NotNull(dbField, "SchemaGuidColumn dbField");
		}
		readonly SchemaGuidColumn dbField;

		protected override ZGuid[] GetTargetOrgHeaderPKsToMatchTo(BusinessObject matchingBO)
		{
			var orgPK = (ZGuid)matchingBO[dbField];
			if (orgPK.IsValid)
			{
				return new[] { orgPK };
			}

			return null;
		}
	}
}
