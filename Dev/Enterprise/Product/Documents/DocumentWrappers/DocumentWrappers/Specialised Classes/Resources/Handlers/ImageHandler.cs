using System.Drawing;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers.Resources.Handlers
{
	public class ImageHandler : IObsoleteValidation
	{
		public Image GetImageWithResourcePath(ZString path)
		{
			Image image = null;
			if (!path.IsEmpty)
			{
				Stream stream = GetType().Assembly.GetManifestResourceStream(path);
				if (stream != null)
				{
					image = Image.FromStream(stream);
				}
			}
			return image;
		}
	}
}
