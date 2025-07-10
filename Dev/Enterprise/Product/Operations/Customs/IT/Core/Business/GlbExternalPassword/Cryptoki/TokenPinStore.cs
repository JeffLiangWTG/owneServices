using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public sealed class TokenPinStore : ITokenPinStore
{
	ZString ITokenPinStore.GetPin() => pin;

	void ITokenPinStore.ResetPin() => pin = ZString.Empty;

	void ITokenPinStore.SetPin(ZString pin)
	{
		Argument.NotNullOrEmpty(pin, nameof(pin));

		this.pin = pin;
	}

	bool ITokenPinStore.IsPinCached() => !pin.IsEmpty;

	ZString pin;
}
