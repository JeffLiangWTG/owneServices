namespace Enterprise.BufferManagement.Service.Shared
{
	public class AcceptabilityBandBoundariesDto
	{
		public int CautionLowerBoundary { get; set; }
		public int GoodLowerBoundary { get; set; }
		public int ExcellentLowerBoundary { get; set; }
		public int ExcellentUpperBoundary { get; set; }
		public int GoodUpperBoundary { get; set; }
		public int CautionUpperBoundary { get; set; }
	}
}
