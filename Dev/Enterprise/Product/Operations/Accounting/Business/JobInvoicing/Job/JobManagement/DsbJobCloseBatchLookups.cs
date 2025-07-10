//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDsbJobCloseBatchLookups
//
//    This class should be used for overriding collections in AutoDsbJobCloseBatchLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class DsbJobCloseBatchLookups : AutoDsbJobCloseBatchLookups
	{
		public DsbJobCloseBatchLookups(AutoDsbJobCloseBatch parent) : base(parent)
		{
		}

		#region StatusList

		public virtual ICodeDescriptionPairList StatusList
		{
			get { return DSBJobCloseBatchStatusList; }
		}

		public static ICodeDescriptionPairList DSBJobCloseBatchStatusList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(AccountingConstants.DsbJobBatchStatus.Open, ResString.GetMultilingualString("b18d9df3-7885-4362-9ae1-90d3b84a9135", "Open"));
				list.AddPair(AccountingConstants.DsbJobBatchStatus.RequireApproval, ResString.GetMultilingualString("14d3ebcc-7653-4011-9142-eb28d754bcec", "Requested"));
				list.AddPair(AccountingConstants.DsbJobBatchStatus.Approve, ResString.GetMultilingualString("5e02fb60-2e83-4886-9b44-2827048a5bfb", "Approved"));
				list.AddPair(AccountingConstants.DsbJobBatchStatus.Close, ResString.GetMultilingualString("d9211200-b4f1-45f5-9236-827f4677d697", "Closed"));
				list.AddPair(AccountingConstants.DsbJobBatchStatus.Cancel, ResString.GetMultilingualString("24b08427-577f-4d2c-ba5b-6c7f81246ed6", "Cancel"));
				return list;
			}
		}

		#endregion
	}
}
