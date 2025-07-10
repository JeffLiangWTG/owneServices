using System;
using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;

namespace Enterprise.Customs.EU.Business
{
	public static class CertificateHelper
	{
		public static bool TryGetHexValue(string str, out byte[] value)
		{
			if (string.IsNullOrWhiteSpace(str) || str.Length % 2 != 0)
			{
				value = null;
				return false;
			}

			byte[] res = new byte[str.Length / 2];
			for (int vOfs = 0, sOfs = 0; vOfs < res.Length; vOfs++, sOfs += 2)
			{
				if (!byte.TryParse(str.Substring(sOfs, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
				{
					value = null;
					return false;
				}

				res[vOfs] = b;
			}

			value = res;
			return true;
		}

		public static ICryptokiCertificateProvider GetNewCryptokiCertificateProvider(string certificateSource)
		{
			var builders = ObjectFactory.Get<Hashtable>(nameof(ICryptokiCertificateProvider));
			var objectHandle = (ObjectHandle)builders[certificateSource];
			var supporter = objectHandle?.GetObject();
			return (ICryptokiCertificateProvider)supporter;
		}

		public static Chipset ParseChipset(string chipsetCode)
		{
			if (Enum.TryParse<Chipset>(chipsetCode, out var chipset))
			{
				return chipset;
			}

			throw new CryptographicException(FormattableString.Invariant($"Unknown chipset: '{chipsetCode}'."));
		}
	}
}
