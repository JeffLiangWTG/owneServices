using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Core.Forms.Testing
{
	sealed class ZGridDropEditForTesting : ZGridDropEdit
	{
		public ZGridDropEditForTesting()
		{
			ButtonClicked = false;
		}

		public bool ButtonClicked { get; set; }
		protected override bool IsLeftMouseButtonClicked()
		{
			return ButtonClicked;
		}

		public bool IsMouseOnAValidRowOverride { get; set; }
		protected override bool IsMouseOnAValidRow()
		{
			return IsMouseOnAValidRowOverride;
		}
	}
}
