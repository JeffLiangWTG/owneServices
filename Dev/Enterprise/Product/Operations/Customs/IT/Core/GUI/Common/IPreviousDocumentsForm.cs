using CargoWise.Types;

namespace Enterprise.Customs.IT.GUI.Common;

public interface IPreviousDocumentsForm
{
	ZString GetUniversalTariffType();

	ZDateTime GetEffectiveDate();
}
