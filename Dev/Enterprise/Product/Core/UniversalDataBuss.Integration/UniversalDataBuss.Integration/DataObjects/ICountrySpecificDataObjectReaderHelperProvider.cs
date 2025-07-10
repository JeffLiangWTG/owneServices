using Enterprise.Integration;

namespace Enterprise.UniversalDataBuss.Integration.DataObjects
{
	public interface ICountrySpecificDataObjectReaderHelperProvider
	{
		IUniversalDataObjectReaderHelper GetUniversalDataObjectReaderHelper(IUniversalObjectFactory factory, string countryCode);
	}
}
