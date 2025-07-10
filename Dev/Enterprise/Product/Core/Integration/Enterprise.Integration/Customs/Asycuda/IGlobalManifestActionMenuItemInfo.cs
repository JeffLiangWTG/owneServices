namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ASYCUDA
		{
			public interface IGlobalManifestActionMenuItemInfo
			{
				object GetNewMenuItem();
				bool Precondition();
			}
		}
	}
}
