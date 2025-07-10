using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Newtonsoft.Json;
using WTG.TrustedMessaging;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class MyAccountTrustedMessenger<T>
		where T : TrustedInfo
	{
		public MyAccountTrustedMessenger()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public MyAccountTrustedMessageProcessResult<T> ReadMessage(HttpRequestMessage request, TrustedRequest trustedRequest, BusinessObjectFactory factory)
		{
			string signHeader = null;
			string iVString = null;
			if (request.Headers.TryGetValues("SIGNED", out var signHeaders) && request.Headers.TryGetValues("WTG_I", out var iVStrings))
			{
				signHeader = signHeaders.FirstOrDefault();
				iVString = iVStrings.FirstOrDefault();
			}

			if (string.IsNullOrWhiteSpace(signHeader) || string.IsNullOrWhiteSpace(iVString))
			{
				return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.Malformed);
			}

			if (trustedRequest.Product == null || trustedRequest.SystemId == null)
			{
				return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.InvalidSystemInfo);
			}

			var trustedSystem = EdiTrustedSystem.Load(factory, trustedRequest.Product, trustedRequest.SystemId);
			if (trustedSystem == null)
			{
				if (ProductTypes.IsEnterpriseFamily(trustedRequest.Product) &&
					EdiTrustedSystem.FindDatabaseByDatabaseNumber(factory, trustedRequest.SystemId) != null)
				{
					return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.CertificateMismatched);
				}
				else
				{
					return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.InvalidSystemInfo);
				}
			}

			var utcNow = ZDateTime.UtcNow;
			if (trustedSystem.ETS_SecretKey.IsEmpty || trustedSystem.ETS_SecretKeyExpiryUtc < utcNow)
			{
				return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.SecretKeyNotUpToDate);
			}

			var trustedMessage = new TrustedMessage()
			{
				EncryptedContent = trustedRequest.EncryptedContent,
				Signature = signHeader,
				IV = iVString
			};

			var messenger = new TrustedMessenger(CertProvidersByDatabase.GetOrAdd(trustedSystem.PK, () => ObjectFactory.New<ICertificatesProvider>(trustedSystem)));
			string message;
			try
			{
				message = messenger.ReadMessageContent(trustedMessage, trustedSystem.ETS_SecretKey);
			}
			catch (CertificateMismatchedException)
			{
				return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.CertificateMismatched);
			}
			catch (Exception)
			{
				return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.SecretKeyNotUpToDate);
			}

			if (string.IsNullOrWhiteSpace(message))
			{
				return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.Malformed);
			}

			T trustedInfo = null;

			try
			{
				trustedInfo = JsonConvert.DeserializeObject<T>(message);
			}
			catch (JsonReaderException e)
			{
				ErrorReporter.ReportOnce("Trusted Messenger JSON Parsing Exception", FormattableString.Invariant($"{e.Message} Message: {message}"));
			}

			if (trustedInfo == null)
			{
				return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.Malformed);
			}

			if (string.IsNullOrEmpty(message) || trustedInfo == null)
			{
				return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.Malformed);
			}

			if (!new ZString(trustedInfo.Product).EqualsIgnoringCase(trustedRequest.Product) || !new ZString(trustedInfo.SystemId).EqualsIgnoringCase(trustedRequest.SystemId))
			{
				return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.InvalidSystemInfo);
			}

			if (trustedInfo.InfoExpires == DateTime.MinValue || trustedInfo.InfoExpires < utcNow)
			{
				return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.InfoExpired);
			}

			return new MyAccountTrustedMessageProcessResult<T>(MyAccountTrustedMessageProcessStatus.Successful, trustedInfo, trustedSystem);
		}

		readonly Dictionary<ZGuid, ICertificatesProvider> CertProvidersByDatabase = new Dictionary<ZGuid, ICertificatesProvider>();
	}
}
