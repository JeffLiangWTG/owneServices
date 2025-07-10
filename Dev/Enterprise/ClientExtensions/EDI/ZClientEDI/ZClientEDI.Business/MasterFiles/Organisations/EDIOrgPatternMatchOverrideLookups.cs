using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgPatternMatchOverrideLookups : OrgPatternMatchOverrideLookups
	{
		public EDIOrgPatternMatchOverrideLookups(EDIOrgPatternMatchOverride parent)
			: base(parent)
		{
		}

		protected override CodeDescriptionPairList OO_Relationship_ListCore
		{
			get
			{
				var list = base.OO_Relationship_ListCore;
				list.AddPair(EDIConstants.OrgPatternMatchOverrideRelationships.EHubClientID, Res.GetString("b1cf6541-cdfd-4e57-9803-cb68d9c1ebc7", "eHub Client ID"));
				return list;
			}
		}
	}
}
