using System.IO;
#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace Enterprise.ZArchitecture.Web.Shared
{
	public static class HttpContextExtensions
	{
		public static string GetImageMIMEType(this HttpContext context)
		{
#if NETFRAMEWORK
			var extension = Path.GetExtension(context?.Request.FilePath);
#else
			var path = context?.Request?.Path.HasValue == true
				? context.Request.Path.Value
				: null;

			var extension = Path.GetExtension(path);
#endif
			if (string.IsNullOrEmpty(extension))
			{
				return "image/png"; // Default or safe fallback
			}

			var imageType = GetImageMIMEExtension(extension);

			return $"image/{imageType}"; // MIME type
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "file extension, MIME type")]
		static string GetImageMIMEExtension(string extension)
		{
			switch (extension?.ToUpperInvariant())
			{
				case ".BMP":
				case ".GIF":
					return extension.Substring(1);
				case ".JPG":
				case ".JPEG":
					return "jpg";
			}

			return "png";
		}
	}
}
