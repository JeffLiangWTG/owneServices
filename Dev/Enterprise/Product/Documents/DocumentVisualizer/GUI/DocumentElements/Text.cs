using System.Diagnostics;
using System.Drawing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	[DebuggerDisplay("[{Location.X}, {Location.Y}] [{Location.X+Size.Width}, {Location.Y+Size.Height}] {Content}")]
	sealed class Text : DocumentElement, IText
	{
		public Text(PointF location, SizeF size, RectangleF padding, string content)
			: this(location, size, content)
		{
			this.Content = content;
			Padding = padding;
		}

		public Text(PointF location, SizeF size, string content)
			: base(location, size)
		{
			this.Content = content;
			Padding = RectangleF.Empty;
		}

		public override ElementType ElementType => ElementType.Text;
		public string Content { get; }
		public RectangleF Padding { get; }
		public IFont Font { get; set; } = Core.Font.Empty;
		public Alignment HAlignment { get; set; }
		public Alignment VAlignment { get; set; }
		public bool Wrap { get; set; }
	}
}