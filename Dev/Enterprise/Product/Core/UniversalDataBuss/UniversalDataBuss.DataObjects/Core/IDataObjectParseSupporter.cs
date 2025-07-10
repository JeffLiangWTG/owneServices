using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public interface IDataObjectParseSupporter
	{
		ZBool IsElementSupported(string elementName);

		ZString GetErrorTextWhenNonSupportedElementsFound();
	}
}
