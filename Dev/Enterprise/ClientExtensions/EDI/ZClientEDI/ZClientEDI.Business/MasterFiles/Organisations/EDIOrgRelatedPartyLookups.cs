using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgRelatedPartyLookups : OrgRelatedPartyLookups
	{
		public const string ContractingPartyCode = "COP";
		public const string ContractingPartyDescription = "Contracting Party";
		public const string WARPConstant = "WRP";
		public const string WARPNominatedAgentPartyDescription = "WARP Nominated Agent";
		public const string WARPReferringCustomerPartyDescription = "WARP Referring Customer";
		public const string ERequestVisibilityGroupCode = "ERQ";
		public const string ERequestVisibilityGroupDescription = "eRequest Visibility Group";

		public const string EdiEnterpriseProductTypeCode = ProductTypes.Codes.Enterprise;
		public EDIOrgRelatedPartyLookups(AutoOrgRelatedParty parent)
			: base(parent)
		{
		}

		public override PartyTypeDescriptionOnlyList PartyTypeList
		{
			get
			{
				if (partyTypeList == null)
				{
					partyTypeList = new PartyTypeDescriptionOnlyList();
					partyTypeList.AddPair(ContractingPartyCode, ContractingPartyDescription);
					partyTypeList.AddPair(WARPConstant, WARPReferringCustomerPartyDescription);
					partyTypeList.AddPair(ERequestVisibilityGroupCode, ERequestVisibilityGroupDescription);
					partyTypeList.Sort();
				}
				return partyTypeList;
			}
		}

		#region ProductTypeList

		public CodeDescriptionPairList ProductTypeList
		{
			get
			{
				return new ProductTypes();
			}
		}

		#endregion

		PartyTypeDescriptionOnlyList partyTypeList;

		public override RelatedPartyTypeList AllPartyTypeList
		{
			get
			{
				if (allPartyTypeList == null)
				{
					allPartyTypeList = new RelatedPartyTypeList();
					allPartyTypeList.AddPair(ContractingPartyCode, ContractingPartyDescription);
					allPartyTypeList.AddPair(WARPConstant, WARPReferringCustomerPartyDescription);
					allPartyTypeList.AddPair(ERequestVisibilityGroupCode, ERequestVisibilityGroupDescription);
					allPartyTypeList.Sort();
				}
				return allPartyTypeList;
			}
		}
		RelatedPartyTypeList allPartyTypeList;

		public override PartyTypeDescriptionOnlyList ParentPartyTypeList
		{
			get
			{
				if (parentPartyTypeList == null)
				{
					parentPartyTypeList = new PartyTypeDescriptionOnlyList();
					parentPartyTypeList.AddPair(ContractingPartyCode, ContractingPartyDescription);
					parentPartyTypeList.AddPair(WARPConstant, WARPNominatedAgentPartyDescription);
					parentPartyTypeList.AddPair(ERequestVisibilityGroupCode, ERequestVisibilityGroupDescription);
					parentPartyTypeList.Sort();
				}
				return parentPartyTypeList;
			}
		}
		PartyTypeDescriptionOnlyList parentPartyTypeList;

		public override RelatedPartyTypeList AllParentTypeList
		{
			get
			{
				if (allParentTypeList == null)
				{
					allParentTypeList = new RelatedPartyTypeList();
					allParentTypeList.AddPair(ContractingPartyCode, ContractingPartyDescription);
					allParentTypeList.AddPair(WARPConstant, WARPNominatedAgentPartyDescription);
					allParentTypeList.AddPair(ERequestVisibilityGroupCode, ERequestVisibilityGroupDescription);
					allParentTypeList.Sort();
				}
				return allParentTypeList;
			}
		}
		RelatedPartyTypeList allParentTypeList;
	}
}
