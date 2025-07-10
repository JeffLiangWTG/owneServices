namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface INctsSettings
			{
				bool IsNctsEnabled { get; }
				bool IsUsingPhase5(string countryCode);
			}
		}
	}
}
