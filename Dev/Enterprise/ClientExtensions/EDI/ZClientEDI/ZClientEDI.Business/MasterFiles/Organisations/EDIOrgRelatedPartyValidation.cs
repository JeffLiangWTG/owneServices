using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgRelatedPartyValidation : OrgRelatedPartyValidation
	{
		public EDIOrgRelatedPartyValidation(AutoOrgRelatedParty parent)
			: base(parent)
		{
		}

		public EDIOrgRelatedParty RelatedParty
		{
			get { return (EDIOrgRelatedParty)base.Parent; }
		}

		#region PR_FreightDirection

		protected override void CheckPR_FreightDirection()
		{
			if (RelatedParty.PR_PartyType == EDIOrgRelatedPartyLookups.ContractingPartyCode
				|| RelatedParty.PR_PartyType == EDIOrgRelatedPartyLookups.WARPConstant
				|| RelatedParty.PR_PartyType == EDIOrgRelatedPartyLookups.ERequestVisibilityGroupCode)
			{
				//No mandatory or list validation required - direction is readonly on these party type
			}
			else
			{
				base.CheckPR_FreightDirection();
			}
		}

		#endregion

		#region PR_PartyType

		protected override void CheckPR_PartyType()
		{
			if (RelatedParty.PR_PartyType != EDIOrgRelatedPartyLookups.WARPConstant)
			{
				base.CheckPR_PartyType();
			}
		}

		#endregion

		#region PR_OH_RelatedParty

		protected override void CheckPR_OH_RelatedParty()
		{
			base.CheckPR_OH_RelatedParty();
			if (RelatedParty.PR_PartyType == EDIOrgRelatedPartyLookups.WARPConstant && RelatedParty.RelatedParty == RelatedParty.Parent)
			{
				RelatedParty.PR_OH_RelatedPartyInfo.AddError("You cannot have this organization be a party of itself for this type of party.");
			}
			var warningMsg = EDIOrgHeaderValidationHelper.ValidateENTCodeForRelatedParty(RelatedParty);
			if (!string.IsNullOrEmpty(warningMsg))
			{
				RelatedParty.PR_OH_RelatedPartyInfo.AddWarning(string.Format("This organisation has a different enterprise code than {0}. As this is a rare scenario, the relationship should be verified as being correct.", RelatedParty.EDIParentOrg.NameAndCode));
			}
		}

		#endregion

		#region PR_Location

		protected override void CheckPR_Location()
		{
			if (RelatedParty.PR_PartyType != EDIOrgRelatedPartyLookups.WARPConstant)
			{
				base.CheckPR_Location();
			}
		}

		#endregion

		#region PR_FreightTransportMode

		protected override void CheckPR_FreightTransportMode()
		{
			if (RelatedParty.PR_PartyType != EDIOrgRelatedPartyLookups.WARPConstant)
			{
				base.CheckPR_FreightTransportMode();
			}
		}

		#endregion

		#region PR_FreightContainerMode

		protected override void CheckPR_FreightContainerMode()
		{
			if (RelatedParty.PR_PartyType != EDIOrgRelatedPartyLookups.WARPConstant)
			{
				base.CheckPR_FreightContainerMode();
			}
		}

		#endregion

		#region PR_RN_NKImporterCountry

		protected override void CheckPR_RN_NKImporterCountry()
		{
			if (RelatedParty.PR_PartyType != EDIOrgRelatedPartyLookups.WARPConstant)
			{
				base.CheckPR_RN_NKImporterCountry();
			}
		}

		#endregion

		#region Company Level

		protected override void CheckCompanyLevel()
		{
			var checkBase = true;

			if (RelatedParty.PR_PartyType == EDIOrgRelatedPartyLookups.ContractingPartyCode
				&& RelatedParty.CompanyLevel != CompanyLevelList.Codes.ENT)
			{
				checkBase = false;
				RelatedParty.CompanyLevelInfo.AddError("Contracting party should be unique for all companies. The Company Level must be 'ENT' for this type of party.");
			}

			if (RelatedParty.PR_PartyType == EDIOrgRelatedPartyLookups.WARPConstant)
			{
				checkBase = false;
				if (RelatedParty.CompanyLevel != CompanyLevelList.Codes.ENT)
				{
					RelatedParty.CompanyLevelInfo.AddError("WARP should be unique for all companies. The Company Level must be 'ENT' for this type of party.");
				}
			}

			if (checkBase)
			{
				base.CheckCompanyLevel();
			}
		}

		#endregion
	}
}

