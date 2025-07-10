//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccEInvoicingTransactionPivotLookups
//
//    This class should be used for overriding collections in AutoAccEInvoicingTransactionPivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class AccEInvoicingTransactionPivotLookups : AutoAccEInvoicingTransactionPivotLookups
	{
		public AccEInvoicingTransactionPivotLookups(AutoAccEInvoicingTransactionPivot parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ActionTypeList => new CodeDescriptionPairList(OLookUpEditType.EInvoicingPivotActionType);

		public CodeDescriptionPairList StatusList => PivotStatusList;

		public static CodeDescriptionPairList PivotStatusList => CreatePivotStatusList();

		static CodeDescriptionPairList CreatePivotStatusList()
		{
			var pivotStatusList = new CodeDescriptionPairList(OLookUpEditType.EInvoicingPivotState);
			string pendingDescriptionFromRegistry = AccountingMasterFilesRegistry.Instance.EReportingPivotPendingStatusDescription.Value;
			if (!string.IsNullOrWhiteSpace(pendingDescriptionFromRegistry))
			{
				pivotStatusList.OverwriteDescriptionForCodeInPlace(Core.Constants.EInvoicingPivotState.Pending, pendingDescriptionFromRegistry);
			}
			return pivotStatusList;
		}
	}
}
