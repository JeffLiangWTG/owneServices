namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class TR
		{
			public interface INctsHeaderProvider
			{
				INctsMessageProvider GetHeaderProvider(ICusInBondHeader header);
			}

			public interface INctsMessageProvider
			{
			}
		}
	}
}
