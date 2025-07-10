namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	static class SnapshotBuilderHelper
	{
		internal static T? NullIfNotSpecified<T>(this T value, bool valueSpecified) where T : struct => valueSpecified ? value : null;
	}
}
