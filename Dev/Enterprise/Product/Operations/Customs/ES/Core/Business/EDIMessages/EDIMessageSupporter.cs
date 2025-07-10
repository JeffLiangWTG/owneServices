using System;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging
{
	public class EDIMessageSupporter
	{
		#region PayloadStructure
		public static class PayloadStructure
		{
			public const string VIA = "VIA=";
			public const string DAT = "&DAT=";
			public const string FIR = "&FIR=";
		}
		#endregion

		#region Constants
		public static class Constants
		{
			public const string IDReference = "WTGCW1ES";
		}
		#endregion

		public ZString GetPayload(ZString edifactMessage, ZBlob certificateBytes, ZString password)
		{
			using (var certificate = new X509Certificate2(certificateBytes, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable))
			{
				Argument.NotNull(certificate, "certificate");

				var via = PayloadStructure.VIA + Constants.IDReference;
				var dat = PayloadStructure.DAT + edifactMessage;

				var data = Encoding.ASCII.GetBytes(via + dat);
				var content = new ContentInfo(data);
				var signedCms = new SignedCms(content, true);
				var signer = new CmsSigner(certificate);
				signer.IncludeOption = X509IncludeOption.EndCertOnly;

				signedCms.ComputeSignature(signer);
				var fir = Convert.ToBase64String(signedCms.Encode());

				var datEncoded = PayloadStructure.DAT + UrlEncodeForEScustoms(edifactMessage);
				var firEncoded = PayloadStructure.FIR + UrlEncodeForEScustoms(fir);
				return via + datEncoded + firEncoded;
			}
		}

		string UrlEncodeForEScustoms(string str)
		{
			str = str.Replace("%", "%25");
			str = str.Replace("+", "%2B");
			str = str.Replace("/", "%2F");
			str = str.Replace("&", "%26");

			return str;
		}
	}
}
