using CargoWise.Types;

namespace Enterprise.Customs.IN.Business;

public interface ITokenPinStore
{
	void SetPin(ZString value);

	ZString GetPin();

	void ResetPin();
}
