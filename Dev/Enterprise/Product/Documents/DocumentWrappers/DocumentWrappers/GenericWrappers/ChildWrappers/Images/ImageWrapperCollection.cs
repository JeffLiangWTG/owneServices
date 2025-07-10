using System.Collections.Generic;
using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ImageWrapperCollection : GenericWrapperCollection<ImageWrapper>
	{
		public ImageWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public ImageWrapperCollection(IEnumerable<Image> images, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (Image image in images)
			{
				if (image != null)
				{
					Add(new ImageWrapper(image, factory));
				}
			}
		}
	}
}
