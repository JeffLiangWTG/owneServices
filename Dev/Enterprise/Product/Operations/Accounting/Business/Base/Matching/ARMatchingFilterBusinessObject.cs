using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class ARMatchingFilterBusinessObject : MatchingFilterBusinessObject
	{
		public ARMatchingFilterBusinessObject()
			: base()
		{
		}

		public override OrgHeaderCollection PrimaryOrgHeaders => FindboxLookupCollections.GetDebtorCollection(Factory);

		protected override ZString RelatedPartyType => RelatedPartyTypeList.Codes.ARSettlementGroup;

		protected override ZString RelatedPartyFreightDirection => RelatedPartyDirectionList.Codes.AR;

		protected override string LayoutContextName => "ARNewMatchGroupForm";

		protected override bool EnableRelatedDisbursementTransactions => AccountingUtils.ShouldShowRelatedDisbursementTransactions();
	}
}
