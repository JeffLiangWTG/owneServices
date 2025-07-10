using System.Drawing;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.BufferManagement.Integration
{
	[Immutable]
	public struct ImageWithTooltip
	{
		public ImageWithTooltip(Image image, ZString tooltip)
		{
			this.image = image;
			this.tooltip = tooltip;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Not immutable, but no one is going to touch this.")]
		readonly Image image;
		readonly ZString tooltip;

		public Image Image => image;
		public ZString ImageTooltip => tooltip;

		public override bool Equals(object obj)
		{
			var other = (ImageWithTooltip)obj;
			return Image == other.Image
				&& ImageTooltip == other.ImageTooltip;
		}

		public override int GetHashCode()
		{
			var imageHashCode = Image != null ? Image.GetHashCode() : 0;
			var tooltipHashCode = !ImageTooltip.IsEmpty ? ImageTooltip.GetHashCode() : 0;

			if (imageHashCode == 0 && tooltipHashCode == 0)
			{
				return base.GetHashCode();
			}
			else
			{
				return imageHashCode ^ tooltipHashCode;
			}
		}

		public static bool operator ==(ImageWithTooltip i1, ImageWithTooltip i2)
		{
			return i1.Image == i2.Image
				&& i1.ImageTooltip == i2.ImageTooltip;
		}

		public static bool operator !=(ImageWithTooltip i1, ImageWithTooltip i2)
		{
			return !(i1 == i2);
		}
	}
}
