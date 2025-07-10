namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class eManifest
			{
				public interface ISyntaxErrorsProvider
				{
					string GetSourceMessageTextWithErrorMarks(IEDIMessage message);
				}
			}
		}
	}
}