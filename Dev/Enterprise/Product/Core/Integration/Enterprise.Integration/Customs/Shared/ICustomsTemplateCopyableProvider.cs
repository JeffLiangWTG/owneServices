using Enterprise.Integration.Freight;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICustomsTemplateCopyableProvider
			{
				void CloneCountrySpecificData(ICommonShipment originalObject, ICommonShipment clonedObject);
			}
		}
	}
}
