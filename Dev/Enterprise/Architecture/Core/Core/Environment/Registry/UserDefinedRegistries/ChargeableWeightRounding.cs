namespace Enterprise.ZArchitecture.Core
{
	public enum ChargeableWeightRoundingType { Up, Down, None }

	public static class ChargeableWeightRoundingScales
	{
		public const string Scale05 = "0.5";
		public const string Scale10 = "1.0";
		public const string DefaultScale = Scale05;
	}
}
