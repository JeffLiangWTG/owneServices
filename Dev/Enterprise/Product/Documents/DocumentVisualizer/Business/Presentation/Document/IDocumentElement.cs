using System.Drawing;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IDocumentElement
	{
		/// <summary>
		/// Location of the shape in display units -> inch/100
		/// </summary>
		PointF Location { get; }

		/// <summary>
		/// Size of the shape in display units -> inch/100
		/// </summary>
		SizeF Size { get; }

		int ZOrder { get; }

		ElementType ElementType { get; }
	}
}