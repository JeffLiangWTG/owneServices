using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class Drawing : DocumentElement
	{
		public Drawing(PointF location, SizeF size, IReadOnlyCollection<byte> imageData, Action<string> errorCallback)
			: base(location, size)
		{
			lazyImage =  new Lazy<Image>(() => GetImage(imageData, errorCallback));
		}

		readonly Lazy<Image> lazyImage;

		public override ElementType ElementType
		{
			get { return ElementType.Drawing; }
		}

		public Image Image
		{
			get { return lazyImage.Value; }
		}

		Image GetImage(IReadOnlyCollection<byte> imageData, Action<string> errorCallback)
		{
			if (imageData != null)
			{
				try
				{
					return (Image)(new ImageConverter()).ConvertFrom(imageData.ToArray());
				}
				catch (ArgumentException e)
				{
					errorCallback?.Invoke((NoResString)"Invalid Image: " + e.Message);
				}
			}
			return null;
		}
	}
}
