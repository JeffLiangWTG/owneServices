using System;

namespace Enterprise.ZArchitecture.GUI
{
	public class PanelLayoutRightRuler : IPanelLayoutPart
	{
		public PanelLayoutRightRuler(int position)
		{
			Position = position;
		}

		public int Position { get; }

		public string Name => FormattableString.Invariant($"RightRuler-{Position}"); // For debugging
	}
}
