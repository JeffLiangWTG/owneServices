using System.Drawing;

namespace Enterprise.Core.Forms
{
	/// <summary>
	/// This interface wraps the required attributes of a physical screen as used
	/// by CachedScreenInfo and it's subsequent consumers.
	/// </summary>
	public interface IPhysicalScreenInfo
	{
		bool IsPrimary { get; }
		Rectangle WorkingArea { get; }
		Rectangle Bounds { get; }
		int Depth { get; }
		uint DpiX { get; }
		uint DpiY { get; }
	}
}
