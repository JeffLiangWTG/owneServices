using CargoWise.Integration;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class JP
		{
			public static partial class AFR
			{
				public interface IHeaderDocumentWrapperProvider
				{
					IDocumentWrapper GetDocumentWrapper(IJPAFRHeader header);
				}
			}
		}
	}
}
