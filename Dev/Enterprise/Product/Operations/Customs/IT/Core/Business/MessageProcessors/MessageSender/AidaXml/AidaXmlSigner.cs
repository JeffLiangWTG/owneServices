using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Cryptoki.Common.ClientServerApi;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business;

public sealed class AidaXmlSigner : IAidaXmlSigner
{
	byte[] IAidaXmlSigner.Sign(byte[] xmlBytes, ICryptokiGlbExternalPassword cryptokiCertificate, DateTime signatureTime)
	{
		CheckContentToSignIsValid(xmlBytes);
		var tokenPinStore = CheckCryptokiCertificateIsValid(cryptokiCertificate);

		var cryptoApi = ObjectFactory.Get<ICryptoApi>();
		var chipset = CertificateHelper.ParseChipset(cryptokiCertificate.GP_Name);
		var certSerialNumber = cryptokiCertificate.GP_CertificateSerialNumber;
		var pin = tokenPinStore.GetPin();

		try
		{
			return cryptoApi.SignXadesWithToken(
				xmlBytes,
				chipset,
				certSerialNumber,
				pin,
				signatureTime);
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			tokenPinStore.ResetPin();
			throw new AidaXmlSignerException(ex.Message, ex);
		}
	}

	void CheckContentToSignIsValid(byte[] xmlBytes)
	{
		Argument.NotNull(xmlBytes, nameof(xmlBytes), Res.GetString("32CA7F04-22C3-48C5-9E38-417837CD250D", "XML message is null. Cannot proceed with signing."));

		if (xmlBytes.Length == 0)
		{
			throw new ArgumentException(Res.GetString("F2C48DCE-E308-462A-8327-75BC15C692F5", "XML message is empty. Cannot proceed with signing."), nameof(xmlBytes));
		}
	}

	ITokenPinStore CheckCryptokiCertificateIsValid(ICryptokiGlbExternalPassword cryptokiCertificate)
	{
		Argument.NotNull(cryptokiCertificate, nameof(cryptokiCertificate));
		return Argument.NotNull(cryptokiCertificate.TokenPinStore, nameof(cryptokiCertificate.TokenPinStore));
	}
}
