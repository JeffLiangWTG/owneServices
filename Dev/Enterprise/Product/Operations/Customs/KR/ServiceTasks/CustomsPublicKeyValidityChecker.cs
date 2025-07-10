using System;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;

namespace Enterprise.Customs.KR.ServiceTasks
{
	internal class CustomsPublicKeyValidityChecker
	{
		internal (string, bool) CheckCustomsPublicKeyStartAndExiryDates()
		{
			string message = null;
			bool isError = false;

			var certificateRawData = KRCustomsRegistry.Instance.CustomsCertificate.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (certificateRawData != null)
			{
				var startAndExpiryDate = GetCustomsPublicKeyStartAndExpiryDates();

				var startDate = startAndExpiryDate.Item1;
				var expiryDate = startAndExpiryDate.Item2;

				if (!expiryDate.IsValid || !startDate.IsValid)
				{
					message = Res.GetString("6188E5D5-212B-482F-BA5C-DE685E12647B", "There is something wrong with the KR Customs Public Key and the start and/or expiry date are invalid.");
					isError = true;
				}
				else if (startDate.IsInTheFutureDatePartOnly)
				{
					message = Res.GetString("F46C820B-4095-49CA-82DD-2E3FFFE337D7", "The KR Customs Public Key Start Date is still in the future and is not valid to use yet.");
					isError = true;
				}
				else if (expiryDate.IsInThePastDatePartOnly)
				{
					message = Res.GetString("82068AB1-B94A-4A4A-B29A-B95F137121A2", "KR Customs Public Key is already expired.");
					isError = true;
				}
				else
				{
					var diffDays = (expiryDate - ZDateTime.Today).Days;
					if (diffDays == 0)
					{
						message = Res.GetString("5AF8E78E-2144-459B-B7B3-36E18A97AFE3", "KR customs public key will expire tomorrow.");
						isError = false;
					}
					else if (diffDays <= 7)
					{
						message = Res.GetString("AFB9DEE0-D08D-4C43-8065-DB636FA03773", "KR Customs Public Key is about to be expired in the next 7 days.");
						isError = false;
					}
				}
			}
			return (message, isError);
		}

		internal virtual (ZDateTime, ZDateTime) GetCustomsPublicKeyStartAndExpiryDates()
		{
			var startDate = ZDateTime.Invalid;
			var expiryDate = ZDateTime.Invalid;

			var certificateRawData = KRCustomsRegistry.Instance.CustomsCertificate.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (certificateRawData != null)
			{
				var certificate = new X509Certificate2(certificateRawData);

				startDate = certificate.NotBefore;
				expiryDate = certificate.NotAfter;
			}

			return (startDate, expiryDate);
		}
	}
}
