using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using WTG.RtfConverter;

namespace Enterprise.BufferManagement.Service.Helpers
{
	static class CommonServiceHelper
	{
		public static (string hash, string content) BlobToStringWithHash(this ZBlob value)
		{
			var content = value.ToUTF8();
			var hash = HashHelper.GetHash(content);

			return (hash, content);
		}

		public static (string hash, string content) BlobToHtmlWithHash(this ZBlob value)
		{
			var (hash, stringValue) = value.BlobToStringWithHash();
			string content;

			if (!ZArchitecture.Core.ORtfTextUtil.IsRtf(stringValue))
			{
				var textToHtml = new PlainTextToHtmlConverter();
				content = textToHtml.Convert(stringValue);
			}
			else
			{
				var rtfToHtml = new RtfToHtmlConverter();
				content = rtfToHtml.Convert(stringValue);
			}

			return (hash, content);
		}

		public static ZBlob HtmlToZBlob(this string html) => ZBlob.FromUTF8(new HtmlToRtfConverter().Convert(html));
	}
}
