using System;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Cryptoki.Common.ClientServerApi;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IN.Business;

public sealed class INMessageSigner : ITextSigner
{
	public INMessageSigner(ICryptokiDetails cryptokiCertificate)
	{
		certificateDetails = Argument.NotNull(cryptokiCertificate, nameof(cryptokiCertificate));
		Argument.NotNull(cryptokiCertificate.TokenPinStore, nameof(cryptokiCertificate.TokenPinStore));
	}

	readonly ICryptokiDetails certificateDetails;

	string ITextSigner.Sign(string message)
	{
		Argument.NotNullOrEmpty(message, nameof(message), "Message is null or empty. Cannot proceed with signing.");

		if (Globals.IsTest)
		{
			return "--Signed--" + System.Environment.NewLine + message;
		}

		var tokenPinStore = certificateDetails.TokenPinStore;
		var cryptoApi = ObjectFactory.Get<ICryptoApi>();
		var pin = tokenPinStore.GetPin();
		var messageBytes = Encoding.UTF8.GetBytes(message);

		try
		{
			var signedMessageBytes = cryptoApi.SignIcegateMessageWithToken(
										messageBytes,
										certificateDetails.LibraryName,
										certificateDetails.CertificateSerialNumber,
										pin);
			return Encoding.UTF8.GetString(signedMessageBytes);
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			tokenPinStore.ResetPin();
			throw;
		}
	}
}
