namespace Enterprise.BufferManagement.Integration
{
	public interface IBMPluralizationHelper
	{
		public string GetCountText(int count, string singular = "workflow", string plural = null);

		public string GetSingularOrPlural(int count, string singular = "workflow", string plural = null);
	}
}
