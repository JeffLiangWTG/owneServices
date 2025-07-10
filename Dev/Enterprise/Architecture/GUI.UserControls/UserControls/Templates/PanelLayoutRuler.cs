using System;

namespace Enterprise.ZArchitecture.GUI
{
	public class PanelLayoutRuler : IPanelLayoutPart
	{
		public PanelLayoutRuler(int position)
		{
			Position = position;
		}

		public int Position { get; }

		public string Name => FormattableString.Invariant($"Ruler-{Position}"); // For debugging
	}
}
