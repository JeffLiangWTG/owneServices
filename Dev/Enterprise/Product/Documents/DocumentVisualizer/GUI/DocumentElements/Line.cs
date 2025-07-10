using System.Drawing;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class Line : DocumentElement
	{
		public Line(PointF location, SizeF size, IPen pen)
			: base(location, size)
		{
			Pen = Argument.NotNull(pen, nameof(pen));
		}

		public override ElementType ElementType => ElementType.Line;

		public IPen Pen { get; }
	}
}