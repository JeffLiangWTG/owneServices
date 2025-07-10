using System;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.ResourceStrings.Cache
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1178:Use source generated static methods", Justification = "The implementation of the ResourceString")]
	public class CustomizableDataResourceStrings
	{
		public CustomizableDataResourceStrings(ICustomizableDataCaptionSource source)
		{
			Argument.NotNull(source, nameof(source)); // Suggested By ReviewBot
			this.source = source;
		}

		public ResourceString GetMultilingualString(object context, string caption)
		{
			Argument.NotNull(caption, nameof(caption));
			return ResString._GetMultilingualString(source.Asmid, source.GetKey(context, caption), caption);
		}

		public ICustomizableDataCaptionSource Source
		{
			get
			{
				return source;
			}
		}

		readonly ICustomizableDataCaptionSource source;

		public static string GetCustomizableDataKey(string prefix, string caption)
		{
			Argument.NotNull(caption, nameof(caption)); // Suggested By ReviewBot

			var builder = new StringBuilder();
			builder.Append(prefix).Append("$");
			var captionBytes = Encoding.UTF8.GetBytes(caption);
			var captionKeyPart = Convert.ToBase64String(captionBytes);
			if (captionKeyPart.Length > (keyMaxLength - builder.Length))
			{
				using (var md5 = MD5.Create())
				{
					builder.Append(Convert.ToBase64String(md5.ComputeHash(captionBytes))).Append("#");
				}
			}

			builder.Append(captionKeyPart);
			if (builder.Length > keyMaxLength)
			{
				return builder.ToString(0, keyMaxLength);
			}
			else
			{
				return builder.ToString();
			}
		}

		public static ResourceString GetMultilingualString(ICustomizableDataCaptionSource source, object context, string caption)
		{
			Argument.NotNull(caption, nameof(caption));
			Argument.NotNull(source, nameof(source));
			return new CustomizableDataResourceStrings(source).GetMultilingualString(context, caption);
		}

		const int keyMaxLength = 200;
	}
}
