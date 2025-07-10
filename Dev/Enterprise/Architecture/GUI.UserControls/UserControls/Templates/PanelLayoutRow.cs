using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;

namespace Enterprise.ZArchitecture.GUI
{
	public class PanelLayoutRow
	{
		readonly List<IPanelLayoutPart> parts = new List<IPanelLayoutPart>();

		public IReadOnlyList<IPanelLayoutPart> Parts => parts;

		internal void Add(IPanelLayoutPart controlLayoutInfo)
		{
			parts.Add(controlLayoutInfo);
		}

		public ControlReference AlignToControl { get; set; }

		public VerticalAlignment VerticalAlignment { get; set; }
	}
}
