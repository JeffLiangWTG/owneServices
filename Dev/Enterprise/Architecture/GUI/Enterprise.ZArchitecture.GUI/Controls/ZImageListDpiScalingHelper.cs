using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public static class ZImageListDpiScalingHelper
	{
		public static void SetScaledImagesFromImageListStreamer(ImageList imageList, ImageListStreamer imageListStreamer)
		{
#if !WINZOR
			if (ControlDpiScalingHelper.BaseDpiX != (int)ControlDpiScalingHelper.DpiX)
			{
				var resourceImageList = new ImageList();
				resourceImageList.ImageStream = imageListStreamer;
				var newImageSize = ControlDpiScalingHelper.NewScaledSize(resourceImageList.ImageSize);

				imageList.Images.Clear();
				imageList.ImageSize = newImageSize;

				foreach (Image resourceImage in resourceImageList.Images)
				{
					imageList.Images.Add(NewBitmap(resourceImage, newImageSize));
				}

				return;
			}
#endif
			imageList.ImageStream = imageListStreamer;
		}

#if !WINZOR
		static Image NewBitmap(Image image, Size newSize)
		{
			return new Bitmap(image, newSize);
		}
#endif
	}
}
