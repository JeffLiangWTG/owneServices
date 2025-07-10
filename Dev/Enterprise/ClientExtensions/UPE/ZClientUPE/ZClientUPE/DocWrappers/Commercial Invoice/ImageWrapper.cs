using System.Drawing;
using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class ImageWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ImageWrapper(Image image)
		{
			fImage = image;
		}

		public Image Image
		{
			get
			{
				return fImage;
			}
		}

		readonly Image fImage;
	}
}
