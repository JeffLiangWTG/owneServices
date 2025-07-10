using System;
using CargoWise.Types;

namespace Enterprise.Customs.IN.Business;

public sealed class CertificateTokenPinStore : ITokenPinStore
{
	public static CertificateTokenPinStore Instance => instance ??= new CertificateTokenPinStore();

	CertificateTokenPinStore()
	{ }

	[ThreadStatic]
	static CertificateTokenPinStore instance;

	void ITokenPinStore.SetPin(ZString pin) => this.pin = pin;

	ZString ITokenPinStore.GetPin() => pin;

	void ITokenPinStore.ResetPin() => pin = ZString.Empty;

	ZString pin;
}
