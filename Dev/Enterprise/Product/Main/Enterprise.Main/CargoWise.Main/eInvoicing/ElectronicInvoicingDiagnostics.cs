using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Common;
using CargoWise.Cryptoki.Common.ClientServerApi;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = CargoWise.Main.Res;
using ResString = CargoWise.Main.ResString;

namespace Enterprise.Accounting.Business.EInvoicing.HardwareTokenSigning
{
	public static class ElectronicInvoicingDiagnostics
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant test message for signature.")]
		public static void HandleTestHardwareTokenSignature(object sender, EventArgs e)
		{
			// IMPORTANT: this has no unit test coverage due to dependence on hardware tokens.
			var caption = ResString.GetMultilingualString("381bf5b1-011e-403d-b24b-0d46b25c4e39", "Test Hardware Token Signature");
			var initialMessage = Res.GetString("db0d5b99-e72e-4643-bdd2-ac708a08e976", @"This process will test your hardware token for signing electronic messages.
Please insert your hardware token now.

The process may take several minutes. Do you wish to continue?");
			if (Globals.Message.Show(initialMessage, caption, ZMessageBoxButtons.YesNo, ZDialogResult.Yes) != ZDialogResult.Yes)
			{
				return;
			}

			var cryptoApi = RemoteCryptoApi.Instance;

			var allChipsets = Enum.GetValues(typeof(Chipset));
			var supportedChipsetsAndCertificates = new Dictionary<Chipset, CertificateInfo[]>();
			foreach (Chipset ch in allChipsets)
			{
				try
				{
					var certs = cryptoApi.GetCertificatesFromToken(ch);
					if (certs != null)
					{
						supportedChipsetsAndCertificates.Add(ch, certs);
					}
				}
				catch (System.IO.IOException)
				{
					// Unsupported chipset is indicated by exception
				}
				catch (CryptographicException)
				{
					// Unsupported chipset is indicated by exception
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var failedLoadCertificatesMessage = Res.GetString("177f3803-3bfd-4638-9837-78f1f3233d4b", "Loading certificates for chipset '{0}' failed with unexpected error:\r\n{1} - {2}", ch, ex.GetType().Name, ex.GetFullMessage());
					Globals.Message.ShowError(failedLoadCertificatesMessage, caption);
				}
			}

			if (supportedChipsetsAndCertificates.Count == 0)
			{
				var noSupportedChipsetMessage = Res.GetString("ac7d2005-d0dc-49ef-9bc2-57b2c13e26e4", "No supported chipset was found for your hardware token. Please check your token is connected, copy required DLLs to Windows System folder, and ensure you are using the most recent version of Remote Desktop Services.");
				Globals.Message.ShowInformation(noSupportedChipsetMessage, caption);
				return;
			}

			var noTokensAvailable = !supportedChipsetsAndCertificates.Values.Any(x => x.Length > 0);
			if (noTokensAvailable)
			{
				var noAvailableTokenMessage = Res.GetString("038eaf8a-6522-4c46-b595-9cd3a0660bd4", "No hardware token was found connected to your computer (or no certificates are present). Please ensure you are using the most recent version of Remote Desktop Services, and run this test again.");
				Globals.Message.ShowInformation(noAvailableTokenMessage, caption);
				return;
			}
			var mulitpleTokensAvailable = supportedChipsetsAndCertificates.Values.Count(x => x.Length > 0) > 1;
			if (mulitpleTokensAvailable)
			{
				var mulitpleChipsetsMessage = Res.GetString("cfaee35b-de54-4bf3-a007-39cff94d33d7", "Multiple hardware tokens were identified ({0}). Only one hardware token is supported for this test. Please disconnect all other hardware tokens and run this test again.", string.Join(",", supportedChipsetsAndCertificates.Keys));
				Globals.Message.ShowInformation(mulitpleChipsetsMessage, caption);
				return;
			}

			var lookup = supportedChipsetsAndCertificates.Single(kvp => kvp.Value.Length > 0);
			var chipset = lookup.Key;
			var chipsetInfo = lookup.Value.First();
			var chipsetIdentifiedWithInfoMessage = Res.GetString("37150a2d-1ed9-4521-b768-5b623a0ee85d", @"Your hardware token chipset was identified as: {0}
Manufacturer: {1}
Model: {2}
Serial: {3}
Label: {4}", chipset, chipsetInfo.TokenManufacturerId, chipsetInfo.TokenModel, chipsetInfo.TokenSerialNumber, chipsetInfo.TokenLabel);
			Globals.Message.ShowInformation(chipsetIdentifiedWithInfoMessage, caption);

			var certificates = supportedChipsetsAndCertificates[chipset]
								.Select(c => new X509Certificate2(c.Content))
								.OrderByDescending(c => c.NotBefore)
								.ToArray();
			var certificateDetailMessages = certificates.Select((c, i) => Res.GetString("e49c0432-a1c4-484d-8eda-1e802443d927", @"Certificate: {0}
  Subject: {1}
  Thumb Print: {2}
  Serial Number: {3}", i + 1, c.Subject, c.Thumbprint, c.SerialNumber));
			var certificateDetailMessage = string.Join("\r\n\r\n", certificateDetailMessages);
			var certificateListMessage = Res.GetString("a7378307-85df-4078-9302-b13f6487ea11", @"Found {0:N0} certificates:
{1}", certificates.Length, certificateDetailMessage);
			Globals.Message.ShowInformation(certificateListMessage, caption);

			var certificate = certificates.First();
			if (certificates.Length > 1)
			{
				var serialNumberQueryMessage = Res.GetString("8b60b45a-3e9d-4c48-a672-a0de3392e807", "Enter certificate number to use for signing test", caption, 0, 32);
				var certNumberString = Globals.Message.QueryDefaultValue("1", serialNumberQueryMessage, caption, 0, 64);
				if (!int.TryParse(certNumberString, System.Globalization.NumberStyles.Integer, DefaultCulture.Instance, out var certNumber))
				{
					return;
				}
				if (certNumber < 1 || certNumber > certificates.Length)
				{
					var invalidCertificateNumberMessage = Res.GetString("247a9d20-71d7-41b3-a3b6-20cd1b62f27d", "Certificate #{0} is not valid.", certNumber);
					Globals.Message.ShowError(invalidCertificateNumberMessage, caption);
					return;
				}
				certificate = certificates[certNumber - 1];
			}
			var serialBytes = AccountingUtils.ParseHex(certificate.SerialNumber);

			var pin = Globals.Message.QueryDefaultValue("123456", "Enter token PIN", caption, 4, 32);

			const string messageToSign = "Testing hardware token signature";
			var messageBytes = System.Text.Encoding.ASCII.GetBytes(messageToSign);
			var messageDigest = cryptoApi.Sha256(messageBytes);

			var preSigningMessage = Res.GetString("ae9a7f2c-a85d-43df-9c6e-cead7090b291", @"Testing signature.
Message: ""{0}""
Bytes: {1}
Digest: {2}", messageToSign, BitConverter.ToString(messageBytes), BitConverter.ToString(messageDigest));
			Globals.Message.ShowInformation(preSigningMessage, caption);

			try
			{
				var signature = cryptoApi.SignWithToken(chipset, pin, serialBytes, messageDigest);
				var successfulSignatureMessage = Res.GetString("e8cb6eed-c8f0-466e-9701-ee99dd532d15", "Signature succeeded:\r\n{0}", BitConverter.ToString(signature));
				Globals.Message.ShowInformation(successfulSignatureMessage, caption);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var failedSignatureMessage = Res.GetString("f4ad2181-058a-433a-9b3a-3bd2af24a942", "Signature failed:\r\n{0} - {1}", ex.GetType().Name, ex.GetFullMessage());
				Globals.Message.ShowError(failedSignatureMessage, caption);
			}
		}
	}
}
