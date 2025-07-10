using System.Drawing;
using Enterprise.DocumentWrappers.Resources.Handlers;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentWrappers
{
	sealed class FIATALogoProvider : IFIATALogoProvider
	{
		public Image GetFIATALogo(string countryCode)
		{
			if (countryCode?.Length == 2
				&& FreightDataRegistry.Instance.FIATAAuthorised.Value)
			{
				var logoPath = DocConstants.Resources.FIATA.GetGraphicLogoPaths().GetDescriptionFromCode(countryCode);

				if (!string.IsNullOrEmpty(logoPath))
				{
					var imageHandler = new ImageHandler();
					return imageHandler.GetImageWithResourcePath(logoPath);
				}
			}

			return null;
		}

		public Image GetFIATATextLogo(bool forSeaWaybill)
		{
			var textLogoCode = string.Empty;

			if (FreightDataRegistry.Instance.FIATAAuthorised.Value)
			{
				textLogoCode = forSeaWaybill
					? DocConstants.Resources.FIATA.TextLogoCodes.FWB
					: DocConstants.Resources.FIATA.TextLogoCodes.ICC;
			}
			else
			{
				textLogoCode = forSeaWaybill
					? DocConstants.Resources.FIATA.TextLogoCodes.SWB
					: DocConstants.Resources.FIATA.TextLogoCodes.HBL;
			}

			var logoPath = DocConstants.Resources.FIATA.GetTextLogoPaths().GetDescriptionFromCode(textLogoCode);

			if (!string.IsNullOrEmpty(logoPath))
			{
				var imageHandler = new ImageHandler();
				return imageHandler.GetImageWithResourcePath(logoPath);
			}

			return null;
		}
	}
}