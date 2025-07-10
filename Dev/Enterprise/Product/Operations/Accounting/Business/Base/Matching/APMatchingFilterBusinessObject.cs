using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class APMatchingFilterBusinessObject : MatchingFilterBusinessObject
	{
		public APMatchingFilterBusinessObject()
			: base()
		{
		}

		public override ZQuery FilterForDBReload
		{
			get
			{
				var filter = base.FilterForDBReload;
				if (IsFilterForDBReloadNoResultQuery)
				{
					filter.IsNoResultQuery = true;
				}
				return filter;
			}
		}

		public void SetFilterForDBReloadIsNoResultQuery()
		{
			IsFilterForDBReloadNoResultQuery = true;
		}
		ZBool IsFilterForDBReloadNoResultQuery;

		public override OrgHeaderCollection PrimaryOrgHeaders => FindboxLookupCollections.GetCreditorCollection(Factory);

		protected override ZString RelatedPartyType => RelatedPartyTypeList.Codes.APSettlementGroup;

		protected override ZString RelatedPartyFreightDirection => RelatedPartyDirectionList.Codes.AP;

		protected override string LayoutContextName => "APNewMatchGroupForm";
	}
}
