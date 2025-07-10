using System.Diagnostics;
using System.Drawing;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	[DebuggerDisplay("[{Location.X}, {Location.Y}] [{Location.X+Size.Width}, {Location.Y+Size.Height}]")]
	abstract class DocumentElement : IDocumentElement
	{
		protected DocumentElement(PointF location, SizeF size)
		{
			Location = location;
			Size = size;
		}

		public PointF Location { get; }
		public SizeF Size { get; }
		public int ZOrder { get; set; }
		public abstract ElementType ElementType { get; }
	}
}