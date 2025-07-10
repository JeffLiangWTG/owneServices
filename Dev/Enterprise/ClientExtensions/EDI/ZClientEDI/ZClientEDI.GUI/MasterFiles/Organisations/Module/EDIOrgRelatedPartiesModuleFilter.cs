using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class EDIOrgRelatedPartiesModuleFilter : OrgRelatedPartiesModuleFilter
	{
		public EDIOrgRelatedPartiesModuleFilter(ZString description)
			: base(description)
		{
		}

		public EDIOrgRelatedPartiesModuleFilter(ZString description, GetRelatedPartiesQuery queryDelegate)
			: base(description, queryDelegate)
		{
		}

		public override RelatedPartyTypeList CreatePartyList()
		{
			RelatedPartyTypeList partyTypes = base.CreatePartyList();
			partyTypes.AddPair(EDIOrgRelatedPartyLookups.WARPConstant, EDIOrgRelatedPartyLookups.WARPReferringCustomerPartyDescription);
			partyTypes.AddPair(EDIOrgRelatedPartyLookups.ContractingPartyCode, EDIOrgRelatedPartyLookups.ContractingPartyDescription);
			partyTypes.AddPair(EDIOrgRelatedPartyLookups.ERequestVisibilityGroupCode, EDIOrgRelatedPartyLookups.ERequestVisibilityGroupDescription);
			partyTypes.Sort();
			return partyTypes;
		}

		public override bool ShouldCalculateDirection
		{
			get
			{
				switch (PartyType)
				{
					case EDIOrgRelatedPartyLookups.WARPConstant:
					case EDIOrgRelatedPartyLookups.ContractingPartyCode:
					case EDIOrgRelatedPartyLookups.ERequestVisibilityGroupCode:
						return false;
					default:
						return base.ShouldCalculateDirection;
				}
			}
		}
	}
}
