using System.Drawing;

namespace Enterprise.ZArchitecture.GUI
{
	public interface ICaptionedComponents
	{
		object GetCaptionedComponentAt(Point p);
		Rectangle GetCaptionedComponentRect(object component);
		object GetCaptionedComponentData(object component);
	}
}
