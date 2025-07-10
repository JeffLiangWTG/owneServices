using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class BMPluralizationHelper : IBMPluralizationHelper
	{
		public string GetCountText(int count, string singular = "workflow", string plural = null) => $"{count} {GetSingularOrPlural(count, singular, plural)}";

		public string GetSingularOrPlural(int count, string singular = "workflow", string plural = null)
		{
			plural = plural ?? $"{singular}s";
			return count == 1 ? singular : plural;
		}
	}
}
