using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Integration;

public static partial class Customs
{
	public static partial class Shared
	{
		public interface ICustomsDocumentWrapperProvider
		{
			IDocumentWrapper GetDocumentWrapper(BusinessObject parent);
		}
	}
}
