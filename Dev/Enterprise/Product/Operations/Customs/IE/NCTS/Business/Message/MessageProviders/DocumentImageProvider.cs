using System.Globalization;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class DocumentImageProvider : IE.Business.DocumentImageProvider
	{
		public DocumentImageProvider(IeDoc eDoc) : base(eDoc)
		{
		}

		protected override string GetSizeText(ZDecimal fileSizeInMB)
		{
			ZString result;
			var sizeFormat = "{0:0.###}{1}";

			if (fileSizeInMB < 1)
			{
				var lenInKB = fileSizeInMB * 1024;

				if (lenInKB < 1)
				{
					var lenInBytes = lenInKB * 1024;
					result = string.Format(CultureInfo.CurrentCulture, sizeFormat, lenInBytes, "B");
				}
				else
				{
					result = string.Format(CultureInfo.CurrentCulture, sizeFormat, lenInKB, "KB");
				}
			}
			else if (fileSizeInMB < 1024)
			{
				result = string.Format(CultureInfo.CurrentCulture, sizeFormat, fileSizeInMB, "MB");
			}
			else
			{
				result = string.Format(CultureInfo.CurrentCulture, sizeFormat, fileSizeInMB / 1024, "GB");
			}
			return result;
		}
	}
}
