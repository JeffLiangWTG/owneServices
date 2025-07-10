using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface IFirstPlaceOfUseOrProcessingProvider
	{
		PlaceOfUseOrProcessing FirstPlaceOfUseOrProcessing { get; }

		ZString FirstPlaceOfUseOrProcessingDescription { get; }

		ZPropertyInfo FirstPlaceOfUseOrProcessingDescriptionInfo { get; }
	}
}
