
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Module;

public static class ModelViewConstants
{
	public static class CHCusEntryHeader
	{
		public const string Name = "CHCusEntryHeader";
		public const string ClusterKey = "CH_ClusterKey";
		public const string LastEComplaintStatus = "CH_LastEComplaintStatus";
	}

	public static FilterCategory AmountFilter => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("DFABC1A3-E237-4602-9EB4-40E0A6713FAE", "Amount Filters"));
}
