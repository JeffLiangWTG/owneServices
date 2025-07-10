using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public interface ICountrySpecificLabelTranslator
	{
		ZString? GetTranslation(LabelsEnum? label, params object[] parameters);
	}
}
