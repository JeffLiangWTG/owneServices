using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface ITokenPinStore
{
	ZString GetPin();

	void SetPin(ZString pin);

	void ResetPin();

	bool IsPinCached();
}
