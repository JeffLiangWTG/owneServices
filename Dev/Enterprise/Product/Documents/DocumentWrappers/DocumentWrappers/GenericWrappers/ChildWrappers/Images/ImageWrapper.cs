using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Image"), WrapperTypeName("Image")]
	public class ImageWrapper : GenericWrapper
	{
		public ImageWrapper(Image image, BusinessObjectFactory factory)
			: base(null, factory)
		{
			Image = image;
		}

		public Image Image { get; private set; }
	}
}
